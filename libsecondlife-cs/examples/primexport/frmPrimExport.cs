using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using libsecondlife;

namespace primexport
{
    public class frmPrimExport : Form
    {
        private GroupBox grpLogin;
        private Label label3;
        private Label label2;
        private Label label1;
        private TextBox txtPassword;
        private TextBox txtLastName;
        private Button cmdConnect;
        private TextBox txtFirstName;
        private Button cmdCapture;
        private TextBox txtLog;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private SecondLife client;
        private NewPrimCallback primCallback;
        private string currentText;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.grpLogin = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.txtLastName = new System.Windows.Forms.TextBox();
            this.cmdConnect = new System.Windows.Forms.Button();
            this.txtFirstName = new System.Windows.Forms.TextBox();
            this.cmdCapture = new System.Windows.Forms.Button();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.grpLogin.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpLogin
            // 
            this.grpLogin.Controls.Add(this.label3);
            this.grpLogin.Controls.Add(this.label2);
            this.grpLogin.Controls.Add(this.label1);
            this.grpLogin.Controls.Add(this.txtPassword);
            this.grpLogin.Controls.Add(this.txtLastName);
            this.grpLogin.Controls.Add(this.cmdConnect);
            this.grpLogin.Controls.Add(this.txtFirstName);
            this.grpLogin.Enabled = false;
            this.grpLogin.Location = new System.Drawing.Point(12, 204);
            this.grpLogin.Name = "grpLogin";
            this.grpLogin.Size = new System.Drawing.Size(560, 80);
            this.grpLogin.TabIndex = 51;
            this.grpLogin.TabStop = false;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(280, 24);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(120, 16);
            this.label3.TabIndex = 50;
            this.label3.Text = "Password";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(152, 24);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(120, 16);
            this.label2.TabIndex = 50;
            this.label2.Text = "Last Name";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(16, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(120, 16);
            this.label1.TabIndex = 50;
            this.label1.Text = "First Name";
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(280, 40);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new System.Drawing.Size(120, 20);
            this.txtPassword.TabIndex = 2;
            // 
            // txtLastName
            // 
            this.txtLastName.Location = new System.Drawing.Point(152, 40);
            this.txtLastName.Name = "txtLastName";
            this.txtLastName.Size = new System.Drawing.Size(112, 20);
            this.txtLastName.TabIndex = 1;
            // 
            // cmdConnect
            // 
            this.cmdConnect.Location = new System.Drawing.Point(424, 40);
            this.cmdConnect.Name = "cmdConnect";
            this.cmdConnect.Size = new System.Drawing.Size(120, 24);
            this.cmdConnect.TabIndex = 3;
            this.cmdConnect.Text = "Connect";
            this.cmdConnect.Click += new System.EventHandler(this.cmdConnect_Click);
            // 
            // txtFirstName
            // 
            this.txtFirstName.Location = new System.Drawing.Point(16, 40);
            this.txtFirstName.Name = "txtFirstName";
            this.txtFirstName.Size = new System.Drawing.Size(120, 20);
            this.txtFirstName.TabIndex = 0;
            // 
            // cmdCapture
            // 
            this.cmdCapture.Enabled = false;
            this.cmdCapture.Location = new System.Drawing.Point(12, 12);
            this.cmdCapture.Name = "cmdCapture";
            this.cmdCapture.Size = new System.Drawing.Size(560, 49);
            this.cmdCapture.TabIndex = 52;
            this.cmdCapture.Text = "Start Capture";
            this.cmdCapture.Click += new System.EventHandler(this.cmdCapture_Click);
            // 
            // txtLog
            // 
            this.txtLog.Location = new System.Drawing.Point(12, 67);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(560, 131);
            this.txtLog.TabIndex = 53;
            // 
            // frmPrimExport
            // 
            this.ClientSize = new System.Drawing.Size(587, 299);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.cmdCapture);
            this.Controls.Add(this.grpLogin);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(595, 326);
            this.MinimumSize = new System.Drawing.Size(595, 326);
            this.Name = "frmPrimExport";
            this.Text = "Prim Exporter";
            this.TopMost = true;
            this.grpLogin.ResumeLayout(false);
            this.grpLogin.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public frmPrimExport()
        {
            InitializeComponent();

            primCallback = new NewPrimCallback(PrimSeen);

            try
            {
                client = new SecondLife("keywords.txt", "message_template.msg");
                grpLogin.Enabled = true;
            }
            catch (Exception error)
            {
                MessageBox.Show(this, error.ToString());
            }
        }

        private void Log(string text)
        {
            currentText = text;

            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(UpdateLog));
            }
            else
            {
                UpdateLog();
            }
        }

        private void UpdateLog()
        {
            txtLog.Text += currentText + Environment.NewLine;
        }

        private void PrimSeen(Simulator simulator, PrimObject prim, U64 regionHandle, ushort timeDilation)
        {
            uint type = 0;
            string output = "";

            output += "<primitive name=\"Object\" description=\"\" key=\"Num_000" + prim.LocalID + "\" version=\"2\">" + Environment.NewLine;
            output += "<states>" + Environment.NewLine +
                "<physics params=\"\">false</physics>" + Environment.NewLine +
                "<temporary params=\"\">false</temporary>" + Environment.NewLine +
                "<phantom params=\"\">false</phantom>" + Environment.NewLine +
                "</states>" + Environment.NewLine;
            output += "<properties>" + Environment.NewLine +
                "<levelofdetail val=\"9\" />" + Environment.NewLine;

            if (prim.ProfileCurve == 1 && prim.PathCurve == 16)
            {
                type = 0;
            }
            else if (prim.ProfileCurve == 0 && prim.PathCurve == 16)
            {
                type = 1;
            }
            else if (prim.ProfileCurve == 3 && prim.PathCurve == 16)
            {
                type = 2;
            }
            else if (prim.ProfileCurve == 5 && prim.PathCurve == 32)
            {
                type = 3;
            }
            else if (prim.ProfileCurve == 0 && prim.PathCurve == 32)
            {
                type = 4;
            }
            else if (prim.ProfileCurve == 1 && prim.PathCurve == 32)
            {
                type = 5;
            }
            else if (prim.ProfileCurve == 3 && prim.PathCurve == 32)
            {
                type = 6;
            }
            else
            {
                Console.WriteLine("Unhandled prim type, ProfileCurve=" +
                    prim.ProfileCurve + ", PathCurve=" + prim.PathCurve);
                type = 0;
            }

            output += "<type val=\"" + type + "\" />" + Environment.NewLine;
            output += "<position x=\"" + string.Format("{0:F6}", prim.Position.X) +
                "\" y=\"" + string.Format("{0:F6}", prim.Position.Y) +
                "\" z=\"" + string.Format("{0:F6}", prim.Position.Z) + "\" />" + Environment.NewLine;
            output += "<rotation x=\"" + string.Format("{0:F6}", prim.Rotation.X) +
                "\" y=\"" + string.Format("{0:F6}", prim.Rotation.Y) +
                "\" z=\"" + string.Format("{0:F6}", prim.Rotation.Z) +
                "\" s=\"" + string.Format("{0:F6}", prim.Rotation.S) + "\" />" + Environment.NewLine;
            output += "<size x=\"" + string.Format("{0:F3}", prim.Scale.X) +
                "\" y=\"" + string.Format("{0:F3}", prim.Scale.Y) +
                "\" z=\"" + string.Format("{0:F3}", prim.Scale.Z) + "\" />" + Environment.NewLine;
            
            if (type == 1)
            {
                output += "<cut x=\"" + prim.ProfileBegin + "\" y=\"" + prim.ProfileEnd + "\" />" + Environment.NewLine;
                output += "<dimple x=\"" + prim.PathBegin + "\" y=\"" + prim.PathEnd + "\" />" + Environment.NewLine;
            }
            else
            {
                output += "<cut x=\"" + prim.PathBegin + "\" y=\"" + prim.PathEnd + "\" />" + Environment.NewLine;
                output += "<dimple x=\"" + prim.ProfileBegin + "\" y=\"" + prim.ProfileEnd + "\" />" + Environment.NewLine;
            }
            
            output += "<advancedcut x=\"" + prim.ProfileBegin + "\" y=\"" + prim.ProfileEnd + "\" />" + Environment.NewLine;
            output += "<hollow val=\"" + prim.ProfileHollow + "\" />" + Environment.NewLine;
            output += "<twist x=\"" + prim.PathTwistBegin + "\" y=\"" + prim.PathTwist + "\" />" + Environment.NewLine;
            output += "<topsize x=\"" + Math.Abs(prim.PathScaleX - 1.0F) + "\" y=\"" +
                Math.Abs(prim.PathScaleY - 1.0F) + "\" />" + Environment.NewLine;
            output += "<holesize x=\"" + (1.0F - prim.PathScaleX) + "\" y=\"" + (1.0F - prim.PathScaleY) + "\" />" + Environment.NewLine;
            output += "<topshear x=\"" + prim.PathShearX + "\" y=\"" + prim.PathShearY + "\" />" + Environment.NewLine;
            // prim.blender stores taper values a bit different than the SL network layer
            output += "<taper x=\"" + /*Math.Abs(prim.PathScaleX - 1.0F)*/ prim.PathTaperX + "\" y=\"" +
                /*Math.Abs(prim.PathScaleY - 1.0F)*/ prim.PathTaperY + "\" />" + Environment.NewLine;
            output += "<revolutions val=\"" + prim.PathRevolutions + "\" />" + Environment.NewLine;
            output += "<radiusoffset val=\"" + prim.PathRadiusOffset + "\" />" + Environment.NewLine;
            output += "<skew val=\"" + prim.PathSkew + "\" />" + Environment.NewLine;
            output += "<material val=\"" + prim.Material + "\" />" + Environment.NewLine;
            // TODO: Hollowshape. 16-21 = circle, 32-37 = square, 48-53 = triangle
            output += "<hollowshape val=\"0\" />" + Environment.NewLine;

            output += "<textures params=\"\">" + Environment.NewLine +
                "</textures>" + Environment.NewLine +
                "<scripts params=\"\">" + Environment.NewLine +
                "</scripts>" + Environment.NewLine +
                "</properties>" + Environment.NewLine +
                "</primitive>" + Environment.NewLine;

            Log(output);
        }

        private void cmdConnect_Click(object sender, EventArgs e)
        {
            cmdCapture.Text = "Start Capture";
            cmdCapture.Enabled = false;

            if (cmdConnect.Text == "Connect")
            {
                cmdConnect.Text = "Disconnect";
                txtFirstName.Enabled = txtLastName.Enabled = txtPassword.Enabled = false;

                Hashtable loginParams = NetworkManager.DefaultLoginValues(txtFirstName.Text,
                    txtLastName.Text, txtPassword.Text, "00:00:00:00:00:00", "last", 1, 50, 50, 50,
                    "Win", "0", "primexport", "jhurliman@wsu.edu");

                // HAX
                cmdCapture.Text = "Stop Capture";
                client.Objects.OnNewPrim += primCallback;

                if (client.Network.Login(loginParams))
                {
                    cmdCapture.Enabled = true;
                }
                else
                {
                    MessageBox.Show(this, "Error logging in: " + client.Network.LoginError);
                    cmdConnect.Text = "Connect";
                    txtFirstName.Enabled = txtLastName.Enabled = txtPassword.Enabled = true;
                }
            }
            else
            {
                client.Network.Logout();
                cmdConnect.Text = "Connect";
                txtFirstName.Enabled = txtLastName.Enabled = txtPassword.Enabled = true;
            }
        }

        private void cmdCapture_Click(object sender, EventArgs e)
        {
            if (cmdCapture.Text == "Start Capture")
            {
                cmdCapture.Text = "Stop Capture";
                client.Objects.OnNewPrim += primCallback;
            }
            else
            {
                cmdCapture.Text = "Start Capture";
                client.Objects.OnNewPrim -= primCallback;
            }
        }
    }
}
