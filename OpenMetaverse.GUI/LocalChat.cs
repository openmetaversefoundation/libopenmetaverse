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
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace OpenMetaverse.GUI
{
    /// <summary>
    /// Panel GUI component for interfacing with local chat
    /// </summary>
    public class LocalChat : Panel
    {
        /// <summary>
        /// A file that output should be logged to (or null, to disable logging)
        /// </summary>
        public string LogFile = null;

        private GridClient _Client;
        private RichTextBox rtfOutput = new RichTextBox();
        private TextBox txtInput = new TextBox();

        /// <summary>
        /// Gets or sets the GridClient associated with this control
        /// </summary>
        public GridClient Client
        {
            get { return _Client; }
            set { if (value != null) InitializeClient(value); }
        }

        /// <summary>
        /// Panel control for an unspecified client's local chat interaction
        /// </summary>
        public LocalChat()
        {
            rtfOutput.Anchor = AnchorStyles.Bottom | AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            rtfOutput.BackColor = Color.FromKnownColor(KnownColor.Window);
            rtfOutput.Width = this.Width;
            rtfOutput.Height = this.Height - txtInput.Height;
            rtfOutput.ReadOnly = true;
            rtfOutput.Top = 0;
            rtfOutput.Left = 0;           

            txtInput.Dock = DockStyle.Bottom;
            txtInput.KeyDown += new KeyEventHandler(txtInput_KeyDown);

            this.Controls.AddRange(new Control[] { txtInput, rtfOutput });
        }

        /// <summary>
        /// Panel control for the specified client's local chat interaction
        /// </summary>
        public LocalChat(GridClient client) : this ()
        {
            _Client = client;
        }

        /// <summary>
        /// Adds text of a specified color to the display output
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="text"></param>
        /// <param name="color"></param>
        public void LogChat(string name, ChatType type, string message, Color color)
        {
            if (!this.IsHandleCreated) return;

            if (this.InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)delegate { LogChat(name, type, message, color); });
            }
            else
            {
                rtfOutput.SelectionStart = rtfOutput.Text.Length;
                rtfOutput.SelectionColor = color;
                DateTime now = DateTime.Now;
                string output;
                string volume;

                if (message.Length > 3 && message.Substring(0, 4).ToLower() == "/me ")
                {
                    string text = message.Substring(4);
                    if (type == ChatType.Shout) volume = "(shouted) ";
                    else if (type == ChatType.Whisper) volume = "(whispered) ";
                    else volume = String.Empty;
                    output = string.Format("{0}[{1}:{2}] {3}{4} {5}", Environment.NewLine, now.Hour.ToString().PadLeft(2, '0'), now.Minute.ToString().PadLeft(2, '0'), volume, name, text);
                }
                else
                {
                    if (type == ChatType.Shout) volume = " shouts";
                    else if (type == ChatType.Whisper) volume = " whispers";
                    else volume = String.Empty;
                    output = string.Format("{0}[{1}:{2}] {3}{4}: {5}", Environment.NewLine, now.Hour.ToString().PadLeft(2, '0'), now.Minute.ToString().PadLeft(2, '0'), name, volume, message);
                }

                rtfOutput.SelectedText = output;                
                rtfOutput.ScrollToCaret();

                if (LogFile != null)
                    File.AppendAllText(LogFile, output);
            }
        }

        /// <summary>
        /// Thread-safe method for adding text of a specified color to the display output
        /// </summary>
        /// <param name="text"></param>
        /// <param name="color"></param>
        public void LogText(string text, Color color)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)delegate { LogText(text, color); });
            }
            else
            {
                rtfOutput.SelectionStart = rtfOutput.Text.Length;
                rtfOutput.SelectionColor = color;
                DateTime now = DateTime.Now;
                string output = string.Format("{0}[{1}:{2}] {3}", Environment.NewLine, now.Hour.ToString().PadLeft(2, '0'), now.Minute.ToString().PadLeft(2, '0'), text);
                rtfOutput.SelectedText = output;
                rtfOutput.ScrollToCaret();

                if (LogFile != null)
                    File.AppendAllText(LogFile, output);
            }
        }

        private void InitializeClient(GridClient client)
        {
            _Client = client;
            _Client.Self.OnChat += new AgentManager.ChatCallback(Self_OnChat);
        }

        void Self_OnChat(string message, ChatAudibleLevel audible, ChatType type, ChatSourceType sourceType, string fromName, UUID id, UUID ownerid, Vector3 position)
        {
            if (audible == ChatAudibleLevel.Fully && type != ChatType.StartTyping && type != ChatType.StopTyping)
            {
                Color color;
                if (sourceType == ChatSourceType.Agent) color = Color.FromKnownColor(KnownColor.ControlText);
                else color = Color.FromKnownColor(KnownColor.GrayText);
                LogChat(fromName, type, message, color);
            }
        }

        void txtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && txtInput.Text != string.Empty)
            {
                e.SuppressKeyPress = true;
                _Client.Self.Chat(txtInput.Text, 0, e.Control ? ChatType.Shout : ChatType.Normal);
                txtInput.Clear();
            }
        }
    }

}
