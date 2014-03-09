using System;

namespace GridProxyGUI
{
    public partial class About : Gtk.Dialog
    {
        public About()
        {
            this.Build();
        }

        protected void OnButtonOkClicked (object sender, EventArgs e)
        {
            Hide();
        }
    }
}

