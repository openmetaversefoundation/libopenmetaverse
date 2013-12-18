using System;
using Gtk;
using GridProxyGUI;
using OpenMetaverse.StructuredData;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Net;
using System.Collections.Generic;

public partial class MainWindow
{
    string SessionFileName;
    List<string> LoginServers = new List<string>();

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
                if (Options.Instance["listed_address"] == address.ToString())
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

    }

    void SaveSettings()
    {
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

        Options.Instance["listed_address"] = cbListen.ActiveText;
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
