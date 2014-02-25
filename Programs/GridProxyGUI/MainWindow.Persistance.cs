using System;
using Gtk;
using GridProxyGUI;
using OpenMetaverse.StructuredData;
using OpenMetaverse.Packets;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Net;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace GridProxyGUI
{
    public enum ItemType : int
    {
        Unknown = 0,
        Login,
        UDP,
        Cap,
        EQ
    }

    public class FilterItem
    {
        public delegate void FilterItemChangedDelegate(object sender, EventArgs e);
        public event FilterItemChangedDelegate FilterItemChanged;
        bool enabled;
        public bool Enabled
        {
            get { return enabled; }
            set
            {
                enabled = value;
                if (FilterItemChanged != null)
                {
                    FilterItemChanged(this, EventArgs.Empty);
                }
            }
        }
        public string Name;
        public ItemType Type;

        public OSDMap ToOSD()
        {
            OSDMap ret = new OSDMap();

            ret["name"] = Name;
            ret["type"] = (int)Type;
            ret["enabled"] = enabled;

            return ret;
        }

        public static FilterItem FromOSD(OSDMap map)
        {
            var ret = new FilterItem();

            ret.Name = map["name"];
            ret.enabled = map["enabled"];
            ret.Type = (ItemType)map["type"].AsInteger();

            return ret;
        }
    }

}

public partial class MainWindow
{
    string SessionFileName;
    List<string> LoginServers = new List<string>();

    ConcurrentDictionary<string, FilterItem> UDPFilterItems = new ConcurrentDictionary<string, FilterItem>();
    ConcurrentDictionary<string, FilterItem> CapFilterItems = new ConcurrentDictionary<string, FilterItem>();
    ListStore udpStore = new ListStore(typeof(FilterItem));
    ListStore capStore = new ListStore(typeof(FilterItem));
    FilterScroller udpScroller, capScroller;

    void LoadFromOSD(ConcurrentDictionary<string, FilterItem> items, OSDArray filters)
    {
        try
        {
            foreach (var filter in filters)
            {
                if (filter is OSDMap)
                {
                    var item = FilterItem.FromOSD((OSDMap)filter);
                    items[item.Name] = item;
                }
            }
        }
        catch { }
    }

    void InitUDPFilters(OSDArray filters)
    {
        UDPFilterItems.Clear();
        udpStore.Clear();

        if (filters != null)
        {
            LoadFromOSD(UDPFilterItems, filters);
        }

        if (!UDPFilterItems.ContainsKey("Login Request"))
        {
            UDPFilterItems["Login Request"] = new FilterItem() { Enabled = true, Name = "Login Request", Type = ItemType.Login };
        }

        if (!UDPFilterItems.ContainsKey("Login Response"))
        {
            UDPFilterItems["Login Response"] = new FilterItem() { Enabled = true, Name = "Login Response", Type = ItemType.Login };
        }

        foreach (string name in Enum.GetNames(typeof(PacketType)))
        {
            if (!string.IsNullOrEmpty(name) && !UDPFilterItems.ContainsKey(name))
            {
                var item = new FilterItem() { Enabled = false, Name = name, Type = ItemType.UDP };
                UDPFilterItems[name] = item;
            }
        }

        List<string> keys = new List<string>(UDPFilterItems.Keys);
        keys.Sort((a, b) => { return string.Compare(a.ToLower(), b.ToLower()); });

        udpStore.AppendValues(UDPFilterItems["Login Request"]);
        udpStore.AppendValues(UDPFilterItems["Login Response"]);

        foreach (var key in keys)
        {
            UDPFilterItems[key].FilterItemChanged += item_FilterItemChanged;
            if (UDPFilterItems[key].Type == ItemType.Login) continue;
            udpStore.AppendValues(UDPFilterItems[key]);
        }
    }

    void InitCapFilters(OSDArray filters)
    {
        CapFilterItems.Clear();
        capStore.Clear();

        if (filters != null)
        {
            LoadFromOSD(CapFilterItems, filters);
        }

        List<string> keys = new List<string>(CapFilterItems.Keys);
        keys.Sort((a, b) =>
        {
            if (CapFilterItems[a].Type == ItemType.Cap && CapFilterItems[b].Type == ItemType.EQ)
                return -1;

            if (CapFilterItems[a].Type == ItemType.EQ && CapFilterItems[b].Type == ItemType.Cap)
                return 1;

            return string.Compare(a.ToLower(), b.ToLower());
        });

        foreach (var key in keys)
        {
            CapFilterItems[key].FilterItemChanged += item_FilterItemChanged;
            capStore.AppendValues(CapFilterItems[key]);
        }
    }

    void InitProxyFilters()
    {
        capScroller = new FilterScroller(containerFilterCap, capStore);
        udpScroller = new FilterScroller(containerFilterUDP, udpStore);

        OSDArray filters = null;

        if (Options.Instance.ContainsKey("udp_filters") && Options.Instance["udp_filters"] is OSDArray)
        {
            filters = (OSDArray)Options.Instance["udp_filters"];
        }
        InitUDPFilters(filters);

        filters = null;
        if (Options.Instance.ContainsKey("cap_filters") && Options.Instance["cap_filters"] is OSDArray)
        {
            filters = (OSDArray)Options.Instance["cap_filters"];
        }
        InitCapFilters(filters);
    }

    void item_FilterItemChanged(object sender, EventArgs e)
    {
        if (proxy == null) return;

        FilterItem item = (FilterItem)sender;
        if (item.Type == ItemType.Cap)
        {
            proxy.AddCapsDelegate(item.Name, item.Enabled);
        }
        else if (item.Type == ItemType.UDP)
        {
            proxy.AddUDPDelegate(item.Name, item.Enabled);
        }
    }

    void ApplyProxyFilters()
    {
        foreach (var item in UDPFilterItems.Values)
        {
            item_FilterItemChanged(item, EventArgs.Empty);
        }

        foreach (var item in CapFilterItems.Values)
        {
            item_FilterItemChanged(item, EventArgs.Empty);
        }
    }

    void LoadSavedSettings()
    {
        if (Options.Instance.ContainsKey("main_split_pos"))
        {
            mainSplit.Position = Options.Instance["main_split_pos"];
            mainSplit.PositionSet = true;
        }
        if (Options.Instance.ContainsKey("main_width"))
        {
            Resize(Options.Instance["main_width"], Options.Instance["main_height"]);
            Move(Options.Instance["main_x"], Options.Instance["main_y"]);
        }


        // populate the listen box with the known IP Addresses of this host
        cbListen.AppendText("127.0.0.1");
        int selected = 0;
        try
        {
            int current = 0;
            foreach (var address in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                if (address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork || address.ToString() == "127.0.0.1") continue;
                current++;
                if (Options.Instance["listen_address"] == address.ToString())
                {
                    selected = current;
                }
                cbListen.AppendText(address.ToString());
            }
        }
        catch { }
        cbListen.Active = selected;

        if (Options.Instance["login_servers"] is OSDArray)
        {
            var servers = (OSDArray)Options.Instance["login_servers"];
            for (int i = 0; i < servers.Count; i++)
            {
                LoginServers.Add(servers[i]);
            }
        }

        if (LoginServers.Count < 3)
        {
            LoginServers.Add("https://login.agni.lindenlab.com/cgi-bin/login.cgi");
            LoginServers.Add("https://login.aditi.lindenlab.com/cgi-bin/login.cgi");
            LoginServers.Add("http://login.osgrid.org/");
        }

        foreach (var server in LoginServers)
        {
            cbLoginURL.AppendText(server);
        }

        selected = Options.Instance["selected_login_server"];

        if (selected >= 0 && selected < LoginServers.Count)
        {
            cbLoginURL.Active = selected;
        }

        int port = 8080;
        if (Options.Instance.ContainsKey("listen_port"))
        {
            port = Options.Instance["listen_port"];
        }
        txtPort.Text = port.ToString();

    }

    internal FilterItem GetFilter(Session session)
    {
        if (session is SessionCaps || session is SessionEvent)
        {
            foreach (var filter in CapFilterItems.Values)
            {
                if (filter.Name == session.Name)
                {
                    return filter;
                }
            }
        }
        else
        {
            foreach (var filter in UDPFilterItems.Values)
            {
                if (filter.Name == session.Name)
                {
                    return filter;
                }
            }
        }

        return null;
    }

    OSDArray ListStoreToOSD(ListStore list)
    {
        OSDArray ret = new OSDArray();
        list.Foreach((model, path, iter) =>
        {
            var item = model.GetValue(iter, 0) as FilterItem;
            if (null != item)
            {
                ret.Add(item.ToOSD());
            }

            return false;
        });
        return ret;
    }

    void SaveSettings()
    {
        Options.Instance["udp_filters"] = ListStoreToOSD(udpStore);
        Options.Instance["cap_filters"] = ListStoreToOSD(capStore);

        int port = 8080;
        int.TryParse(txtPort.Text, out port);
        Options.Instance["listen_port"] = port;

        var currentServer = cbLoginURL.ActiveText.Trim();
        Uri uriResult;
        if (Uri.TryCreate(currentServer, UriKind.Absolute, out uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps)
            && !LoginServers.Contains(currentServer))
        {
            LoginServers.Add(currentServer);
            Options.Instance["selected_login_server"] = LoginServers.Count - 1;
        }
        else
        {
            Options.Instance["selected_login_server"] = cbLoginURL.Active;
        }

        if (LoginServers.Count > 0)
        {
            OSDArray loginServers = new OSDArray();
            foreach (var s in LoginServers)
            {
                loginServers.Add(s);
            }
            Options.Instance["login_servers"] = loginServers;
        }

        Options.Instance["listen_address"] = cbListen.ActiveText;
        Options.Instance["main_split_pos"] = mainSplit.Position;
        int x, y;

        GetSize(out x, out y); ;
        Options.Instance["main_width"] = x;
        Options.Instance["main_height"] = y;

        GetPosition(out x, out y);
        Options.Instance["main_x"] = x;
        Options.Instance["main_y"] = y;

        Options.Instance.Save();
        Application.Quit();
    }

    void SaveSession()
    {
        OSDMap s = new OSDMap();
        OSDArray array = new OSDArray();

        foreach (object[] row in messages.Messages)
        {
            array.Add(((Session)row[0]).Serialize());
        }

        s["Version"] = "1.0";
        s["Description"] = "Grid Proxy Session Archive";
        s["Messages"] = array;

        System.Threading.ThreadPool.QueueUserWorkItem((sync) =>
        {
            try
            {
                using (var file = File.Open(SessionFileName, FileMode.Create))
                {
                    using (var compressed = new GZipStream(file, CompressionMode.Compress))
                    {
                        using (var writer = new System.Xml.XmlTextWriter(compressed, new UTF8Encoding(false)))
                        {
                            writer.Formatting = System.Xml.Formatting.Indented;
                            writer.WriteStartDocument();
                            writer.WriteStartElement(String.Empty, "llsd", String.Empty);
                            OSDParser.SerializeLLSDXmlElement(writer, s);
                            writer.WriteEndElement();
                        }
                    }
                }
            }
            catch { }
        });
    }

    void OpenSession(string fileName)
    {
        System.Threading.ThreadPool.QueueUserWorkItem((sync) =>
        {
            OSD data;
            try
            {
                using (var file = File.OpenRead(fileName))
                {
                    using (var compressed = new GZipStream(file, CompressionMode.Decompress))
                    {
                        data = OSDParser.DeserializeLLSDXml(compressed);
                    }
                }
            }
            catch
            {
                return;
            }

            Application.Invoke((sender, e) =>
            {
                try
                {
                    if (data != null && data is OSDMap)
                    {
                        messages.Messages.Clear();

                        OSDMap map = (OSDMap)data;
                        OSDArray msgs = (OSDArray)map["Messages"];
                        foreach (var msgOSD in msgs)
                        {
                            var msg = (OSDMap)msgOSD;
                            var session = Session.FromOSD(msg);
                            if (session != null)
                            {
                                messages.Messages.AppendValues(session);
                            }
                        }

                        SessionFileName = fileName;
                    }
                }
                catch { }
            });
        });
    }

}
