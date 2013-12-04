using System;
using Gtk;
using GridProxyGUI;
using OpenMetaverse.StructuredData;
using System.IO;
using System.IO.Compression;
using System.Text;


public partial class MainWindow
{
    string SessionFileName;

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
                    }
                }
                catch { }
            });
        });
    }

}
