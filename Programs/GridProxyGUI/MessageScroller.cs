using System;
using System.Collections.Generic;
using Gtk;

namespace GridProxyGUI
{
    internal class MessageScroller : TreeView
    {
        public static string[] ColumnLabels = { "Nr", "Timestamp", "Protocol", "Type", "Size", "URL", "Content Type" };
        Dictionary<string, int> ColumnMap = new Dictionary<string, int>();

        public ListStore Messages;
        public bool AutoScroll = true;

        public MessageScroller()
        {

            for (int i = 0; i < ColumnLabels.Length; i++)
            {
                CellRendererText cell = new CellRendererText();
                TreeViewColumn col = new TreeViewColumn();
                col.PackStart(cell, true);
                col.SetCellDataFunc(cell, CellDataFunc);
                col.Title = ColumnLabels[i];
                AppendColumn(col);
                ColumnMap[ColumnLabels[i]] = i;
            }

            Model = Messages = new ListStore(typeof(Session));
            HeadersVisible = true;
            ShowAll();
        }

        void CellDataFunc(TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter)
        {
            var item = model.GetValue(iter, 0) as Session;
            if (item != null)
            {
                if (ColumnMap.ContainsKey(column.Title))
                {
                    int pos = ColumnMap[column.Title];
                    if (pos < item.Columns.Length)
                    {
                        ((CellRendererText)cell).Text = item.Columns[pos];
                    }
                }
            }
        }

        public void AddSession(Session s)
        {
            TreeIter iter = Messages.AppendValues(s);
            if (AutoScroll)
            {
                ScrollToCell(Messages.GetPath(iter), null, true, 0, 0);
            }
        }
    }
}
