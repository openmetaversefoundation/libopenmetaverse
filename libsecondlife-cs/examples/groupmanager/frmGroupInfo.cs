using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using libsecondlife;
using libsecondlife.AssetSystem;

namespace groupmanager
{
    public partial class frmGroupInfo : Form
    {
        Group Group;
        SecondLife Client;
        
        public frmGroupInfo(Group group, SecondLife client)
        {
            Group = group;
            Client = client;

            ImageManager im = new ImageManager(Client);
            byte[] j2cdata = im.RequestImage(new LLUUID("c77a1c21-e604-7d2c-2c89-5539ce853466")); //group.InsigniaID);

            BytesToFile(j2cdata, "output.j2c");

            JasperWrapper.jas_init();
            byte[] imagedata = JasperWrapper.jasper_decode_j2c_to_tga(j2cdata);

            BytesToFile(imagedata, "output.tga");

            MemoryStream imageStream = new MemoryStream(imagedata, false);
            Image image = Image.FromStream(imageStream, false, false);

            InitializeComponent();

            picInsignia.Image = image;
        }

        public void BytesToFile(byte[] bytes, string filename)
        {
            FileStream filestream = new FileStream(filename, FileMode.Create);
            BinaryWriter writer = new BinaryWriter(filestream);
            writer.Write(bytes);
            writer.Close();
            filestream.Close();
        }
    }
}
