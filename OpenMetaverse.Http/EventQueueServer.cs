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

        WebServer server;
        BlockingQueue<EventQueueEvent> eventQueue = new BlockingQueue<EventQueueEvent>();
        int currentID = 0;
        bool running = true;

        public EventQueueServer(WebServer server)
        {
            this.server = server;
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

        public bool EventQueueHandler(ref HttpListenerContext context)
        {
            // Decode the request
            OSD request = null;

            try { request = OSDParser.DeserializeLLSDXml(context.Request.InputStream); }
            catch (Exception) { }

            if (request != null && request.Type == OSDType.Map)
            {
                OSDMap requestMap = (OSDMap)request;
                int ack = requestMap["ack"].AsInteger();
                bool done = requestMap["done"].AsBoolean();

                if (ack != currentID - 1)
                {
                    Logger.Log.WarnFormat("[EventQueue] Received an ack for id {0}, last id sent was {1}",
                        ack, currentID - 1);
                }

                if (!done)
                {
                    StartEventQueueThread(context);

                    // Tell HttpServer to leave the connection open
                    return false;
                }
                else
                {
                    Logger.Log.InfoFormat("[EventQueue] Shutting down the event queue {0} at the client's request",
                        context.Request.Url);
                    Stop();

                    context.Response.KeepAlive = context.Request.KeepAlive;
                    return true;
                }
            }
            else
            {
                Logger.Log.WarnFormat("[EventQueue] Received a request with invalid or missing LLSD at {0}, closing the connection",
                    context.Request.Url);

                context.Response.KeepAlive = context.Request.KeepAlive;
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return true;
            }
        }

        void StartEventQueueThread(HttpListenerContext httpContext)
        {
            // Spawn a new thread to hold the connection open and return from our precious IOCP thread
            Thread thread = new Thread(new ThreadStart(
                delegate()
                {
                    EventQueueEvent eventQueueEvent = null;
                    int totalMsPassed = 0;

                    while (running)
                    {
                        if (eventQueue.Dequeue(BATCH_WAIT_INTERVAL, ref eventQueueEvent))
                        {
                            // An event was dequeued
                            totalMsPassed = 0;

                            List<EventQueueEvent> eventsToSend = new List<EventQueueEvent>();
                            eventsToSend.Add(eventQueueEvent);

                            DateTime start = DateTime.Now;
                            int batchMsPassed = 0;

                            // Wait BATCH_WAIT_INTERVAL milliseconds looking for more events,
                            // or until the size of the current batch equals MAX_EVENTS_PER_RESPONSE
                            while (batchMsPassed < BATCH_WAIT_INTERVAL && eventsToSend.Count < MAX_EVENTS_PER_RESPONSE)
                            {
                                if (eventQueue.Dequeue(BATCH_WAIT_INTERVAL - batchMsPassed, ref eventQueueEvent))
                                    eventsToSend.Add(eventQueueEvent);

                                batchMsPassed = (int)(DateTime.Now - start).TotalMilliseconds;
                            }

                            SendResponse(httpContext, eventsToSend);
                            return;
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
                                SendResponse(httpContext, null);
                                return;
                            }
                        }
                    }

                    Logger.Log.Info("[EventQueue] Handler thread is no longer running");
                }
            ));

            thread.Start();
        }

        void SendResponse(HttpListenerContext httpContext, List<EventQueueEvent> eventsToSend)
        {
            httpContext.Response.KeepAlive = httpContext.Request.KeepAlive;

            if (eventsToSend != null)
            {
                OSDArray responseArray = new OSDArray(eventsToSend.Count);

                // Put all of the events in an array
                for (int i = 0; i < eventsToSend.Count; i++)
                {
                    EventQueueEvent currentEvent = eventsToSend[i];

                    OSDMap eventMap = new OSDMap(2);
                    eventMap.Add("body", currentEvent.Body);
                    eventMap.Add("message", OSD.FromString(currentEvent.Name));
                    responseArray.Add(eventMap);
                }

                // Create a map containing the events array and the id of this response
                OSDMap responseMap = new OSDMap(2);
                responseMap.Add("events", responseArray);
                responseMap.Add("id", OSD.FromInteger(currentID++));

                // Serialize the events and send the response
                byte[] buffer = OSDParser.SerializeLLSDXmlBytes(responseMap);
                httpContext.Response.ContentType = "application/xml";
                httpContext.Response.ContentLength64 = buffer.Length;
                httpContext.Response.OutputStream.Write(buffer, 0, buffer.Length);
                httpContext.Response.OutputStream.Close();
                httpContext.Response.Close();
            }
            else
            {
                // The 502 response started as a bug in the LL event queue server implementation,
                // but is now hardcoded into the protocol as the code to use for a timeout
                httpContext.Response.StatusCode = (int)HttpStatusCode.BadGateway;
                httpContext.Response.Close();
            }
        }
    }
}
