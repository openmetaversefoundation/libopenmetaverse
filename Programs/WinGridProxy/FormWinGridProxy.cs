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

        private List<ListViewItem> QueuedSessions;
        private System.Threading.Timer SessionQueue;
        private int SessionQueueInterval;
        private bool monoRuntime;

        private const string PROTO_CAPABILITIES = "Cap";
        private const string PROTO_EVENTMESSAGE = "Event";
        private const string PROTO_PACKETSTRING = "UDP";
        private const string PROTO_AUTHENTICATE = "https";

        private readonly Color Color_Login = Color.OldLace;
        private readonly Color Color_Packet = Color.LightYellow;
        private readonly Color Color_Cap = Color.Honeydew;
        private readonly Color Color_Event = Color.AliceBlue;

        public FormWinGridProxy()
        {
            InitializeComponent();

            Logger.Log("WinGridProxy ready", Helpers.LogLevel.Info);

            PacketDecoder.InitializeDecoder();

            if (FireEventAppender.Instance != null)
            {
                FireEventAppender.Instance.MessageLoggedEvent += new MessageLoggedEventHandler(Instance_MessageLoggedEvent);
            }
            
            // Attempt to work around some mono bugs
            monoRuntime = Type.GetType("Mono.Runtime") != null; // Officially supported way of detecting mono
            if (monoRuntime)
            {
                SessionQueueInterval = 500;
                SessionQueue = new System.Threading.Timer(new TimerCallback(SessionQueueWorker), null, SessionQueueInterval, SessionQueueInterval);
                QueuedSessions = new List<ListViewItem>();
                Font fixedFont = new Font(FontFamily.GenericMonospace, 9f, FontStyle.Regular, GraphicsUnit.Point);
                richTextBoxDecodedRequest.Font =
                    richTextBoxDecodedResponse.Font =
                    richTextBoxNotationRequest.Font =
                    richTextBoxNotationResponse.Font =
                    richTextBoxRawRequest.Font =
                    richTextBoxRawResponse.Font = fixedFont;
            }

            // populate the listen box with the known IP Addresses of this host
            IPHostEntry iphostentry = Dns.GetHostByName(Dns.GetHostName());
            foreach (IPAddress address in iphostentry.AddressList)
                comboBoxListenAddress.Items.Add(address.ToString());

            ProxyManager.OnPacketLog += ProxyManager_OnPacketLog;
            ProxyManager.OnMessageLog += ProxyManager_OnMessageLog;
            ProxyManager.OnLoginResponse += ProxyManager_OnLoginResponse;
            ProxyManager.OnCapabilityAdded += ProxyManager_OnCapabilityAdded;
            ProxyManager.OnEventMessageLog += ProxyManager_OnEventMessageLog;
        }

        #region Event Handlers for Messages/Packets
     
        /// <summary>
        /// Adds a new EventQueue message to the Message Filters listview.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="stage"></param>
        void ProxyManager_OnEventMessageLog(CapsRequest req, CapsStage stage)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new MethodInvoker(delegate()
                {
                    ProxyManager_OnEventMessageLog(req, stage);
                }));
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
                this.BeginInvoke(new MethodInvoker(delegate()
                {
                    ProxyManager_OnCapabilityAdded(cap);
                }));
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
                this.BeginInvoke(new MethodInvoker(delegate()
                    {
                        ProxyManager_OnLoginResponse(request, direction);
                    }));
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
                    //session.ImageIndex = (request is XmlRpcRequest) ? 1 : 0;

                    AddSession(sessionEntry);
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

            ListViewItem sessionItem = new ListViewItem(new string[] { PacketCounter.ToString(), sessionPacket.Protocol, sessionPacket.Name, sessionPacket.Length.ToString(), sessionPacket.Host, sessionPacket.ContentType });
            sessionItem.Tag = sessionPacket;
            sessionItem.ImageIndex = (int)sessionPacket.Direction;

            AddSession(sessionItem);
        }

        /// <summary>
        /// Handle Capabilities 
        /// </summary>        
        private void ProxyManager_OnMessageLog(CapsRequest req, CapsStage stage)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new MethodInvoker(delegate()
                    {
                        ProxyManager_OnMessageLog(req, stage);
                    }));
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

                    string[] s = { PacketCounter.ToString(),capsSession.Protocol, capsSession.Name, capsSession.Length.ToString(), capsSession.Host, capsSession.ContentType };
                    ListViewItem session = new ListViewItem(s);
                    
                    session.ImageIndex = (int)direction;                    
                    session.Tag = capsSession;

                    session.BackColor = found.BackColor;

                    AddSession(session);
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

                loadFilterSelectionsToolStripMenuItem.Enabled = saveFilterSelectionsToolStripMenuItem.Enabled = true;

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
                loadFilterSelectionsToolStripMenuItem.Enabled = saveFilterSelectionsToolStripMenuItem.Enabled = false;
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

        private void listViewSessions_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {

            if (e.IsSelected && listViewSessions.SelectedItems.Count == 1)
            {
                // update the context menus
                contextMenuStripSessions_Opening(sender, null);

                tabControlMain.SelectTab("tabPageInspect");

                object tag = e.Item.Tag;

                if (tag is Session)
                {
                    Session session = (Session)tag;

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
            foreach (ListViewItem item in listViewSessions.Items)
            {
                item.Selected = true;
            }
        }

        // unselect all items in session list
        private void sessionSelectNone_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewSessions.Items)
            {
                item.Selected = false;
            }
        }

        // invert selection
        private void sessionInvertSelection_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewSessions.Items)
            {
                item.Selected = !item.Selected;
            }
        }

        // remove all sessions
        private void sessionRemoveAll_Click(object sender, EventArgs e)
        {
            listViewSessions.Items.Clear();
        }

        // remove sessions that are currently selected
        private void sessionRemoveSelected_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewSessions.Items)
            {
                if (item.Selected)
                    listViewSessions.Items.Remove(item);
            }
        }

        // remove sessions that are not currently selected
        private void sessionRemoveUnselected_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewSessions.Items)
            {
                if (!item.Selected)
                    listViewSessions.Items.Remove(item);
            }
        }

        // Colorize selected sessions
        private void sessionMarkSelected_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = (ToolStripMenuItem)sender;

            foreach (ListViewItem item in listViewSessions.Items)
            {
                if (item.Selected)
                    item.BackColor = Color.FromName(menu.Text);
            }
            sessionSelectNone_Click(sender, e);
        }

        // Unmark selected sessions
        private void sessionUnmarkSelected_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewSessions.Items)
            {
                if (item.Selected)
                    item.BackColor = Color.White;
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
            foreach (ListViewItem item in listViewSessions.Items)
            {
                if (item.SubItems[2].Text.Equals(toolStripMenuItemSelectPacketName.Tag) && !item.Selected)
                    item.Selected = true;
            }
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
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                else if (strPacketOrMessage.Equals("Messages"))// && listViewMessageFilters.Items.ContainsKey(toolStripMenuItemSelectPacketName.Tag.ToString()))
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
                markToolStripMenuItem2.Enabled =
                findToolStripMenuItem1.Enabled =
                toolStripMenuSessionsRemove.Enabled =
                        selectToolStripMenuItem2.Enabled = true;
            }
            else
            {
                markToolStripMenuItem2.Enabled =
                findToolStripMenuItem1.Enabled =
                toolStripMenuSessionsRemove.Enabled =
                        selectToolStripMenuItem2.Enabled = false;
            }
        }

        private void findSessions_Click(object sender, EventArgs e)
        {
            FilterOptions opts = new FilterOptions((listViewSessions.SelectedItems.Count > 0));
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

        // Enable Inject button if box contains text
        private void richTextBoxInject_TextChanged(object sender, EventArgs e)
        {
            buttonInjectPacket.Enabled = (richTextBoxInject.TextLength > 0);
        }

        private void buttonInjectPacket_Click(object sender, EventArgs e)
        {
            proxy.InjectPacket(richTextBoxInject.Text, true);
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
            if(e.Item.Group.Name.Equals("Packets"))
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
                foreach (ListViewItem item in listViewSessions.Items)
                {
                    if (item.Tag is Session)
                    {
                        Session data = null;
                        if (item.Tag is SessionCaps)
                        {
                            data = (SessionCaps)item.Tag;
                        }
                        else if (item.Tag is SessionEvent)
                        {
                            data = (SessionEvent)item.Tag;
                        }
                        else if (item.Tag is SessionLogin)
                        {
                            data = (SessionLogin)item.Tag;
                        }
                        else if (item.Tag is SessionPacket)
                        {
                            data = (SessionPacket)item.Tag;
                        }
                        else
                        {
                            Console.WriteLine("Not a valid session type?");
                            continue;
                        }
                        //Type t = item.Tag.GetType();
                        
                        //Session data = (SessionCaps)item.Tag;
                        OSDMap session = new OSDMap();
                        //session["name"] = OSD.FromString(item.Name);
                        //session["image_index"] = OSD.FromInteger(item.ImageIndex);
                        session["id"] = OSD.FromString(item.SubItems[0].Text);
                        //session["protocol"] = OSD.FromString(item.SubItems[1].Text);
                        //session["packet"] = OSD.FromString(item.SubItems[2].Text);
                        //session["size"] = OSD.FromString(item.SubItems[3].Text);
                        //session["host"] = OSD.FromString(item.SubItems[4].Text);
                        session["type"] = OSD.FromString(data.GetType().ToString());
                        session["tag"] = OSD.FromBinary(data.Serialize());                        
                        sessionArray.Add(session);
                    }
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

        private void loadSessionArchiveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                OSDMap map = (OSDMap)OSDParser.DeserializeLLSDNotation(File.ReadAllText(openFileDialog1.FileName));
                
                OSDArray sessionsArray = (OSDArray)map["sessions"];
                                
                listViewSessions.Items.Clear();
                listViewSessions.BeginUpdate();
                for (int i = 0; i < sessionsArray.Count; i++)
                {                                        
                    OSDMap session = (OSDMap)sessionsArray[i];
                    
                    Session importedSession = (Session)m_CurrentAssembly.CreateInstance(session["type"].AsString());                                                                                                    
                    importedSession.Deserialize(session["tag"].AsBinary());

                    ListViewItem addedItem = new ListViewItem(new string[] {
                        session["id"].AsString(), 
                        importedSession.Protocol,
                        importedSession.Name,
                        importedSession.Length.ToString(),
                        importedSession.Host, 
                        importedSession.ContentType});
                    AddSession(addedItem);
                    //addedItem.ImageIndex = session["image_index"].AsInteger();
                    addedItem.ImageIndex = (int)importedSession.Direction;
                    addedItem.BackColor = Color.GhostWhite; // give imported items a different color
                    addedItem.Tag = importedSession;
                }

                listViewSessions.EndUpdate();
            }
        }

        //Generic ListView sort event
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

            Store.SerializeToFile(fileName);
        }

        private void RestoreSavedSettings(string fileName)
        {
            // load saved settings from OSD Formatted file

            if (Store.DeserializeFromFile(fileName))
            {                
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
                        //addedItem.Group = listViewMessageFilters.Groups[kvp.Value.Group];

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
                this.BeginInvoke(new MethodInvoker(delegate()
                {
                    SearchSessions(opts);
                }));
            }
            else
            {
                int resultCount = 0;

                foreach (ListViewItem item in listViewSessions.Items)
                {
                    if (opts.UnMarkPrevious)
                        item.BackColor = Color.White;

                    if (opts.SearchSelected && !item.Selected)
                    {
                        continue;
                    }

                    if (
                        (opts.MatchCase
                        && (item.SubItems[2].Text.Contains(opts.SearchText)
                        /*|| TagToString(item.Tag, item.SubItems[2].Text).Contains(opts.SearchText)*/)
                        ) // no case matching
                        || ((item.SubItems[2].Text.ToLower().Contains(opts.SearchText.ToLower())
                        /*|| TagToString(item.Tag, item.SubItems[2].Text).ToLower().Contains(opts.SearchText.ToLower())*/
                            ))
                        )
                    {
                        resultCount++;

                        if (opts.MarkMatches)
                            item.BackColor = opts.HighlightMatchColor;

                        if (opts.SelectResults)
                            item.Selected = true;
                        else
                            item.Selected = false;
                    }
                }

                //toolStripMainLabel.Text = String.Format("Search found {0} Matches", resultCount);
            }
        }

        //private string TagToString(object tag, string key)
        //{
        //    if (tag is XmlRpcRequest)
        //    {
        //        XmlRpcRequest requestData = (XmlRpcRequest)tag;
        //        return requestData.ToString();
        //    }
        //    else if (tag is XmlRpcResponse)
        //    {
        //        XmlRpcResponse responseData = (XmlRpcResponse)tag;

        //        return responseData.ToString();
        //    }
        //    else if (tag is Packet)
        //    {
        //        Packet packet = (Packet)tag;
        //        return PacketDecoder.PacketToString(packet);
        //        //return DecodePacket.PacketToString(packet);
        //    }
        //    else if (tag is CapsRequest)
        //    {
        //        CapsRequest capsData = (CapsRequest)tag;

        //        if (capsData.Request != null)
        //        {
        //            return capsData.Request.ToString();
        //        }

        //        if (capsData.Response != null)
        //        {
        //            return capsData.Response.ToString();
        //        }
        //        return "Unable to decode CapsRequest";
        //    }
        //    else if (tag is OSD)
        //    {
        //        OSD osd = (OSD)tag;
        //        if (osd.Type == OSDType.Map)
        //        {
        //            OSDMap data = (OSDMap)osd;
        //            IMessage message;
        //            if (data.ContainsKey("body"))
        //                message = OpenMetaverse.Messages.MessageUtils.DecodeEvent(key, (OSDMap)data["body"]);
        //            else
        //                message = OpenMetaverse.Messages.MessageUtils.DecodeEvent(key, data);

        //            if (message != null)
        //                return IMessageToString(message, 0);
        //            else
        //                return "No Decoder for " + key + System.Environment.NewLine
        //                    + osd.ToString();
        //        }
        //        else
        //        {
        //            return osd.ToString();
        //        }
        //    }
        //    else
        //    {
        //        return "Could not decode object type: " + tag.GetType().ToString();
        //    }
        //}

        #endregion

        #region XML Tree

        private void updateTreeView(string xml, TreeView treeView)
        {
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
                Console.WriteLine("Error during xml conversion:" + ex.Message);
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
                this.BeginInvoke(new MethodInvoker(delegate()
                {
                    timer1_Tick(sender, e);
                }));
            }
            else
            {
                label1PacketsOut.Text = String.Format("{0} ({1} bytes)", PacketsOutCounter, PacketsOutBytes);
                labelPacketsIn.Text = String.Format("{0} ({1} bytes)", PacketsInCounter, PacketsInBytes);
                labelPacketsTotal.Text = String.Format("{0} ({1} bytes)", PacketsOutCounter + PacketsInCounter, PacketsOutBytes + PacketsInBytes);

                labelCapsIn.Text = String.Format("{0} ({1} bytes)", CapsInCounter, CapsInBytes);
                labelCapsOut.Text = String.Format("{0} ({1} bytes)", CapsOutCounter, CapsOutBytes);
                labelCapsTotal.Text = String.Format("{0} ({1} bytes)", CapsInCounter + CapsOutCounter, CapsOutBytes + CapsInBytes);
            }
        }

        #endregion

        private void EditToolStripButton_DropDownOpening(object sender, EventArgs e)
        {
            if (listViewSessions.Items.Count > 0)
            {
                toolStripMenuSessionsRemove.Enabled =
                removeToolStripMenuItem2.Enabled =
                selectToolStripMenuItem1.Enabled =
                saveSessionArchiveToolStripMenuItem.Enabled =
                toolStripMenuItemRemoveAll.Enabled = true;

                if (listViewSessions.SelectedItems.Count < listViewSessions.Items.Count)
                {
                    toolStripMenuItemRemoveUnselected.Enabled = true;
                }
                else
                {
                    toolStripMenuItemRemoveUnselected.Enabled = false;
                }

                if (listViewSessions.SelectedItems.Count > 0)
                {
                    markToolStripMenuItem1.Enabled =
                    toolStripSeparatorSelectPacketProto.Visible =
                    toolStripMenuItemSelectPacketName.Visible =
                    noneToolStripMenuItem2.Enabled =
                    copyToolStripMenuItem1.Enabled =
                    toolStripMenuItemRemoveSelected.Enabled = true;
                }
                else
                {
                    markToolStripMenuItem1.Enabled =
                    toolStripSeparatorSelectPacketProto.Visible =
                    toolStripMenuItemSelectPacketName.Visible =
                    noneToolStripMenuItem2.Enabled =
                    noneToolStripMenuItem2.Enabled =
                    copyToolStripMenuItem1.Enabled =
                    toolStripMenuItemRemoveSelected.Enabled = false;
                }

                if (listViewSessions.SelectedItems.Count > 0
                    && listViewSessions.SelectedItems.Count != listViewSessions.Items.Count)
                {
                    toolStripMenuItemRemoveUnselected.Enabled =
                    invertToolStripMenuItem1.Enabled =
                    noneToolStripMenuItem2.Enabled = true;
                }
                else
                {
                    toolStripMenuItemRemoveUnselected.Enabled =
                    invertToolStripMenuItem1.Enabled =
                    noneToolStripMenuItem2.Enabled = false;
                }

            }
            else
            {
                toolStripMenuSessionsRemove.Enabled =
                toolStripSeparatorSelectPacketProto.Visible =
                    //                toolStripMenuItemSelectProtocol.Visible =
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

            if (listViewPacketFilters.Items.Count + listViewSessions.Items.Count > 0)
            {
                saveFilterSelectionsToolStripMenuItem.Enabled = true;
            }
            else
            {
                saveFilterSelectionsToolStripMenuItem.Enabled = false;
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
                BeginInvoke(new MethodInvoker(delegate()
                {
                    Instance_MessageLoggedEvent(sender, e);
                }));
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

        private void SessionQueueWorker(object sender)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new MethodInvoker(() => SessionQueueWorker(sender)));
                return;
            }

            lock (QueuedSessions)
            {
                if (QueuedSessions.Count > 0)
                {
                    listViewSessions.BeginUpdate();
                    listViewSessions.Items.AddRange(QueuedSessions.ToArray());
                    listViewSessions.EndUpdate();
                    QueuedSessions.Clear();
                }
            }
        }

        private void DirectAddSession(ListViewItem item)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new MethodInvoker(() => DirectAddSession(item)));
            }
            else
            {
                listViewSessions.Items.Add(item);                
            }
        }

        private void AddSession(ListViewItem item)
        {
            if (!monoRuntime)
            {
                DirectAddSession(item);
            }
            else
            {
                lock (QueuedSessions)
                {
                    QueuedSessions.Add(item);
                }
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
        // Column sorter
        private void listViewSessions_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ListView listView1 = (ListView)sender;

            ListViewSorter Sorter = new ListViewSorter();
            listView1.ListViewItemSorter = Sorter;
            if (!(listView1.ListViewItemSorter is ListViewSorter))
                return;

            Sorter = (ListViewSorter)listView1.ListViewItemSorter;

            if (Sorter.LastSort == e.Column)
            {
                if (listView1.Sorting == SortOrder.Ascending)
                    listView1.Sorting = SortOrder.Descending;
                else
                    listView1.Sorting = SortOrder.Ascending;
            }
            else
            {
                listView1.Sorting = SortOrder.Descending;
            }
            Sorter.ByColumn = e.Column;

            listView1.Sort();

            listView1.Columns[e.Column].Width = -2;// = listView1.Columns[e.Column].Text + " " + '\u23BC';
        }

        private void buttonSaveRequestHex_Click(object sender, EventArgs e)
        {
            if (hexBoxRequest.ByteProvider != null && hexBoxRequest.ByteProvider.Length > 0)
            {
                saveFileDialog3.FileName = listViewSessions.SelectedItems[0].Name;
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
            else
            {
                // no bytes to read!
            }
        }

        private void buttonExportRawHex_Click(object sender, EventArgs e)
        {
            if (hexBoxResponse.ByteProvider != null && hexBoxResponse.ByteProvider.Length > 0)
            {
                saveFileDialog3.FileName = listViewSessions.SelectedItems[0].Name;
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
            else
            {
                // no bytes to read!
            }
        }          
    }

    public class ListViewSorter : System.Collections.IComparer
    {
        public int Compare(object o1, object o2)
        {
            if (!(o1 is ListViewItem))
                return 0;
            if (!(o2 is ListViewItem))
                return 0;

            int result;

            ListViewItem lvi1 = (ListViewItem)o2;            
            ListViewItem lvi2 = (ListViewItem)o1;

            if (lvi1.ListView.Columns[ByColumn].Tag == null
                || lvi1.ListView.Columns[ByColumn].Tag == null)
            {
                return 0;
            }

            if (lvi1.ListView.Columns[ByColumn].Tag.ToString().ToLower().Equals("number"))
            {
                float fl1 = float.Parse(lvi1.SubItems[ByColumn].Text);
                float fl2 = float.Parse(lvi2.SubItems[ByColumn].Text);

                if (lvi1.ListView.Sorting == SortOrder.Ascending)
                    result = fl1.CompareTo(fl2);
                else
                    result = fl2.CompareTo(fl1);
            }
            else if (lvi1.ListView.Columns[ByColumn].Tag.ToString().ToLower().Equals("string"))
            {
                string str1 = lvi1.SubItems[ByColumn].Text;
                string str2 = lvi2.SubItems[ByColumn].Text;

                if (lvi1.ListView.Sorting == SortOrder.Ascending)
                    result = String.Compare(str1, str2);
                else
                    result = String.Compare(str2, str1);
            }
            else
            {
                return 0;
            }

            LastSort = ByColumn;

            return (result);
        }


        public int ByColumn
        {
            get { return Column; }
            set { Column = value; }
        }
        int Column = 0;

        public int LastSort
        {
            get { return LastColumn; }
            set { LastColumn = value; }
        }
        int LastColumn = 0;
    }   
}
