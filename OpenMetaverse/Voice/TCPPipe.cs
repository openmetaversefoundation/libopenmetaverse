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
using System.Net.Sockets;

namespace OpenMetaverse.Voice
{
    public class TCPPipe
    {
        protected class SocketPacket
        {
            public System.Net.Sockets.Socket TCPSocket;
            public byte[] DataBuffer = new byte[1];
        }

        public delegate void OnReceiveLineCallback(string line);
        public delegate void OnDisconnectedCallback(SocketException se);

        public event OnReceiveLineCallback OnReceiveLine;
        public event OnDisconnectedCallback OnDisconnected;

        protected Socket _TCPSocket;
        protected IAsyncResult _Result;
        protected AsyncCallback _Callback;
        protected string _Buffer = String.Empty;

        public bool Connected
        {
            get
            {
                if (_TCPSocket != null && _TCPSocket.Connected)
                    return true;
                else
                    return false;
            }
        }

        public TCPPipe()
        {
        }

        public SocketException Connect(string address, int port)
        {
            if (_TCPSocket != null && _TCPSocket.Connected)
                Disconnect();

            try
            {
                IPAddress ip;
                if (!IPAddress.TryParse(address, out ip))
                {
                    IPAddress[] ips = Dns.GetHostAddresses(address);
                    ip = ips[0];
                }
                _TCPSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint ipEndPoint = new IPEndPoint(ip, port);
                _TCPSocket.Connect(ipEndPoint);
                if (_TCPSocket.Connected)
                {
                    WaitForData();
                    return null;
                }
                else
                {
                    return new SocketException(10000);
                }
            }
            catch (SocketException se)
            {
                return se;
            }
        }

        public void Disconnect()
        {
            _TCPSocket.Disconnect(true);
        }

        public void SendData(byte[] data)
        {
            if (Connected)
                _TCPSocket.Send(data);
            else
                throw new InvalidOperationException("socket is not connected");
        }

        public void SendLine(string message)
        {
            if (Connected)
            {
                byte[] byData = System.Text.Encoding.ASCII.GetBytes(message + "\n");
                _TCPSocket.Send(byData);
            }
            else
            {
                throw new InvalidOperationException("socket is not connected");
            }
        }

        void WaitForData()
        {
            try
            {
                if (_Callback == null) _Callback = new AsyncCallback(OnDataReceived);
                SocketPacket packet = new SocketPacket();
                packet.TCPSocket = _TCPSocket;
                _Result = _TCPSocket.BeginReceive(packet.DataBuffer, 0, packet.DataBuffer.Length, SocketFlags.None, _Callback, packet);
            }
            catch (SocketException se)
            {
                Console.WriteLine(se.Message);
            }
        }

        static char[] splitNull = { '\0' };
        static string[] splitLines = { "\r", "\n", "\r\n" };

        void ReceiveData(string data)
        {
            if (OnReceiveLine == null) return;

            //string[] splitNull = { "\0" };
            string[] line = data.Split(splitNull, StringSplitOptions.None);
            _Buffer += line[0];
            //string[] splitLines = { "\r\n", "\r", "\n" };
            string[] lines = _Buffer.Split(splitLines, StringSplitOptions.None);
            if (lines.Length > 1)
            {
                int i;
                for (i = 0; i < lines.Length - 1; i++)
                {
                    if (lines[i].Trim().Length > 0) OnReceiveLine(lines[i]);
                }
                _Buffer = lines[i];
            }
        }

        void OnDataReceived(IAsyncResult asyn)
        {
            try
            {
                SocketPacket packet = (SocketPacket)asyn.AsyncState;
                int end = packet.TCPSocket.EndReceive(asyn);
                char[] chars = new char[end + 1];
                System.Text.Decoder d = System.Text.Encoding.UTF8.GetDecoder();
                d.GetChars(packet.DataBuffer, 0, end, chars, 0);
                System.String data = new System.String(chars);
                ReceiveData(data);
                WaitForData();
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine("WARNING: Socket closed unexpectedly");
            }
            catch (SocketException se)
            {
                if (!_TCPSocket.Connected)
                {
                    if(OnDisconnected != null)
                        OnDisconnected(se);
                }
            }
        }

    }
}
