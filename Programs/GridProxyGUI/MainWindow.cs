using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Gtk;
using GridProxyGUI;
using WinGridProxy;
using OpenMetaverse.Packets;

public partial class MainWindow : Gtk.Window
{
    ProxyManager proxy = null;
    ConcurrentDictionary<string, UDPFilterItem> UDPFilterItems = new ConcurrentDictionary<string, UDPFilterItem>();
    ConcurrentDictionary<string, UDPFilterItem> CapFilterItems = new ConcurrentDictionary<string, UDPFilterItem>();
    ListStore udpStore, capStore;
    FilterScroller capScroller;

    public MainWindow()
        : base(Gtk.WindowType.Toplevel)
    {
        Build();
        SetIconFromFile("libomv.png");
        tabsMain.Page = 1;
        mainSplit.Position = 600;
        txtSummary.ModifyFont(Pango.FontDescription.FromString("monospace bold 9"));

        ProxyLogger.Init();

        ProxyManager.OnCapabilityAdded += new ProxyManager.CapsAddedHandler(ProxyManager_OnCapabilityAdded);
        ProxyManager.OnEventMessageLog += new ProxyManager.EventQueueMessageHandler(ProxyManager_OnEventMessageLog);
        ProxyManager.OnMessageLog += new ProxyManager.MessageLogHandler(ProxyManager_OnMessageLog);
    }

    void ProxyManager_OnCapabilityAdded(GridProxy.CapInfo cap)
    {
        Application.Invoke((sender, e) =>
        {
            if (null == capStore)
            {
                capStore = new ListStore(typeof(UDPFilterItem));
            }

            if (!CapFilterItems.ContainsKey(cap.CapType))
            {
                UDPFilterItem item = new UDPFilterItem() { Enabled = true, Name = cap.CapType };
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
                capStore = new ListStore(typeof(UDPFilterItem));
            }

            if (!CapFilterItems.ContainsKey(req.Info.CapType))
            {
                UDPFilterItem item = new UDPFilterItem() { Enabled = true, Name = req.Info.CapType };
                CapFilterItems[item.Name] = item;
                capStore.AppendValues(item);
            }
            else
            {
                ProxyManager_OnMessageLog(req, GridProxy.CapsStage.Response);
            }

            if (null == capScroller)
            {
                capScroller = new FilterScroller(containerFilterCap, capStore);
            }
        });
    }

    void ProxyManager_OnMessageLog(GridProxy.CapsRequest req, GridProxy.CapsStage stage)
    {
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
    
        UDPFilterItems["Login Request"] = new UDPFilterItem() { Enabled = false, Name = "Login Request" };
        UDPFilterItems["Login Response"] = new UDPFilterItem() { Enabled = true, Name = "Login Response" };
        foreach (string name in Enum.GetNames(typeof(PacketType)))
        {
            if (!string.IsNullOrEmpty(name))
            {
                UDPFilterItems[name] = new UDPFilterItem() { Enabled = false, Name = name };
            }
        }
    }

    void InitProxyFilters()
    {
        InitUDPFilters();

        udpStore = new ListStore(typeof(UDPFilterItem));
        List<string> keys = new List<string>(UDPFilterItems.Keys);
        keys.Sort((a, b) => { return string.Compare(a.ToLower(), b.ToLower()); });

        udpStore.AppendValues(UDPFilterItems["Login Request"]);
        udpStore.AppendValues(UDPFilterItems["Login Response"]);

        foreach (var key in keys)
        {
            if (key == "Login Request" || key == "Login Response") continue;
            udpStore.AppendValues(UDPFilterItems[key]);
        }

        new FilterScroller(containerFilterUDP, udpStore);
    }

}