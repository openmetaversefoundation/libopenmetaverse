using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;
using libsecondlife;
using libsecondlife.Packets;

namespace Baker
{
    public partial class frmBaker : Form
    {
        private SecondLife Client = new SecondLife();
        //private PictureBox[,] Boxes = new PictureBox[16, 16];
        //private System.Timers.Timer UpdateTimer = new System.Timers.Timer(500);
        private string FirstName, LastName, Password;

        //LLVector3 center = new LLVector3(128, 128, 40);
        //LLVector3 up = new LLVector3(0, 0, 0.9999f);
        //LLVector3 forward = new LLVector3(0, 0.9999f, 0);
        //LLVector3 left = new LLVector3(0.9999f, 0, 0);

        public frmBaker(string firstName, string lastName, string password)
        {
            FirstName = firstName;
            LastName = lastName;
            Password = password;

            // Build the picture boxes
            //this.SuspendLayout();
            //for (int y = 0; y < 16; y++)
            //{
            //    for (int x = 0; x < 16; x++)
            //    {
            //        Boxes[x, y] = new System.Windows.Forms.PictureBox();
            //        PictureBox box = Boxes[x, y];
            //        ((System.ComponentModel.ISupportInitialize)(box)).BeginInit();
            //        box.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            //        box.Name = x + "," + y;
            //        box.Location = new System.Drawing.Point(x * 16, y * 16);
            //        box.Size = new System.Drawing.Size(16, 16);
            //        box.Visible = true;
            //        box.MouseUp += new MouseEventHandler(box_MouseUp);
            //        ((System.ComponentModel.ISupportInitialize)(box)).EndInit();

            //        this.Controls.Add(box);
            //    }
            //}
            //this.ResumeLayout();

            InitializeComponent();
        }

        private void frmBaker_Load(object sender, EventArgs e)
        {
            if (Client.Network.Login(FirstName, LastName, Password, "Baker", "jhurliman@wsu.edu"))
            {
                //UpdateTimer.Elapsed += new System.Timers.ElapsedEventHandler(UpdateTimer_Elapsed);
                //UpdateTimer.Start();
            }
            else
            {
                Console.WriteLine("Login failed: " + Client.Network.LoginError);
                Console.ReadKey();
                this.Close();
                return;
            }
        }

        //void UpdateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        //{
        //    forward.Y += 0.2f;
        //    left.X += 0.2f;

        //    if (forward.Y >= 1.0f) forward.Y = 0.0f;
        //    if (left.X >= 1.0f) left.X = 0.0f;

        //    // Spin our camera in circles at the center of the sim to load all the terrain
        //    Client.Self.UpdateCamera(MainAvatar.AgentUpdateFlags.NONE, center, forward, left, up,
        //        LLQuaternion.Identity, LLQuaternion.Identity, 384.0f, false);
        //}

        private void frmBaker_FormClosing(object sender, FormClosingEventArgs e)
        {
            Client.Network.Logout();
        }
    }
}