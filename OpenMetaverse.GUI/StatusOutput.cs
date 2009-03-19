/*
 * Copyright (c) 2007-2009, openmetaverse.org
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
using System.Drawing;
using System.Windows.Forms;

namespace OpenMetaverse.GUI
{
    public class StatusOutput : RichTextBox
    {
        private GridClient _Client;

        /// <summary>
        /// Gets or sets the GridClient associated with this control
        /// </summary>
        public GridClient Client
        {
            get { return _Client; }
            set { if (value != null) InitializeClient(value); }
        }

        public StatusOutput()
        {
            this.ReadOnly = true;
            this.BackColor = Color.White;
        }

        public void LogText(string text, Color color)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)delegate { LogText(text, color); });
            }
            else
            {
                this.SelectionStart = this.Text.Length;
                this.SelectionColor = color;
                DateTime now = DateTime.Now;
                this.SelectedText = string.Format("{0}[{1}:{2}] {3}", Environment.NewLine, now.Hour.ToString().PadLeft(2, '0'), now.Minute.ToString().PadLeft(2, '0'), text);
                this.ScrollToCaret();
            }
        }

        private void InitializeClient(GridClient client)
        {
            _Client = client;
            _Client.Network.OnCurrentSimChanged += new NetworkManager.CurrentSimChangedCallback(Network_OnCurrentSimChanged);
            _Client.Network.OnDisconnected += new NetworkManager.DisconnectedCallback(Network_OnDisconnected);
            _Client.Network.OnLogin += new NetworkManager.LoginCallback(Network_OnLogin);
            _Client.Self.OnAlertMessage += new AgentManager.AlertMessageCallback(Self_OnAlertMessage);
            _Client.Self.OnMoneyBalanceReplyReceived += new AgentManager.MoneyBalanceReplyCallback(Self_OnMoneyBalanceReplyReceived);
        }

        void Self_OnMoneyBalanceReplyReceived(UUID transactionID, bool transactionSuccess, int balance, int metersCredit, int metersCommitted, string description)
        {
            if (description != String.Empty) LogText(description, Color.Green);
            LogText("Balance: L$" + balance, Color.Green);
        }

        void Network_OnCurrentSimChanged(Simulator PreviousSimulator)
        {
            LogText("Entered region \"" + Client.Network.CurrentSim.Name + "\".", Color.Black);
        }

        void Network_OnDisconnected(NetworkManager.DisconnectType reason, string message)
        {
            LogText("Disconnected" + (message != null && message != String.Empty ? ": " + message : "."), Color.Black);
        }

        void Network_OnLogin(LoginStatus login, string message)
        {
            if (login == LoginStatus.Failed) LogText("Login failed: " + message, Color.Red);
            else if (login != LoginStatus.Success) LogText(message, Color.Black);
        }

        void Self_OnAlertMessage(string message)
        {
            LogText(message, Color.Gray);
        }

    }
}
