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
using System.Data;
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
using OpenMetaverse.Messages.Linden;
using System.Xml;
using Nwc.XmlRpc;

namespace WinGridProxy
{
    public partial class FormWinGridProxy : Form
    {
        private static SettingsStore Store = new SettingsStore();

        private static bool IsProxyRunning = false;

        private bool AutoScrollSessions = false;

        ProxyManager proxy;


        private int PacketCounter = 0;

        private int CapsInCounter = 0;
        private int CapsInBytes = 0;
        private int CapsOutCounter = 0;
        private int CapsOutBytes = 0;

        private int PacketsInCounter = 0;
        private int PacketsInBytes;
        private int PacketsOutCounter = 0;
        private int PacketsOutBytes;

        public FormWinGridProxy()
        {
            InitializeComponent();

            ProxyManager.OnPacketLog += new ProxyManager.PacketLogHandler(ProxyManager_OnPacketLog);
            ProxyManager.OnMessageLog += new ProxyManager.MessageLogHandler(ProxyManager_OnMessageLog);
            ProxyManager.OnLoginResponse += new ProxyManager.LoginLogHandler(ProxyManager_OnLoginResponse);
            ProxyManager.OnCapabilityAdded += new ProxyManager.CapsAddedHandler(ProxyManager_OnCapabilityAdded);
            ProxyManager.OnEventMessageLog += new ProxyManager.EventQueueMessageHandler(ProxyManager_OnEventMessageLog);
        }

        #region Event Handlers for Messages/Packets

        public ListViewItem FindListViewItem(ListView listView, string key, bool searchAll)
        {
            foreach (ListViewItem item in listView.Items)
            {
                if (item.Text.Equals(key)
                    || (searchAll && item.SubItems.ContainsKey(key)))
                    return item;
            }
            return null;
        }

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
                    ListViewItem addedItem = listViewMessageFilters.Items.Add(new ListViewItem(req.Info.CapType, new ListViewGroup("EventQueue Messages")));
                    addedItem.SubItems.Add("EventMessage");
                    addedItem.BackColor = Color.AliceBlue;

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
        /// <param name="cap"></param>
        void ProxyManager_OnCapabilityAdded(CapInfo cap)
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
                ListViewItem foundCap = listViewMessageFilters.FindItemWithText(cap.CapType);
                if (foundCap == null)
                {
                    ListViewItem addedItem = listViewMessageFilters.Items.Add(new ListViewItem(cap.CapType, new ListViewGroup("Capabilities Messages")));
                    addedItem.SubItems.Add("CapMessage");
                    addedItem.BackColor = Color.Honeydew;

                    if (autoAddNewDiscoveredMessagesToolStripMenuItem.Checked)
                        addedItem.Checked = true;
                }
            }
        }

        void ProxyManager_OnPacketLog(Packet packet, Direction direction, IPEndPoint endpoint)
        {
            PacketAnalyzer_OnPacketLog(packet, direction, endpoint);
        }

        void ProxyManager_OnLoginResponse(object request, Direction direction)
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
                PacketCounter++;

                string loginType = (request is XmlRpcRequest) ? "Login Request" : "Login Response";
                ListViewItem session = new ListViewItem(new string[] { PacketCounter.ToString(), "HTTPS", loginType, request.ToString().Length.ToString(), comboBoxLoginURL.Text, "xml-rpc"});
                session.Tag = request;
                session.ImageIndex = (request is XmlRpcRequest) ? 1 : 0;
                listViewSessions.Items.Add(session);
            }
        }

        void PacketAnalyzer_OnPacketLog(Packet packet, Direction direction, IPEndPoint endpoint)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new MethodInvoker(delegate()
                {
                    PacketAnalyzer_OnPacketLog(packet, direction, endpoint);
                }));
            }
            else
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

                
                ListViewItem session = new ListViewItem(new string[] { PacketCounter.ToString(), "UDP", packet.Type.ToString(), packet.Length.ToString(), endpoint.ToString(), "binary udp"});
                session.Tag = packet;
                session.ImageIndex = (direction == Direction.Incoming) ? 0 : 1;
                listViewSessions.Items.Add(session);

                if (listViewSessions.Items.Count > 0 && AutoScrollSessions)
                    listViewSessions.EnsureVisible(listViewSessions.Items.Count - 1);
            }
        }

        void ProxyManager_OnMessageLog(CapsRequest req, CapsStage stage)
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
                    string cType = String.Empty;
                    if(req.RawRequest != null)
                    {
                        size += req.RawRequest.Length;
                        cType = req.RequestHeaders.Get("Content-Type"); //req.RequestHeaders["Content-Type"];
                    }
                    if (req.RawResponse != null)
                    {
                        size += req.RawResponse.Length;
                        cType = req.ResponseHeaders.Get("Content-Type");
                    }
                    string[] s = { PacketCounter.ToString(), found.SubItems[1].Text, req.Info.CapType, size.ToString(), req.Info.URI, cType };
                    ListViewItem session = new ListViewItem(s);
                    session.BackColor = found.BackColor;

                    session.Tag = req;

                    if (stage == CapsStage.Request)
                    {
                        CapsOutCounter++;
                        CapsOutBytes += req.Request.ToString().Length;
                        session.ImageIndex = 1;
                    }
                    else
                    {
                        CapsInCounter++;
                        CapsInBytes += req.Response.ToString().Length;
                        session.ImageIndex = 0;
                    }

                    listViewSessions.Items.Add(session);
                }
                else
                {
                    if (found == null)
                    {
                        // must be a new event not in KnownCaps, lets add it to the listview
                        ListViewItem addedItem = listViewMessageFilters.Items.Add(new ListViewItem(req.Info.CapType));
                        addedItem.BackColor = Color.AliceBlue;

                        if (autoAddNewDiscoveredMessagesToolStripMenuItem.Checked)
                            addedItem.Checked = true;
                    }
                }

                if (listViewSessions.Items.Count > 0 && AutoScrollSessions)
                    listViewSessions.EnsureVisible(listViewSessions.Items.Count - 1);
            }
        }

        #endregion

        #region GUI Event Handlers

        private void buttonStartProxy_Click(object sender, EventArgs e)
        {

            if (button1.Text.StartsWith("Start") && IsProxyRunning.Equals(false))
            {
                proxy = new ProxyManager(textBoxProxyPort.Text, textBoxProxyListenIP.Text, comboBoxLoginURL.Text);
                // disable any gui elements
                textBoxProxyListenIP.Enabled = textBoxProxyPort.Enabled = comboBoxLoginURL.Enabled = false;

                InitProxyFilters();

                proxy.Start();

                // enable any gui elements
                toolStripDropDownButton5.Enabled =
                grpUDPFilters.Enabled = grpCapsFilters.Enabled = IsProxyRunning = true;
                button1.Text = "Stop Proxy";

                if (enableStatisticsToolStripMenuItem.Checked && !timer1.Enabled)
                    timer1.Enabled = true;
            }
            else if (button1.Text.StartsWith("Stop") && IsProxyRunning.Equals(true))
            {
                // stop the proxy
                proxy.Stop();
                grpUDPFilters.Enabled = grpCapsFilters.Enabled = IsProxyRunning = false;
                button1.Text = "Start Proxy";
                textBoxProxyListenIP.Enabled = textBoxProxyPort.Enabled = comboBoxLoginURL.Enabled = true;

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

                tabControl1.SelectTab("tabPageInspect");
                object tag = e.Item.Tag;

                if (tag is XmlRpcRequest)
                {
                    XmlRpcRequest requestData = (XmlRpcRequest)tag;

                    richTextBoxDecodedRequest.Text = requestData.ToString();
                    richTextBoxRawRequest.Text = requestData.ToString();
                    richTextBoxNotationRequest.Text = "Notation Not Available for XML Request";
                    updateTreeView(requestData.ToString(), treeViewXMLRequest);
                    Be.Windows.Forms.DynamicByteProvider data = new Be.Windows.Forms.DynamicByteProvider(Utils.StringToBytes(requestData.ToString()));
                    hexBoxRequest.ByteProvider = data;

                    richTextBoxDecodedResponse.Text = String.Empty;
                    richTextBoxRawResponse.Text = String.Empty;
                    richTextBoxNotationResponse.Text = String.Empty;
                    treeViewXmlResponse.Nodes.Clear();
                    hexBoxResponse.ByteProvider = null;
                }
                else if (tag is XmlRpcResponse)
                {
                    XmlRpcResponse responseData = (XmlRpcResponse)tag;

                    richTextBoxDecodedResponse.Text = responseData.ToString();
                    richTextBoxRawResponse.Text = responseData.ToString();
                    richTextBoxNotationResponse.Text = "Notation Not Available for XML Request";
                    updateTreeView(responseData.ToString(), treeViewXmlResponse);
                    Be.Windows.Forms.DynamicByteProvider data = new Be.Windows.Forms.DynamicByteProvider(Utils.StringToBytes(responseData.ToString()));
                    hexBoxResponse.ByteProvider = data;

                    richTextBoxDecodedRequest.Text = String.Empty;
                    richTextBoxRawRequest.Text = String.Empty;
                    richTextBoxNotationRequest.Text = String.Empty;
                    treeViewXMLRequest.Nodes.Clear();
                    hexBoxRequest.ByteProvider = null;

                }
                else if (tag is Packet)
                {
                    Packet packet = (Packet)tag;

                    Be.Windows.Forms.DynamicByteProvider data = new Be.Windows.Forms.DynamicByteProvider(packet.ToBytes());
                    // we have no conversion from Packet to xml or notation
                    richTextBoxNotationRequest.Text = String.Empty;
                    richTextBoxNotationResponse.Text = String.Empty;
                    treeViewXmlResponse.Nodes.Clear();
                    treeViewXMLRequest.Nodes.Clear();

                    // 0 = incoming, 1 = outgoing
                    if (e.Item.ImageIndex == 0)
                    {
                        richTextBoxDecodedResponse.Text = TagToString(tag, listViewSessions.FocusedItem.SubItems[2].Text);
                        richTextBoxRawResponse.Text = TagToString(tag, listViewSessions.FocusedItem.SubItems[2].Text);
                        richTextBoxNotationResponse.Text = "Notation Not Available for Packet Types";
                        hexBoxResponse.ByteProvider = data;

                        richTextBoxDecodedRequest.Text = String.Empty;
                        richTextBoxRawRequest.Text = String.Empty;
                        hexBoxRequest.ByteProvider = null;
                    } 
                    else 
                    {
                        richTextBoxDecodedRequest.Text = TagToString(tag, listViewSessions.FocusedItem.SubItems[2].Text);
                        richTextBoxRawRequest.Text = TagToString(tag, listViewSessions.FocusedItem.SubItems[2].Text);
                        richTextBoxNotationRequest.Text = "Notation Not Available for Packet Types";
                        hexBoxRequest.ByteProvider = data;

                        richTextBoxDecodedResponse.Text = String.Empty;
                        richTextBoxRawResponse.Text = String.Empty;
                        hexBoxResponse.ByteProvider = null;
                    }
                    
                }
                else if (tag is CapsRequest)
                {
                    CapsRequest capsData = (CapsRequest)tag;

                    if (capsData.Request != null)
                    {
                        StringBuilder rawRequest = new StringBuilder();

                        if (capsData.RequestHeaders != null)
                        {
                            foreach (string key in capsData.RequestHeaders.Keys)
                            {
                                rawRequest.AppendFormat("{0}: {1}" + System.Environment.NewLine, key, capsData.RequestHeaders[key]);
                            }
                            rawRequest.AppendLine();
                        }
                        rawRequest.AppendLine(Utils.BytesToString(capsData.RawRequest));

                        OSD requestOSD = OSDParser.DeserializeLLSDXml(capsData.RawRequest);

                        richTextBoxDecodedRequest.Text = TagToString(requestOSD, listViewSessions.FocusedItem.SubItems[2].Text);//.ToString();
                        richTextBoxRawRequest.Text = rawRequest.ToString();
                        richTextBoxNotationRequest.Text = requestOSD.ToString();
                        updateTreeView(Utils.BytesToString(capsData.RawRequest), treeViewXMLRequest);
                        Be.Windows.Forms.DynamicByteProvider data = new Be.Windows.Forms.DynamicByteProvider(capsData.RawRequest);
                        hexBoxRequest.ByteProvider = data;
                    }
                    else
                    {
                        richTextBoxDecodedRequest.Text = "No Data";
                        richTextBoxRawRequest.Text = "No Data";
                        richTextBoxNotationRequest.Text = "No Data";
                        treeViewXMLRequest.Nodes.Clear();
                        hexBoxRequest.ByteProvider = null;

                    }

                    if (capsData.Response != null)
                    {
                        StringBuilder rawResponse = new StringBuilder();

                        if (capsData.ResponseHeaders != null)
                        {
                            foreach (string key in capsData.ResponseHeaders.Keys)
                            {
                                rawResponse.AppendFormat("{0}: {1}" + System.Environment.NewLine, key, capsData.ResponseHeaders[key]);
                            }
                            rawResponse.AppendLine();
                        }
                        rawResponse.AppendLine(Utils.BytesToString(capsData.RawResponse));

                        OSD responseOSD = OSDParser.DeserializeLLSDXml(capsData.RawResponse);

                        richTextBoxDecodedResponse.Text = TagToString(responseOSD, listViewSessions.FocusedItem.SubItems[2].Text);//.ToString();
                        richTextBoxRawResponse.Text = rawResponse.ToString();
                        richTextBoxNotationResponse.Text = responseOSD.ToString();
                        updateTreeView(Utils.BytesToString(capsData.RawResponse), treeViewXmlResponse);
                        Be.Windows.Forms.DynamicByteProvider data = new Be.Windows.Forms.DynamicByteProvider(capsData.RawResponse);
                        hexBoxResponse.ByteProvider = data;
                    }
                    else
                    {
                        richTextBoxDecodedResponse.Text = "No Data";
                        richTextBoxRawResponse.Text = "No Data";
                        richTextBoxNotationResponse.Text = "No Data";
                        treeViewXmlResponse.Nodes.Clear();
                        hexBoxResponse.ByteProvider = null;
                    }
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (IsProxyRunning)
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
                labelRequestHex.Text = string.Format("Ln {0}    Col {1}    bytes {2}",
                    hexBoxRequest.CurrentLine, hexBoxRequest.CurrentPositionInLine, hexBoxRequest.ByteProvider.Length);
            }
        }

        // Update Response Hexbox status bar with current cursor location
        void ReplyPosition_Changed(object sender, EventArgs e)
        {
            if (hexBoxResponse.ByteProvider != null)
            {
                labelResponseHex.Text = string.Format("Ln {0}    Col {1}    bytes {2}",
                    hexBoxResponse.CurrentLine, hexBoxResponse.CurrentPositionInLine, hexBoxResponse.ByteProvider.Length);
            }
        }

        /// <summary>Enable or Disable Autoscrolling of the session list, Updates the Preferences and context menus</summary>
        /// <param name="sender">The ToolStripMenuItem sending the event</param>
        /// <param name="e"></param>
        private void sessionEnableAutoScroll_CheckedChanged(object sender, EventArgs e)
        {
            ToolStripMenuItem autoscroll = (ToolStripMenuItem)sender;
            AutoScrollSessions = autoScrollSessionsToolStripMenuItem.Checked = toolStripMenuItemAutoScroll.Checked = autoscroll.Checked;
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

        //private void sessionSelectAllProtocol_Click(object sender, EventArgs e)
        //{
        //    foreach (ListViewItem item in listViewSessions.Items)
        //    {
        //        if (item.SubItems[1].Text.Equals(toolStripMenuItemSelectProtocol.Tag) && !item.Selected)
        //            item.Selected = true;
        //    }
        //}

        // stop capturing selected filters
        private void filterDisableByPacketName_CheckedChanged(object sender, EventArgs e)
        {
            if (enableDisableFilterByNameToolStripMenuItem.Tag != null)
            {
                ListViewItem found = listViewMessageFilters.FindItemWithText(enableDisableFilterByNameToolStripMenuItem.Tag.ToString());

                if (found != null)
                {
                    listViewMessageFilters.Items[found.Index].Checked = enableDisableFilterByNameToolStripMenuItem.Checked;
                }
                else
                {
                    found = listViewPacketFilters.FindItemWithText(enableDisableFilterByNameToolStripMenuItem.Tag.ToString());

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
                string strPacketOrMessage = (listViewSessions.FocusedItem.SubItems[1].Text.Equals("UDP")) ? "Packets" : "Messages";

                enableDisableFilterByNameToolStripMenuItem.Text = String.Format("Capture {0} {1}", listViewSessions.FocusedItem.SubItems[2].Text, strPacketOrMessage);
                toolStripMenuItemSelectPacketName.Tag = enableDisableFilterByNameToolStripMenuItem.Tag = listViewSessions.FocusedItem.SubItems[2].Text;

                toolStripMenuItemSelectPacketName.Text = String.Format("All {0} {1}", listViewSessions.FocusedItem.SubItems[2].Text, strPacketOrMessage);

                //                toolStripMenuItemSelectProtocol.Text = String.Format("All {0} {1}", listViewSessions.FocusedItem.SubItems[1].Text, strPacketOrMessage);

                //              toolStripMenuItemSelectProtocol.Visible = 
                enableDisableFilterByNameToolStripMenuItem.Visible =
                toolStripSeparatorSelectPacketProto.Visible =
                toolStripSeparatorFilterPacketByName.Visible =
                toolStripMenuItemSelectPacketName.Visible = true;

                // find checkstate of selected menuitem in packets or messages filters checkedListBoxes
                bool ctxChecked = false;

                if (strPacketOrMessage.Equals("Packets"))
                {
                    ListViewItem found = listViewPacketFilters.FindItemWithText(toolStripMenuItemSelectPacketName.Tag.ToString());
                    if (found != null)
                        ctxChecked = found.Checked;
                }
                else if (strPacketOrMessage.Equals("Messages"))// && listViewMessageFilters.Items.ContainsKey(toolStripMenuItemSelectPacketName.Tag.ToString()))
                {
                    ListViewItem found = listViewMessageFilters.FindItemWithText(toolStripMenuItemSelectPacketName.Tag.ToString());
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
            }
        }

        private void listViewMessageFilters_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            proxy.AddCapsDelegate(e.Item.Text, e.Item.Checked);
        }

        private void listViewPacketFilters_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            proxy.AddUDPDelegate(packetTypeFromName(e.Item.Text), e.Item.Checked);
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
            Stream myStream;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if ((myStream = saveFileDialog1.OpenFile()) != null)
                {
                    StreamWriter wText = new StreamWriter(myStream);
                    OSDMap map = new OSDMap(1);
                    OSDArray sessionArray = new OSDArray();
                    foreach (ListViewItem item in listViewSessions.Items)
                    {
                        OSDMap session = new OSDMap();
                        session["name"] = OSD.FromString(item.Name);
                        session["image_index"] = OSD.FromInteger(item.ImageIndex);
                        session["id"] = OSD.FromString(item.SubItems[0].Text);
                        session["protocol"] = OSD.FromString(item.SubItems[1].Text);
                        session["packet"] = OSD.FromString(item.SubItems[2].Text);
                        session["size"] = OSD.FromString(item.SubItems[3].Text);
                        session["host"] = OSD.FromString(item.SubItems[4].Text);
                        session["tag"] = OSD.FromObject(item.Tag);
                        sessionArray.Add(session);
                    }

                    map["sessions"] = sessionArray;
                    wText.Write(map.ToString());
                    wText.Flush();

                    myStream.Close();
                }
            }
        }

        private void loadSessionArchiveToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                OSD osd = OSDParser.DeserializeLLSDNotation(File.ReadAllText(openFileDialog1.FileName));
                OSDMap map = (OSDMap)osd;
                OSDArray sessionsArray = (OSDArray)map["sessions"];

                listViewSessions.Items.Clear();
                listViewSessions.BeginUpdate();
                for (int i = 0; i < sessionsArray.Count; i++)
                {
                    OSDMap session = (OSDMap)sessionsArray[i];
                    ListViewItem addedItem = listViewSessions.Items.Add(new ListViewItem(new string[] {
                        session["id"].AsString(), 
                        session["protocol"].AsString(),
                        session["packet"].AsString(),
                        session["size"].AsString(),
                        session["host"].AsString()}));

                    addedItem.ImageIndex = session["image_index"].AsInteger();
                    addedItem.Tag = session["tag"].ToString();
                }

                listViewSessions.EndUpdate();
            }
        }

        //Generic ListView sort event
        private void listViewFilterSorter_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ListView lv = (ListView)sender;
            //this.listViewPacketFilters.ListViewItemSorter = new ListViewItemComparer(e.Column);
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
            // warn if connected!
            this.Close();
        }

        #region Helpers

        /// <summary>
        /// Decode an IMessage object into a string of key/value pairs
        /// </summary>
        /// <param name="message">The IMessage object</param>
        /// <returns>A formatted string containing the names and values of the source object</returns>
        public static string IMessageToString(IMessage message)
        {
            StringBuilder result = new StringBuilder();
            // common/custom types
            result.AppendFormat("Message Type {0}" + System.Environment.NewLine, message.GetType().Name);

            foreach (FieldInfo messageField in message.GetType().GetFields())
            {
                // a byte array
                if (messageField.GetValue(message).GetType() == typeof(Byte[]))
                {
                    result.AppendFormat("{0, 30}: ({1})" + System.Environment.NewLine,
                    messageField.Name, messageField.FieldType.Name);
                    result.AppendFormat("{0}" + System.Environment.NewLine, Utils.BytesToHexString((byte[])messageField.GetValue(message), string.Format("{0,30}", "")));
                }

                // an array of class objects
                else if (messageField.FieldType.IsArray)
                {
                    object messageObjectData = messageField.GetValue(message);
                    result.AppendFormat("-- {0} --" + System.Environment.NewLine, messageField.FieldType.Name);
                    foreach (object nestedArrayObject in messageObjectData as Array)
                    {
                        result.AppendFormat("     [{0}]" + System.Environment.NewLine, nestedArrayObject.GetType().Name);

                        foreach (FieldInfo nestedField in nestedArrayObject.GetType().GetFields())
                        {
                            var nt = nestedField.GetValue(nestedArrayObject).GetType().Name;
                            if (nestedField.FieldType.IsEnum)
                            {
                                result.AppendFormat("{0, 30}: {1} {2} ({3})" + System.Environment.NewLine,
                                    nestedField.Name,
                                    Enum.Format(nestedField.GetValue(nestedArrayObject).GetType(),
                                    nestedField.GetValue(nestedArrayObject), "D"),
                                    nestedField.GetValue(nestedArrayObject),
                                    nestedField.GetValue(nestedArrayObject).GetType().Name);
                            }
                            else
                            {
                                result.AppendFormat("{0, 30}: {1} ({2})" + System.Environment.NewLine,
                                 nestedField.Name,
                                 nestedField.GetValue(nestedArrayObject),
                                 nestedField.GetValue(nestedArrayObject).GetType().Name);
                            }
                        }
                    }
                }
                else
                {
                    if (messageField.FieldType.IsEnum)
                    {
                        result.AppendFormat("{0, 30}: {1} {2} ({3})" + System.Environment.NewLine,
                            messageField.Name,
                            Enum.Format(messageField.GetValue(message).GetType(),
                            messageField.GetValue(message), "D"),
                            messageField.GetValue(message), messageField.FieldType.Name);
                    }
                    else
                    {
                        result.AppendFormat("{0, 30}: {1} ({2})" + System.Environment.NewLine,
                        messageField.Name, messageField.GetValue(message), messageField.FieldType.Name);
                    }
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Creates a formatted string containing the values of a Packet
        /// </summary>
        /// <param name="packet">The Packet</param>
        /// <returns>A formatted string of values of the nested items in the Packet object</returns>
        public static string PacketToString(Packet packet)
        {
            StringBuilder result = new StringBuilder();

            result.AppendFormat("Packet Type: {0}" + System.Environment.NewLine, packet.Type);

            foreach (FieldInfo packetField in packet.GetType().GetFields())
            {
                object packetDataObject = packetField.GetValue(packet);

                result.AppendFormat("-- {0,20} --" + System.Environment.NewLine, packetField.Name);
                foreach (FieldInfo packetValueField in packetField.GetValue(packet).GetType().GetFields())
                {
                    result.AppendFormat("{0,30}: {1}" + System.Environment.NewLine,
                        packetValueField.Name, packetValueField.GetValue(packetDataObject));
                }

                // handle blocks that are arrays
                if (packetDataObject.GetType().IsArray)
                {
                    foreach (object nestedArrayRecord in packetDataObject as Array)
                    {
                        foreach (FieldInfo packetArrayField in nestedArrayRecord.GetType().GetFields())
                        {
                            if (packetArrayField.GetValue(nestedArrayRecord).GetType() == typeof(System.Byte[]))
                            {
                                result.AppendFormat("{0,30}: {1}" + System.Environment.NewLine,
                                    packetArrayField.Name,
                                    new Color4((byte[])packetArrayField.GetValue(nestedArrayRecord), 0, false).ToString());
                            }
                            else
                            {
                                result.AppendFormat("{0,30}: {1}" + System.Environment.NewLine,
                                    packetArrayField.Name, packetArrayField.GetValue(nestedArrayRecord));
                            }
                        }
                    }
                }
                else
                {
                    // handle non array data blocks
                    foreach (PropertyInfo packetPropertyField in packetField.GetValue(packet).GetType().GetProperties())
                    {
                        // Handle fields named "Data" specifically, this is generally binary data, we'll display it as hex values
                        if (packetPropertyField.PropertyType.Equals(typeof(System.Byte[]))
                            && packetPropertyField.Name.Equals("Data"))
                        {
                            result.AppendFormat("{0}" + System.Environment.NewLine,
                                Utils.BytesToHexString((byte[])packetPropertyField.GetValue(packetDataObject, null),
                                packetPropertyField.Name));
                        }
                        // decode bytes into strings
                        else if (packetPropertyField.PropertyType.Equals(typeof(System.Byte[])))
                        {
                            // Handle TextureEntry fields specifically
                            if (packetPropertyField.Name.Equals("TextureEntry"))
                            {
                                byte[] tebytes = (byte[])packetPropertyField.GetValue(packetDataObject, null);

                                Primitive.TextureEntry te = new Primitive.TextureEntry(tebytes, 0, tebytes.Length);
                                result.AppendFormat("{0,30}:\n{1}", packetPropertyField.Name, te.ToString());
                            }
                            else
                            {
                                result.AppendFormat("{0,30}: {1} [{2}]" + System.Environment.NewLine,
                                    packetPropertyField.Name,
                                    Utils.BytesToString((byte[])packetPropertyField.GetValue(packetDataObject, null)),
                                    packetDataObject.GetType());
                            }
                        }
                        else
                        {
                            // this seems to be limited to the length property, since all others have been previously handled
                            if (packetPropertyField.Name != "Length")
                            {
                                result.AppendFormat("{0,30}: {1} [{2}]" + System.Environment.NewLine,
                                    packetPropertyField.Name, packetPropertyField.GetValue(packetDataObject, null),
                                    packetPropertyField.GetType());
                            }
                        }
                    }
                }
            }
            return result.ToString();
        }

        private void SaveAllSettings(string fileName)
        {
            Store.MessageSessions.Clear();
            Store.PacketSessions.Clear();

            foreach (ListViewItem item in listViewPacketFilters.Items)
            {
                FilterEntry entry = new FilterEntry();
                entry.Checked = item.Checked;
                entry.pType = item.SubItems[1].Text;
                Store.PacketSessions.Add(item.Text, entry);
            }

            foreach (ListViewItem item in listViewMessageFilters.Items)
            {
                FilterEntry entry = new FilterEntry();
                entry.Checked = item.Checked;
                entry.pType = item.SubItems[1].Text;
                Store.MessageSessions.Add(item.Text, entry);
            }

            Store.StatisticsEnabled = enableStatisticsToolStripMenuItem.Checked;
            Store.AutoScrollEnabled = autoScrollSessionsToolStripMenuItem.Checked;
            Store.SaveSessionOnExit = saveOptionsOnExitToolStripMenuItem.Checked;
            Store.AutoCheckNewCaps = autoAddNewDiscoveredMessagesToolStripMenuItem.Checked;

            Store.SerializeToFile(fileName);
        }

        private void RestoreSavedSettings(string fileName)
        {
            // load saved settings from OSD Formatted file

            if (Store.DeserializeFromFile(fileName))
            {
                autoScrollSessionsToolStripMenuItem.Checked = Store.AutoScrollEnabled;
                enableStatisticsToolStripMenuItem.Checked = Store.StatisticsEnabled;
                saveOptionsOnExitToolStripMenuItem.Checked = Store.SaveSessionOnExit;
                autoAddNewDiscoveredMessagesToolStripMenuItem.Checked = Store.AutoCheckNewCaps;

                // Update message filter listview
                listViewMessageFilters.BeginUpdate();
                foreach (KeyValuePair<string, FilterEntry> kvp in Store.MessageSessions)
                {
                    ListViewItem foundMessage = listViewPacketFilters.FindItemWithText(kvp.Key);
                    if (foundMessage == null)
                    {
                        ListViewItem addedItem = listViewMessageFilters.Items.Add(kvp.Key);
                        addedItem.Checked = kvp.Value.Checked;
                        addedItem.SubItems.Add(kvp.Value.pType);
                        addedItem.BackColor = (kvp.Value.pType.Equals("CapMessage")) ? Color.Honeydew : Color.AliceBlue;
                    }
                    else
                    {
                        foundMessage.Checked = kvp.Value.Checked;
                    }
                }
                listViewMessageFilters.EndUpdate();

                // updateTreeView packet filter listview
                listViewPacketFilters.BeginUpdate();
                foreach (KeyValuePair<string, FilterEntry> kvp in Store.PacketSessions)
                {
                    ListViewItem foundPacket = listViewPacketFilters.FindItemWithText(kvp.Key);
                    if (foundPacket == null)
                    {
                        ListViewItem addedItem = listViewPacketFilters.Items.Add(new ListViewItem(kvp.Key));
                        addedItem.Checked = kvp.Value.Checked;
                        addedItem.SubItems.Add(kvp.Value.pType);
                    }
                    else
                    {
                        foundPacket.Checked = kvp.Value.Checked;
                    }
                }
                listViewPacketFilters.EndUpdate();
            }
        }

        private void InitProxyFilters()
        {
            RestoreSavedSettings("settings.osd");

            Type packetTypeType = typeof(PacketType);
            System.Reflection.MemberInfo[] packetTypes = packetTypeType.GetMembers();

            listViewPacketFilters.BeginUpdate();
            for (int i = 0; i < packetTypes.Length; i++)
            {
                if (packetTypes[i].MemberType == System.Reflection.MemberTypes.Field
                    && packetTypes[i].DeclaringType == packetTypeType)
                {

                    string name = packetTypes[i].Name;

                    PacketType pType;

                    try
                    {
                        pType = packetTypeFromName(name);
                        ListViewItem found = listViewPacketFilters.FindItemWithText(name);
                        if (!String.IsNullOrEmpty(name) && found == null)
                        {
                            ListViewItem addedItem = listViewPacketFilters.Items.Add(new ListViewItem(name));
                            addedItem.SubItems.Add("UDP");
                        }
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }

            //listViewPacketFilters.Sort();

            listViewPacketFilters.EndUpdate();
        }

        private static PacketType packetTypeFromName(string name)
        {
            Type packetTypeType = typeof(PacketType);
            System.Reflection.FieldInfo f = packetTypeType.GetField(name);
            if (f == null) throw new ArgumentException("Bad packet type");

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
                            || TagToString(item.Tag, item.SubItems[2].Text).Contains(opts.SearchText))
                        ) // no case matching
                        || ((item.SubItems[2].Text.ToLower().Contains(opts.SearchText.ToLower())
                            || TagToString(item.Tag, item.SubItems[2].Text).ToLower().Contains(opts.SearchText.ToLower())
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

                toolStripMainLabel.Text = String.Format("Search found {0} Matches", resultCount);
            }
        }

        private string TagToString(object tag, string key)
        {
            if (tag is XmlRpcRequest)
            {
                XmlRpcRequest requestData = (XmlRpcRequest)tag;
                return requestData.ToString();
            }
            else if (tag is XmlRpcResponse)
            {
                XmlRpcResponse responseData = (XmlRpcResponse)tag;

                return responseData.ToString();
            }
            else if (tag is Packet)
            {
                Packet packet = (Packet)tag;

                return PacketToString(packet);
            }
            else if (tag is CapsRequest)
            {
                CapsRequest capsData = (CapsRequest)tag;

                if (capsData.Request != null)
                {
                    return capsData.Request.ToString();
                }

                if (capsData.Response != null)
                {
                    return capsData.Response.ToString();
                }
                return "Unable to decode CapsRequest";
            }
            else if (tag is OSD)
            {
                OSD osd = (OSD)tag;
                if (osd.Type == OSDType.Map)
                {
                    OSDMap data = (OSDMap)osd;
                    IMessage message;
                    if (data.ContainsKey("body"))
                        message = Caps.DecodeEvent(key, (OSDMap)data["body"]);
                    else
                        message = Caps.DecodeEvent(key, data);

                    if (message != null)
                        return IMessageToString(message);
                    else
                        return "No Decoder for " + key + System.Environment.NewLine
                            + osd.ToString();
                }
                else
                {
                    return osd.ToString();
                }
            }
            else
            {
                return "Could not decode object type: " + tag.GetType().ToString();
            }
        }

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
    }
}
