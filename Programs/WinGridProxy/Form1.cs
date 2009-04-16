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
        private static bool IsProxyRunning = false;

        ProxyManager proxy;
        private Assembly openmvAssembly;

        private int PacketCounter = 0;

        public Form1()
        {
            InitializeComponent();

            ProxyManager.OnPacketLog += new ProxyManager.PacketLogHandler(ProxyManager_OnPacketLog);
            ProxyManager.OnMessageLog += new ProxyManager.MessageLogHandler(ProxyManager_OnMessageLog);
            ProxyManager.OnLoginResponse += new ProxyManager.LoginLogHandler(ProxyManager_OnLoginResponse);
            openmvAssembly = Assembly.Load("OpenMetaverse");
            if (openmvAssembly == null) throw new Exception("Assembly load exception");

            InitProxyFilters();

        }

        private void InitProxyFilters()
        {
            Type packetTypeType = typeof(PacketType);
            System.Reflection.MemberInfo[] packetTypes = packetTypeType.GetMembers();
            checkedListBox1.BeginUpdate();
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
                        checkedListBox1.Items.Add(name, false);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }
            checkedListBox1.Sorted = true;
            checkedListBox1.EndUpdate();
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
                string dir = (request is XmlRpcRequest) ? ">>" : "<<";
                string t = (request is XmlRpcRequest) ? "Request" : "Response";

                string[] s = { PacketCounter.ToString() + " " + dir, "HTTPS", t, textBoxLoginURL.Text };
                ListViewItem session = new ListViewItem(s);
                session.Tag = request;

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

                        foreach (DictionaryEntry kvp in ht)
                        {
                            //Console.WriteLine("hash: {0} -> {1}", kvp.Key, kvp.Value);
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
                string dir = (direction == Direction.Incoming) ? "<<" : ">>";
                string[] s = { PacketCounter.ToString() + " " + dir, "UDP", packet.Type.ToString(), endpoint.ToString(), packet.Length.ToString() };
                ListViewItem session = new ListViewItem(s);
                session.Tag = packet;

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
                string dir = (stage == CapsStage.Request) ? ">>" : "<<";
                string[] s = { PacketCounter.ToString() + " " + dir, "CAPS", req.Info.CapType, req.Info.URI, "" };
                ListViewItem session = new ListViewItem(s);
                session.Tag = req;

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
                proxy.Start();
                IsProxyRunning = true;
                button1.Text = "Stop Proxy";
            }
            else if (button1.Text.StartsWith("Stop") && IsProxyRunning.Equals(true))
            {
                // stop the proxy
                
                proxy.Stop();
                IsProxyRunning = false;
                button1.Text = "Start Proxy";
                textBoxProxyListenIP.Enabled = textBoxProxyPort.Enabled = textBoxLoginURL.Enabled = true;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                checkedListBox1.SetItemChecked(i, checkBox1.Checked);
            }
        }

        private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            //Console.WriteLine("Item {0} is now {1}", checkedListBox1.Items[e.Index], e.NewValue);
            if (e.CurrentValue != e.NewValue)
            {
                proxy.AddUDPDelegate(packetTypeFromName(checkedListBox1.Items[e.Index].ToString()), (e.NewValue == CheckState.Checked));
            }
        }

        private void buttonRefreshCapsList_Click(object sender, EventArgs e)
        {
            foreach (KeyValuePair<string, CapInfo> kvp in proxy.GetCapabilities())
            {
                checkedListBox2.BeginUpdate();
                if (!checkedListBox2.Items.Contains(kvp.Value.CapType))
                {
                    checkedListBox2.Items.Add(kvp.Value.CapType);
                }
                checkedListBox2.Sorted = true;
                checkedListBox2.EndUpdate();
            }
        }

        private void checkBoxCheckallCaps_CheckedChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBox2.Items.Count; i++)
            {
                checkedListBox2.SetItemChecked(i, checkBox2.Checked);
            }
        }

        private void checkedListBoxCaps_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.CurrentValue != e.NewValue)
            {
                proxy.AddCapsDelegate(checkedListBox2.Items[e.Index].ToString(), (e.NewValue == CheckState.Checked));
            }
        }

        // This is from omv Utils, once we get it prettied up put it back there
        public static string PacketToString(Packet packet)
        {
            StringBuilder result = new StringBuilder();

            foreach (FieldInfo packetField in packet.GetType().GetFields())
            {
                object packetDataObject = packetField.GetValue(packet);

                result.AppendFormat("-- {0} --" + System.Environment.NewLine, packetField.Name);
                result.AppendFormat("-- {0} --" + System.Environment.NewLine, packet.Type);
                foreach (FieldInfo packetValueField in packetField.GetValue(packet).GetType().GetFields())
                {
                    result.AppendFormat("a){0,30}: {1}" + System.Environment.NewLine,
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
                            //result.AppendFormat("e){0,20}: {1}" + System.Environment.NewLine,
                            //    packetPropertyField.Name, packetPropertyField.GetValue(packetDataObject, null));
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
                object t = e.Item.Tag;

                if (t is XmlRpcRequest)
                {
                    Console.WriteLine("XmlRpcRequest");
                    XmlRpcRequest r = (XmlRpcRequest)t;

                    richTextBoxRawLog.Text = r.ToString();
                    updateTreeView(r.ToString());
                }
                else if (t is XmlRpcResponse)
                {
                    Console.WriteLine("XmlRpcResponse");
                    XmlRpcResponse r = (XmlRpcResponse)t;
                    Hashtable tbl = (Hashtable)r.Value;
                    richTextBoxRawLog.Text = "";
                    foreach (DictionaryEntry kvp in tbl)
                    {
                        StringBuilder sb = new StringBuilder("Hash Table Not Decoded");
                        if(kvp.Value.GetType() == typeof(ArrayList))
                        {
                            // iterate the hash table and append the values
                            //foreach (var item in kvp.Value as ArrayList)
                            //{
                            //    sb.Append(item + System.Environment.NewLine);
                            //}
                            richTextBoxRawLog.AppendText(String.Format("{0,15}: {1}" + System.Environment.NewLine, kvp.Key, sb.ToString()));
                        } else {
                            richTextBoxRawLog.AppendText(String.Format("{0,15}: {1}" + System.Environment.NewLine, kvp.Key, kvp.Value));
                        }
                    }
                }
                else if (t is Packet)
                {
                    Console.WriteLine("Packet");
                    richTextBoxRawLog.Text = PacketToString((Packet)t);
                }
                else if (t is CapsRequest)
                {
                    Console.WriteLine("CAPS");
                    CapsRequest obj = (CapsRequest)t;
                    Console.WriteLine(t.ToString());
                    if (obj.Response != null)
                    {
                        richTextBoxRawLog.Text = obj.Response.ToString();
                        richTextBox1.Text = OSDParser.SerializeLLSDXmlString(obj.Response);
                        updateTreeView(OSDParser.SerializeLLSDXmlString(obj.Response));
                    }
                    else
                    {
                        richTextBoxRawLog.Text = "No Message Data";
                        treeView1.Nodes.Clear();
                    }
                    
                }
                else
                {
                    richTextBoxRawLog.Text = String.Format("Unknown packet/message: {0}:{1}" + System.Environment.NewLine, e.Item.Text, t.GetType());
                    
                }
            }
        }

        private void updateTreeView(string xml)
        {
            try
            {
                treeView1.Nodes.Clear();
                
                XmlDocument tmpxmldoc = new XmlDocument();
                tmpxmldoc.LoadXml(xml);
                FillTree(tmpxmldoc.DocumentElement, treeView1.Nodes);
                treeView1.ExpandAll();
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

        ProxyFrame Proxy;
        ProxyPlugin analyst;
        public ProxyManager(string port, string listenIP, string loginUri)
        {

            port = string.Format("--proxy-login-port={0}", port);

            IPAddress remoteIP; // not used
            if (IPAddress.TryParse(listenIP, out remoteIP))
                listenIP = String.Format("--proxy-client-facing-address={0}", listenIP);
            else
                listenIP = "--proxy-client-facing-address=127.0.0.1";

            if (String.IsNullOrEmpty(loginUri))
                loginUri = "https://login.agni.lindenlab.com/cgi-bin/login.cgi";


            string[] args = { port, listenIP, loginUri };
            /*
             help"] = new ArgumentParser(ParseHelp);
argumentParsers["proxy-help"] = new ArgumentParser(ParseHelp);
argumentParsers["proxy-login-port"] = new ArgumentParser(ParseLoginPort);
argumentParsers["proxy-client-facing-address"] = new ArgumentParser(ParseClientFacingAddress);
argumentParsers["proxy-remote-facing-address"] = new ArgumentParser(ParseRemoteFacingAddress);
argumentParsers["proxy-remote-login-uri"] = new ArgumentParser(ParseRemoteLoginUri);
argumentParsers["verbose"] = new ArgumentParser(ParseVerbose);
argumentParsers["quiet"] = new ArgumentPar
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
                Proxy.proxy.AddDelegate(packetType, Direction.Incoming, new PacketDelegate(PacketHandler));
                Proxy.proxy.AddDelegate(packetType, Direction.Outgoing, new PacketDelegate(PacketHandler));
            }
            else
            {
                Proxy.proxy.RemoveDelegate(packetType, Direction.Incoming, new PacketDelegate(PacketHandler));
                Proxy.proxy.RemoveDelegate(packetType, Direction.Outgoing, new PacketDelegate(PacketHandler));
            }
        }

        private Packet PacketHandler(Packet packet, IPEndPoint endPoint)
        {
            if (OnPacketLog != null)
                OnPacketLog(packet, Direction.Incoming, endPoint);

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
