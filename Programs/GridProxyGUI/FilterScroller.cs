using System;
using System.Collections.Generic;
using Gtk;
using GridProxyGUI;

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
    }

    public class FilterScroller : ScrolledWindow
    {
        ListStore store;

        public FilterScroller(Container parent, ListStore store)
        {
            this.store = store;

            TreeView tvFilterUDP = new TreeView();
            TreeViewColumn cbCol = new TreeViewColumn();
            TreeViewColumn udpCol = new TreeViewColumn();

            CellRendererToggle cbCell = new CellRendererToggle();
            cbCell.Toggled += new ToggledHandler(cbCell_Toggled);
            cbCell.Activatable = true;
            cbCol.PackStart(cbCell, true);
            cbCol.SetCellDataFunc(cbCell, renderToggleCell);
            tvFilterUDP.AppendColumn(cbCol);

            CellRendererText cell = new CellRendererText();
            udpCol.PackStart(cell, true);
            udpCol.SetCellDataFunc(cell, renderTextCell);
            tvFilterUDP.AppendColumn(udpCol);

            tvFilterUDP.Model = store;
            tvFilterUDP.HeadersVisible = false;
            tvFilterUDP.Selection.Mode = SelectionMode.Single;

            foreach (var child in new List<Widget>(parent.Children))
            {
                parent.Remove(child);
            }

            Add(tvFilterUDP);
            ShadowType = ShadowType.EtchedIn;
            parent.Add(this);
            parent.ShowAll();
        }

        void cbCell_Toggled(object o, ToggledArgs args)
        {
            TreeIter iter;
            if (store.GetIterFromString(out iter, args.Path))
            {
                FilterItem item = store.GetValue(iter, 0) as FilterItem;
                if (null != item)
                {
                    item.Enabled = !item.Enabled;
                    store.SetValue(iter, 0, item);
                }
            }
        }

        void renderTextCell(TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter)
        {
            var item = model.GetValue(iter, 0) as FilterItem;
            if (item != null)
            {
                ((CellRendererText)cell).Text = item.Name;
            }
        }

        void renderToggleCell(TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter)
        {
            var item = model.GetValue(iter, 0) as FilterItem;
            if (item != null)
            {
                ((CellRendererToggle)cell).Active = item.Enabled;
            }
        }

    }
}
