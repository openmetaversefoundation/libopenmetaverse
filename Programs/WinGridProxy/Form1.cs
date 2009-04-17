using System;
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
using Nwc.XmlRpc;
using System.Xml;

namespace WinGridProxy
{
    public partial class Form1 : Form
    {
        private static SettingsStore Store = new SettingsStore();

        private static bool IsProxyRunning = false;

        ProxyManager proxy;
        private Assembly openmvAssembly;

        private int PacketCounter = 0;
        
        private int CapsInCounter = 0;
        private int CapsInBytes = 0;
        private int CapsOutCounter = 0;
        private int CapsOutBytes = 0;

        private int PacketsInCounter = 0;
        private int PacketsInBytes;
        private int PacketsOutCounter = 0;
        private int PacketsOutBytes;

        public Form1()
        {
            InitializeComponent();

            ProxyManager.OnPacketLog += new ProxyManager.PacketLogHandler(ProxyManager_OnPacketLog);
            ProxyManager.OnMessageLog += new ProxyManager.MessageLogHandler(ProxyManager_OnMessageLog);
            ProxyManager.OnLoginResponse += new ProxyManager.LoginLogHandler(ProxyManager_OnLoginResponse);
            openmvAssembly = Assembly.Load("OpenMetaverse");
            if (openmvAssembly == null) throw new Exception("Assembly load exception");

            //Store.DeserializeFromFile("settings.osd");

            //InitProxyFilters();
            
        }

        private void InitProxyFilters()
        {
            Store.DeserializeFromFile("settings.osd");

            Type packetTypeType = typeof(PacketType);
            System.Reflection.MemberInfo[] packetTypes = packetTypeType.GetMembers();
            checkedListBoxFiltersPackets.BeginUpdate();
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
                        checkedListBoxFiltersPackets.Items.Add(name, false);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }
            checkedListBoxFiltersPackets.Sorted = true;


            // load from previous stored settings
            

            checkedListBoxFiltersPackets.EndUpdate();

            foreach (KeyValuePair<string, bool> kvp in Store.MessageSessions)
            {
                checkedListBoxFiltersMessages.Items.Add(kvp.Key, kvp.Value);
            }

        }

        private static PacketType packetTypeFromName(string name)
        {
            Type packetTypeType = typeof(PacketType);
            System.Reflection.FieldInfo f = packetTypeType.GetField(name);
            if (f == null) throw new ArgumentException("Bad packet type");

            return (PacketType)Enum.ToObject(packetTypeType, (int)f.GetValue(packetTypeType));
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

                string t = (request is XmlRpcRequest) ? "Login Request" : "Login Response";
                string l = request.ToString().Length.ToString();
                string[] s = { PacketCounter.ToString(), "HTTPS", t, l, textBoxLoginURL.Text };
                ListViewItem session = new ListViewItem(s);
                session.Tag = request;
                session.ImageIndex = (request is XmlRpcRequest) ? 1 : 0;

                listViewSessions.Items.Add(session);

                // TODO: this needs to refresh the Capabilities filters
                if (request is XmlRpcResponse)
                {
                    XmlRpcResponse r = (XmlRpcResponse)request;
                    if (!r.IsFault)
                    {
                        Hashtable ht = (Hashtable)r.Value;
                        string st = String.Empty;

                        if (ht.ContainsKey("login"))
                        {
                            if ((string)ht["login"] == "true")
                            {
                                
                                Console.WriteLine("Refresh");
                                buttonRefreshCapsList_Click(this, new EventArgs());
                            }

                        }
                    }
                }
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
                
                string[] s = { PacketCounter.ToString(), "UDP", packet.Type.ToString(), packet.Length.ToString(), endpoint.ToString() };
                ListViewItem session = new ListViewItem(s);
                
                session.Tag = packet;

                if (direction == Direction.Incoming)
                {
                    session.ImageIndex = 0; 
                    PacketsInCounter++;
                    PacketsInBytes += packet.Length;
                }
                else
                {
                    session.ImageIndex = 1;
                    PacketsOutCounter++;
                    PacketsOutBytes += packet.Length;
                }

                listViewSessions.Items.Add(session);
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
                PacketCounter++;

                string size = (stage == CapsStage.Request) ? req.Request.ToString().Length.ToString() : req.Response.ToString().Length.ToString();
                string[] s = { PacketCounter.ToString(), "CAPS", req.Info.CapType, size, req.Info.URI };
                ListViewItem session = new ListViewItem(s);
                
                session.Tag = req;

                if (stage == CapsStage.Request)
                {
                    CapsOutCounter++;
                    CapsOutBytes += req.Request.ToString().Length;
                    session.ImageIndex = 0;
                }
                else
                {
                    CapsInCounter++;
                    CapsInBytes += req.Response.ToString().Length;
                    session.ImageIndex = 1;
                }

                listViewSessions.Items.Add(session);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            

            if (button1.Text.StartsWith("Start") && IsProxyRunning.Equals(false))
            {
                

                proxy = new ProxyManager(textBoxProxyPort.Text, textBoxProxyListenIP.Text, textBoxLoginURL.Text);
                // start the proxy
                textBoxProxyListenIP.Enabled = textBoxProxyPort.Enabled = textBoxLoginURL.Enabled = false;

                InitProxyFilters();

                proxy.Start();
                grpUDPFilters.Enabled = grpCapsFilters.Enabled = IsProxyRunning = true;
                button1.Text = "Stop Proxy";

                if (!timer1.Enabled)
                    timer1.Enabled = true;
                proxy.AddCapsDelegate("ParcelProperties", true);
                proxy.AddCapsDelegate("AgentGroupDataUpdate", true);
            }
            else if (button1.Text.StartsWith("Stop") && IsProxyRunning.Equals(true))
            {
                // stop the proxy
                proxy.Stop();
                grpUDPFilters.Enabled = grpCapsFilters.Enabled = IsProxyRunning = false;
                button1.Text = "Start Proxy";
                textBoxProxyListenIP.Enabled = textBoxProxyPort.Enabled = textBoxLoginURL.Enabled = true;

                if (timer1.Enabled)
                    timer1.Enabled = false;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBoxFiltersPackets.Items.Count; i++)
            {
                checkedListBoxFiltersPackets.SetItemChecked(i, checkBox1.Checked);
            }
        }

        private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.CurrentValue != e.NewValue)
            {
                proxy.AddUDPDelegate(packetTypeFromName(checkedListBoxFiltersPackets.Items[e.Index].ToString()), (e.NewValue == CheckState.Checked));
            }
        }

        private void buttonRefreshCapsList_Click(object sender, EventArgs e)
        {
            OSDMap map = new OSDMap(1);

            OSDArray capsArray = new OSDArray();
            foreach (KeyValuePair<string, CapInfo> kvp in proxy.GetCapabilities())
            {
                OSDMap cap = new OSDMap(1);
                cap["capability"] = OSD.FromString(kvp.Value.CapType);
                cap["Enabled"] = OSD.FromBoolean(true);
                capsArray.Add(cap);
                checkedListBoxFiltersMessages.BeginUpdate();
                if (!checkedListBoxFiltersMessages.Items.Contains(kvp.Value.CapType))
                {
                    checkedListBoxFiltersMessages.Items.Add(kvp.Value.CapType);
                }
                checkedListBoxFiltersMessages.Sorted = true;
                checkedListBoxFiltersMessages.EndUpdate();
            }
            map["Capabilities"] = capsArray;

            System.IO.File.WriteAllText("capabilities.osd", map.ToString());

        }

        private void checkBoxCheckallCaps_CheckedChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBoxFiltersMessages.Items.Count; i++)
            {
                checkedListBoxFiltersMessages.SetItemChecked(i, checkBox2.Checked);
            }
        }

        private void checkedListBoxCaps_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.CurrentValue != e.NewValue)
            {
                proxy.AddCapsDelegate(checkedListBoxFiltersMessages.Items[e.Index].ToString(), (e.NewValue == CheckState.Checked));
            }
        }

        // This is from omv Utils, once we get it prettied up put it back there
        public static string PacketToString(Packet packet)
        {
            StringBuilder result = new StringBuilder();

            //result.AppendFormat("{0}" + System.Environment.NewLine, packet.Type);

            foreach (FieldInfo packetField in packet.GetType().GetFields())
            {
                object packetDataObject = packetField.GetValue(packet);

                result.AppendFormat("-- {0,30} --" + System.Environment.NewLine, packetField.Name);
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
                                    packetArrayField.Name, new Color4((byte[])packetArrayField.GetValue(nestedArrayRecord), 0, false).ToString());// c4.ToString());                                
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
                            result.AppendFormat("{0,5}: {1}" + System.Environment.NewLine,
                                packetPropertyField.Name,
                                Utils.BytesToHexString((byte[])packetPropertyField.GetValue(packetDataObject, null), packetPropertyField.Name));
                        }
                        // decode bytes into strings
                        else if (packetPropertyField.PropertyType.Equals(typeof(System.Byte[])))
                        {
                            result.AppendFormat("{0,30}: {1}" + System.Environment.NewLine,
                                packetPropertyField.Name,
                                Utils.BytesToString((byte[])packetPropertyField.GetValue(packetDataObject, null)));
                        }
                        else
                        {
                            // this seems to be limited to the length property, since all others have been previously handled
                            if (packetPropertyField.Name != "Length")
                            {
                                result.AppendFormat("{0,30}: {1}" + System.Environment.NewLine,
                                    packetPropertyField.Name, packetPropertyField.GetValue(packetDataObject, null));
                            }
                        }
                    }
                }
            }
            return result.ToString();
        }

        private void listViewSessions_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.IsSelected)
            {
                tabControl1.SelectTab("tabPageInspect");
                object tag = e.Item.Tag;
           
                if (tag is XmlRpcRequest)
                {
                    XmlRpcRequest requestData = (XmlRpcRequest)tag;

                    richTextBoxRawLogRequest.Text = requestData.ToString();
                    updateTreeView(requestData.ToString(), treeViewRequestXml);

                    Be.Windows.Forms.DynamicByteProvider data = new Be.Windows.Forms.DynamicByteProvider(Utils.StringToBytes(requestData.ToString()));
                    hexBoxRequest.ByteProvider = data;

                    hexBoxResponse.ByteProvider = null;
                    richTextBoxRawLogResponse.Text = String.Empty;
                    treeViewResponseXml.Nodes.Clear();
                }
                else if (tag is XmlRpcResponse)
                {
                    XmlRpcResponse responseData = (XmlRpcResponse)tag;

                    richTextBoxRawLogResponse.Text = responseData.ToString();
                    updateTreeView(responseData.ToString(), treeViewResponseXml);
                    
                    Be.Windows.Forms.DynamicByteProvider data = new Be.Windows.Forms.DynamicByteProvider(Utils.StringToBytes(responseData.ToString()));
                    hexBoxResponse.ByteProvider = data;

                    hexBoxRequest.ByteProvider = null;
                    richTextBoxRawLogRequest.Text = "No Data";
                    treeViewRequestXml.Nodes.Clear();
                }
                else if (tag is Packet)
                {
                    Packet packet = (Packet)tag;
                    
                    richTextBoxRawLogResponse.Text = PacketToString(packet);
                    
                    Be.Windows.Forms.DynamicByteProvider data = new Be.Windows.Forms.DynamicByteProvider(packet.ToBytes());
                    hexBoxResponse.ByteProvider = data;
                }
                else if (tag is CapsRequest)
                {
                    CapsRequest capsData = (CapsRequest)tag;

                    if (capsData.Request != null)
                    {
                        richTextBoxRawLogRequest.Text = capsData.Request.ToString();
                        updateTreeView(OSDParser.SerializeLLSDXmlString(capsData.Request), treeViewRequestXml);
                        Be.Windows.Forms.DynamicByteProvider data = new Be.Windows.Forms.DynamicByteProvider(Utils.StringToBytes(capsData.Request.ToString()));
                        hexBoxRequest.ByteProvider = data;
                    }
                    else
                    {
                        richTextBoxRawLogRequest.Text = "No Data";
                        treeViewRequestXml.Nodes.Clear();
                        hexBoxRequest.ByteProvider = null;
                    }

                    if (capsData.Response != null)
                    {
                        richTextBoxRawLogResponse.Text = capsData.Response.ToString();
                        updateTreeView(OSDParser.SerializeLLSDXmlString(capsData.Response), treeViewResponseXml);
                        Be.Windows.Forms.DynamicByteProvider data = new Be.Windows.Forms.DynamicByteProvider(Utils.StringToBytes(capsData.Response.ToString()));
                        hexBoxResponse.ByteProvider = data;
                    } else {
                        richTextBoxRawLogResponse.Text = "No Data";
                        treeViewResponseXml.Nodes.Clear();
                        hexBoxResponse.ByteProvider = null;
                    }
                }
            }
        }

        private void updateTreeView(string xml, TreeView treeView)
        {
            try
            {
                treeViewResponseXml.Nodes.Clear();
                
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

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (IsProxyRunning)
                proxy.Stop();

            SaveAllSettings("settings.osd");
        }

        private void SaveAllSettings(string fileName)
        {
            Store.MessageSessions.Clear();
            Store.PacketSessions.Clear();

            for (int i = 0; i < checkedListBoxFiltersMessages.Items.Count; i++)
            {
                bool cbchecked = false;
                if (checkedListBoxFiltersMessages.CheckedItems.Contains(checkedListBoxFiltersMessages.Items[i]))
                    cbchecked = true;

                Store.MessageSessions.Add(checkedListBoxFiltersMessages.Items[i].ToString(), cbchecked);
            }

            for (int i = 0; i < checkedListBoxFiltersPackets.Items.Count; i++)
            {
                bool cbchecked = false;
                if (checkedListBoxFiltersPackets.CheckedItems.Contains(checkedListBoxFiltersPackets.Items[i]))
                    cbchecked = true;

                Store.PacketSessions.Add(checkedListBoxFiltersPackets.Items[i].ToString(), cbchecked);
            }

            Store.SerializeToFile(fileName);
        }

        void Position_Changed(object sender, EventArgs e)
        {
            //toolStripStatusLabel1.Text = string.Format("Ln {0}    Col {1}    Bytes {2}",
            //    hexBoxResponse.CurrentLine, hexBoxResponse.CurrentPositionInLine, hexBoxResponse.ByteProvider.Length);
        }

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

        // select all items
        private void allToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewSessions.Items)
            {
                item.Selected = true;
            }
        }

        // unselect all items
        private void noneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewSessions.Items)
            {
                item.Selected = false;
            }
        }

        // invert selection
        private void invertSelectionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewSessions.Items)
            {
                item.Selected = !item.Selected;
            }
        }   

        // remove all sessions
        private void allToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listViewSessions.Items.Clear();
        }

        // remove sessions that are currently selected
        private void selectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewSessions.Items)
            {
                if (item.Selected)
                    listViewSessions.Items.Remove(item);
            }
        }

        // remove sessions that are not currently selected
        private void unselectedSessionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewSessions.Items)
            {
                if (!item.Selected)
                    listViewSessions.Items.Remove(item);
            }
        }

        private void MarkColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = (ToolStripMenuItem)sender;
            
            foreach (ListViewItem item in listViewSessions.Items)
            {
                if (item.Selected)
                    item.BackColor = Color.FromName(menu.Text);
            }
            noneToolStripMenuItem_Click(sender, e);
        }

        private void removeToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewSessions.Items)
            {
                if (item.Selected)
                    item.BackColor = Color.White;
            }
            noneToolStripMenuItem_Click(sender, e);
        }    
    }   

    public class ProxyManager
    {
        public delegate void PacketLogHandler(Packet packet, Direction direction, IPEndPoint endpoint);
        public static event PacketLogHandler OnPacketLog;

        public delegate void MessageLogHandler(CapsRequest req, CapsStage stage);
        public static event MessageLogHandler OnMessageLog;

        public delegate void LoginLogHandler(object request, Direction direction);
        public static event LoginLogHandler OnLoginResponse;

        private string _Port;
        private string _ListenIP;
        private string _LoginURI;

        ProxyFrame Proxy;
        ProxyPlugin analyst;

        public ProxyManager(string port, string listenIP, string loginUri)
        {

            _Port = string.Format("--proxy-login-port={0}", port);

            IPAddress remoteIP; // not used
            if (IPAddress.TryParse(listenIP, out remoteIP))
                _ListenIP = String.Format("--proxy-client-facing-address={0}", listenIP);
            else
                _ListenIP = "--proxy-client-facing-address=127.0.0.1";

            if (String.IsNullOrEmpty(loginUri))
                _LoginURI = "https://login.agni.lindenlab.com/cgi-bin/login.cgi";
            else
                _LoginURI = loginUri;


            string[] args = { _Port, _ListenIP, _LoginURI };
            /*
                help
                proxy-help
                proxy-login-port
                proxy-client-facing-address
                proxy-remote-facing-address
                proxy-remote-login-uri
                verbose
                quiet
             */

            Proxy = new ProxyFrame(args);

            analyst = new PacketAnalyzer(Proxy);
            analyst.Init();

            Proxy.proxy.SetLoginRequestDelegate(new XmlRpcRequestDelegate(LoginRequest));
            Proxy.proxy.SetLoginResponseDelegate(new XmlRpcResponseDelegate(LoginResponse));

            AddCapsDelegate("SeedCapability", true);
        }

        public void Start()
        {

            Proxy.proxy.Start();
        }

        public void Stop()
        {
            Proxy.proxy.Stop();
        }

        private void LoginRequest(XmlRpcRequest request)
        {
            if (OnLoginResponse != null)
                OnLoginResponse(request, Direction.Outgoing);
        }

        private void LoginResponse(XmlRpcResponse response)
        {
            if (OnLoginResponse != null)
                OnLoginResponse(response, Direction.Incoming);
        }


        internal Dictionary<string, CapInfo> GetCapabilities()
        {
            return Proxy.proxy.KnownCaps;
        }

        internal void AddCapsDelegate(string capsKey, bool add)
        {
            if (add)
                Proxy.proxy.AddCapsDelegate(capsKey, new CapsDelegate(CapsHandler));
            else
                Proxy.proxy.RemoveCapRequestDelegate(capsKey, new CapsDelegate(CapsHandler));

        }

        private bool CapsHandler(CapsRequest req, CapsStage stage)
        {
            if (OnMessageLog != null)
                OnMessageLog(req, stage);
            //Console.WriteLine(req);
            return false;
        }

        internal void AddUDPDelegate(PacketType packetType, bool add)
        {
            if (add)
            {
                Proxy.proxy.AddDelegate(packetType, Direction.Incoming, new PacketDelegate(PacketInHandler));
                Proxy.proxy.AddDelegate(packetType, Direction.Outgoing, new PacketDelegate(PacketOutHandler));
            }
            else
            {
                Proxy.proxy.RemoveDelegate(packetType, Direction.Incoming, new PacketDelegate(PacketInHandler));
                Proxy.proxy.RemoveDelegate(packetType, Direction.Outgoing, new PacketDelegate(PacketOutHandler));
            }
        }

        private Packet PacketInHandler(Packet packet, IPEndPoint endPoint)
        {
            if (OnPacketLog != null)
                OnPacketLog(packet, Direction.Incoming, endPoint);

            return packet;
        }

        private Packet PacketOutHandler(Packet packet, IPEndPoint endPoint)
        {
            if (OnPacketLog != null)
                OnPacketLog(packet, Direction.Outgoing, endPoint);

            return packet;
        }


    }

    public class PacketAnalyzer : ProxyPlugin
    {
        private ProxyFrame frame;
        private Proxy proxy;

        public PacketAnalyzer(ProxyFrame frame)
        {
            this.frame = frame;
            this.proxy = frame.proxy;
        }

        public override void Init()
        {

        }
    }
}
