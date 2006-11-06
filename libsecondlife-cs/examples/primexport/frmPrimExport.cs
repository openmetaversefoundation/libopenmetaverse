using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
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
        private Button cmdExport;
        private TextBox txtLog;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private SecondLife client;
        private NewPrimCallback primCallback;
        private string currentText;
        private Dictionary<ulong, PrimObject> Prims;
        private string Filename = "";

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
            this.cmdExport = new System.Windows.Forms.Button();
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
            // cmdExport
            // 
            this.cmdExport.Location = new System.Drawing.Point(12, 12);
            this.cmdExport.Name = "cmdExport";
            this.cmdExport.Size = new System.Drawing.Size(560, 49);
            this.cmdExport.TabIndex = 52;
            this.cmdExport.Text = "Export Prims";
            this.cmdExport.Click += new System.EventHandler(this.cmdExport_Click);
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
            this.Controls.Add(this.cmdExport);
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

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            frmPrimExport exportForm = new frmPrimExport();
            exportForm.ShowDialog();
        }

        public frmPrimExport()
        {
            InitializeComponent();

            primCallback = new NewPrimCallback(PrimSeen);
            Prims = new Dictionary<ulong, PrimObject>();

            client = new SecondLife();
            client.OnLogMessage += new LogCallback(client_OnLogMessage);
            client.Objects.RequestAllObjects = true;
            client.Objects.OnNewPrim += primCallback;

            grpLogin.Enabled = true;
        }

        void client_OnLogMessage(string message, Helpers.LogLevel level)
        {
            Log("libsl: " + level.ToString() + ": " + message);
        }

        private void Log(string text)
        {
            currentText = text;

            lock (txtLog)
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new MethodInvoker(UpdateLog));
                }
                else
                {
                    UpdateLog();
                }
            }
        }

        private void UpdateLog()
        {
            txtLog.AppendText(currentText + Environment.NewLine);
        }

        private void PrimSeen(Simulator simulator, PrimObject prim, ulong regionHandle, ushort timeDilation)
        {
            lock (Prims)
            {
                if (Prims.ContainsKey(prim.LocalID))
                {
                    Prims.Remove(prim.LocalID);
                }

                Prims.Add(prim.LocalID, prim);
                Log("Saw prim " + prim.ID.ToString());
            }
        }

        private void ExportPrims()
        {
            FileStream file = null;
            StreamWriter stream = null;

            try
            {
                file = new FileStream(Filename, FileMode.Create);
                stream = new StreamWriter(file);

                uint type = 0;
                string output;

                stream.WriteLine("<primitives>");

                lock (Prims)
                {
                    foreach (PrimObject prim in Prims.Values)
                    {
                        LLVector3 position = prim.Position;
                        LLQuaternion rotation = prim.Rotation;

                        output = "";

                        if (prim.ParentID != 0)
                        {
                            // This prim is part of a linkset, we need to adjust it's position and rotation
                            if (Prims.ContainsKey(prim.ParentID))
                            {
                                // The child prim only stores a relative position, add the world position of the parent prim
                                position += Prims[prim.ParentID].Position;

                                // The child prim only stores a relative rotation, start with the parent prim rotation
                                rotation = rotation * Prims[prim.ParentID].Rotation;
                            }
                            else
                            {
                                // We don't have the base position for this child prim, can't render it
                                Log("Couldn't export child prim " + prim.ID.ToString() + ", parent prim is missing");
                                continue;
                            }
                        }

                        output += "<primitive name=\"Object\" description=\"\" key=\"Num_000" + prim.LocalID + "\" version=\"2\">" + Environment.NewLine;
                        output += "<states><physics params=\"\">false</physics><temporary params=\"\">false</temporary><phantom params=\"\">false</phantom></states>" + Environment.NewLine;
                        output += "<properties>" + Environment.NewLine +
                            "<levelofdetail val=\"9\" />" + Environment.NewLine;

                        switch (prim.ProfileCurve + prim.PathCurve)
                        {
                            case 17:
                                // PRIM_TYPE_BOX
                                type = 0;
                                break;
                            case 16:
                                // PRIM_TYPE_CYLINDER
                                type = 1;
                                break;
                            case 19:
                                // PRIM_TYPE_PRISM
                                type = 2;
                                break;
                            case 37:
                                // PRIM_TYPE_SPHERE
                                type = 3;
                                break;
                            case 32:
                                // PRIM_TYPE_TORUS
                                type = 4;
                                break;
                            case 33:
                                // PRIM_TYPE_TUBE
                                type = 5;
                                break;
                            case 35:
                                // PRIM_TYPE_RING
                                type = 6;
                                break;
                            default:
                                Log("Not exporting an unhandled prim, ProfileCurve=" +
                                    prim.ProfileCurve + ", PathCurve=" + prim.PathCurve);
                                continue;
                        }

                        output += "<type val=\"" + type + "\" />" + Environment.NewLine;
                        output += "<position x=\"" + string.Format("{0:F6}", position.X) +
                            "\" y=\"" + string.Format("{0:F6}", position.Y) +
                            "\" z=\"" + string.Format("{0:F6}", position.Z) + "\" />" + Environment.NewLine;
                        output += "<rotation x=\"" + string.Format("{0:F6}", rotation.X) +
                            "\" y=\"" + string.Format("{0:F6}", rotation.Y) +
                            "\" z=\"" + string.Format("{0:F6}", rotation.Z) +
                            "\" s=\"" + string.Format("{0:F6}", rotation.W) + "\" />" + Environment.NewLine;
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
                        output += "<taper x=\"" + /*Math.Abs(prim.PathScaleX - 1.0F)*/ prim.PathTaperX + "\" y=\"" +
                            /*Math.Abs(prim.PathScaleY - 1.0F)*/ prim.PathTaperY + "\" />" + Environment.NewLine;
                        output += "<revolutions val=\"" + prim.PathRevolutions + "\" />" + Environment.NewLine;
                        output += "<radiusoffset val=\"" + prim.PathRadiusOffset + "\" />" + Environment.NewLine;
                        output += "<skew val=\"" + prim.PathSkew + "\" />" + Environment.NewLine;
                        output += "<material val=\"" + prim.Material + "\" />" + Environment.NewLine;
                        // TODO: Hollowshape. 16-21 = circle, 32-37 = square, 48-53 = triangle
                        output += "<hollowshape val=\"0\" />" + Environment.NewLine;

                        output += "<textures params=\"\">" +
                            "</textures>" +
                            "<scripts params=\"\">" +
                            "</scripts>" + Environment.NewLine +
                            "</properties>" + Environment.NewLine +
                            "</primitive>" + Environment.NewLine;

                        stream.WriteLine(output);
                    }
                }

                stream.WriteLine("</primitives>");
            }
            catch (Exception e)
            {
                MessageBox.Show("There was an error writing the prims file, check the log for details",
                    "primexport Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Log("Error writing prims to " + Filename + ": " + e.ToString());
            }
            finally
            {
                if (stream != null) stream.Close();
                if (file != null) file.Close();
                cmdExport.Enabled = true;
            }
        }

        private void cmdConnect_Click(object sender, EventArgs e)
        {
            if (cmdConnect.Text == "Connect")
            {
                cmdConnect.Text = "Disconnect";
                txtFirstName.Enabled = txtLastName.Enabled = txtPassword.Enabled = false;

                if (!client.Network.Login(txtFirstName.Text, txtLastName.Text, txtPassword.Text, 
                    "primexport", "jhurliman@wsu.edu"))
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

        private void cmdExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "Prim.Blender files (*.prims)|*.prims";
            save.RestoreDirectory = true;

            if (save.ShowDialog() == DialogResult.OK)
            {
                Filename = save.FileName;
                cmdExport.Enabled = false;
                Invoke(new MethodInvoker(ExportPrims));
            }
        }
    }
}
