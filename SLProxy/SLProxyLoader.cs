using SLProxy;
using libsecondlife;
using Nwc.XmlRpc;
using libsecondlife.Packets;
using System.Reflection;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;


namespace SLProxy
{
    public class ProxyFrame
    {
        public Proxy proxy;
        private Dictionary<string, CommandDelegate> commandDelegates = new Dictionary<string, CommandDelegate>();
        private LLUUID agentID;
        private LLUUID sessionID;
        private LLUUID inventoryRoot;
        private bool logLogin = false;
        private string[] args;

        public delegate void CommandDelegate(string[] words);

        public string[] Args
        {
            get { return args; }
        }

        public LLUUID AgentID
        {
            get { return agentID; }
        }

        public LLUUID SessionID
        {
            get { return sessionID; }
        }

        public LLUUID InventoryRoot
        {
            get { return inventoryRoot; }
        }

        public void AddCommand(string cmd, CommandDelegate deleg)
        {
            commandDelegates[cmd] = deleg;
        }

        public ProxyFrame(string[] args)
        {
            //bool externalPlugin = false;
            this.args = args;

            ProxyConfig proxyConfig = new ProxyConfig("SLProxy", "Austin Jennings / Andrew Ortman", args);
            proxy = new Proxy(proxyConfig);

            // add delegates for login
            proxy.SetLoginRequestDelegate(new XmlRpcRequestDelegate(LoginRequest));
            proxy.SetLoginResponseDelegate(new XmlRpcResponseDelegate(LoginResponse));

            // add a delegate for outgoing chat
            proxy.AddDelegate(PacketType.ChatFromViewer, Direction.Outgoing, new PacketDelegate(ChatFromViewerOut));

            //  handle command line arguments
            foreach (string arg in args)
                if (arg == "--log-login")
                    logLogin = true;
                else if (arg.Substring(0, 2) == "--")
                {
                    int ipos = arg.IndexOf("=");
                    if (ipos != -1)
                    {
                        string sw = arg.Substring(0, ipos);
                        string val = arg.Substring(ipos + 1);
                        Console.WriteLine("arg '" + sw + "' val '" + val + "'");
                        if (sw == "--load")
                        {
                            //externalPlugin = true;
                            LoadPlugin(val);
                        }
                    }
                }

            commandDelegates["/load"] = new CommandDelegate(CmdLoad);
       }

        private void CmdLoad(string[] words)
        {
            if (words.Length != 2)
                SayToUser("Usage: /load <plugin name>");
            else
            {
                try
                {
                    LoadPlugin(words[1]);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }

        public void LoadPlugin(string name)
        {

            Assembly assembly = Assembly.LoadFile(Path.GetFullPath(name));
            foreach (Type t in assembly.GetTypes())
            {
                try
                {
                    if (t.IsSubclassOf(typeof(ProxyPlugin)))
                    {
                        ConstructorInfo info = t.GetConstructor(new Type[] { typeof(ProxyFrame) });
                        ProxyPlugin plugin = (ProxyPlugin)info.Invoke(new object[] { this });
                        plugin.Init();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }

        }

        // LoginRequest: dump a login request to the console
        private void LoginRequest(XmlRpcRequest request)
        {
            if (logLogin)
            {
                Console.WriteLine("==> Login Request");
                Console.WriteLine(request);
            }
        }

        // Loginresponse: dump a login response to the console
        private void LoginResponse(XmlRpcResponse response)
        {
            System.Collections.Hashtable values = (System.Collections.Hashtable)response.Value;
            if (values.Contains("agent_id"))
                agentID = new LLUUID((string)values["agent_id"]);
            if (values.Contains("session_id"))
                sessionID = new LLUUID((string)values["session_id"]);
            if (values.Contains("inventory-root")) 
            {
                inventoryRoot = new LLUUID(
                    (string)((System.Collections.Hashtable)(((System.Collections.ArrayList)values["inventory-root"])[0]))["folder_id"]
                    );
                Console.WriteLine("inventory root: " + inventoryRoot);
            }

            if (logLogin)
            {
                Console.WriteLine("<== Login Response");
                Console.WriteLine(response);
            }
        }

        // ChatFromViewerOut: outgoing ChatFromViewer delegate; check for Analyst commands
        private Packet ChatFromViewerOut(Packet packet, IPEndPoint sim)
        {
            // deconstruct the packet
            ChatFromViewerPacket cpacket = (ChatFromViewerPacket)packet;
            string message = System.Text.Encoding.UTF8.GetString(cpacket.ChatData.Message).Replace("\0", "");

            if (message.Length > 1 && message[0] == '/')
            {
                string[] words = message.Split(' ');
                if (commandDelegates.ContainsKey(words[0]))
                {
                    // this is an Analyst command; act on it and drop the chat packet
                    ((CommandDelegate)commandDelegates[words[0]])(words);
                    return null;
                }
            }

            return packet;
        }

        // SayToUser: send a message to the user as in-world chat
        public void SayToUser(string message)
        {
            ChatFromSimulatorPacket packet = new ChatFromSimulatorPacket();
            packet.ChatData.FromName = Helpers.StringToField("SLProxy");
            packet.ChatData.SourceID = LLUUID.Random();
            packet.ChatData.OwnerID = agentID;
            packet.ChatData.SourceType = (byte)2;
            packet.ChatData.ChatType = (byte)1;
            packet.ChatData.Audible = (byte)1;
            packet.ChatData.Position = new LLVector3(0, 0, 0);
            packet.ChatData.Message = Helpers.StringToField(message);
            proxy.InjectPacket(packet, Direction.Incoming);
        }

    }


    public abstract class ProxyPlugin : MarshalByRefObject
    {
        // public abstract ProxyPlugin(ProxyFrame main);
        public abstract void Init();
    }

}
