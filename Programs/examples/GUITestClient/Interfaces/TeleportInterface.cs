using System;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using OpenMetaverse;

namespace OpenMetaverse.GUITestClient
{
    class TeleportInterface : Interface
    {
        private System.Windows.Forms.Button     cmdTeleport;
        private System.Windows.Forms.TextBox    txtLocation;
        private System.Windows.Forms.Label      lblLocation;

        public TeleportInterface(frmTestClient testClient)
        {
            Name = "Teleport";
            Description = "Teleport your's agent in SL Grid";
        }

        public override void Initialize()
        {
            txtLocation         = new System.Windows.Forms.TextBox();
            txtLocation.Size    = new System.Drawing.Size(238, 24);
            txtLocation.Top     = 100;
            txtLocation.Left    = 12;

            lblLocation         = new System.Windows.Forms.Label();
            lblLocation.Size    = new System.Drawing.Size(238, 24);
            lblLocation.Top     = txtLocation.Top - 16;
            lblLocation.Left    = txtLocation.Left;
            lblLocation.Text    = "Location (eg: sim/x/y/z)";

            cmdTeleport         = new System.Windows.Forms.Button();
            cmdTeleport.Size    = new System.Drawing.Size(120, 24);
            cmdTeleport.Top     = 100; cmdTeleport.Left = 257;
            cmdTeleport.Text    = "Teleport !";
            cmdTeleport.Click   += new System.EventHandler(this.cmdTeleport_OnClick);

            TabPage.Controls.Add(txtLocation);
            TabPage.Controls.Add(lblLocation);
            TabPage.Controls.Add(cmdTeleport);
        }

        private void cmdTeleport_OnClick(object sender, System.EventArgs e)
        {
            String destination = txtLocation.Text.Trim();

            string[] tokens = destination.Split(new char[] { '/' });
            if (tokens.Length != 4)
                goto error_handler;

            string sim = tokens[0];
            float x, y, z;
            if (!float.TryParse(tokens[1], out x) ||
                !float.TryParse(tokens[2], out y) ||
                !float.TryParse(tokens[3], out z))
            {
                goto error_handler;
            }

            if (Client.Self.Teleport(sim, new Vector3(x, y, z)))
                MessageBox.Show("Teleported to " + Client.Network.CurrentSim, "Teleport");
            else
                MessageBox.Show("Teleport failed: " + Client.Self.TeleportMessage, "Teleport");
            return;
            
            error_handler:
                MessageBox.Show("Location must to be sim/x/y/z", "Teleport");
        }
        
        public override void Paint(object sender, PaintEventArgs e)
        {
            ;
        }
    }
}
