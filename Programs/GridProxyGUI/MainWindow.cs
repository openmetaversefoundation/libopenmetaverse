using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Gtk;
using GridProxyGUI;
using OpenMetaverse.Packets;
using System.Timers;

public partial class MainWindow : Gtk.Window
{
    ProxyManager proxy = null;
    ConcurrentDictionary<string, FilterItem> UDPFilterItems = new ConcurrentDictionary<string, FilterItem>();
    ConcurrentDictionary<string, FilterItem> CapFilterItems = new ConcurrentDictionary<string, FilterItem>();
    ListStore udpStore, capStore;
    FilterScroller capScroller;
    MessageScroller messages;

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
        SetIconFromFile("libomv.png");
        tabsMain.Page = 1;
        mainSplit.Position = 600;
        txtSummary.ModifyFont(Pango.FontDescription.FromString("monospace bold 9"));
        sessionLogScroller.Add(messages = new MessageScroller());

        StatsTimer = new Timer(1000.0);
        StatsTimer.Elapsed += StatsTimer_Elapsed;
        StatsTimer.Enabled = true;

        ProxyLogger.Init();

        ProxyManager.OnPacketLog += ProxyManager_OnPacketLog;
        ProxyManager.OnCapabilityAdded += new ProxyManager.CapsAddedHandler(ProxyManager_OnCapabilityAdded);
        ProxyManager.OnEventMessageLog += new ProxyManager.EventQueueMessageHandler(ProxyManager_OnEventMessageLog);
        ProxyManager.OnMessageLog += new ProxyManager.MessageLogHandler(ProxyManager_OnMessageLog);
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

    void StatsTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
        Application.Invoke((xsender, xe) =>
        {
            
        });
    }

    void ProxyManager_OnCapabilityAdded(GridProxy.CapInfo cap)
    {
        Application.Invoke((sender, e) =>
        {
            if (null == capStore)
            {
                capStore = new ListStore(typeof(FilterItem));
            }

            if (!CapFilterItems.ContainsKey(cap.CapType))
            {
                FilterItem item = new FilterItem() { Name = cap.CapType, Type = ItemType.Cap };
                item.FilterItemChanged += item_FilterItemChanged;
                item.Enabled = true;
                CapFilterItems[item.Name] = item;
                capStore.AppendValues(item);
            }

            if (null == capScroller)
            {
                capScroller = new FilterScroller(containerFilterCap, capStore);
            }
        });
    }

    void ProxyManager_OnEventMessageLog(GridProxy.CapsRequest req, GridProxy.CapsStage stage)
    {
        Application.Invoke((sender, e) =>
        {
            if (null == capStore)
            {
                capStore = new ListStore(typeof(FilterItem));
            }

            if (null == capScroller)
            {
                capScroller = new FilterScroller(containerFilterCap, capStore);
            }

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
                string contentType = String.Empty;
                if (req.RawRequest != null)
                {
                    size += req.RawRequest.Length;
                    contentType = req.RequestHeaders.Get("Content-Type");
                }
                if (req.RawResponse != null)
                {
                    size += req.RawResponse.Length;
                    contentType = req.ResponseHeaders.Get("Content-Type");
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

    void item_FilterItemChanged(object sender, EventArgs e)
    {
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
        ProxyLogger.OnLogLine += new ProxyLogger.Log(Logger_OnLogLine);
        proxy = new ProxyManager(txtPort.Text, cbListen.ActiveText, cbLoginURL.ActiveText);
        proxy.Start();
    }

    protected void StopProxy()
    {
        AppendLog("Proxy stopped" + Environment.NewLine);
        ProxyLogger.OnLogLine -= new ProxyLogger.Log(Logger_OnLogLine);
        if (proxy != null) proxy.Stop();
        proxy = null;
        foreach (var child in new List<Widget>(containerFilterUDP.Children))
        {
            containerFilterUDP.Remove(child);
        }
    }

    protected void OnDeleteEvent(object sender, DeleteEventArgs a)
    {
        StopProxy();
        Application.Quit();
        a.RetVal = true;
    }

    protected void OnExitActionActivated(object sender, EventArgs e)
    {
        StopProxy();
        Application.Quit();
    }

    protected void OnBtnStartClicked(object sender, EventArgs e)
    {
        if (btnStart.Label.StartsWith("Start"))
        {
            btnStart.Label = "Stop Proxy";
            StartPoxy();
            InitProxyFilters();
        }
        else if (btnStart.Label.StartsWith("Stop"))
        {
            btnStart.Label = "Start Proxy";
            StopProxy();
        }
    }

    void InitUDPFilters()
    {
        if (UDPFilterItems.Count > 0) return;

        UDPFilterItems["Login Request"] = new FilterItem() { Enabled = false, Name = "Login Request", Type = ItemType.Login };
        UDPFilterItems["Login Response"] = new FilterItem() { Enabled = true, Name = "Login Response", Type = ItemType.Login};
        foreach (string name in Enum.GetNames(typeof(PacketType)))
        {
            if (!string.IsNullOrEmpty(name))
            {
                var item = new FilterItem() { Enabled = false, Name = name, Type = ItemType.UDP };
                UDPFilterItems[name] = item;
            }
        }
    }

    void InitProxyFilters()
    {
        InitUDPFilters();

        udpStore = new ListStore(typeof(FilterItem));
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

        new FilterScroller(containerFilterUDP, udpStore);
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
}