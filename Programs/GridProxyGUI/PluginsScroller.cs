using System;
using System.Collections.Generic;
using System.IO;
using Gtk;
using GridProxy;
using GridProxyGUI;
using System.Reflection;

namespace GridProxyGUI
{
    public class PluginInfo
    {
        public bool LoadOnStartup;
        public string Path;
        
        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(Path)) return string.Empty;
                return System.IO.Path.GetFileName(Path);
            }
        }
        
        public string Dir
        {
            get
            {
                if (string.IsNullOrEmpty(Path)) return string.Empty;
                return System.IO.Path.GetDirectoryName(Path);
            }
        }
        public List<string> Modules = new List<string>();
    }

    public class PluginsScroller : TreeView
    {

        public ListStore Store;

        public PluginsScroller()
        {
            TreeViewColumn col = new TreeViewColumn();
            col.Title = "Load on Starup";
            CellRendererToggle cell = new CellRendererToggle();
            cell.Toggled += new ToggledHandler((sender, e) =>
            {
                TreeIter iter;
                if (Store.GetIterFromString(out iter, e.Path))
                {
                    PluginInfo item = Store.GetValue(iter, 0) as PluginInfo;
                    if (null != item)
                    {
                        item.LoadOnStartup = !item.LoadOnStartup;
                        Store.SetValue(iter, 0, item);
                    }
                }
            });
            cell.Activatable = true;
            col.PackStart(cell, true);
            col.SetCellDataFunc(cell, (TreeViewColumn column, CellRenderer xcell, TreeModel model, TreeIter iter) =>
            {
                var item = Store.GetValue(iter, 0) as PluginInfo;
                if (item != null)
                {
                    ((CellRendererToggle)cell).Active = item.LoadOnStartup;
                }
            });
            AppendColumn(col);

            col = new TreeViewColumn();
            col.Title = "Plugin";
            CellRendererText cellText = new CellRendererText();
            col.PackStart(cellText, true);
            col.SetCellDataFunc(cellText, (TreeViewColumn column, CellRenderer xcell, TreeModel model, TreeIter iter) =>
            {
                var item = Store.GetValue(iter, 0) as PluginInfo;
                if (item != null)
                {
                    ((CellRendererText)xcell).Text = string.Format("{0} ({1})", item.Name, string.Join(", ", item.Modules.ToArray()));
                }
            });
            AppendColumn(col);

            col = new TreeViewColumn();
            col.Title = "Path";
            cellText = new CellRendererText();
            col.PackStart(cellText, true);
            col.SetCellDataFunc(cellText, (TreeViewColumn column, CellRenderer xcell, TreeModel model, TreeIter iter) =>
            {
                var item = Store.GetValue(iter, 0) as PluginInfo;
                if (item != null)
                {
                    ((CellRendererText)xcell).Text = item.Path;
                }
            });
            AppendColumn(col);

            Store = new ListStore(typeof(PluginInfo));
            Model = Store;
            HeadersVisible = true;
            ShowAll();
        }

        List<FileFilter> GetFileFilters()
        {
            List<FileFilter> filters = new List<FileFilter>();

            FileFilter filter = new FileFilter();
            filter.Name = "Grid Proxy Plugin (*.dll; *.exe)";
            filter.AddPattern("*.dll");
            filter.AddPattern("*.exe");
            filters.Add(filter);

            filter = new FileFilter();
            filter.Name = "All Files (*.*)";
            filter.AddPattern("*.*");
            filters.Add(filter);

            return filters;
        }


        public bool LoadAssembly(PluginInfo pinfo, ProxyFrame proxy)
        {

            try
            {
                pinfo.Modules.Clear();
                bool started = false;
                Assembly assembly = Assembly.LoadFile(System.IO.Path.GetFullPath(pinfo.Path));
                foreach (Type t in assembly.GetTypes())
                {
                    if (t.IsSubclassOf(typeof(ProxyPlugin)))
                    {
                        ConstructorInfo info = t.GetConstructor(new Type[] { typeof(ProxyFrame) });
                        if (info == null)
                        {
                            OpenMetaverse.Logger.Log(string.Format("No suitable contructor for {0} in {1}", t.ToString(), pinfo.Name), OpenMetaverse.Helpers.LogLevel.Warning);
                        }
                        else
                        {
                            ProxyPlugin plugin = (ProxyPlugin)info.Invoke(new object[] { proxy });
                            plugin.Init();
                            pinfo.Modules.Add(t.ToString());
                            started = true;
                        }
                    }
                }

                if (started)
                {
                    OpenMetaverse.Logger.Log(string.Format("Loaded {0} plugins from {1}: {2}", pinfo.Modules.Count.ToString(), pinfo.Name, string.Join(", ", pinfo.Modules.ToArray())), OpenMetaverse.Helpers.LogLevel.Info);
                }
                else
                {
                    OpenMetaverse.Logger.Log("Found no plugins in " + pinfo.Name, OpenMetaverse.Helpers.LogLevel.Warning);
                }

                return started;
            }
            catch (Exception e)
            {
                OpenMetaverse.Logger.Log("Failed loading plugins from " + pinfo.Path + ": " + e.Message, OpenMetaverse.Helpers.LogLevel.Error);
            }

            return false;
        }

        public void LoadPlugin(ProxyFrame proxy)
        {
            if (proxy == null) return;

            var od = new Gtk.FileChooserDialog(null, "Load Plugin", null, FileChooserAction.Open, "Cancel", ResponseType.Cancel, "Open", ResponseType.Accept);
            foreach (var filter in GetFileFilters()) od.AddFilter(filter);

            if (od.Run() == (int)ResponseType.Accept)
            {
                PluginInfo plugin = new PluginInfo();
                plugin.Path = od.Filename;
                bool found = false;
                Store.Foreach((model, path, iter) =>
                {
                    var item = model.GetValue(iter, 0) as PluginInfo;
                    if (null != item && item.Path == plugin.Path)
                    {
                        return found = true;
                    }

                    return false;
                });

                if (!found && LoadAssembly(plugin, proxy))
                {
                    Store.AppendValues(plugin);
                }
            }
            od.Destroy();

        }

    }
}
