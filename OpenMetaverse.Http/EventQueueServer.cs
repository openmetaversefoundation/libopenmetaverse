/*
 * Copyright (c) 2008, openmetaverse.org
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the openmetaverse.org nor the names
 *   of its contributors may be used to endorse or promote products derived from
 *   this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Threading;
using OpenMetaverse.StructuredData;
using HttpServer;
using HttpListener = HttpServer.HttpListener;

namespace OpenMetaverse.Http
{
    public class EventQueueEvent
    {
        public string Name;
        public OSDMap Body;

        public EventQueueEvent(string name, OSDMap body)
        {
            Name = name;
            Body = body;
        }
    }

    public class EventQueueServer
    {
        /// <summary>The number of milliseconds to wait before the connection times out
        /// and an empty response is sent to the client. This value should be higher
        /// than BATCH_WAIT_INTERVAL for the timeout to function properly</summary>
        const int CONNECTION_TIMEOUT = 120000;

        /// <summary>This interval defines the amount of time to wait, in milliseconds,
        /// for new events to show up on the queue before sending a response to the 
        /// client and completing the HTTP request. The interval also specifies the 
        /// maximum time that can pass before the queue shuts down after Stop() or the
        /// class destructor is called</summary>
        const int BATCH_WAIT_INTERVAL = 200;

        /// <summary>Since multiple events can be batched together and sent in the same
        /// response, this prevents the event queue thread from infinitely dequeueing 
        /// events and never sending a response if there is a constant stream of new 
        /// events</summary>
        const int MAX_EVENTS_PER_RESPONSE = 5;

        HttpListener server;
        BlockingQueue<EventQueueEvent> eventQueue;
        int currentID;
        volatile bool running;
        volatile bool threadRunning;
        IHttpClientContext context;
        IHttpRequest request;
        IHttpResponse response;

        public EventQueueServer(HttpListener server)
        {
            this.server = server;
            eventQueue = new BlockingQueue<EventQueueEvent>();
            running = true;
            currentID = 1;
        }

        ~EventQueueServer()
        {
            Stop();
        }

        public void Stop()
        {
            running = false;
        }

        public void SendEvent(string eventName, OSDMap body)
        {
            SendEvent(new EventQueueEvent(eventName, body));
        }

        public void SendEvent(EventQueueEvent eventQueueEvent)
        {
            if (!running)
                throw new InvalidOperationException("Cannot add event while the queue is stopped");

            eventQueue.Enqueue(eventQueueEvent);
        }

        public void SendEvents(IList<EventQueueEvent> events)
        {
            if (!running)
                throw new InvalidOperationException("Cannot add event while the queue is stopped");

            for (int i = 0; i < events.Count; i++)
                eventQueue.Enqueue(events[i]);
        }

        public bool EventQueueHandler(IHttpClientContext context, IHttpRequest request, IHttpResponse response)
        {
            // Decode the request
            OSD osdRequest = null;

            try { osdRequest = OSDParser.DeserializeLLSDXml(request.Body); }
            catch (Exception) { }

            if (request != null && osdRequest.Type == OSDType.Map)
            {
                OSDMap requestMap = (OSDMap)osdRequest;
                int ack = requestMap["ack"].AsInteger();
                bool done = requestMap["done"].AsBoolean();

                if (ack != currentID - 1 && ack != 0)
                {
                    Logger.Log.WarnFormat("[EventQueue] Received an ack for id {0}, last id sent was {1}",
                        ack, currentID - 1);
                }

                if (!done)
                {
                    if (threadRunning)
                    {
                        Logger.Log.Info("[EventQueue] New connection opened to the event queue while a previous connection is open. Closing old connection");
                        
                        // If the old connection is still open, queue a signal to close it. Otherwise, just wait for the closed
                        // connection to be detected by the handler thread
                        if (context.Stream != null && context.Stream.CanWrite)
                            SendEvent(null);

                        while (threadRunning && running)
                            Thread.Sleep(50);

                        Logger.Log.Info("[EventQueue] Old connection closed");
                    }

                    this.context = context;
                    this.request = request;
                    this.response = response;

                    // Spawn a new thread to hold the connection open and return from our precious IOCP thread
                    Thread thread = new Thread(new ThreadStart(EventQueueThread));
                    thread.Start();

                    // Tell HttpServer to leave the connection open
                    return false;
                }
                else
                {
                    Logger.Log.InfoFormat("[EventQueue] Shutting down the event queue {0} at the client's request",
                        request.Uri);
                    Stop();

                    response.Connection = request.Connection;
                    return true;
                }
            }
            else
            {
                Logger.Log.WarnFormat("[EventQueue] Received a request with invalid or missing LLSD at {0}, closing the connection",
                    request.Uri);

                response.Connection = request.Connection;
                response.Status = HttpStatusCode.BadRequest;
                return true;
            }
        }

        void EventQueueThread()
        {
            threadRunning = true;
            EventQueueEvent eventQueueEvent = null;
            int totalMsPassed = 0;

            while (running && context.Stream != null && context.Stream.CanWrite)
            {
                if (eventQueue.Dequeue(BATCH_WAIT_INTERVAL, ref eventQueueEvent))
                {
                    // An event was dequeued
                    totalMsPassed = 0;
                    List<EventQueueEvent> eventsToSend = null;

                    if (eventQueueEvent != null)
                    {
                        eventsToSend = new List<EventQueueEvent>();
                        eventsToSend.Add(eventQueueEvent);

                        DateTime start = DateTime.Now;
                        int batchMsPassed = 0;

                        // Wait BATCH_WAIT_INTERVAL milliseconds looking for more events,
                        // or until the size of the current batch equals MAX_EVENTS_PER_RESPONSE
                        while (batchMsPassed < BATCH_WAIT_INTERVAL && eventsToSend.Count < MAX_EVENTS_PER_RESPONSE)
                        {
                            if (eventQueue.Dequeue(BATCH_WAIT_INTERVAL - batchMsPassed, ref eventQueueEvent) && eventQueueEvent != null)
                                eventsToSend.Add(eventQueueEvent);

                            batchMsPassed = (int)(DateTime.Now - start).TotalMilliseconds;
                        }
                    }
                    else
                    {
                        Logger.Log.Info("[EventQueue] Dequeued a signal to close the handler thread");
                    }

                    // Make sure we can actually send the events right now
                    if (context.Stream == null || !context.Stream.CanWrite)
                    {
                        Logger.Log.Info("[EventQueue] Connection is closed, requeuing events and closing the handler thread");
                        if (eventsToSend != null)
                        {
                            for (int i = 0; i < eventsToSend.Count; i++)
                                eventQueue.Enqueue(eventsToSend[i]);
                        }
                        goto ThreadDone;
                    }

                    SendResponse(context, request, response, eventsToSend);
                    goto ThreadDone;
                }
                else
                {
                    // BATCH_WAIT_INTERVAL milliseconds passed with no event. Check if the connection
                    // has timed out yet.
                    totalMsPassed += BATCH_WAIT_INTERVAL;

                    if (totalMsPassed >= CONNECTION_TIMEOUT)
                    {
                        Logger.Log.DebugFormat(
                            "[EventQueue] {0}ms passed without an event, timing out the event queue",
                            totalMsPassed);
                        SendResponse(context, request, response, null);
                        goto ThreadDone;
                    }
                }
            }

        ThreadDone:
            threadRunning = false;
            Logger.Log.Debug("[EventQueue] Handler thread is exiting");
        }

        void SendResponse(IHttpClientContext context, IHttpRequest request, IHttpResponse response, List<EventQueueEvent> eventsToSend)
        {
            response.Connection = request.Connection;

            if (eventsToSend != null)
            {
                OSDArray responseArray = new OSDArray(eventsToSend.Count);

                // Put all of the events in an array
                for (int i = 0; i < eventsToSend.Count; i++)
                {
                    EventQueueEvent currentEvent = eventsToSend[i];

                    OSDMap eventMap = new OSDMap(2);
                    eventMap.Add("message", OSD.FromString(currentEvent.Name));
                    eventMap.Add("body", currentEvent.Body);
                    responseArray.Add(eventMap);
                }

                // Create a map containing the events array and the id of this response
                OSDMap responseMap = new OSDMap(2);
                responseMap.Add("events", responseArray);
                responseMap.Add("id", OSD.FromInteger(currentID++));

                // Serialize the events and send the response
                string responseBody = OSDParser.SerializeLLSDXmlString(responseMap);

                Logger.Log.Debug("[EventQueue] Sending " + responseArray.Count + " events over the event queue");
                context.Respond(HttpHelper.HTTP11, HttpStatusCode.OK, "OK", responseBody, "application/xml");
            }
            else
            {
                Logger.Log.Debug("[EventQueue] Sending a timeout response over the event queue");

                // The 502 response started as a bug in the LL event queue server implementation,
                // but is now hardcoded into the protocol as the code to use for a timeout
                context.Respond(HttpHelper.HTTP10, HttpStatusCode.BadGateway, "Upstream error:", "Upstream error:", null);
            }
        }
    }
}
