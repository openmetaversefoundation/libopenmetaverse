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
            proxy = new ProxyManager();

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
                
                string[] s = { PacketCounter.ToString() + " " + dir, "HTTPS", t, textBoxLoginURL.Text};
                ListViewItem session = new ListViewItem(s);
                session.Tag = request;

                listViewSessions.Items.Add(session);

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
                string[] s = {PacketCounter.ToString() + " " + dir , "UDP", packet.Type.ToString(), endpoint.ToString(), packet.Length.ToString()};
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
            Dictionary<string, CapInfo> dict = proxy.GetCapabilities();
            foreach (KeyValuePair<string, CapInfo> kvp in dict)
            {
                if (!checkedListBox2.Items.Contains(kvp.Value.CapType))
                    checkedListBox2.Items.Add(kvp.Value.CapType);
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
                    result.AppendFormat("{0}: {1}" + System.Environment.NewLine,
                        packetValueField.Name, packetValueField.GetValue(packetDataObject));
                }

                // handle blocks that are arrays
                if (packetDataObject.GetType().IsArray)
                {
                    foreach (object nestedArrayRecord in packetDataObject as Array)
                    {
                        foreach (FieldInfo packetArrayField in nestedArrayRecord.GetType().GetFields())
                        {
                            result.AppendFormat("{0} {1}" + System.Environment.NewLine,
                                packetArrayField.Name, packetArrayField.GetValue(nestedArrayRecord));
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
                            result.AppendFormat("{0}: {1}" + System.Environment.NewLine,
                                packetPropertyField.Name,
                                Utils.BytesToHexString((byte[])packetPropertyField.GetValue(packetDataObject, null), packetPropertyField.Name));
                        }
                        // decode bytes into strings
                        else if (packetPropertyField.PropertyType.Equals(typeof(System.Byte[])))
                        {
                            result.AppendFormat("{0}: {1}" + System.Environment.NewLine,
                                packetPropertyField.Name,
                                Utils.BytesToString((byte[])packetPropertyField.GetValue(packetDataObject, null)));
                        }
                        else
                        {
                            result.AppendFormat("{0}: {1}" + System.Environment.NewLine,
                                packetPropertyField.Name, packetPropertyField.GetValue(packetDataObject, null));
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
                object t = e.Item.Tag;

                if (t is XmlRpcRequest)
                {
                    Console.WriteLine("XmlRpcRequest");
                    XmlRpcRequest r = (XmlRpcRequest)t;

                    richTextBoxRawLog.Text = r.ToString();
                }
                else if (t is XmlRpcResponse)
                {
                    Console.WriteLine("XmlRpcResponse");
                    XmlRpcResponse r = (XmlRpcResponse)t;
                    Hashtable tbl = (Hashtable)r.Value;
                    richTextBoxRawLog.Text = "";
                    foreach (DictionaryEntry kvp in tbl)
                    {
                        richTextBoxRawLog.AppendText(String.Format("{0}={1}" + System.Environment.NewLine, kvp.Key, kvp.Value));
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
                        treeView1.Nodes.Add(OSDParser.SerializeLLSDXmlString(obj.Response));
                        //XmlToTree(OSDParser.SerializeLLSDXmlString(obj.Response), treeView1);
                    }
                    else
                        richTextBoxRawLog.Text = "No Message Data";
                }
                else
                {
                    richTextBoxRawLog.Text = String.Format("Unknown packet/message: {0}:{1}" + System.Environment.NewLine, e.Item.Text, t.GetType());
                }
            }
        }

        //private void XmlToTree(string xml, TreeView tree)
        //{
        //    try
        //    {
        //        //this.Cursor = System.Windows.Forms.Cursors.WaitCursor;

          

        //        // Load the XML file.
        //        XmlDocument dom = new XmlDocument();
        //        dom.Load("<?xml version=\"1.0\" encoding=\"utf-8\" ?> " + xml);

        //        // Load the XML into the TreeView.
        //        treeView1.Nodes.Clear();
        //        treeView1.Nodes.Add(new TreeNode(strRootNode));
        //        // SECTION 2. Initialize the TreeView control.
        //        treeView1.Nodes.Clear();
        //        treeView1.Nodes.Add(new TreeNode(dom.DocumentElement.Name));
        //        TreeNode tNode = new TreeNode();
        //        tNode = treeView1.Nodes[0];

        //        // SECTION 3. Populate the TreeView with the DOM nodes.
        //        AddNode(dom.DocumentElement, tNode);
        //        treeView1.ExpandAll();

        //        //this.Cursor = System.Windows.Forms.Cursors.Default;
        //    }

        //    catch (Exception ex)
        //    {
        //        this.Cursor = System.Windows.Forms.Cursors.Default;
        //        MessageBox.Show(ex.Message, "Error");
        //    }    

        //}

        private void AddNode(XmlNode inXmlNode, TreeNode inTreeNode)
        {
            XmlNode xNode;
            TreeNode tNode;
            XmlNodeList nodeList;
            int i;

            // Loop through the XML nodes until the leaf is reached.
            // Add the nodes to the TreeView during the looping process.
            if (inXmlNode.HasChildNodes)
            {
                nodeList = inXmlNode.ChildNodes;
                for (i = 0; i <= nodeList.Count - 1; i++)
                {
                    xNode = inXmlNode.ChildNodes[i];
                    inTreeNode.Nodes.Add(new TreeNode(xNode.Name));
                    tNode = inTreeNode.Nodes[i];
                    AddNode(xNode, tNode);
                }
            }
            else
            {
                // Here you need to pull the data from the XmlNode based on the
                // type of node, whether attribute values are required, and so forth.
                inTreeNode.Text = (inXmlNode.OuterXml).Trim();
            }
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
        public ProxyManager()
        {
            string[] args = {};
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
            if(OnPacketLog != null)
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
