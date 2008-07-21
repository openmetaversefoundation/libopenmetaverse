using System;
using System.Windows.Forms;
using System.Collections.Generic;

namespace libsecondlife.GUITestClient
{
    public abstract class Interface
    {
        public string Name;
        public string Description;
        public SecondLife Client;
        public TabPage TabPage;

        public abstract void Initialize();

        public abstract void Paint(object sender, PaintEventArgs e);

        /// <summary>
        /// When set to true, think will be called.
        /// </summary>
        public bool Active;
    }
}
