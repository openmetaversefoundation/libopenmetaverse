/*
 * Copyright (c) 2007-2008, Second Life Reverse Engineering Team
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
using System.Net;
using System.IO;
using System.Threading;

namespace libsecondlife.Capabilities
{
    public class CapsBase
    {
        #region Callback Data Classes

        public class OpenWriteCompletedEventArgs
        {
            public Stream Result;
            public Exception Error;
            public bool Cancelled;
            public object UserState;

            public OpenWriteCompletedEventArgs(Stream result, Exception error, bool cancelled, object userState)
            {
                Result = result;
                Error = error;
                Cancelled = cancelled;
                UserState = userState;
            }
        }

        public class UploadDataCompletedEventArgs
        {
            public byte[] Result;
            public Exception Error;
            public bool Cancelled;
            public object UserState;

            public UploadDataCompletedEventArgs(byte[] result, Exception error, bool cancelled, object userState)
            {
                Result = result;
                Error = error;
                Cancelled = cancelled;
                UserState = userState;
            }
        }

        public class DownloadDataCompletedEventArgs
        {
            public byte[] Result;
            public Exception Error;
            public bool Cancelled;
            public object UserState;
        }

        public class DownloadStringCompletedEventArgs
        {
            public Uri Address;
            public string Result;
            public Exception Error;
            public bool Cancelled;
            public object UserState;

            public DownloadStringCompletedEventArgs(Uri address, string result, Exception error, bool cancelled, object userState)
            {
                Address = address;
                Result = result;
                Error = error;
                Cancelled = cancelled;
                UserState = userState;
            }
        }

        public class DownloadProgressChangedEventArgs
        {
            public long BytesReceived;
            public int ProgressPercentage;
            public long TotalBytesToReceive;
            public object UserState;

            public DownloadProgressChangedEventArgs(long bytesReceived, long totalBytesToReceive, object userToken)
            {
                BytesReceived = bytesReceived;
                ProgressPercentage = (int)(((float)bytesReceived / (float)totalBytesToReceive) * 100f);
                TotalBytesToReceive = totalBytesToReceive;
                UserState = userToken;
            }
        }

        public class UploadProgressChangedEventArgs
        {
            public long BytesReceived;
            public long BytesSent;
            public int ProgressPercentage;
            public long TotalBytesToReceive;
            public long TotalBytesToSend;
            public object UserState;

            public UploadProgressChangedEventArgs(long bytesReceived, long totalBytesToReceive, long bytesSent, long totalBytesToSend, object userState)
            {
                BytesReceived = bytesReceived;
                TotalBytesToReceive = totalBytesToReceive;
                ProgressPercentage = (int)(((float)bytesSent / (float)totalBytesToSend) * 100f);
                BytesSent = bytesSent;
                TotalBytesToSend = totalBytesToSend;
                UserState = userState;
            }
        }

        #endregion Callback Data Classes

        public delegate void OpenWriteCompletedEventHandler(object sender, OpenWriteCompletedEventArgs e);
        public delegate void UploadDataCompletedEventHandler(object sender, UploadDataCompletedEventArgs e);
        public delegate void DownloadStringCompletedEventHandler(object sender, DownloadStringCompletedEventArgs e);
        public delegate void DownloadProgressChangedEventHandler(object sender, DownloadProgressChangedEventArgs e);
        public delegate void UploadProgressChangedEventHandler(object sender, UploadProgressChangedEventArgs e);

        public event OpenWriteCompletedEventHandler OpenWriteCompleted;
        public event UploadDataCompletedEventHandler UploadDataCompleted;
        public event DownloadStringCompletedEventHandler DownloadStringCompleted;
        public event DownloadProgressChangedEventHandler DownloadProgressChanged;
        public event UploadProgressChangedEventHandler UploadProgressChanged;

        public WebHeaderCollection Headers = new WebHeaderCollection();
        public IWebProxy Proxy;

        public Uri Location { get { return location; } }
        public bool IsBusy { get { return isBusy; } }
        public WebHeaderCollection ResponseHeaders { get { return responseHeaders; } }

        protected WebHeaderCollection responseHeaders;
        protected Uri location;
        protected bool isBusy;
        protected Thread asyncThread;
        protected System.Text.Encoding encoding = System.Text.Encoding.Default;

        public CapsBase(Uri location)
        {
            this.location = location;
        }

        public void OpenWriteAsync(Uri address)
        {
            OpenWriteAsync(address, null, null);
        }

        public void OpenWriteAsync(Uri address, string method)
        {
            OpenWriteAsync(address, method, null);
        }

        public void OpenWriteAsync(Uri address, string method, object userToken)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            SetBusy();

            asyncThread = new Thread(delegate(object state)
            {
                object[] args = (object[])state;
                WebRequest request = null;

                try
                {
                    request = SetupRequest((Uri)args[0]);
                    Stream stream = request.GetRequestStream();

                    OnOpenWriteCompleted(new OpenWriteCompletedEventArgs(
                        stream, null, false, args[2]));
                }
                catch (ThreadInterruptedException)
                {
                    if (request != null)
                        request.Abort();

                    OnOpenWriteCompleted(new OpenWriteCompletedEventArgs(
                        null, null, true, args[2]));
                }
                catch (Exception e)
                {
                    OnOpenWriteCompleted(new OpenWriteCompletedEventArgs(
                        null, e, false, args[2]));
                }
            });

            object[] cbArgs = new object[] { address, method, userToken };
            asyncThread.Start(cbArgs);
        }

        public void UploadDataAsync(Uri address, byte[] data)
        {
            UploadDataAsync(address, null, data, null);
        }

        public void UploadDataAsync(Uri address, string method, byte[] data)
        {
            UploadDataAsync(address, method, data, null);
        }

        public void UploadDataAsync(Uri address, string method, byte[] data, object userToken)
        {
            if (address == null)
                throw new ArgumentNullException("address");
            if (data == null)
                throw new ArgumentNullException("data");

            SetBusy();

            asyncThread = new Thread(delegate(object state)
            {
                object[] args = (object[])state;
                byte[] data2;

                try
                {
                    data2 = UploadDataCore((Uri)args[0], (string)args[1], (byte[])args[2], args[3]);

                    OnUploadDataCompleted(
                        new UploadDataCompletedEventArgs(data2, null, false, args[3]));
                }
                catch (ThreadInterruptedException)
                {
                    OnUploadDataCompleted(
                        new UploadDataCompletedEventArgs(null, null, true, args[3]));
                }
                catch (Exception e)
                {
                    OnUploadDataCompleted(
                        new UploadDataCompletedEventArgs(null, e, false, args[3]));
                }
            });

            object[] cbArgs = new object[] { address, method, data, userToken };
            asyncThread.Start(cbArgs);
        }

        public void DownloadStringAsync (Uri address)
		{
			DownloadStringAsync (address, null);
		}

        public void DownloadStringAsync(Uri address, object userToken)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            SetBusy();

            asyncThread = new Thread(delegate(object state)
            {
                object[] args = (object[])state;

                try
                {
                    string data = encoding.GetString(DownloadDataCore((Uri)args[0], args[1]));
                    OnDownloadStringCompleted(
                        new DownloadStringCompletedEventArgs(location, data, null, false, args[1]));
                }
                catch (ThreadInterruptedException)
                {
                    OnDownloadStringCompleted(
                        new DownloadStringCompletedEventArgs(location, null, null, true, args[1]));
                }
                catch (Exception e)
                {
                    OnDownloadStringCompleted(
                        new DownloadStringCompletedEventArgs(location, null, e, false, args[1]));
                }
            });

            object[] cbArgs = new object[] { address, userToken };
            asyncThread.Start(cbArgs);
        }

        public void CancelAsync()
        {
            if (asyncThread == null)
                return;

            Thread t = asyncThread;
            CompleteAsync();
            t.Interrupt();
        }

        protected void CompleteAsync()
        {
            isBusy = false;
            asyncThread = null;
        }

        protected void SetBusy()
        {
            CheckBusy();
            isBusy = true;
        }

        protected void CheckBusy()
        {
            if (isBusy)
                throw new NotSupportedException("CapsBase does not support concurrent I/O operations.");
        }

        protected Stream ProcessResponse(WebResponse response)
        {
            responseHeaders = response.Headers;
            return response.GetResponseStream();
        }

        protected byte[] ReadAll(Stream stream, int length, object userToken, bool uploading)
        {
            MemoryStream ms = null;

            bool nolength = (length == -1);
            int size = ((nolength) ? 8192 : length);
            if (nolength)
                ms = new MemoryStream();

            long total = 0;
            int nread = 0;
            int offset = 0;
            byte[] buffer = new byte[size];

            while ((nread = stream.Read(buffer, offset, size)) != 0)
            {
                if (nolength)
                {
                    ms.Write(buffer, 0, nread);
                }
                else
                {
                    offset += nread;
                    size -= nread;
                }

                if (uploading)
                {
                    if (UploadProgressChanged != null)
                    {
                        total += nread;
                        UploadProgressChanged(this, new UploadProgressChangedEventArgs(nread, length, 0, 0, userToken));
                    }
                }
                else
                {
                    if (DownloadProgressChanged != null)
                    {
                        total += nread;
                        DownloadProgressChanged(this, new DownloadProgressChangedEventArgs(nread, length, userToken));
                    }
                }
            }

            if (nolength)
                return ms.ToArray();

            return buffer;
        }

        protected WebRequest SetupRequest(Uri uri)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);

            if (request == null)
                throw new ArgumentException("Could not create an HttpWebRequest from the given Uri", "address");

            location = uri;

            if (Proxy != null)
                request.Proxy = Proxy;

            request.Method = "POST";

            if (Headers != null && Headers.Count != 0)
            {
                string expect = Headers["Expect"];
                string contentType = Headers["Content-Type"];
                string accept = Headers["Accept"];
                string connection = Headers["Connection"];
                string userAgent = Headers["User-Agent"];
                string referer = Headers["Referer"];

                if (!String.IsNullOrEmpty(expect))
                    request.Expect = expect;

                if (!String.IsNullOrEmpty(accept))
                    request.Accept = accept;

                if (!String.IsNullOrEmpty(contentType))
                    request.ContentType = contentType;

                if (!String.IsNullOrEmpty(connection))
                    request.Connection = connection;

                if (!String.IsNullOrEmpty(userAgent))
                    request.UserAgent = userAgent;

                if (!String.IsNullOrEmpty(referer))
                    request.Referer = referer;
            }

            // Disable keep-alive by default
            request.KeepAlive = false;
            // Set the closed connection (idle) time to one second
            request.ServicePoint.MaxIdleTime = 1000;
            // Disable stupid Expect-100: Continue header
            request.ServicePoint.Expect100Continue = false;
            // Crank up the max number of connections (default is 2!)
            request.ServicePoint.ConnectionLimit = 20;

            return request;
        }

        protected WebRequest SetupRequest(Uri uri, string method)
        {
            WebRequest request = SetupRequest(uri);
            request.Method = method;
            return request;
        }

        protected byte[] UploadDataCore(Uri address, string method, byte[] data, object userToken)
        {
            HttpWebRequest request = (HttpWebRequest)SetupRequest(address);
            // Re-enable Keep-Alive
            //request.KeepAlive = true;

            try
            {
                int contentLength = data.Length;
                request.ContentLength = contentLength;

                using (Stream stream = request.GetRequestStream())
                {
                    // Most uploads are very small chunks of data, use an optimized path for these
                    if (contentLength < 4096)
                    {
                        stream.Write(data, 0, contentLength);
                    }
                    else
                    {
                        // Upload chunks directly instead of buffering to memory
                        request.AllowWriteStreamBuffering = false;

                        MemoryStream ms = new MemoryStream(data);

                        byte[] buffer = new byte[checked((uint)Math.Min(4096, (int)contentLength))];
                        int bytesRead = 0;

                        while ((bytesRead = ms.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            stream.Write(buffer, 0, bytesRead);

                            if (UploadProgressChanged != null)
                            {
                                UploadProgressChanged(this, new UploadProgressChangedEventArgs(0, 0, bytesRead, contentLength, userToken));
                            }
                        }

                        ms.Close();
                    }
                }

                WebResponse response = request.GetResponse();
                Stream st = ProcessResponse(response);

                return ReadAll(st, (int)response.ContentLength, userToken, true);
            }
            catch (ThreadInterruptedException)
            {
                if (request != null)
                    request.Abort();
                throw;
            }
        }

        protected byte[] DownloadDataCore(Uri address, object userToken)
        {
            WebRequest request = null;

            try
            {
                request = SetupRequest(address, "GET");
                WebResponse response = request.GetResponse();
                Stream st = ProcessResponse(response);
                return ReadAll(st, (int)response.ContentLength, userToken, false);
            }
            catch (ThreadInterruptedException)
            {
                if (request != null)
                    request.Abort();
                throw;
            }
            catch (Exception ex)
            {
                throw new WebException("An error occurred performing a WebClient request.", ex);
            }
        }

        protected virtual void OnOpenWriteCompleted(OpenWriteCompletedEventArgs args)
        {
            CompleteAsync();
            if (OpenWriteCompleted != null)
                OpenWriteCompleted(this, args);
        }

        protected virtual void OnUploadDataCompleted(UploadDataCompletedEventArgs args)
        {
            CompleteAsync();
            if (UploadDataCompleted != null)
                UploadDataCompleted(this, args);
        }

        protected virtual void OnDownloadStringCompleted(DownloadStringCompletedEventArgs args)
        {
            CompleteAsync();
            if (DownloadStringCompleted != null)
                DownloadStringCompleted(this, args);
        }
    }
}
