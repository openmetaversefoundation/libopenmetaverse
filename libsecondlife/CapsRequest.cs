/*
 * Copyright (c) 2007, Second Life Reverse Engineering Team
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the Second Life Reverse Engineering Team nor the names
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
using System.Collections;
using System.Net;
using System.Text;
using libsecondlife.LLSD;

namespace libsecondlife
{
    public class CapsRequest : HttpBase
    {
        public delegate void CapsResponseCallback(object response, HttpRequestState state);

        public event CapsResponseCallback OnCapsResponse;

        public Simulator Simulator;

        public CapsRequest(string capsURI)
            : this(capsURI, String.Empty, null)
        {
        }

        public CapsRequest(string capsURI, string proxy)
            : this(capsURI, proxy, null)
        {
        }

        public CapsRequest(string capsURI, Simulator simulator)
            : this(capsURI, String.Empty, simulator)
        {
        }

        public CapsRequest(string capsURI, string proxy, Simulator simulator)
            : base(capsURI, proxy)
        {
            Simulator = simulator;
        }

        public new void MakeRequest()
        {
            base.MakeRequest(new byte[0], null, 0, null);
        }

        protected override void RequestSent(HttpRequestState request)
        {
            ;
        }

        protected override void RequestReply(HttpRequestState state, bool success, WebException exception)
        {
            object response = null;

            if (success)
            {
                response = LLSDParser.DeserializeXml(state.ResponseData);
            }
            else if (exception != null && exception.Message.Contains("502"))
            {
                // These are normal, retry the request automatically
                MakeRequest(state.RequestData, "application/xml", 0, null);

                return;
            }

            // Only fire the callback if there is response data or the call has
            // not been aborted. Timeouts and 502 errors don't count as aborting,
            // although 502 errors are already handled above
            if (response != null || !_Aborted)
            {
                if (OnCapsResponse != null)
                {
                    try { OnCapsResponse(response, state); }
                    catch (Exception e) { SecondLife.LogStatic(e.ToString(), Helpers.LogLevel.Error); }
                }
            }
        }
    }
}
