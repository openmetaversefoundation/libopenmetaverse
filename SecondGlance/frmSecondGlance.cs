/*
 *
 * Copyright (c) 2007 John Hurliman
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Net;
using System.Windows.Forms;
using libsecondlife;
using libsecondlife.Packets;
using SLProxy;

namespace SecondGlance
{
    public partial class frmSecondGlance : Form
    {
        private Proxy Proxy;
        private Queue<LoggedPacket> Inbox = new Queue<LoggedPacket>();
        private System.Timers.Timer DisplayTimer = new System.Timers.Timer(200);

        public frmSecondGlance()
        {
            InitializeComponent();

            PacketType[] types = (PacketType[])Enum.GetValues(typeof(PacketType));

            // Fill up the "To Log" combo box with options
            foreach (PacketType type in types)
            {
                if (type != PacketType.Default) cboToLog.Items.Add(type);
            }

            // Set the default selection to the first entry
            cboToLog.SelectedIndex = 0;

            // Setup the proxy
            ProxyConfig proxyConfig = new ProxyConfig("Second Glance", "John Hurliman <jhurliman@wsu.edu>", 
                new string[0]);
            Proxy = new Proxy(proxyConfig);

            Proxy.Start();

            // Start the timer that moves packets from the queue and displays them
            DisplayTimer.Elapsed += new System.Timers.ElapsedEventHandler(DisplayTimer_Elapsed);
            DisplayTimer.Start();
        }

        void DisplayTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.BeginInvoke(new MethodInvoker(UpdateDisplay));
        }

        void UpdateDisplay()
        {
            lock (Inbox)
            {
                while (Inbox.Count > 0)
                {
                    lstPackets.Items.Add(Inbox.Dequeue());
                }
            }
        }

        private void frmSecondGlance_FormClosing(object sender, FormClosingEventArgs e)
        {
            DisplayTimer.Stop();
            Proxy.Stop();
        }

        private void cmdAddLogger_Click(object sender, EventArgs e)
        {
            if (!cboLogged.Items.Contains(cboToLog.SelectedItem))
            {
                PacketType type = (PacketType)cboToLog.SelectedItem;

                cboLogged.Items.Add(type);

                Proxy.AddDelegate(type, Direction.Incoming, new PacketDelegate(IncomingPacketHandler));
                Proxy.AddDelegate(type, Direction.Outgoing, new PacketDelegate(OutgoingPacketHandler));
            }
        }

        private void cmdDontLog_Click(object sender, EventArgs e)
        {
            ;
        }

        private Packet IncomingPacketHandler(Packet packet, IPEndPoint sim)
        {
            return PacketHandler(packet, sim, Direction.Incoming);
        }

        private Packet OutgoingPacketHandler(Packet packet, IPEndPoint sim)
        {
            return PacketHandler(packet, sim, Direction.Outgoing);
        }

        private Packet PacketHandler(Packet packet, IPEndPoint sim, Direction direction)
        {
            LoggedPacket logged = new LoggedPacket();
            logged.Packet = packet;
            logged.Sim = sim;
            logged.Direction = direction;

            lock (Inbox)
                Inbox.Enqueue(logged);

            // TODO: Packet modifications

            return packet;
        }
    }

    public class LoggedPacket
    {
        public Packet Packet;
        public IPEndPoint Sim;
        public Direction Direction;

        public override string ToString()
        {
            string text = (Direction == Direction.Incoming) ? "<-- " : "--> ";
            text += Sim.ToString() + " " + Packet.Type.ToString();

            return text;
        }
    }
}
