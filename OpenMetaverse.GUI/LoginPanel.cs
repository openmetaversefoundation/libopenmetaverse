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
using System.Threading;
using System.Windows.Forms;

namespace OpenMetaverse.GUI
{
    public class LoginPanel : Panel
    {
        private GridClient _Client;
        private LoginParams _LoginParams = new LoginParams();
        private Thread LoginThread;
        private Dictionary<string, string> _Accounts = new Dictionary<string, string>();
        private Button btnLogin = new Button();
        private Label label1 = new Label();
        private Label label2 = new Label();
        private Label label3 = new Label();
        private ComboBox listNames = new ComboBox();
        private ComboBox listStart = new ComboBox();
        private TextBox txtPass = new TextBox();

        /// <summary>
        /// Gets or sets the GridClient associated with this control
        /// </summary>
        public GridClient Client
        {
            get { return _Client; }
            set { if (value != null) InitializeClient(value); }
        }

        /// <summary>
        /// Gets or sets the LoginParams associated with this control's GridClient object
        /// </summary>
        public LoginParams LoginParams
        {
            get { return _LoginParams; }
            set { _LoginParams = value; }
        }

        /// <summary>
        /// First name parsed from the textbox control
        /// </summary>
        public string FirstName
        {
            get
            {
                string[] names = listNames.Text.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                return names.Length > 0 ? names[0] : String.Empty;
            }
        }

        /// <summary>
        /// Last name parsed from the textbox control
        /// </summary>
        public string LastName
        {
            get
            {
                string[] names = listNames.Text.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                return names.Length > 1 ? names[1] : String.Empty;
            }
        }

        /// <summary>
        /// Password value returned from the textbox control
        /// </summary>
        public string Password
        {
            get { return txtPass.Text; }
        }

        /// <summary>
        /// Complete start URI based on textbox value
        /// </summary>
        public string StartURI
        {
            get
            {
                if (listStart.Text == String.Empty)
                    return "last";

                if (listStart.Text == "Last" || listStart.Text == "Home")
                    return listStart.Text.ToLower();

                else
                {
                    string[] start = listStart.Text.Split(new char[] { '/' });
                    int x; int y; int z;

                    if (start.Length < 2 || int.TryParse(start[1], out x)) x = 128;
                    if (start.Length < 3 || int.TryParse(start[2], out y)) y = 128;
                    if (start.Length < 4 || int.TryParse(start[3], out z)) z = 0;
                    
                    return NetworkManager.StartLocation(start[0], x, y, z);
                }
            }
        }


        /// <summary>
        /// Panel control for an unspecified client's login preferences
        /// </summary>
        public LoginPanel()
        {
            btnLogin.Text = "Login";
            btnLogin.Size = new Size(50, 23);
            btnLogin.Location = new Point(6, 7);
            btnLogin.TabIndex = 0;

            label1.Text = "Name:";
            label1.AutoSize = true;
            label1.Location = new Point(60, 12);

            label2.Text = "Pass:";
            label2.AutoSize = true;
            label2.Location = new Point(266, 12);

            label3.Text = "Start:";
            label3.AutoSize = true;
            label3.Location = new Point(428, 12);

            listNames.Location = new Point(100, 8);
            listNames.Size = new Size(160, 18);
            listNames.TabIndex = 1;
            listNames.SelectedIndexChanged += new EventHandler(listNames_SelectedIndexChanged);

            txtPass.Location = new Point(302, 8);
            txtPass.PasswordChar = '*';
            txtPass.Size = new Size(120, 18);
            txtPass.TabIndex = 2;

            listStart.Items.Add("Last");
            listStart.Items.Add("Home");
            listStart.SelectedIndex = 0;
            listStart.Location = new Point(463, 8);
            listStart.Size = new Size(120, 18);
            listStart.TabIndex = 3;

            this.Dock = DockStyle.Top;
            this.Height = 50;

            this.Controls.Add(btnLogin);
            this.Controls.Add(label1);
            this.Controls.Add(label2);
            this.Controls.Add(label3);
            this.Controls.Add(listNames);
            this.Controls.Add(listStart);
            this.Controls.Add(txtPass);

            btnLogin.Click += new EventHandler(btnLogin_Click);
        }

        /// <summary>
        /// Begins login sequence using the parameters defined in .LoginParams
        /// </summary>
        public void Login()
        {
            LoginThread = new Thread(new ThreadStart(delegate() { _Client.Network.Login(_LoginParams); }));
            LoginThread.Start();
        }

        private void InitializeClient(GridClient client)
        {
            _Client = client;

            _Client.Network.Disconnected += Network_OnDisconnected;
            _Client.Network.LoginProgress += Network_OnLogin;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (btnLogin.Text == "Login")
            {
                if (FirstName == String.Empty || LastName == String.Empty)
                {
                    MessageBox.Show("Please enter a valid first and last name.", "Login", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

                btnLogin.Text = "Logout";

                _LoginParams.FirstName = FirstName;
                _LoginParams.LastName = LastName;
                _LoginParams.Password = Password;
                _LoginParams.Start = StartURI;

                LoginThread = new Thread(new ThreadStart(delegate() { _Client.Network.Login(_LoginParams); }));
                LoginThread.Start();
            }
            else if (btnLogin.Text == "Logout")
            {
                if (LoginThread != null)
                {
                    if (LoginThread.IsAlive)
                        LoginThread.Abort();

                    LoginThread = null;
                }

                if (_Client != null)
                {
                    if (_Client.Network.Connected)
                        _Client.Network.Logout();
                }

                btnLogin.Text = "Login";
            }
        }

        private void listNames_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listNames.SelectedIndex > -1)
            {
                lock (_Accounts)
                {
                    string pass;
                    if (_Accounts.TryGetValue(listNames.Text, out pass))
                        txtPass.Text = pass;
                }
            }
            else txtPass.Text = String.Empty;
        }

        private void Network_OnDisconnected(object sender, DisconnectedEventArgs e)
        {
            if (!this.IsHandleCreated) return;

            this.BeginInvoke((MethodInvoker)delegate
            {
                btnLogin.Text = "Login";
            });
        }

        private void Network_OnLogin(object sender, LoginProgressEventArgs e)
        {
            if (!this.IsHandleCreated) return;

            this.BeginInvoke((MethodInvoker)delegate
            {
                btnLogin.Text = "Logout";
            });

            if (e.Status == LoginStatus.Success)
            {
                lock (_Accounts)
                {
                    _Accounts[_Client.Self.Name] = _LoginParams.Password;

                    if (!listNames.Items.Contains(_Client.Self.Name))
                    {
                        this.BeginInvoke((MethodInvoker)delegate
                        {
                            int index = listNames.Items.Add(_Client.Self.Name);
                            listNames.SelectedIndex = index;
                        });
                    }
                }
            }
            else if (e.Status == LoginStatus.Failed)
            {
                this.BeginInvoke((MethodInvoker)delegate
                {
                    btnLogin.Text = "Login";
                });
            }
            else
            {
                this.BeginInvoke((MethodInvoker)delegate
                {
                    listNames.Text = _LoginParams.FirstName + " " + _LoginParams.LastName;
                    txtPass.Text = _LoginParams.Password;
                });
            }
        }

    }
}
