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
using System.Windows.Forms;

namespace OpenMetaverse.GUI
{
    /// <summary>
    /// Panel GUI component for interfacing with local chat
    /// </summary>
    public class LocalChat : Panel
    {
        private GridClient _Client;
        private RichTextBox _rtfOutput = new RichTextBox();
        private TextBox _txtInput = new TextBox();

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
            _rtfOutput.Anchor = AnchorStyles.Bottom | AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _rtfOutput.BackColor = Color.FromKnownColor(KnownColor.Window);
            _rtfOutput.Width = this.Width;
            _rtfOutput.Height = this.Height - _txtInput.Height;
            _rtfOutput.ReadOnly = true;
            _rtfOutput.Top = 0;
            _rtfOutput.Left = 0;           

            _txtInput.Dock = DockStyle.Bottom;
            _txtInput.KeyDown += new KeyEventHandler(_txtInput_KeyDown);

            this.Controls.AddRange(new Control[] { _txtInput, _rtfOutput });
        }

        /// <summary>
        /// Panel control for the specified client's local chat interaction
        /// </summary>
        public LocalChat(GridClient client) : this ()
        {
            _Client = client;
        }

        private void InitializeClient(GridClient client)
        {
            _Client = client;
            _Client.Self.OnChat += new AgentManager.ChatCallback(Self_OnChat);
        }

        public void LogChat(string name, ChatType type, string text, Color color)
        {
            if (!this.IsHandleCreated) return;

            if (this.InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)delegate { LogChat(name, type, text, color); });
            }
            else
            {
                _rtfOutput.SelectionStart = _rtfOutput.Text.Length;
                _rtfOutput.SelectionColor = color;
                DateTime now = DateTime.Now;
                string volume;
                if (type == ChatType.Shout) volume = " shouts";
                else if (type == ChatType.Whisper) volume = " whispers";
                else volume = string.Empty;
                _rtfOutput.SelectedText = string.Format("{0}[{1}:{2}] {3}{4}: {5}", Environment.NewLine, now.Hour.ToString().PadLeft(2, '0'), now.Minute.ToString().PadLeft(2, '0'), name, volume, text);
                _rtfOutput.ScrollToCaret();
            }
        }

        public void LogText(string text, Color color)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)delegate { LogText(text, color); });
            }
            else
            {
                _rtfOutput.SelectionStart = _rtfOutput.Text.Length;
                _rtfOutput.SelectionColor = color;
                DateTime now = DateTime.Now;
                _rtfOutput.SelectedText = string.Format("{0}[{1}:{2}] {3}", Environment.NewLine, now.Hour.ToString().PadLeft(2, '0'), now.Minute.ToString().PadLeft(2, '0'), text);
                _rtfOutput.ScrollToCaret();
            }
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

        void _txtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && _txtInput.Text != string.Empty)
            {
                e.SuppressKeyPress = true;
                _Client.Self.Chat(_txtInput.Text, 0, e.Control ? ChatType.Shout : ChatType.Normal);
                _txtInput.Clear();
            }
        }
    }

}
