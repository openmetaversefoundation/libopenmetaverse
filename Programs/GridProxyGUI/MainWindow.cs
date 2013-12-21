using GridProxyGUI;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Gtk;
using OpenMetaverse.Packets;
using Logger = OpenMetaverse.Logger;
using System.Timers;
using System.Text.RegularExpressions;

public partial class MainWindow : Gtk.Window
{
    ProxyManager proxy = null;
    MessageScroller messages;
    PluginsScroller plugins;

    // stats tracking
    int PacketCounter;
    int CapsInCounter;
    int CapsInBytes;
    int CapsOutCounter;
    int CapsOutBytes;
    int PacketsInCounter;
    int PacketsInBytes;
    int PacketsOutCounter;
    int PacketsOutBytes;

    Timer StatsTimer;

    public MainWindow()
        : base(Gtk.WindowType.Toplevel)
    {
        Build();
        LoadSavedSettings();

        ProxyLogger.Init();
        ProxyLogger.OnLogLine += new ProxyLogger.Log(Logger_OnLogLine);

        tabsMain.Page = 0;

        string font;
        if (PlatformDetection.IsMac)
        {
            txtSummary.ModifyFont(Pango.FontDescription.FromString("monospace bold"));
            font = "monospace";
            IgeMacIntegration.IgeMacMenu.GlobalKeyHandlerEnabled = true;
            IgeMacIntegration.IgeMacMenu.MenuBar = menuMain;
            MenuItem quit = new MenuItem("Quit");
            quit.Activated += (object sender, EventArgs e) => OnExitActionActivated(sender, e);
            IgeMacIntegration.IgeMacMenu.QuitMenuItem = quit;
            menuMain.Hide();
            menuSeparator.Hide();
        }
        else
        {
            txtSummary.ModifyFont(Pango.FontDescription.FromString("monospace bold 9"));
            font = "monospace 9";
        }

        btnLoadPlugin.Sensitive = false;
        txtPort.TextInserted += (object o, TextInsertedArgs args) =>
        {
            if (args.Length != 1) return;
            if (!char.IsDigit(args.Text[0]))
            {
                txtPort.DeleteText(args.Position - 1, args.Position);
            }
        };

        InitProxyFilters();

        txtRequest.ModifyFont(Pango.FontDescription.FromString(font));
        CreateTags(txtRequest.Buffer);
        txtRequestRaw.ModifyFont(Pango.FontDescription.FromString(font));
        txtRequestNotation.ModifyFont(Pango.FontDescription.FromString(font));

        txtResponse.ModifyFont(Pango.FontDescription.FromString(font));
        CreateTags(txtResponse.Buffer);
        txtResponseRaw.ModifyFont(Pango.FontDescription.FromString(font));
        txtResponseNotation.ModifyFont(Pango.FontDescription.FromString(font));


        sessionLogScroller.Add(messages = new MessageScroller(this));
        scrolledwindowPlugin.Add(plugins = new PluginsScroller());
        messages.CursorChanged += messages_CursorChanged;
        StatsTimer = new Timer(1000.0);
        StatsTimer.Elapsed += StatsTimer_Elapsed;
        StatsTimer.Enabled = true;

        ProxyManager.OnLoginResponse += ProxyManager_OnLoginResponse;
        ProxyManager.OnPacketLog += ProxyManager_OnPacketLog;
        ProxyManager.OnCapabilityAdded += new ProxyManager.CapsAddedHandler(ProxyManager_OnCapabilityAdded);
        ProxyManager.OnEventMessageLog += new ProxyManager.EventQueueMessageHandler(ProxyManager_OnEventMessageLog);
        ProxyManager.OnMessageLog += new ProxyManager.MessageLogHandler(ProxyManager_OnMessageLog);
    }

    void StatsTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
        Application.Invoke((xsender, xe) =>
        {
            lblUDPIn.Text = string.Format("Packets In {0} ({1} bytes)", PacketsInCounter, PacketsInBytes);
            lblUDPOut.Text = string.Format("Packets Out {0} ({1} bytes)", PacketsOutCounter, PacketsOutBytes);
            lblUDPTotal.Text = string.Format("Packets Total {0} ({1} bytes)", PacketsInCounter + PacketsOutCounter, PacketsInBytes + PacketsOutBytes);

            lblCapIn.Text = string.Format("Caps In {0} ({1} bytes)", CapsInCounter, CapsInBytes);
            lblCapOut.Text = string.Format("Caps Out {0} ({1} bytes)", CapsOutCounter, CapsOutBytes);
            lblCapTotal.Text = string.Format("Caps Total {0} ({1} bytes)", CapsInCounter + CapsOutCounter, CapsInBytes + CapsOutBytes);

        });
    }

    void ProxyManager_OnLoginResponse(object request, GridProxy.Direction direction)
    {
        Application.Invoke((xsender, xe) =>
        {
            string loginType;

            if (request is Nwc.XmlRpc.XmlRpcRequest)
            {
                loginType = "Login Request";
            }
            else
            {
                loginType = "Login Response";
            }

            if (UDPFilterItems.ContainsKey(loginType) && UDPFilterItems[loginType].Enabled)
            {
                PacketCounter++;

                SessionLogin sessionLogin = new SessionLogin(request, direction, cbLoginURL.ActiveText, request.GetType().Name + " " + loginType);

                sessionLogin.Columns = new string[] { PacketCounter.ToString(), sessionLogin.TimeStamp.ToString("HH:mm:ss.fff"),
                        sessionLogin.Protocol, sessionLogin.Name, sessionLogin.Length.ToString(), sessionLogin.Host, sessionLogin.ContentType };

                messages.AddSession(sessionLogin);
            }

        });
    }

    void ProxyManager_OnPacketLog(Packet packet, GridProxy.Direction direction, System.Net.IPEndPoint endpoint)
    {
        Application.Invoke((xsender, xe) =>
        {
            PacketCounter++;

            if (direction == GridProxy.Direction.Incoming)
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

            sessionPacket.Columns = new string[] { PacketCounter.ToString(), sessionPacket.TimeStamp.ToString("HH:mm:ss.fff"), sessionPacket.Protocol, sessionPacket.Name, sessionPacket.Length.ToString(), sessionPacket.Host, sessionPacket.ContentType };
            messages.AddSession(sessionPacket);
        });
    }

    void ProxyManager_OnCapabilityAdded(GridProxy.CapInfo cap)
    {
        Application.Invoke((sender, e) =>
        {
            if (!CapFilterItems.ContainsKey(cap.CapType))
            {
                FilterItem item = new FilterItem() { Name = cap.CapType, Type = ItemType.Cap };
                item.FilterItemChanged += item_FilterItemChanged;
                item.Enabled = true;
                CapFilterItems[item.Name] = item;
                capStore.AppendValues(item);
            }
        });
    }

    void ProxyManager_OnEventMessageLog(GridProxy.CapsRequest req, GridProxy.CapsStage stage)
    {
        Application.Invoke((sender, e) =>
        {
            if (!CapFilterItems.ContainsKey(req.Info.CapType))
            {
                FilterItem item = new FilterItem() { Enabled = true, Name = req.Info.CapType, Type = ItemType.EQ };
                item.FilterItemChanged += item_FilterItemChanged;
                CapFilterItems[item.Name] = item;
                capStore.AppendValues(item);
            }

            ProxyManager_OnMessageLog(req, GridProxy.CapsStage.Response);
        });
    }

    void ProxyManager_OnMessageLog(GridProxy.CapsRequest req, GridProxy.CapsStage stage)
    {
        Application.Invoke((sender, e) =>
        {
            if (CapFilterItems.ContainsKey(req.Info.CapType))
            {
                var filter = CapFilterItems[req.Info.CapType];
                if (!filter.Enabled) return;

                PacketCounter++;

                int size = 0;
                if (req.RawRequest != null)
                {
                    size += req.RawRequest.Length;
                }

                if (req.RawResponse != null)
                {
                    size += req.RawResponse.Length;
                }

                GridProxy.Direction direction;
                if (stage == GridProxy.CapsStage.Request)
                {
                    CapsOutCounter++;
                    CapsOutBytes += req.Request.ToString().Length;
                    direction = GridProxy.Direction.Outgoing;
                }
                else
                {
                    CapsInCounter++;
                    CapsInBytes += req.Response.ToString().Length;
                    direction = GridProxy.Direction.Incoming;
                }

                string proto = filter.Type.ToString();

                Session capsSession = null;
                if (filter.Type == ItemType.Cap)
                {
                    capsSession = new SessionCaps(req.RawRequest, req.RawResponse, req.RequestHeaders,
                    req.ResponseHeaders, direction, req.Info.URI, req.Info.CapType, proto, req.FullUri);
                }
                else
                {
                    capsSession = new SessionEvent(req.RawResponse, req.ResponseHeaders, req.Info.URI, req.Info.CapType, proto);
                }

                capsSession.Columns = new string[] { PacketCounter.ToString(), capsSession.TimeStamp.ToString("HH:mm:ss.fff"), capsSession.Protocol, capsSession.Name, capsSession.Length.ToString(), capsSession.Host, capsSession.ContentType };
                messages.AddSession(capsSession);

            }
        });
    }

    void Logger_OnLogLine(object sender, LogEventArgs e)
    {
        Gtk.Application.Invoke((sx, ex) =>
        {
            AppendLog(e.Message);
        });
    }

    void AppendLog(string msg)
    {
        var end = txtSummary.Buffer.EndIter;
        txtSummary.Buffer.Insert(ref end, msg);
    }

    protected void StartPoxy()
    {
        AppendLog("Starting proxy..." + Environment.NewLine);
        try
        {
            proxy = new ProxyManager(txtPort.Text, cbListen.ActiveText, cbLoginURL.ActiveText);
            proxy.Start();
            btnLoadPlugin.Sensitive = true;
            ApplyProxyFilters();
        }
        catch (Exception ex)
        {
            Logger.Log("Failed to start proxy: " + ex.Message, OpenMetaverse.Helpers.LogLevel.Error);
            try
            {
                proxy.Stop();
            }
            catch { }
            btnStart.Label = "Start Proxy";
            proxy = null;
        }
    }

    protected void StopProxy()
    {
        AppendLog("Proxy stopped" + Environment.NewLine);
        if (proxy != null) proxy.Stop();
        proxy = null;
        plugins.Store.Clear();
        btnLoadPlugin.Sensitive = false;
    }

    protected void OnDeleteEvent(object sender, DeleteEventArgs a)
    {
        StopProxy();
        SaveSettings();
        a.RetVal = true;
    }

    protected void OnExitActionActivated(object sender, EventArgs e)
    {
        StopProxy();
        SaveSettings();
    }

    protected void OnBtnStartClicked(object sender, EventArgs e)
    {
        if (btnStart.Label.StartsWith("Start"))
        {
            btnStart.Label = "Stop Proxy";
            StartPoxy();
        }
        else if (btnStart.Label.StartsWith("Stop"))
        {
            btnStart.Label = "Start Proxy";
            StopProxy();
        }
    }

    void SetAllToggles(bool on, ListStore store)
    {
        if (null == store) return;

        store.Foreach((model, path, iter) =>
        {
            var item = model.GetValue(iter, 0) as FilterItem;
            if (null != item)
            {
                item.Enabled = on;
                model.SetValue(iter, 0, item);
            }

            return false;
        });
    }

    protected void OnCbSelectAllUDPToggled(object sender, EventArgs e)
    {
        SetAllToggles(cbSelectAllUDP.Active, udpStore);
    }

    protected void OnCbSelectAllCapToggled(object sender, EventArgs e)
    {
        SetAllToggles(cbSelectAllCap.Active, capStore);
    }

    protected void OnCbAutoScrollToggled(object sender, EventArgs e)
    {
        messages.AutoScroll = cbAutoScroll.Active;
    }

    void messages_CursorChanged(object sender, EventArgs e)
    {
        var tv = (TreeView)sender;
        var paths = tv.Selection.GetSelectedRows();
        TreeIter iter;

        if (paths.Length == 1 && tv.Model.GetIter(out iter, paths[0]))
        {
            var item = tv.Model.GetValue(iter, 0) as Session;
            if (item != null)
            {
                ColorizePacket(txtRequest.Buffer, item.ToPrettyString(GridProxy.Direction.Outgoing));
                txtRequestRaw.Buffer.Text = item.ToRawString(GridProxy.Direction.Outgoing);
                txtRequestNotation.Buffer.Text = item.ToStringNotation(GridProxy.Direction.Outgoing);

                ColorizePacket(txtResponse.Buffer, item.ToPrettyString(GridProxy.Direction.Incoming));
                txtResponseRaw.Buffer.Text = item.ToRawString(GridProxy.Direction.Incoming);
                txtResponseNotation.Buffer.Text = item.ToStringNotation(GridProxy.Direction.Incoming);

                SetVisibility();
            }
        }
    }

    void SetVisibility()
    {
        tabsMain.Page = 2;

        var w1 = vboxInspector.Children.GetValue(0) as Widget;
        var w2 = vboxInspector.Children.GetValue(1) as Widget;

        if (w1 == null || w2 == null) return;

        // check if request is empry
        if (txtRequest.Buffer.Text.Trim().Length < 3 && txtRequestRaw.Buffer.Text.Trim().Length < 3)
        {
            w1.Hide();
        }
        else
        {
            w1.Show();
        }

        // check if request is empry
        if (txtResponse.Buffer.Text.Trim().Length < 3 && txtResponseRaw.Buffer.Text.Trim().Length < 3)
        {
            w2.Hide();
        }
        else
        {
            w2.Show();
        }

    }

    void CreateTags(TextBuffer buffer)
    {
        TextTag tag = buffer.TagTable.Lookup("bold");
        if (tag == null)
        {
            tag = new TextTag("bold");
            tag.Weight = Pango.Weight.Bold;
            buffer.TagTable.Add(tag);
        }

        tag = buffer.TagTable.Lookup("type");
        if (tag == null)
        {
            tag = new TextTag("type");
            tag.ForegroundGdk = new Gdk.Color(43, 145, 175);
            buffer.TagTable.Add(tag);
        }

        tag = buffer.TagTable.Lookup("header");
        if (tag == null)
        {
            tag = new TextTag("header");
            tag.ForegroundGdk = new Gdk.Color(0, 96, 0);
            tag.BackgroundGdk = new Gdk.Color(206, 226, 252);
            buffer.TagTable.Add(tag);
        }

        tag = buffer.TagTable.Lookup("block");
        if (tag == null)
        {
            tag = new TextTag("block");
            tag.ForegroundGdk = new Gdk.Color(255, 215, 0);
            buffer.TagTable.Add(tag);
        }

        tag = buffer.TagTable.Lookup("tag");
        if (tag == null)
        {
            tag = new TextTag("tag");
            tag.ForegroundGdk = new Gdk.Color(255, 255, 255);
            tag.BackgroundGdk = new Gdk.Color(40, 40, 40);
            buffer.TagTable.Add(tag);
        }

        tag = buffer.TagTable.Lookup("counter");
        if (tag == null)
        {
            tag = new TextTag("counter");
            tag.ForegroundGdk = new Gdk.Color(0, 64, 0);
            buffer.TagTable.Add(tag);
        }

        tag = buffer.TagTable.Lookup("UUID");
        if (tag == null)
        {
            tag = new TextTag("UUID");
            tag.ForegroundGdk = new Gdk.Color(148, 0, 211);
            buffer.TagTable.Add(tag);
        }
    }

    void ColorizePacket(TextBuffer buffer, string text)
    {
        if (!text.StartsWith("Message Type:") && !text.StartsWith("Packet Type:"))
        {
            buffer.Text = text;
            return;
        }

        buffer.Text = string.Empty;
        text = text.Replace("\r", "");
        TextIter iter = buffer.StartIter;

        Regex typesRegex = new Regex(@"\[(?<Type>\w+|\w+\[\])\]|\((?<Enum>.*)\)|\s-- (?<Header>\w+|\w+ \[\]) --\s|(?<BlockSep>\s\*\*\*\s)|(?<Tag>\s<\w+>\s|\s<\/\w+>\s)|(?<BlockCounter>\s\w+\[\d+\]\s)|(?<UUID>[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12})", RegexOptions.ExplicitCapture);

        MatchCollection matches = typesRegex.Matches(text);
        int pos = 0;

        if (matches.Count == 0)
        {
            buffer.Text = text;
        }

        foreach (Match match in matches)
        {
            string tag = "bold";

            buffer.Insert(ref iter, text.Substring(pos, match.Index - pos));
            pos += match.Index - pos;

            if (!String.IsNullOrEmpty(match.Groups["Type"].Value))
            {
                tag = "type";
            }
            else if (!String.IsNullOrEmpty(match.Groups["Enum"].Value))
            {
                tag = "type";
            }
            else if (!String.IsNullOrEmpty(match.Groups["Header"].Value))
            {
                tag = "header";
            }
            else if (!String.IsNullOrEmpty(match.Groups["BlockSep"].Value))
            {
                tag = "block";
            }
            else if (!String.IsNullOrEmpty(match.Groups["Tag"].Value))
            {
                tag = "tag";
            }
            else if (!String.IsNullOrEmpty(match.Groups["BlockCounter"].Value))
            {
                tag = "counter";
            }
            else if (!String.IsNullOrEmpty(match.Groups["UUID"].Value))
            {
                tag = "UUID";
            }

            buffer.InsertWithTagsByName(ref iter, text.Substring(pos, match.Length), tag);
            pos += match.Length;
        }
    }

    List<FileFilter> GetFileFilters()
    {
        List<FileFilter> filters = new List<FileFilter>();

        FileFilter filter = new FileFilter();
        filter.Name = "Grid Proxy Compressed (*.gpz)";
        filter.AddPattern("*.gpz");
        filters.Add(filter);

        filter = new FileFilter();
        filter.Name = "All Files (*.*)";
        filter.AddPattern("*.*");
        filters.Add(filter);


        return filters;
    }

    public void RedrawFilters()
    {
        containerFilterCap.QueueDraw();
        containerFilterUDP.QueueDraw();
    }

    protected void OnOpenActionActivated(object sender, EventArgs e)
    {
        var od = new Gtk.FileChooserDialog(null, "Open Session", this, FileChooserAction.Open, "Cancel", ResponseType.Cancel, "Open", ResponseType.Accept);
        foreach (var filter in GetFileFilters()) od.AddFilter(filter);

        if (od.Run() == (int)ResponseType.Accept)
        {
            OpenSession(od.Filename);
        }
        od.Destroy();
    }

    protected void OnSaveAsActionActivated(object sender, EventArgs e)
    {
        var od = new Gtk.FileChooserDialog(null, "Save Session", this, FileChooserAction.Save, "Cancel", ResponseType.Cancel, "Save", ResponseType.Accept);
        foreach (var filter in GetFileFilters()) od.AddFilter(filter);

        if (od.Run() == (int)ResponseType.Accept)
        {
            SessionFileName = od.Filename;
            if (string.IsNullOrEmpty(System.IO.Path.GetExtension(SessionFileName)))
            {
                SessionFileName += ".gpz";
            }
            SaveSession();
        }
        od.Destroy();
    }

    protected void OnSaveActionActivated(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(SessionFileName))
        {
            OnSaveAsActionActivated(sender, e);
        }
        else
        {
            SaveSession();
        }
    }

    protected void OnAboutActionActivated(object sender, EventArgs e)
    {
        var about = new GridProxyGUI.About();
        about.SkipTaskbarHint = about.SkipPagerHint = true;
        about.Run();
        about.Destroy();
    }

    protected void OnBtnLoadPluginClicked(object sender, EventArgs e)
    {
        if (proxy == null) return;

        plugins.LoadPlugin(proxy.Proxy);
    }

}
