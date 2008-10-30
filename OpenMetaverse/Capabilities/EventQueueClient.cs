/*
 * Copyright (c) 2007-2008, openmetaverse.org
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
using System.Net;
using System.Threading;
using OpenMetaverse.StructuredData;

namespace OpenMetaverse.Capabilities
{
    public class EventQueueClient
    {
        /// <summary>
        /// 
        /// </summary>
        public delegate void ConnectedCallback();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="body"></param>
        public delegate void EventCallback(string eventName, OSDMap body);

        /// <summary></summary>
        public ConnectedCallback OnConnected;
        /// <summary></summary>
        public EventCallback OnEvent;

        public IWebProxy Proxy;

        public bool Running { get { return _Running && _Client.IsBusy; } }

        protected CapsBase _Client;
        protected bool _Dead;
        protected bool _Running;

        public EventQueueClient(Uri eventQueueLocation)
        {
            _Client = new CapsBase(eventQueueLocation);
            _Client.OpenWriteCompleted += new CapsBase.OpenWriteCompletedEventHandler(Client_OpenWriteCompleted);
            _Client.UploadDataCompleted += new CapsBase.UploadDataCompletedEventHandler(Client_UploadDataCompleted);
        }

        public void Start()
        {
            _Dead = false;
            _Client.OpenWriteAsync(_Client.Location);
        }

        public void Stop(bool immediate)
        {
            _Dead = true;

            if (immediate)
                _Running = false;

            if (_Client.IsBusy)
            {
                Logger.DebugLog("Stopping a running event queue");
                _Client.CancelAsync();
            }
            else
            {
                Logger.DebugLog("Stopping an already dead event queue");
            }
        }

        #region Callback Handlers

        private void Client_OpenWriteCompleted(object sender, CapsBase.OpenWriteCompletedEventArgs e)
        {
            bool raiseEvent = false;

            if (!_Dead)
            {
                if (!_Running) raiseEvent = true;

                // We are connected to the event queue
                _Running = true;
            }

            // Create an EventQueueGet request
            OSDMap request = new OSDMap();
            request["ack"] = new OSD();
            request["done"] = OSD.FromBoolean(false);

            byte[] postData = OSDParser.SerializeLLSDXmlBytes(request);

            _Client.UploadDataAsync(_Client.Location, postData);

            if (raiseEvent)
            {
                Logger.DebugLog("Capabilities event queue connected");

                // The event queue is starting up for the first time
                if (OnConnected != null)
                {
                    try { OnConnected(); }
                    catch (Exception ex) { Logger.Log(ex.Message, Helpers.LogLevel.Error, ex); }
                }
            }
        }

        private void Client_UploadDataCompleted(object sender, CapsBase.UploadDataCompletedEventArgs e)
        {
            OSDArray events = null;
            int ack = 0;

            if (e.Error != null)
            {
                HttpStatusCode code = HttpStatusCode.OK;
                if (e.Error is WebException && ((WebException)e.Error).Response != null)
                    code = ((HttpWebResponse)((WebException)e.Error).Response).StatusCode;

                if (code == HttpStatusCode.NotFound || code == HttpStatusCode.Gone)
                {
                    Logger.Log(String.Format("Closing event queue at {0} due to missing caps URI", _Client.Location),
                        Helpers.LogLevel.Info);

                    _Running = false;
                    _Dead = true;
                }
                else if (code == HttpStatusCode.BadGateway)
                {
                    // This is not good (server) protocol design, but it's normal.
                    // The EventQueue server is a proxy that connects to a Squid
                    // cache which will time out periodically. The EventQueue server
                    // interprets this as a generic error and returns a 502 to us
                    // that we ignore
                }
                else if (!e.Cancelled)
                {
                    // Try to log a meaningful error message
                    if (code != HttpStatusCode.OK)
                    {
                        Logger.Log(String.Format("Unrecognized caps connection problem from {0}: {1}",
                            _Client.Location, code), Helpers.LogLevel.Warning);
                    }
                    else if (e.Error.InnerException != null)
                    {
                        Logger.Log(String.Format("Unrecognized caps exception from {0}: {1}",
                            _Client.Location, e.Error.InnerException.Message), Helpers.LogLevel.Warning);
                    }
                    else
                    {
                        Logger.Log(String.Format("Unrecognized caps exception from {0}: {1}",
                            _Client.Location, e.Error.Message), Helpers.LogLevel.Warning);
                    }
                }
            }
            else if (!e.Cancelled && e.Result != null)
            {
                // Got a response
                OSD result = OSDParser.DeserializeLLSDXml(e.Result);
                if (result != null && result.Type == OSDType.Map)
                {
                    // Parse any events returned by the event queue
                    OSDMap map = (OSDMap)result;

                    events = (OSDArray)map["events"];
                    ack = map["id"].AsInteger();
                }
            }
            else if (e.Cancelled)
            {
                // Connection was cancelled
                Logger.DebugLog("Cancelled connection to event queue at " + _Client.Location);
            }

            if (_Running)
            {
                OSDMap request = new OSDMap();
                if (ack != 0) request["ack"] = OSD.FromInteger(ack);
                else request["ack"] = new OSD();
                request["done"] = OSD.FromBoolean(_Dead);

                byte[] postData = OSDParser.SerializeLLSDXmlBytes(request);

                _Client.UploadDataAsync(_Client.Location, postData);

                // If the event queue is dead at this point, turn it off since
                // that was the last thing we want to do
                if (_Dead)
                {
                    _Running = false;
                    Logger.DebugLog("Sent event queue shutdown message");
                }
            }

            if (OnEvent != null && events != null && events.Count > 0)
            {
                // Fire callbacks for each event received
                foreach (OSDMap evt in events)
                {
                    string msg = evt["message"].AsString();
                    OSDMap body = (OSDMap)evt["body"];

                    try { OnEvent(msg, body); }
                    catch (Exception ex) { Logger.Log(ex.Message, Helpers.LogLevel.Error, ex); }
                }
            }
        }

        #endregion Callback Handlers
    }
}
