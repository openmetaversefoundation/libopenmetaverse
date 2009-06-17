using System;
using System.Windows.Forms;
using GridProxy;
using System.Reflection;
using System.IO;

namespace WinGridProxy
{
    public partial class FormPluginManager : Form
    {
        private ProxyFrame _Frame;
        public FormPluginManager(ProxyFrame frame)
        {
            InitializeComponent();
            _Frame = frame;
        }

        private void buttonLoadPlugin_Click(object sender, EventArgs e)
        {
            if(openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                LoadPlugin(openFileDialog1.FileName);   
            }
        }
        public void LoadPlugin(string name)
        {

            Assembly assembly = Assembly.LoadFile(Path.GetFullPath(name));
            foreach (Type t in assembly.GetTypes())
            {
                try
                {
                    if (t.IsSubclassOf(typeof(ProxyPlugin)))
                    {
                        ConstructorInfo info = t.GetConstructor(new Type[] { typeof(ProxyFrame) });
                        ProxyPlugin plugin = (ProxyPlugin)info.Invoke(new object[] { _Frame });
                        plugin.Init();
                        listView1.Items.Add(new ListViewItem(new []{assembly.ManifestModule.Name, Path.GetFullPath(name)}));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }

        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}
