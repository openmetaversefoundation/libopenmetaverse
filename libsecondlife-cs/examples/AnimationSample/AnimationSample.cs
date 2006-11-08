using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using libsecondlife;
using libsecondlife.Packets;
using System.Collections;

namespace AnimationSample
{
    public partial class Form1 : Form
    {
        SecondLife client;
        public Form1()
        {
            InitializeComponent();
            //Create the SecondLife client object
            client = new SecondLife();

        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            //Build an animation packet
            AgentAnimationPacket packet = new AgentAnimationPacket();

            //create an AgentData block
            AgentAnimationPacket.AgentDataBlock agentdata = new AgentAnimationPacket.AgentDataBlock();
            //Fill in its values
            agentdata.AgentID = client.Self.ID;
            agentdata.SessionID = client.Network.SessionID;
            //Add it in the packet
            packet.AgentData = agentdata;

            //Create an AnimationList block
            AgentAnimationPacket.AnimationListBlock anims = new AgentAnimationPacket.AnimationListBlock();
            //Set the UUID of the animation to avatar_dance1.bvh, a standard animation
            anims.AnimID = new LLUUID("b68a3d7c-de9e-fc87-eec8-543d787e5b0d");
            //Start the animation
            anims.StartAnim = true;
            //Add it to the packet. SInce it's a Variable number block, we have to construct an array.
            packet.AnimationList = new AgentAnimationPacket.AnimationListBlock[] { anims };

            //Send the packet
            client.Network.SendPacket(packet);
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            // Login
            if (!client.Network.Login(txtFirst.Text, txtLast.Text, txtPassword.Text, "animationsample", "jessemalthus@gmail.com"))
            {
                // Login failed
                MessageBox.Show("We're sorry, but login failed. Error: \n " + client.Network.LoginError);
                
            }
            else
            {
                MessageBox.Show("Login succeded. You're at " + client.Self.Position + " on " + client.Network.CurrentSim.Region.Name);
            }
        }
    }
}