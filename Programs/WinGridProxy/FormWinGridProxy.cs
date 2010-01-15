/*
 * Copyright (c) 2009, openmetaverse.org
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
using System.IO;
using System.Net;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Threading;
using GridProxy;
using OpenMetaverse;
using OpenMetaverse.Packets;
using OpenMetaverse.StructuredData;
using OpenMetaverse.Interfaces;
using System.Xml;
using Nwc.XmlRpc;
using Logger = OpenMetaverse.Logger;

namespace WinGridProxy
{
    public partial class FormWinGridProxy : Form
    {
        // only allow one thread at a time to do file I/O operations        
        private object m_FileIOLockerObject = new object();

        // Class for saving and restoring settings
        private static SettingsStore Store = new SettingsStore();

        private static bool m_ProxyRunning;

        private Assembly m_CurrentAssembly = Assembly.GetExecutingAssembly();

        ProxyManager proxy;

        private FormPluginManager pluginManager;

        private int PacketCounter;

        // stats tracking
        private int CapsInCounter;
        private int CapsInBytes;
        private int CapsOutCounter;
        private int CapsOutBytes;
        private int PacketsInCounter;
        private int PacketsInBytes;
        private int PacketsOutCounter;
        private int PacketsOutBytes;

        private bool monoRuntime;

        // Sessions & Sessions ListView related
        private List<Session> m_SessionViewItems;
        private Dictionary<int, ListViewItem> m_SessionViewCache = new Dictionary<int, ListViewItem>();
        private SortOrder m_ListViewSortOrder = SortOrder.None;
        // disable listview virtual items counter being updated
        // if we're scrolling the listview to get around a flicker issue
        // due to an item selected going out of view
        private bool m_AllowUpdate = true;

        private const string PROTO_CAPABILITIES = "Cap";
        private const string PROTO_EVENTMESSAGE = "Event";
        private const string PROTO_PACKETSTRING = "UDP";
        private const string PROTO_AUTHENTICATE = "https";

        // some default colors for session items
        private readonly Color Color_Login = Color.OldLace;
        private readonly Color Color_Packet = Color.LightYellow;
        private readonly Color Color_Cap = Color.Honeydew;
        private readonly Color Color_Event = Color.AliceBlue;

        // some UI customization
        private Dictionary<string, string> m_InstalledViewers = new Dictionary<string, string>();
        private List<string> m_DefaultGridLoginServers;

        // a reference to the last selected session item
        private Session m_CurrentSession;

        public FormWinGridProxy()
        {            
            InitializeComponent();

            m_SessionViewItems = new List<Session>();

            Logger.Log("WinGridProxy ready", Helpers.LogLevel.Info);

            if (FireEventAppender.Instance != null)
            {
                FireEventAppender.Instance.MessageLoggedEvent += new MessageLoggedEventHandler(Instance_MessageLoggedEvent);
            }



            // Attempt to work around some mono inefficiencies
            monoRuntime = Type.GetType("Mono.Runtime") != null; // Officially supported way of detecting mono
            if (monoRuntime)
            {
                Font fixedFont = new Font(FontFamily.GenericMonospace, 9f, FontStyle.Regular, GraphicsUnit.Point);
                richTextBoxDecodedRequest.Font =
                    richTextBoxDecodedResponse.Font =
                    richTextBoxNotationRequest.Font =
                    richTextBoxNotationResponse.Font =
                    richTextBoxRawRequest.Font =
                    richTextBoxRawResponse.Font = fixedFont;
            }

            InitializeInterfaceDefaults();

            ProxyManager.OnPacketLog += ProxyManager_OnPacketLog;
            ProxyManager.OnMessageLog += ProxyManager_OnMessageLog;
            ProxyManager.OnLoginResponse += ProxyManager_OnLoginResponse;
            ProxyManager.OnCapabilityAdded += ProxyManager_OnCapabilityAdded;
            ProxyManager.OnEventMessageLog += ProxyManager_OnEventMessageLog;
        }

        #region GUI Initialization
        private void InitializeInterfaceDefaults()
        {
            // populate the listen box with the known IP Addresses of this host
            IPHostEntry iphostentry = Dns.GetHostByName(Dns.GetHostName());
            foreach (IPAddress address in iphostentry.AddressList)
                comboBoxListenAddress.Items.Add(address.ToString());

            // Initialize login server combo box:
            // * If gridservers.ini exists, read it and use values from that file
            // * If gridservers.ini does not exist or is blank, use some pre-defined defaults
            m_DefaultGridLoginServers = new List<string>();
            string[] gridServers;
            if (File.Exists("gridservers.ini"))
            {
                gridServers = File.ReadAllLines("gridservers.ini");
                for (int i = 0; i < gridServers.Length; i++)
                {
                    if (String.IsNullOrEmpty(gridServers[i]) || !gridServers[i].Trim().StartsWith("http"))
                        continue;
                    m_DefaultGridLoginServers.Add(gridServers[i]);
                }
            }
            string[] loginServers = {"https://login.agni.lindenlab.com/cgi-bin/login.cgi", 
                                            "https://login.aditi.lindenlab.com/cgi-bin/login.cgi",
                                            "http://127.0.0.1:12043",
                                            "http://127.0.0.1:8002", 
                                            "http://osgrid.org:8002"};

            if (m_DefaultGridLoginServers.Count <= 0)
                m_DefaultGridLoginServers.AddRange(loginServers);

            comboBoxLoginURL.Items.AddRange(m_DefaultGridLoginServers.ToArray());
            comboBoxLoginURL.Text = comboBoxLoginURL.Items[0].ToString();

            // Find installed viewers for launch toolbar if running under windows
            string[] viewerDistKeys = { "Linden Research, Inc.", "Open Metaverse Foundation" };
            Microsoft.Win32.RegistryKey viewerKeyRoot = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"Software\Wow6432Node");
            if (viewerKeyRoot == null)
            {
                viewerKeyRoot = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"Software");
            }

            if (viewerKeyRoot != null)
            {
                foreach (string key in viewerDistKeys)
                {
                    Microsoft.Win32.RegistryKey viewerKey = viewerKeyRoot.OpenSubKey(key);
                    if (viewerKey != null)
                    {
                        string[] installed = viewerKey.GetSubKeyNames();
                        foreach (string viewer in installed)
                        {
                            if (!m_InstalledViewers.ContainsKey(viewer))
                            {
                                Microsoft.Win32.RegistryKey me = viewerKey.OpenSubKey(viewer);
                                if (me != null)
                                {
                                    string dir = me.GetValue("").ToString(); // the install directory
                                    string exe = me.GetValue("Exe").ToString(); // the executable name
                                    string ver = me.GetValue("Version").ToString(); // the viewer version

                                    if (!String.IsNullOrEmpty(dir) && !String.IsNullOrEmpty(exe) && !String.IsNullOrEmpty(ver) && File.Exists(Path.Combine(dir, exe)))
                                    {
                                        toolStripComboBox1.Items.Add(viewer + " " + ver);
                                        m_InstalledViewers.Add(viewer + " " + ver, Path.Combine(dir, exe));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (toolStripComboBox1.Items.Count > 0)
            {
                toolStripComboBox1.Text = toolStripComboBox1.Items[0].ToString();
            }
            else
            {
                toolStripQuickLaunch.Visible = false;
            }
        }
        #endregion

        #region Event Handlers for Messages/Packets arriving via GridProxy

        /// <summary>
        /// Adds a new EventQueue message to the Message Filters listview.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="stage"></param>
        void ProxyManager_OnEventMessageLog(CapsRequest req, CapsStage stage)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new MethodInvoker(() => ProxyManager_OnEventMessageLog(req, stage)));
            }
            else
            {
                ListViewItem foundCap = FindListViewItem(listViewMessageFilters, req.Info.CapType, false);

                if (foundCap == null)
                {
                    ListViewItem addedItem = listViewMessageFilters.Items.Add(new ListViewItem(req.Info.CapType,
                        listViewMessageFilters.Groups["EventQueueMessages"]));

                    addedItem.SubItems.Add(PROTO_EVENTMESSAGE);
                    addedItem.BackColor = Color_Event;

                    if (autoAddNewDiscoveredMessagesToolStripMenuItem.Checked)
                        addedItem.Checked = true;
                }
                else
                {
                    ProxyManager_OnMessageLog(req, CapsStage.Response);
                }
            }
        }

        /// <summary>
        /// Adds a new Capability message to the message filters listview
        /// </summary>
        private void ProxyManager_OnCapabilityAdded(CapInfo cap)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new MethodInvoker(() => ProxyManager_OnCapabilityAdded(cap)));
            }
            else
            {
                ListViewItem foundCap = FindListViewItem(listViewMessageFilters, cap.CapType, false);
                if (foundCap == null)
                {
                    ListViewItem addedItem = listViewMessageFilters.Items.Add(new ListViewItem(cap.CapType,
                        listViewMessageFilters.Groups["Capabilities"]));
                    addedItem.SubItems.Add(PROTO_CAPABILITIES);
                    addedItem.BackColor = Color_Cap;

                    if (autoAddNewDiscoveredMessagesToolStripMenuItem.Checked)
                        addedItem.Checked = true;
                }
            }
        }

        /// <summary>
        /// Handle Login Requests/Responses
        /// </summary>        
        private void ProxyManager_OnLoginResponse(object request, Direction direction)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new MethodInvoker(() => ProxyManager_OnLoginResponse(request, direction)));
            }
            else
            {
                string loginType;

                if (request is XmlRpcRequest)
                {
                    loginType = "Login Request";
                }
                else
                {
                    loginType = "Login Response";
                }

                ListViewItem foundItem = FindListViewItem(listViewPacketFilters, loginType, false);

                if (foundItem != null && foundItem.Checked == true)
                {
                    PacketCounter++;

                    SessionLogin sessionLogin = new SessionLogin(request, direction, comboBoxLoginURL.Text, request.GetType().Name + " " + loginType);

                    ListViewItem sessionEntry = new ListViewItem(new string[] { PacketCounter.ToString(), 
                        sessionLogin.Protocol, sessionLogin.Name, sessionLogin.Length.ToString(), sessionLogin.Host, sessionLogin.ContentType });

                    sessionEntry.Tag = sessionLogin;
                    sessionEntry.ImageIndex = (int)sessionLogin.Direction;

                    m_SessionViewItems.Add(sessionLogin);
                }
            }
        }

        // Only raised when we've told GridProxy we want a specific packet type
        private void ProxyManager_OnPacketLog(Packet packet, Direction direction, IPEndPoint endpoint)
        {
            PacketCounter++;

            if (direction == Direction.Incoming)
            {
                PacketsInCounter++;
                PacketsInBytes += packet.Length;
            }
            else
            {
                PacketsOutCounter++;
                PacketsOutBytes += packet.Length;
            }

            SessionPacket sessionPacket = new SessionPacket(packet, direction, endpoint,
                PacketDecoder.InterpretOptions(packet.Header) + " Seq: " + packet.Header.Sequence.ToString() + " Freq:" + packet.Header.Frequency.ToString());

            m_SessionViewItems.Add(sessionPacket);

        }

        /// <summary>
        /// Handle Capabilities 
        /// </summary>        
        private void ProxyManager_OnMessageLog(CapsRequest req, CapsStage stage)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new MethodInvoker(() => ProxyManager_OnMessageLog(req, stage)));
            }
            else
            {
                ListViewItem found = FindListViewItem(listViewMessageFilters, req.Info.CapType, false);

                if (found != null && found.Checked)
                {
                    PacketCounter++;

                    int size = 0;
                    string contentType = String.Empty;
                    if (req.RawRequest != null)
                    {
                        size += req.RawRequest.Length;
                        contentType = req.RequestHeaders.Get("Content-Type"); //req.RequestHeaders["Content-Type"];
                    }
                    if (req.RawResponse != null)
                    {
                        size += req.RawResponse.Length;
                        contentType = req.ResponseHeaders.Get("Content-Type");
                    }

                    Direction direction;
                    if (stage == CapsStage.Request)
                    {
                        CapsOutCounter++;
                        CapsOutBytes += req.Request.ToString().Length;
                        direction = Direction.Outgoing;
                    }
                    else
                    {
                        CapsInCounter++;
                        CapsInBytes += req.Response.ToString().Length;
                        direction = Direction.Incoming;
                    }

                    string proto = found.SubItems[1].Text;

                    Session capsSession = null;
                    if (found.Group.Header.Equals("Capabilities"))
                    {
                        capsSession = new SessionCaps(req.RawRequest, req.RawResponse, req.RequestHeaders,
                        req.ResponseHeaders, direction, req.Info.URI, req.Info.CapType, proto);
                    }
                    else
                    {
                        capsSession = new SessionEvent(req.RawResponse, req.ResponseHeaders, req.Info.URI, req.Info.CapType, proto);
                    }

                    string[] s = { PacketCounter.ToString(), capsSession.Protocol, capsSession.Name, capsSession.Length.ToString(), capsSession.Host, capsSession.ContentType };
                    ListViewItem session = new ListViewItem(s);

                    session.ImageIndex = (int)direction;
                    session.Tag = capsSession;

                    session.BackColor = found.BackColor;

                    m_SessionViewItems.Add(capsSession);
                }
                else
                {
                    if (found == null)
                    {
                        // must be a new event not in KnownCaps, lets add it to the listview
                        ListViewItem addedItem = listViewMessageFilters.Items.Add(new ListViewItem(req.Info.CapType));
                        addedItem.BackColor = Color_Cap;

                        if (autoAddNewDiscoveredMessagesToolStripMenuItem.Checked)
                            addedItem.Checked = true;
                    }
                }
            }
        }

        #endregion

        #region Helpers

        private ListViewItem FindListViewItem(ListView listView, string key, bool searchAll)
        {
            foreach (ListViewItem item in listView.Items)
            {
                if (item.Text.Equals(key)
                    || (searchAll && item.SubItems.ContainsKey(key)))
                    return item;
            }
            return null;
        }

        private void UpdateVirtualListSize(int newSize)
        {
            if (listViewSessions.VirtualListSize != newSize && listViewSessions.InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)(() => UpdateVirtualListSize(newSize)));
            }
            else
            {
                //if(listViewSessions.VirtualListSize != newSize)
                //{
                listViewSessions.VirtualListSize = newSize;
                //}
            }
        }

        private void ClearCache()
        {
            lock (m_SessionViewCache)
            {
                m_SessionViewCache.Clear();
            }
        }

        private static string FormatBytes(long bytes)
        {
            const int scale = 1024;
            string[] orders = new string[] { "GB", "MB", "KB", "Bytes" };
            long max = (long)Math.Pow(scale, orders.Length - 1);

            foreach (string order in orders)
            {
                if (bytes > max)
                    return string.Format("{0:##.##} {1}", decimal.Divide(bytes, max), order);

                max /= scale;
            }
            return "0";
        }
        #endregion

        #region GUI Event Handlers

        private void buttonStartProxy_Click(object sender, EventArgs e)
        {
            if (buttonStartProxy.Text.StartsWith("Start") && m_ProxyRunning.Equals(false))
            {
                proxy = new ProxyManager(textBoxProxyPort.Text, comboBoxListenAddress.Text, comboBoxLoginURL.Text);
                // disable any gui elements
                comboBoxListenAddress.Enabled = textBoxProxyPort.Enabled = comboBoxLoginURL.Enabled = false;

                InitProxyFilters();

                proxy.Start();

                toolStripQuickLaunch.Enabled = loadFilterSelectionsToolStripMenuItem.Enabled = saveFilterSelectionsToolStripMenuItem.Enabled = true;

                // enable any gui elements
                toolStripSplitButton1.Enabled =
                toolStripMenuItemPlugins.Enabled = grpUDPFilters.Enabled = grpCapsFilters.Enabled = m_ProxyRunning = true;
                buttonStartProxy.Text = "Stop Proxy";
                buttonStartProxy.Checked = true;
                if (enableStatisticsToolStripMenuItem.Checked && !timer1.Enabled)
                    timer1.Enabled = true;
            }
            else if (buttonStartProxy.Text.StartsWith("Stop") && m_ProxyRunning.Equals(true))
            {
                toolStripQuickLaunch.Enabled = loadFilterSelectionsToolStripMenuItem.Enabled = saveFilterSelectionsToolStripMenuItem.Enabled = false;
                // stop the proxy
                proxy.Stop();
                toolStripMenuItemPlugins.Enabled = grpUDPFilters.Enabled = grpCapsFilters.Enabled = m_ProxyRunning = false;
                buttonStartProxy.Text = "Start Proxy";
                buttonStartProxy.Checked = false;
                comboBoxListenAddress.Enabled = textBoxProxyPort.Enabled = comboBoxLoginURL.Enabled = true;

                if (!enableStatisticsToolStripMenuItem.Checked && timer1.Enabled)
                    timer1.Enabled = false;
            }
        }

        private void checkBoxCheckAllPackets_CheckedChanged(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewPacketFilters.Items)
            {
                item.Checked = checkBoxCheckAllPackets.Checked;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_ProxyRunning)
                proxy.Stop();

            if (saveOptionsOnExitToolStripMenuItem.Checked)
                SaveAllSettings("settings.osd");
        }

        // select all items in session list
        private void sessionSelectAll_Click(object sender, EventArgs e)
        {
            lock (m_SessionViewItems)
            {
                m_SessionViewItems.ForEach((session) => session.Selected = true);
            }

            ClearCache();
            listViewSessions.Invalidate();
        }

        // unselect all items in session list
        private void sessionSelectNone_Click(object sender, EventArgs e)
        {
            lock (m_SessionViewItems)
            {
                m_SessionViewItems.ForEach((session) => session.Selected = false);
            }

            ClearCache();
            listViewSessions.Invalidate();
        }

        // invert selection
        private void sessionInvertSelection_Click(object sender, EventArgs e)
        {
            lock (m_SessionViewItems)
            {
                m_SessionViewItems.ForEach((session) => session.Selected = !session.Selected);
            }

            ClearCache();
            listViewSessions.Invalidate();
        }

        // remove all sessions
        private void sessionRemoveAll_Click(object sender, EventArgs e)
        {
            lock (m_SessionViewItems)
            {
                m_SessionViewItems.Clear();
            }

            ClearCache();
            listViewSessions.VirtualListSize = 0;
            listViewSessions.Invalidate();
        }

        // remove sessions that are currently selected
        private void sessionRemoveSelected_Click(object sender, EventArgs e)
        {

            // first we'll check for any highlighted items
            if (listViewSessions.SelectedIndices.Count > 0)
            {
                for (int i = 0; i < listViewSessions.SelectedIndices.Count; i++)
                {
                    int index = listViewSessions.SelectedIndices[i];
                    lock (m_SessionViewItems)
                    {
                        m_SessionViewItems.RemoveAt(index);
                    }
                }
            }

            // now we'll check for items that have their selected bool set
            bool hasSelected = false;

            m_SessionViewItems.ForEach(delegate(Session action)
             {
                 if (action.Selected)
                 {
                     hasSelected = true;
                     return;
                 }
             });

            if (hasSelected)
            {
                progressBar1.Step = 1;
                progressBar1.Maximum = m_SessionViewItems.Count;
                progressBar1.Value = 0;
                panelActionProgress.Visible = true;

                lock (m_SessionViewItems)
                {
                    m_SessionViewItems.RemoveAll(delegate(Session sess)
                    {
                        progressBar1.PerformStep();
                        return sess.Selected;
                    });
                }
            }

            ClearCache();
            panelActionProgress.Visible = false;
            listViewSessions.VirtualListSize = m_SessionViewItems.Count;
            listViewSessions.SelectedIndices.Clear();
            listViewSessions.Invalidate();
        }

        // remove sessions that are not currently selected
        private void sessionRemoveUnselected_Click(object sender, EventArgs e)
        {
            progressBar1.Step = m_SessionViewItems.Count / 100;
            progressBar1.Value = 0;
            panelActionProgress.Visible = true;

            lock (m_SessionViewItems)
            {
                for (int i = 0; i < m_SessionViewItems.Count; i++)
                {
                    progressBar1.PerformStep();
                    if (!m_SessionViewItems[i].Selected)
                        m_SessionViewItems.RemoveAt(i);//.Remove(m_SessionViewItems[i]);
                }
            }

            ClearCache();
            listViewSessions.Invalidate();
            panelActionProgress.Visible = false;
        }

        // Colorize selected sessions
        private void sessionMarkSelected_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = (ToolStripMenuItem)sender;

            lock (m_SessionViewItems)
            {
                for (int i = 0; i < m_SessionViewItems.Count; i++)
                {
                    if (m_SessionViewItems[i].Selected)
                        m_SessionViewItems[i].BackColor = Color.FromName(menu.Text);
                }
            }

            sessionSelectNone_Click(sender, e);
        }

        // Unmark selected sessions
        private void sessionUnmarkSelected_Click(object sender, EventArgs e)
        {
            lock (m_SessionViewItems)
            {
                for (int i = 0; i < m_SessionViewItems.Count; i++)
                {
                    if (m_SessionViewItems[i].Selected)
                        m_SessionViewItems[i].BackColor = Color.White;
                }
            }

            sessionSelectNone_Click(sender, e);
        }

        private void aboutWinGridProxyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 about = new AboutBox1();
            about.ShowDialog();
        }

        // Update Request Hexbox status bar with current cursor location
        private void RequestPosition_Changed(object sender, EventArgs e)
        {
            if (hexBoxRequest.ByteProvider != null)
            {
                labelHexRequestStatus.Text = String.Format("Ln {0}    Col {1}    bytes {2}",
                    hexBoxRequest.CurrentLine, hexBoxRequest.CurrentPositionInLine, hexBoxRequest.ByteProvider.Length);
                buttonSaveRequestHex.Visible = (hexBoxRequest.ByteProvider.Length > 0);
            }
            buttonSaveRequestHex.Visible = (hexBoxRequest.ByteProvider != null && hexBoxRequest.ByteProvider.Length > 0);
        }

        // Update Response Hexbox status bar with current cursor location
        void ReplyPosition_Changed(object sender, EventArgs e)
        {
            if (hexBoxResponse.ByteProvider != null)
            {
                labelHexBoxResponseStatus.Text = String.Format("Ln {0}    Col {1}    bytes {2}",
                    hexBoxResponse.CurrentLine, hexBoxResponse.CurrentPositionInLine, hexBoxResponse.ByteProvider.Length);
            }
            buttonExportRawHex.Visible = (hexBoxResponse.ByteProvider != null && hexBoxResponse.ByteProvider.Length > 0);
        }

        // select all specified sessions by packet name
        private void sessionSelectAllPacketType_Click(object sender, EventArgs e)
        {
            lock (m_SessionViewItems)
            {
                for (int i = 0; i < m_SessionViewItems.Count; i++)
                {
                    Session item = m_SessionViewItems[i];
                    if (item.Name.Equals(toolStripMenuItemSelectPacketName.Tag) && !item.Selected)
                        item.Selected = true;
                }
            }

            ClearCache();
            listViewSessions.Invalidate();
        }

        // stop capturing selected filters
        private void filterDisableByPacketName_CheckedChanged(object sender, EventArgs e)
        {
            if (enableDisableFilterByNameToolStripMenuItem.Tag != null)
            {
                ListViewItem found = FindListViewItem(listViewMessageFilters, enableDisableFilterByNameToolStripMenuItem.Tag.ToString(), false);

                if (found != null)
                {
                    listViewMessageFilters.Items[found.Index].Checked = enableDisableFilterByNameToolStripMenuItem.Checked;
                }
                else
                {
                    found = FindListViewItem(listViewPacketFilters, enableDisableFilterByNameToolStripMenuItem.Tag.ToString(), false);

                    if (found != null)
                        listViewPacketFilters.Items[found.Index].Checked = enableDisableFilterByNameToolStripMenuItem.Checked;
                }
            }
        }

        /// <summary>
        /// Setup the context menu prior to it being displayed with specific entries for filtering packets/messages
        /// </summary>
        private void contextMenuStripSessions_Opening(object sender, CancelEventArgs e)
        {
            if (listViewSessions.FocusedItem != null)
            {
                string strPacketOrMessage = (listViewSessions.FocusedItem.SubItems[1].Text.Equals(PROTO_PACKETSTRING)) ? "Packets" : "Messages";

                enableDisableFilterByNameToolStripMenuItem.Text = String.Format("Capture {0} {1}", listViewSessions.FocusedItem.SubItems[2].Text, strPacketOrMessage);
                toolStripMenuItemSelectPacketName.Tag = enableDisableFilterByNameToolStripMenuItem.Tag = listViewSessions.FocusedItem.SubItems[2].Text;

                toolStripMenuItemSelectPacketName.Text = String.Format("All {0} {1}", listViewSessions.FocusedItem.SubItems[2].Text, strPacketOrMessage);

                enableDisableFilterByNameToolStripMenuItem.Visible =
                toolStripSeparatorSelectPacketProto.Visible =
                toolStripSeparatorFilterPacketByName.Visible =
                toolStripMenuItemSelectPacketName.Visible = true;

                // find checkstate of selected menuitem in packets or messages filters checkedListBoxes
                bool ctxChecked = false;

                if (strPacketOrMessage.Equals("Packets"))
                {
                    ListViewItem found = FindListViewItem(listViewPacketFilters, toolStripMenuItemSelectPacketName.Tag.ToString(), false);
                    if (found != null)
                        ctxChecked = found.Checked;
                }
                else if (strPacketOrMessage.Equals("Messages"))
                {
                    ListViewItem found = FindListViewItem(listViewMessageFilters, toolStripMenuItemSelectPacketName.Tag.ToString(), false);
                    if (found != null)
                        ctxChecked = found.Checked;
                }
                enableDisableFilterByNameToolStripMenuItem.Checked = ctxChecked;
            }
            else
            {
                // Hide specific selection options on context menu
                enableDisableFilterByNameToolStripMenuItem.Visible =
                toolStripSeparatorSelectPacketProto.Visible =
                toolStripSeparatorFilterPacketByName.Visible =
                toolStripMenuItemSelectPacketName.Visible = false;
            }

            if (listViewSessions.Items.Count > 0)
            {
                toolStripMenuItemRemoveAll.Visible =
                markToolStripMenuItem2.Enabled =
                findToolStripMenuItem1.Enabled =
                toolStripMenuSessionsRemove.Enabled =
                        selectToolStripMenuItem2.Enabled = true;
            }
            else
            {
                toolStripMenuItemRemoveAll.Visible =
                markToolStripMenuItem2.Enabled =
                findToolStripMenuItem1.Enabled =
                toolStripMenuSessionsRemove.Enabled =
                        selectToolStripMenuItem2.Enabled = false;
            }
        }

        private void findSessions_Click(object sender, EventArgs e)
        {
            FilterOptions opts = new FilterOptions((listViewSessions.SelectedIndices.Count > 0));
            FormSessionSearch search = new FormSessionSearch(ref opts);
            search.ShowDialog();

            if (!String.IsNullOrEmpty(opts.SearchText))
            {
                Thread sThread = new Thread(delegate()
                {
                    SearchSessions(opts);
                });
                sThread.Name = "Search";
                sThread.Start();
            }
        }

        private void saveFilterSelectionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog2.ShowDialog() == DialogResult.OK)
            {
                SaveAllSettings(saveFileDialog2.FileName);
            }
        }

        private void loadFilterSelectionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog2.ShowDialog() == DialogResult.OK)
            {
                RestoreSavedSettings(openFileDialog2.FileName);
                if (listViewSessions.Items.Count > 0)
                {
                    if (MessageBox.Show("Would you like to apply these settings to the current session list?",
                        "Apply Filter", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                    {
                        listViewSessions.BeginUpdate();
                        foreach (ListViewItem item in listViewSessions.Items)
                        {
                            ListViewItem found = FindListViewItem(listViewPacketFilters, item.SubItems[2].Text, false);
                            if (found == null)
                                found = FindListViewItem(listViewMessageFilters, item.SubItems[2].Text, false);

                            if (found != null && !found.Checked)
                                listViewSessions.Items.Remove(item);
                        }
                        listViewSessions.EndUpdate();
                    }
                }
            }
        }

        private void listViewMessageFilters_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            proxy.AddCapsDelegate(e.Item.Text, e.Item.Checked);
        }

        private void listViewPacketFilters_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (e.Item.Group.Name.Equals("Packets"))
                proxy.AddUDPDelegate(PacketTypeFromName(e.Item.Text), e.Item.Checked);
        }

        private void checkBoxCheckallCaps_CheckedChanged(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewMessageFilters.Items)
            {
                item.Checked = checkBoxCheckAllMessages.Checked;
            }
        }
        #endregion

        /// <summary>
        /// Start/Stop the statistics gathering timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void enableStatisticsToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (timer1.Enabled && !enableStatisticsToolStripMenuItem.Checked)
                timer1.Enabled = false;

            if (!timer1.Enabled && enableStatisticsToolStripMenuItem.Checked)
                timer1.Enabled = true;
        }

        private void saveSessionArchiveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                OSDMap map = new OSDMap(1);
                OSDArray sessionArray = new OSDArray();

                foreach (Session item in m_SessionViewItems)
                {
                    OSDMap session = new OSDMap();
                    session["type"] = OSD.FromString(item.GetType().Name);
                    session["tag"] = OSD.FromBinary(item.Serialize());
                    sessionArray.Add(session);
                }

                map["sessions"] = sessionArray;

                try
                {
                    File.WriteAllText(saveFileDialog1.FileName, map.ToString());
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Exception occurred trying to save session archive: " + ex);
                }
            }
        }

        private void SetProgressStep(int step)
        {
            if (progressBar1.InvokeRequired)
            {
                progressBar1.Invoke(new MethodInvoker(delegate()
                {
                    SetProgressStep(step);
                }));
            }
            else
            {
                //progressBar1.Step = step;
                progressBar1.Value += step;
            }
        }

        private void SetProgressVisible(bool visible)
        {
            if (panelActionProgress.InvokeRequired)
            {
                panelActionProgress.BeginInvoke(new MethodInvoker(delegate()
                    {
                        SetProgressVisible(visible);
                    }));
            }
            else
            {
                panelActionProgress.Visible = visible;
            }
        }

        private void loadSessionArchiveToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                FileInfo fi = new FileInfo(openFileDialog1.FileName);
                progressBar1.Maximum = (int)fi.Length;
                progressBar1.Value = 0;

                // toss this job into a thread so the UI remains responsive for large files
                System.Threading.ThreadPool.QueueUserWorkItem(new WaitCallback(delegate(Object obj)
                {
                    lock (m_FileIOLockerObject)
                    {
                        SetProgressVisible(true);
                        StringBuilder fileContents = new StringBuilder((int)fi.Length);
                        using (StreamReader sr = fi.OpenText())
                        {
                            String input;
                            while ((input = sr.ReadLine()) != null)
                            {
                                fileContents.AppendLine(input);
                                SetProgressStep(input.Length);
                            }
                            input = null;
                        }

                        OSDMap map = (OSDMap)OSDParser.DeserializeLLSDNotation(fileContents.ToString());

                        // Give the GC a little push
                        fileContents = null;
                        GC.Collect(0);
                        GC.WaitForPendingFinalizers();

                        // turn the map into a list
                        OSDMapToSessions(map);
                        SetProgressVisible(false);
                    }
                }));
            }
        }

        private void OSDMapToSessions(OSDMap map)
        {
            if (!map.ContainsKey("sessions"))
                return;

            if (listViewSessions.InvokeRequired)
                listViewSessions.BeginInvoke((MethodInvoker)(() => OSDMapToSessions(map)));
            else
            {
                OSDArray sessionsArray = (OSDArray)map["sessions"];

                progressBar1.Maximum = sessionsArray.Count;
                progressBar1.Value = 0;
                progressBar1.Step = 1;

                panelActionProgress.Visible = true;

                listViewSessions.VirtualListSize = 0;
                m_SessionViewItems.Clear();

                ClearCache();

                for (int i = 0; i < sessionsArray.Count; i++)
                {
                    OSDMap session = (OSDMap)sessionsArray[i];
                    
                        Session importedSession = (Session)m_CurrentAssembly.CreateInstance("WinGridProxy." + session["type"].AsString());
                        if (importedSession == null)
                        {

                          //  System.Diagnostics.Debug.Assert(importedSession != null, session["type"].AsString() + );
                        }                        
                        
                        importedSession.Deserialize(session["tag"].AsBinary());
                        m_SessionViewItems.Add(importedSession);
                        progressBar1.PerformStep();                    
                }

                listViewSessions.VirtualListSize = m_SessionViewItems.Count;

                listViewSessions.Invalidate();

                // resize columns to fit whats currently on screen
                listViewSessions.Columns[0].Width =
                    listViewSessions.Columns[1].Width =
                    listViewSessions.Columns[2].Width =
                    listViewSessions.Columns[3].Width =
                    listViewSessions.Columns[5].Width = -2;

                panelActionProgress.Visible = false;

                map = null;
            }
        }

        //Generic ListView sort event used by filter listviews only
        private void listViewFilterSorter_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ListView lv = (ListView)sender;
            ListViewItemComparer columnSorter = new ListViewItemComparer();
            columnSorter.column = e.Column;

            if ((columnSorter.bAscending = (lv.Sorting == SortOrder.Ascending)))
                lv.Sorting = SortOrder.Descending;
            else
                lv.Sorting = SortOrder.Ascending;

            lv.ListViewItemSorter = columnSorter as IComparer;
        }

        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // TODO: warn if client is connected!

            if (timer1 != null)
                timer1.Dispose();

            if (timerCleanupCache != null)
                timerCleanupCache.Dispose();

            this.Close();
        }

        #region Helpers

        private void SaveAllSettings(string fileName)
        {
            Store.MessageSessions.Clear();
            Store.PacketSessions.Clear();

            foreach (ListViewItem item in listViewPacketFilters.Items)
            {
                FilterEntryOptions entry = new FilterEntryOptions();
                entry.Checked = item.Checked;
                entry.Type = item.SubItems[1].Text;
                entry.Group = item.Group.Name;

                if (!Store.PacketSessions.ContainsKey(item.Text))
                    Store.PacketSessions.Add(item.Text, entry);
            }

            foreach (ListViewItem item in listViewMessageFilters.Items)
            {
                FilterEntryOptions entry = new FilterEntryOptions();
                entry.Checked = item.Checked;
                entry.Type = item.SubItems[1].Text;
                entry.Group = item.Group.Name;

                if (!Store.MessageSessions.ContainsKey(item.Text))
                    Store.MessageSessions.Add(item.Text, entry);
            }

            Store.StatisticsEnabled = enableStatisticsToolStripMenuItem.Checked;
            Store.SaveSessionOnExit = saveOptionsOnExitToolStripMenuItem.Checked;
            Store.AutoCheckNewCaps = autoAddNewDiscoveredMessagesToolStripMenuItem.Checked;

            lock (m_FileIOLockerObject)
            {
                Store.SerializeToFile(fileName);
            }
        }

        private void RestoreSavedSettings(string fileName)
        {
            // load saved settings from OSD Formatted file
            lock (m_FileIOLockerObject)
            {
                if (!Store.DeserializeFromFile(fileName))
                    return;
            }

            enableStatisticsToolStripMenuItem.Checked = Store.StatisticsEnabled;
            saveOptionsOnExitToolStripMenuItem.Checked = Store.SaveSessionOnExit;
            autoAddNewDiscoveredMessagesToolStripMenuItem.Checked = Store.AutoCheckNewCaps;

            // Update message filter listview
            listViewMessageFilters.BeginUpdate();
            foreach (KeyValuePair<string, FilterEntryOptions> kvp in Store.MessageSessions)
            {
                ListViewItem foundMessage = FindListViewItem(listViewPacketFilters, kvp.Key, false);
                if (foundMessage == null)
                {
                    ListViewItem addedItem = listViewMessageFilters.Items.Add(
                        new ListViewItem(kvp.Key, listViewMessageFilters.Groups[kvp.Value.Group]));
                    addedItem.Name = kvp.Key;
                    addedItem.Checked = kvp.Value.Checked;
                    addedItem.SubItems.Add(kvp.Value.Type);

                    addedItem.BackColor = (kvp.Value.Type.Equals(PROTO_CAPABILITIES)) ? Color_Cap : Color_Event;
                }
                else
                {
                    foundMessage.Checked = kvp.Value.Checked;
                }
                if (kvp.Value.Type.Equals(PROTO_CAPABILITIES))
                {
                    proxy.AddCapsDelegate(kvp.Key, kvp.Value.Checked);
                }
            }
            listViewMessageFilters.EndUpdate();

            // updateTreeView packet filter listview
            listViewPacketFilters.BeginUpdate();
            foreach (KeyValuePair<string, FilterEntryOptions> kvp in Store.PacketSessions)
            {
                ListViewItem foundPacket = FindListViewItem(listViewPacketFilters, kvp.Key, false);
                if (foundPacket == null)
                {
                    ListViewItem addedItem = listViewPacketFilters.Items.Add(
                        new ListViewItem(kvp.Key, listViewPacketFilters.Groups[kvp.Value.Group]));

                    addedItem.Name = kvp.Key;
                    addedItem.Checked = kvp.Value.Checked;
                    addedItem.SubItems.Add(kvp.Value.Type);

                    addedItem.BackColor = (kvp.Value.Type.Equals(PROTO_AUTHENTICATE)) ? Color_Login : Color_Packet;
                }
                else
                {
                    foundPacket.Checked = kvp.Value.Checked;
                }
                if (kvp.Value.Type.Equals(PROTO_PACKETSTRING))
                {
                    proxy.AddUDPDelegate(PacketTypeFromName(kvp.Key), kvp.Value.Checked);
                }
            }
            listViewPacketFilters.EndUpdate();
        }

        private void InitProxyFilters()
        {
            RestoreSavedSettings("settings.osd");

            listViewPacketFilters.BeginUpdate();
            foreach (string name in Enum.GetNames(typeof(PacketType)))
            {
                ListViewItem found = FindListViewItem(listViewPacketFilters, name, false);
                if (!String.IsNullOrEmpty(name) && found == null)
                {
                    ListViewItem addedItem = listViewPacketFilters.Items.Add(new ListViewItem(name, listViewPacketFilters.Groups["Packets"]));
                    addedItem.Name = name;
                    addedItem.SubItems.Add(PROTO_PACKETSTRING);
                }

            }

            ListViewItem tmp;
            if (!listViewPacketFilters.Items.ContainsKey("Login Request"))
            {
                tmp = listViewPacketFilters.Items.Add(new ListViewItem("Login Request", listViewPacketFilters.Groups["Login"]));
                tmp.Name = "Login Request";
                tmp.BackColor = Color_Login;
                tmp.SubItems.Add("Login");
            }

            if (!listViewPacketFilters.Items.ContainsKey("Login Response"))
            {
                tmp = listViewPacketFilters.Items.Add(new ListViewItem("Login Response", listViewPacketFilters.Groups["Login"]));
                tmp.Name = "Login Response";
                tmp.BackColor = Color_Login;
                tmp.SubItems.Add("Login");
            }

            listViewPacketFilters.EndUpdate();

        }

        private static PacketType PacketTypeFromName(string name)
        {
            Type packetTypeType = typeof(PacketType);
            System.Reflection.FieldInfo f = packetTypeType.GetField(name);
            if (f == null)
            {//throw new ArgumentException("Bad packet type");
                return PacketType.Error;
            }

            return (PacketType)Enum.ToObject(packetTypeType, (int)f.GetValue(packetTypeType));
        }

        private void SearchSessions(FilterOptions opts)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new MethodInvoker(() => SearchSessions(opts)));
            }
            else
            {
                int resultCount = 0;
                progressBar1.Step = 1;// m_SessionViewItems.Count / 100 / 100;
                progressBar1.Maximum = m_SessionViewItems.Count;
                progressBar1.Value = 0;
                panelActionProgress.Visible = true;

                lock (m_SessionViewItems)
                {
                    for (int i = 0; i < m_SessionViewItems.Count; i++)
                    {
                        Session session = m_SessionViewItems[i];

                        if (opts.UnMarkPrevious)
                            session.BackColor = Color.White;

                        if (opts.SearchSelected && !session.Selected)
                            continue;

                        if (
                        (opts.MatchCase
                        && (session.Name.Contains(opts.SearchText)
                        || session.ToPrettyString(session.Direction).Contains(opts.SearchText))
                        ) // no case matching
                        || ((session.Name.ToLower().Contains(opts.SearchText.ToLower())
                        || session.ToPrettyString(session.Direction).ToLower().Contains(opts.SearchText.ToLower())
                            ))
                        )
                        {
                            if (opts.MarkMatches)
                                session.BackColor = opts.HighlightMatchColor;

                            if (opts.SelectResults)
                                session.Selected = true;
                            else
                                session.Selected = false;

                            resultCount++;
                        }
                        progressBar1.PerformStep();
                    }
                }
                ClearCache();
                listViewSessions.Invalidate();
                panelActionProgress.Visible = false;
                toolStripLowerStatusLabel.Text = String.Format("Searched {0} session{2} and found {1} matche{2}", m_SessionViewItems.Count, resultCount,
                    (resultCount != 1) ? "s" : "");
            }
        }

        #endregion

        #region XML Tree

        private void updateTreeView(string xml, TreeView treeView)
        {
            treeView.BeginUpdate();
            try
            {
                treeView.Nodes.Clear();
                XmlDocument tmpxmldoc = new XmlDocument();
                tmpxmldoc.LoadXml(xml);
                FillTree(tmpxmldoc.DocumentElement, treeView.Nodes);
                treeView.ExpandAll();
            }
            catch (Exception ex)
            {
                Logger.Log("Error during XML conversion", Helpers.LogLevel.Error, ex);
            }
            finally
            {
                treeView.EndUpdate();
            }
        }

        private void FillTree(XmlNode node, TreeNodeCollection parentnode)
        {
            // End recursion if the node is a text type
            if (node == null || node.NodeType == XmlNodeType.Text || node.NodeType == XmlNodeType.CDATA)
                return;

            TreeNodeCollection tmptreenodecollection = AddNodeToTree(node, parentnode);

            // Add all the children of the current node to the treeview
            foreach (XmlNode tmpchildnode in node.ChildNodes)
            {
                FillTree(tmpchildnode, tmptreenodecollection);
            }
        }

        private TreeNodeCollection AddNodeToTree(XmlNode node, TreeNodeCollection parentnode)
        {
            TreeNode newchildnode = CreateTreeNodeFromXmlNode(node);

            // if nothing to add, return the parent item
            if (newchildnode == null) return parentnode;

            // add the newly created tree node to its parent
            if (parentnode != null) parentnode.Add(newchildnode);

            return newchildnode.Nodes;
        }

        private TreeNode CreateTreeNodeFromXmlNode(XmlNode node)
        {
            TreeNode tmptreenode = new TreeNode();

            if ((node.HasChildNodes) && (node.FirstChild.Value != null))
            {
                tmptreenode = new TreeNode(node.Name);
                TreeNode tmptreenode2 = new TreeNode(node.FirstChild.Value);
                tmptreenode.Nodes.Add(tmptreenode2);
            }
            else if (node.NodeType != XmlNodeType.CDATA)
            {
                tmptreenode = new TreeNode(node.Name);
            }

            return tmptreenode;
        }

        #endregion

        #region Timers

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new MethodInvoker(() => timer1_Tick(sender, e)));
            }
            else
            {
                label1PacketsOut.Text = String.Format("{0} ({1})", PacketsOutCounter, FormatBytes(PacketsOutBytes));
                labelPacketsIn.Text = String.Format("{0} ({1})", PacketsInCounter, FormatBytes(PacketsInBytes));
                labelPacketsTotal.Text = String.Format("{0} ({1})", PacketsOutCounter + PacketsInCounter, FormatBytes(PacketsOutBytes + PacketsInBytes));

                labelCapsIn.Text = String.Format("{0} ({1})", CapsInCounter, FormatBytes(CapsInBytes));
                labelCapsOut.Text = String.Format("{0} ({1})", CapsOutCounter, FormatBytes(CapsOutBytes));
                labelCapsTotal.Text = String.Format("{0} ({1})", CapsInCounter + CapsOutCounter, FormatBytes(CapsOutBytes + CapsInBytes));

                // pause during scroll
                if (m_AllowUpdate)
                    UpdateVirtualListSize(m_SessionViewItems.Count);

            }
        }

        private void timerExpireCache_Tick(object sender, EventArgs e)
        {
            int expired = 0;
            lock (m_SessionViewCache)
            {
                int[] keys = new int[m_SessionViewCache.Keys.Count];
                m_SessionViewCache.Keys.CopyTo(keys, 0);

                foreach (int key in keys)
                {
                    long expires = 0;
                    if (long.TryParse(m_SessionViewCache[key].Name, out expires))
                    {
                        if (expires > 0 && expires <= DateTime.UtcNow.ToLocalTime().Ticks)
                        {
                            expired++;
                            m_SessionViewCache.Remove(key);
                        }
                    }
                }
            }
        }
        #endregion

        private void EditToolStripButton_DropDownOpening(object sender, EventArgs e)
        {
            lock (m_SessionViewItems)
            {
                if (m_SessionViewItems.Count > 0)
                {
                    toolStripMenuSessionsRemove.Enabled =
                        removeToolStripMenuItem2.Enabled =
                        selectToolStripMenuItem1.Enabled =
                        saveSessionArchiveToolStripMenuItem.Enabled =
                        toolStripMenuItemRemoveAll.Enabled = true;

                    if (listViewSessions.SelectedIndices.Count > 0)
                    {
                        toolStripMenuItemRemoveUnselected.Enabled =
                        markToolStripMenuItem1.Enabled =
                        toolStripSeparatorSelectPacketProto.Visible =
                        toolStripMenuItemSelectPacketName.Visible =
                        noneToolStripMenuItem2.Enabled =
                        copyToolStripMenuItem1.Enabled =
                        toolStripMenuItemRemoveSelected.Enabled = true;
                    }
                    else
                    {
                        toolStripMenuItemRemoveUnselected.Enabled =
                        markToolStripMenuItem1.Enabled =
                        toolStripSeparatorSelectPacketProto.Visible =
                        toolStripMenuItemSelectPacketName.Visible =
                        noneToolStripMenuItem2.Enabled =
                        noneToolStripMenuItem2.Enabled =
                        copyToolStripMenuItem1.Enabled =
                        toolStripMenuItemRemoveSelected.Enabled = false;
                    }

                    //if (listViewSessions.SelectedIndices.Count > 0
                    //    && listViewSessions.SelectedItems.Count != listViewSessions.Items.Count)
                    //{
                    //    toolStripMenuItemRemoveUnselected.Enabled =
                    //    invertToolStripMenuItem1.Enabled =
                    //    noneToolStripMenuItem2.Enabled = true;
                    //}
                    //else
                    //{
                    //    toolStripMenuItemRemoveUnselected.Enabled =
                    //    invertToolStripMenuItem1.Enabled =
                    //    noneToolStripMenuItem2.Enabled = false;
                    //}

                }
                else
                {
                    toolStripMenuSessionsRemove.Enabled =
                    toolStripSeparatorSelectPacketProto.Visible =
                    toolStripMenuItemSelectPacketName.Visible =
                    findToolStripMenuItem.Enabled =
                    selectToolStripMenuItem1.Enabled =
                    removeToolStripMenuItem2.Enabled =
                    toolStripMenuItemRemoveUnselected.Enabled =
                    copyToolStripMenuItem1.Enabled =
                    markToolStripMenuItem1.Enabled =
                    saveSessionArchiveToolStripMenuItem.Enabled =
                    toolStripMenuItemRemoveAll.Enabled = false;
                }

                if (listViewPacketFilters.Items.Count + m_SessionViewItems.Count > 0)
                {
                    saveFilterSelectionsToolStripMenuItem.Enabled = true;
                }
                else
                {
                    saveFilterSelectionsToolStripMenuItem.Enabled = false;
                }
            }
        }

        private void autoColorizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                //listview.BackColor = colorDialog1.Color;
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (pluginManager == null)
                pluginManager = new FormPluginManager(proxy.Proxy);

            pluginManager.ShowDialog();
        }

        void Instance_MessageLoggedEvent(object sender, MessageLoggedEventArgs e)
        {
            if (this.IsDisposed || this.Disposing)
                return;

            if (InvokeRequired)
            {
                BeginInvoke(new MethodInvoker(() => Instance_MessageLoggedEvent(sender, e)));
            }
            else
            {
                string s = String.Format("{0} [{1}] {2} {3}", e.LoggingEvent.TimeStamp, e.LoggingEvent.Level,
                    e.LoggingEvent.RenderedMessage, e.LoggingEvent.ExceptionObject);
                richTextBoxDebugLog.AppendText(s + "\n");
            }
        }

        private void richTextBoxDecodedRequest_TextChanged(object sender, EventArgs e)
        {
            RichTextBox m_rtb = (RichTextBox)sender;

            // don't colorize xml!
            if (m_rtb.Lines.Length <= 0 || m_rtb.Lines[0].StartsWith("<?xml"))
                return;

            Regex typesRegex = new Regex(@"\[(?<Type>\w+|\w+\[\])\]|\((?<Enum>.*)\)|\s-- (?<Header>\w+|\w+ \[\]) --\s|(?<BlockSep>\s\*\*\*\s)|(?<Tag>\s<\w+>\s|\s<\/\w+>\s)|(?<BlockCounter>\s\w+\[\d+\]\s)", RegexOptions.ExplicitCapture);

            MatchCollection matches = typesRegex.Matches(m_rtb.Text);
            foreach (Match match in matches)
            {
                m_rtb.SelectionStart = match.Index + 1;
                m_rtb.SelectionLength = match.Length - 2;
                m_rtb.SelectionFont = new Font(m_rtb.Font.FontFamily, m_rtb.Font.Size, FontStyle.Bold);

                if (!String.IsNullOrEmpty(match.Groups["Type"].Value))
                    m_rtb.SelectionColor = Color.FromArgb(43, 145, 175);
                else if (!String.IsNullOrEmpty(match.Groups["Enum"].Value))
                    m_rtb.SelectionColor = Color.FromArgb(43, 145, 175);
                else if (!String.IsNullOrEmpty(match.Groups["Header"].Value))
                {
                    m_rtb.SelectionColor = Color.Green;
                    m_rtb.SelectionBackColor = Color.LightSteelBlue;
                }
                else if (!String.IsNullOrEmpty(match.Groups["BlockSep"].Value))
                    m_rtb.SelectionColor = Color.Gold;
                else if (!String.IsNullOrEmpty(match.Groups["Tag"].Value))
                {
                    m_rtb.SelectionColor = Color.White;
                    m_rtb.SelectionBackColor = Color.Black;
                }
                else if (!String.IsNullOrEmpty(match.Groups["BlockCounter"].Value))
                    m_rtb.SelectionColor = Color.Green;

            }
        }

        private void asDecodedTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                StringBuilder outString = new StringBuilder();
                foreach (ListViewItem item in listViewSessions.Items)
                {
                    if (item.Tag is Packet)
                    {
                        //outString.AppendLine(DecodePacket.PacketToString((Packet)item.Tag));
                        outString.AppendLine(PacketDecoder.PacketToString((Packet)item.Tag));
                    }

                    if (item.Tag is IMessage)
                    {
                        IMessage msg = (IMessage)item.Tag;
                        outString.AppendLine(msg.Serialize().ToString());
                    }

                    try
                    {
                        File.WriteAllText(saveFileDialog1.FileName, outString.ToString());
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Exception occurred trying to save session archive: " + ex);
                    }
                }
            }
        }

        private void richTextBoxDecodedRequest_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText);
        }

        private void saveSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveAllSettings("settings.osd");
        }


        private void buttonSaveRequestHex_Click(object sender, EventArgs e)
        {
            if (hexBoxRequest.ByteProvider != null && hexBoxRequest.ByteProvider.Length > 0)
            {
                saveFileDialog3.FileName = m_CurrentSession.Name;
                if (saveFileDialog3.ShowDialog() == DialogResult.OK)
                {
                    byte[] bytes = new byte[hexBoxRequest.ByteProvider.Length];
                    for (int i = 0; i < hexBoxRequest.ByteProvider.Length; i++)
                    {
                        bytes[i] = hexBoxRequest.ByteProvider.ReadByte(i);
                    }
                    File.WriteAllBytes(saveFileDialog3.FileName, bytes);
                }
            }
        }

        private void buttonExportRawHex_Click(object sender, EventArgs e)
        {
            if (hexBoxResponse.ByteProvider != null && hexBoxResponse.ByteProvider.Length > 0)
            {
                saveFileDialog3.FileName = m_CurrentSession.Name;
                if (saveFileDialog3.ShowDialog() == DialogResult.OK)
                {
                    byte[] bytes = new byte[hexBoxResponse.ByteProvider.Length];
                    for (int i = 0; i < hexBoxResponse.ByteProvider.Length; i++)
                    {
                        bytes[i] = hexBoxResponse.ByteProvider.ReadByte(i);
                    }
                    File.WriteAllBytes(saveFileDialog3.FileName, bytes);
                }
            }
        }


        /// <summary>
        /// Column Sorting
        /// </summary>
        private void listViewSessions_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (m_ListViewSortOrder == SortOrder.Ascending)
                m_ListViewSortOrder = SortOrder.Descending;
            else
                m_ListViewSortOrder = SortOrder.Ascending;

            string name = ((ListView)sender).Columns[e.Column].Text;
            lock (m_SessionViewItems)
            {
                switch (name)
                {
                    case "#":
                        if (m_ListViewSortOrder == SortOrder.Ascending)
                            m_SessionViewItems.Sort((x, y) => x.TimeStamp.CompareTo(y.TimeStamp));
                        else
                            m_SessionViewItems.Sort((x, y) => y.TimeStamp.CompareTo(x.TimeStamp));
                        break;
                    case "Protocol":
                        if (m_ListViewSortOrder == SortOrder.Ascending)
                            m_SessionViewItems.Sort((x, y) => x.Protocol.CompareTo(y.Protocol));
                        else
                            m_SessionViewItems.Sort((x, y) => y.Protocol.CompareTo(x.Protocol));
                        break;
                    case "Name":
                        if (m_ListViewSortOrder == SortOrder.Ascending)
                            m_SessionViewItems.Sort((x, y) => x.Name.CompareTo(y.Name));
                        else
                            m_SessionViewItems.Sort((x, y) => y.Name.CompareTo(x.Name));
                        break;
                    case "Bytes":
                        if (m_ListViewSortOrder == SortOrder.Ascending)
                            m_SessionViewItems.Sort((x, y) => x.Length.CompareTo(y.Length));
                        else
                            m_SessionViewItems.Sort((x, y) => y.Length.CompareTo(x.Length));
                        break;
                    case "Host":
                        if (m_ListViewSortOrder == SortOrder.Ascending)
                            m_SessionViewItems.Sort((x, y) => x.Host.CompareTo(y.Host));
                        else
                            m_SessionViewItems.Sort((x, y) => y.Host.CompareTo(x.Host));
                        break;
                    case "Content Type":
                        if (m_ListViewSortOrder == SortOrder.Ascending)
                            m_SessionViewItems.Sort((x, y) => x.ContentType.CompareTo(y.ContentType));
                        else
                            m_SessionViewItems.Sort((x, y) => y.ContentType.CompareTo(x.ContentType));
                        break;
                }
            }

            ClearCache();
            listViewSessions.Invalidate();
            ((ListView)sender).Columns[e.Column].Width = -2;
        }

        /// <summary>
        /// Retrieve an item for display in the listview, first trying the cache
        /// </summary>        
        private void listViewSessions_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            lock (m_SessionViewCache)
            {
                if (m_SessionViewCache.ContainsKey(e.ItemIndex))
                {
                    e.Item = m_SessionViewCache[e.ItemIndex];
                }
                else
                {
                    e.Item = GenerateListViewItem(e.ItemIndex);
                    m_SessionViewCache.Add(e.ItemIndex, e.Item);
                }
            }
        }

        private ListViewItem GenerateListViewItem(int index)
        {
            Session sessionItem = null;
            lock (m_SessionViewItems)
            {
                sessionItem = m_SessionViewItems[index];
            }

            ListViewItem sessionViewItem = new ListViewItem(new string[] { sessionItem.TimeStamp.ToLocalTime().ToString("hh:m:s.fff")/*index.ToString()*/, sessionItem.Protocol, sessionItem.Name, 
                    sessionItem.Length.ToString(), sessionItem.Host, sessionItem.ContentType });

            sessionViewItem.Checked = sessionItem.Selected;
            sessionViewItem.Tag = sessionItem;
            sessionViewItem.ImageIndex = (int)sessionItem.Direction;

            sessionViewItem.BackColor = sessionItem.BackColor;

            if (sessionItem is SessionPacket && Int32.Parse(sessionViewItem.SubItems[3].Text) > 1400)
            {
                sessionViewItem.UseItemStyleForSubItems = false;
                sessionViewItem.SubItems[3].ForeColor = Color.Red;
            }

            // this is used for expiring the cache
            sessionViewItem.Name = DateTime.UtcNow.ToLocalTime().AddMinutes(1).Ticks.ToString();

            return sessionViewItem;
        }

        private void listViewSessions_VirtualItemsSelectionRangeChanged(object sender, ListViewVirtualItemsSelectionRangeChangedEventArgs e)
        {
            int end = (e.EndIndex + 2 <= m_SessionViewItems.Count) ? e.EndIndex + 2 : m_SessionViewItems.Count;

            lock (m_SessionViewItems)
            {
                for (int i = e.StartIndex; i < end; i++)
                {
                    m_SessionViewItems[i].Selected = e.IsSelected;
                }
            }

            ClearCache();
            listViewSessions.Invalidate();
        }

        private void listViewSessions_CacheVirtualItems(object sender, CacheVirtualItemsEventArgs e)
        {
            for (int i = e.StartIndex; i < e.EndIndex + 1; i++)
            {
                lock (m_SessionViewCache)
                {
                    if (!m_SessionViewCache.ContainsKey(i))
                        m_SessionViewCache.Add(i, GenerateListViewItem(i));
                }
            }
        }

        private void listViewSessions_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewSessions.SelectedIndices.Count == 1)
            {
                // update the context menus
                contextMenuStripSessions_Opening(sender, null);

                tabControlMain.SelectTab("tabPageInspect");

                int index = listViewSessions.SelectedIndices[0];

                object tag = null;
                if (m_SessionViewCache.ContainsKey(index))
                    tag = m_SessionViewCache[index].Tag;
                else
                    tag = GenerateListViewItem(listViewSessions.SelectedIndices[0]).Tag; //e.Item.Tag;

                if (tag is Session)
                {
                    Session session = (Session)tag;

                    this.m_CurrentSession = session;

                    treeViewXmlResponse.Nodes.Clear();
                    treeViewXMLRequest.Nodes.Clear();

                    Be.Windows.Forms.DynamicByteProvider responseBytes = new Be.Windows.Forms.DynamicByteProvider(session.ToBytes(Direction.Incoming));
                    richTextBoxDecodedResponse.Text = session.ToPrettyString(Direction.Incoming);
                    richTextBoxRawResponse.Text = session.ToRawString(Direction.Incoming);
                    richTextBoxNotationResponse.Text = session.ToStringNotation(Direction.Incoming);
                    hexBoxResponse.ByteProvider = responseBytes;
                    updateTreeView(session.ToXml(Direction.Incoming), treeViewXmlResponse);

                    Be.Windows.Forms.DynamicByteProvider requestBytes = new Be.Windows.Forms.DynamicByteProvider(session.ToBytes(Direction.Outgoing));
                    richTextBoxDecodedRequest.Text = session.ToPrettyString(Direction.Outgoing);
                    richTextBoxRawRequest.Text = session.ToRawString(Direction.Outgoing);
                    richTextBoxNotationRequest.Text = session.ToStringNotation(Direction.Outgoing);
                    hexBoxRequest.ByteProvider = requestBytes;
                    updateTreeView(session.ToXml(Direction.Outgoing), treeViewXMLRequest);

                    RequestPosition_Changed(this, EventArgs.Empty);
                    ReplyPosition_Changed(this, EventArgs.Empty);
                }
                else
                {
                    richTextBoxDecodedResponse.Text = "Unknown data object encountered: " + tag.GetType().ToString();
                }
            }
        }

        private void toolStripButtonLaunchViewer_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(String.Format("{0}", m_InstalledViewers[toolStripComboBox1.Text]), String.Format("--set InstallLanguage en -multiple -loginuri http://{0}:{1}", comboBoxListenAddress.Text, textBoxProxyPort));
        }

        private void listViewSessions_Scrolling(object sender, ScrollingEventArgs e)
        {
            m_AllowUpdate = !e.Scrolling;
            if (!e.Scrolling)
            {
                listViewSessions.TopItem.Focused = true;
            }
        }

        private void comboBoxLoginURL_Leave(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(comboBoxLoginURL.Text))
            {
                if (m_DefaultGridLoginServers.Contains(comboBoxLoginURL.Text))
                {
                    // make the selection the default for next time
                    m_DefaultGridLoginServers.Remove(comboBoxLoginURL.Text);
                }
                m_DefaultGridLoginServers.Insert(0, comboBoxLoginURL.Text);

                File.WriteAllLines("gridservers.ini", m_DefaultGridLoginServers.ToArray());
            }
        }

        private void documentationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://lib.openmetaverse.org/wiki/WinGridProxy");
        }

        #region Inject Tab
        // Enable Inject button if box contains text
        private void richTextBoxInject_TextChanged(object sender, EventArgs e)
        {
            toolStripButtonInject.Enabled = (richTextBoxInject.TextLength > 0);
        }

        private void toolStripButtonInject_Click(object sender, EventArgs e)
        {
            proxy.InjectPacket(richTextBoxInject.Text, true);
        }

        #endregion Inject Tab

        private void listViewSessions_SearchForVirtualItem(object sender, SearchForVirtualItemEventArgs e)
        {

        }
    }   
}
