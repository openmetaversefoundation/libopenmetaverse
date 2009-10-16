using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace OpenMetaverse.GUI
{
    /// <summary>
    /// ToolStrip GUI component for displaying and switching between IM windows
    /// </summary>
    public class MessageBar : ToolStrip
    {
        private GridClient _Client;
        private Dictionary<UUID, MessageBarButton> _Sessions;

        /// <summary>
        /// A ToolBarButton representing an IM session, including its associated window
        /// </summary>
        class MessageBarButton : ToolStripButton
        {
            public delegate void MessageNeedsSendingCallback(UUID targetID, string message);
            public event MessageNeedsSendingCallback OnMessageNeedsSending;

            /// <summary>Target avatar name</summary>
            public string TargetName;
            /// <summary>Target avatar ID</summary>
            public UUID TargetID;
            /// <summary>IM session ID</summary>
            public UUID IMSessionID;
            /// <summary>Window for this IM session</summary>
            public MessageWindow Window;

            /// <summary>
            /// A class representing each IM session and its associated button and window objects
            /// </summary>
            public MessageBarButton(string targetName, UUID targetID, UUID imSessionID)
            {
                TargetName = targetName;
                TargetID = targetID;
                IMSessionID = imSessionID;

                this.Text = targetName;

                Window = new MessageWindow(targetName);
                Window.FormClosing += new FormClosingEventHandler(Window_FormClosing);
                Window.OnTextInput += new MessageWindow.TextInputCallback(Window_OnTextInput);
            }

            void Window_FormClosing(object sender, FormClosingEventArgs e)
            {
                this.Parent.Items.Remove(this);
                this.Dispose();
            }

            void Window_OnTextInput(string text)
            {
                if (OnMessageNeedsSending != null)
                    OnMessageNeedsSending(TargetID, text);
            }
        }

        /// <summary>
        /// A generic form for displaying text and accepting user input
        /// </summary>
        class MessageWindow : Form
        {
            private RichTextBox rtfOutput = new RichTextBox();
            private TextBox txtInput = new TextBox();

            public delegate void TextInputCallback(string text);
            public event TextInputCallback OnTextInput;

            /// <summary>
            /// A generic form for displaying text and accepting user input
            /// </summary>
            public MessageWindow(string title)
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

                this.Text = title;
                this.Controls.AddRange(new Control[] { txtInput, rtfOutput });

                this.Resize += new EventHandler(MessageWindow_Resize);
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
                    rtfOutput.SelectedText = string.Format("{0}[{1}:{2}] {3}", Environment.NewLine, now.Hour.ToString().PadLeft(2, '0'), now.Minute.ToString().PadLeft(2, '0'), text);
                    rtfOutput.ScrollToCaret();
                }
            }

            /// <summary>
            /// Thread-safe method for setting the window title
            /// </summary>
            /// <param name="title"></param>
            public void SetTitle(string title)
            {
                if (this.InvokeRequired)
                {
                    this.BeginInvoke((MethodInvoker)delegate { SetTitle(title); });
                }
                else
                {
                    this.Text = title;
                }
            }

            void MessageWindow_Resize(object sender, EventArgs e)
            {
                if (this.WindowState == FormWindowState.Minimized)
                    this.Hide();
            }

            void txtInput_KeyDown(object sender, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter && txtInput.Text != string.Empty)
                {
                    e.SuppressKeyPress = true;

                    if (OnTextInput != null)
                        OnTextInput(txtInput.Text);

                    txtInput.Clear();
                }
            }
        }

        /// <summary>
        /// Gets or sets the GridClient associated with this control
        /// </summary>
        public GridClient Client
        {
            get { return _Client; }
            set { if (value != null) InitializeClient(value); }
        }

        /// <summary>
        /// ToolStrip control for displaying and switching between an unspecified client's IM windows
        /// </summary>
        public MessageBar()
        {
            _Sessions = new Dictionary<UUID, MessageBarButton>();

            this.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.Location = new System.Drawing.Point(0, 523);
            this.Size = new System.Drawing.Size(772, 25);
            this.TabIndex = 6;
            
            this.ItemClicked += new ToolStripItemClickedEventHandler(MessageBar_ItemClicked);
        }

        public void CreateSession(string name, UUID id, UUID imSessionID, bool open)
        {
            MessageBarButton button = new MessageBarButton(name, id, imSessionID);
            button.Disposed += new EventHandler(button_Disposed);
            button.OnMessageNeedsSending += new MessageBarButton.MessageNeedsSendingCallback(button_OnMessageNeedsSending);
            AddButton(button, open);
        }

        private void AddButton(MessageBarButton button, bool open)
        {
            if (this.InvokeRequired) this.BeginInvoke((MethodInvoker)delegate { AddButton(button, open); });
            else
            {
                _Sessions.Add(button.TargetID, button);
                this.Items.Add((ToolStripItem)button);
                if (open)
                    button.Window.Show();
            }
        }

        private void AddButton(MessageBarButton button, InstantMessage im)
        {
            if (this.InvokeRequired) this.BeginInvoke((MethodInvoker)delegate { AddButton(button, im); });
            else
            {
                AddButton(button, false);
                _Sessions[im.FromAgentID].Window.LogText(im.FromAgentName + ": " + im.Message, Color.FromKnownColor(KnownColor.ControlText));
            }            
        }

        private void InitializeClient(GridClient client)
        {
            _Client = client;
            _Client.Self.IM += Self_IM;
        }

        void Self_IM(object sender, InstantMessageEventArgs e)
        {
            if (e.IM.Dialog == InstantMessageDialog.MessageFromAgent)
            {
                lock (_Sessions)
                {
                    if (_Sessions.ContainsKey(e.IM.FromAgentID))
                    {
                        _Sessions[e.IM.FromAgentID].IMSessionID = e.IM.IMSessionID;
                        _Sessions[e.IM.FromAgentID].Window.LogText(e.IM.FromAgentName + ": " + e.IM.Message, Color.FromKnownColor(KnownColor.ControlText));
                    }
                    else
                    {
                        CreateSession(e.IM.FromAgentName, e.IM.FromAgentID, e.IM.IMSessionID, false);
                    }
                }
            }
        }

        void button_OnMessageNeedsSending(UUID targetID, string message)
        {
            lock (_Sessions)
            {
                MessageBarButton button;
                if (_Sessions.TryGetValue(targetID, out button))
                {
                    button.Window.LogText(Client.Self.Name + ": " + message, Color.FromKnownColor(KnownColor.ControlText));
                    Client.Self.InstantMessage(targetID, message, button.IMSessionID);
                }
            }
        }

        void MessageBar_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            MessageBarButton button = (MessageBarButton)e.ClickedItem;

            if (this.InvokeRequired) this.BeginInvoke((MethodInvoker)delegate { button.Window.Show(); button.Window.Activate(); });
            else { button.Window.Show(); button.Window.Activate(); }
        }
     
        void button_Disposed(object sender, EventArgs e)
        {
            MessageBarButton button = (MessageBarButton)sender;
            lock (_Sessions)
            {
                if (_Sessions.ContainsKey(button.TargetID))
                    _Sessions.Remove(button.TargetID);
            }
        }
    }
}
