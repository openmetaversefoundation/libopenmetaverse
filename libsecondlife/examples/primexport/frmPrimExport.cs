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
        private Label label4;
        private Label label5;
        private Panel panel1;
        private RadioButton radObjects;
        private RadioButton radEntireSim;
        private Panel panel2;
        private RadioButton radPrimBlender;
        private RadioButton radLibPrims;
        private System.ComponentModel.IContainer components = null;

        //
        private SecondLife client;
        private Dictionary<ulong, Primitive> Prims = new Dictionary<ulong, Primitive>();
        private List<ulong> Avatars = new List<ulong>();
        private List<ulong> Attachments = new List<ulong>();
        private string CurrentText = "";
        private string Filename = "";
        private bool EntireSim = true;

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
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.radObjects = new System.Windows.Forms.RadioButton();
            this.radEntireSim = new System.Windows.Forms.RadioButton();
            this.panel2 = new System.Windows.Forms.Panel();
            this.radPrimBlender = new System.Windows.Forms.RadioButton();
            this.radLibPrims = new System.Windows.Forms.RadioButton();
            this.grpLogin.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
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
            this.grpLogin.Location = new System.Drawing.Point(12, 322);
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
            this.cmdExport.Location = new System.Drawing.Point(452, 33);
            this.cmdExport.Name = "cmdExport";
            this.cmdExport.Size = new System.Drawing.Size(120, 24);
            this.cmdExport.TabIndex = 52;
            this.cmdExport.Text = "Export Prims";
            this.cmdExport.Click += new System.EventHandler(this.cmdExport_Click);
            // 
            // txtLog
            // 
            this.txtLog.Location = new System.Drawing.Point(12, 63);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(560, 253);
            this.txtLog.TabIndex = 53;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(12, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(40, 13);
            this.label4.TabIndex = 56;
            this.label4.Text = "Export:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(12, 40);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(42, 13);
            this.label5.TabIndex = 59;
            this.label5.Text = "Format:";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.radObjects);
            this.panel1.Controls.Add(this.radEntireSim);
            this.panel1.Location = new System.Drawing.Point(67, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(194, 31);
            this.panel1.TabIndex = 60;
            // 
            // radObjects
            // 
            this.radObjects.AutoSize = true;
            this.radObjects.Enabled = false;
            this.radObjects.Location = new System.Drawing.Point(81, 8);
            this.radObjects.Name = "radObjects";
            this.radObjects.Size = new System.Drawing.Size(97, 17);
            this.radObjects.TabIndex = 57;
            this.radObjects.Text = "Object/Linkjset";
            this.radObjects.UseVisualStyleBackColor = true;
            // 
            // radEntireSim
            // 
            this.radEntireSim.AutoSize = true;
            this.radEntireSim.Checked = true;
            this.radEntireSim.Location = new System.Drawing.Point(3, 8);
            this.radEntireSim.Name = "radEntireSim";
            this.radEntireSim.Size = new System.Drawing.Size(72, 17);
            this.radEntireSim.TabIndex = 56;
            this.radEntireSim.TabStop = true;
            this.radEntireSim.Text = "Entire Sim";
            this.radEntireSim.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.radPrimBlender);
            this.panel2.Controls.Add(this.radLibPrims);
            this.panel2.Location = new System.Drawing.Point(67, 31);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(288, 31);
            this.panel2.TabIndex = 61;
            // 
            // radPrimBlender
            // 
            this.radPrimBlender.AutoSize = true;
            this.radPrimBlender.Location = new System.Drawing.Point(68, 7);
            this.radPrimBlender.Name = "radPrimBlender";
            this.radPrimBlender.Size = new System.Drawing.Size(168, 17);
            this.radPrimBlender.TabIndex = 60;
            this.radPrimBlender.Text = "prim.Blender (currently broken)";
            this.radPrimBlender.UseVisualStyleBackColor = true;
            // 
            // radLibPrims
            // 
            this.radLibPrims.AutoSize = true;
            this.radLibPrims.Checked = true;
            this.radLibPrims.Location = new System.Drawing.Point(3, 7);
            this.radLibPrims.Name = "radLibPrims";
            this.radLibPrims.Size = new System.Drawing.Size(59, 17);
            this.radLibPrims.TabIndex = 59;
            this.radLibPrims.TabStop = true;
            this.radLibPrims.Text = "libprims";
            this.radLibPrims.UseVisualStyleBackColor = true;
            // 
            // frmPrimExport
            // 
            this.ClientSize = new System.Drawing.Size(587, 414);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.cmdExport);
            this.Controls.Add(this.grpLogin);
            this.MaximizeBox = false;
            this.Name = "frmPrimExport";
            this.Text = "Prim Exporter";
            this.TopMost = true;
            this.grpLogin.ResumeLayout(false);
            this.grpLogin.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
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

            client = new SecondLife();

            // Setup the callbacks
            client.OnLogMessage += new SecondLife.LogCallback(client_OnLogMessage);
            client.Objects.OnNewPrim += new ObjectManager.NewPrimCallback(PrimSeen);
            client.Objects.OnNewAvatar += new ObjectManager.NewAvatarCallback(AvatarSeen);
            client.Objects.OnNewAttachment += new ObjectManager.NewAttachmentCallback(AttachmentSeen);

            // Throttle down unnecessary things
            client.Throttle.Cloud = 0;
            client.Throttle.Land = 0;
            client.Throttle.Wind = 0;

            // Make sure we download all objects
            client.Settings.ALWAYS_REQUEST_OBJECTS = true;

            grpLogin.Enabled = true;
        }

        void client_OnLogMessage(string message, Helpers.LogLevel level)
        {
            Log("libsl: " + level.ToString() + ": " + message + Environment.NewLine);
        }

        private void Log(string text)
        {
            CurrentText = text;

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
            txtLog.AppendText(CurrentText);
        }

        private void PrimSeen(Simulator simulator, Primitive prim, ulong regionHandle, ushort timeDilation)
        {
            lock (Prims)
            {
                if (Prims.ContainsKey(prim.LocalID))
                {
                    Prims.Remove(prim.LocalID);
                }

                Prims.Add(prim.LocalID, prim);
                Log(".");
            }
        }

        void AttachmentSeen(Simulator simulator, Primitive prim, ulong regionHandle, ushort timeDilation)
        {
            lock (Attachments)
            {
                Attachments.Add(prim.LocalID);
            }
        }

        void AvatarSeen(Simulator simulator, Avatar avatar, ulong regionHandle, ushort timeDilation)
        {
            lock (Avatars)
            {
                Avatars.Add(avatar.LocalID);
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

                if (radLibPrims.Checked)
                {
                    ExportLibPrims(stream);
                }
                else
                {
                    ExportPrimBlender(stream);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("There was an error writing the prims file, check the log for details",
                    "primexport Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Log("Error writing prims to " + Filename + ": " + e.ToString() + Environment.NewLine);
            }
            finally
            {
                if (stream != null) stream.Close();
                if (file != null) file.Close();
                cmdExport.Enabled = true;
            }
        }

        private void ExportPrimBlender(StreamWriter stream)
        {
            uint type = 0;
            string output;

            lock (Prims)
            {
                stream.WriteLine("<primitives>");

                foreach (Primitive prim in Prims.Values)
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
                        else if (Avatars.Contains(prim.ParentID) || Attachments.Contains(prim.ParentID))
                        {
                            // Skip this
                        }
                        else
                        {
                            // We don't have the base position for this child prim, can't render it
                            Log("Couldn't export child prim " + prim.ID.ToString() + ", parent prim is missing" +
                                Environment.NewLine);
                            continue;
                        }
                    }

                    output += "<primitive name=\"Object\" description=\"\" key=\"Num_000" + prim.LocalID + "\" version=\"2\">" + Environment.NewLine;
                    output += "<states><physics params=\"\">false</physics><temporary params=\"\">false</temporary><phantom params=\"\">false</phantom></states>" + Environment.NewLine;
                    output += "<properties>" + Environment.NewLine +
                        "<levelofdetail val=\"9\" />" + Environment.NewLine;

                    LLObject.ObjectData data = prim.Data;

                    switch (data.ProfileCurve + data.PathCurve)
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
                                data.ProfileCurve + ", PathCurve=" + data.PathCurve + Environment.NewLine);
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
                        output += "<cut x=\"" + data.ProfileBegin + "\" y=\"" + data.ProfileEnd + "\" />" + Environment.NewLine;
                        output += "<dimple x=\"" + data.PathBegin + "\" y=\"" + data.PathEnd + "\" />" + Environment.NewLine;
                    }
                    else
                    {
                        output += "<cut x=\"" + data.PathBegin + "\" y=\"" + data.PathEnd + "\" />" + Environment.NewLine;
                        output += "<dimple x=\"" + data.ProfileBegin + "\" y=\"" + data.ProfileEnd + "\" />" + Environment.NewLine;
                    }

                    output += "<advancedcut x=\"" + data.ProfileBegin + "\" y=\"" + data.ProfileEnd + "\" />" + Environment.NewLine;
                    output += "<hollow val=\"" + data.ProfileHollow + "\" />" + Environment.NewLine;
                    output += "<twist x=\"" + data.PathTwistBegin + "\" y=\"" + data.PathTwist + "\" />" + Environment.NewLine;
                    output += "<topsize x=\"" + Math.Abs(data.PathScaleX - 1.0f) + "\" y=\"" +
                        Math.Abs(data.PathScaleY - 1.0f) + "\" />" + Environment.NewLine;
                    output += "<holesize x=\"" + (1.0f - data.PathScaleX) + "\" y=\"" + (1.0f - data.PathScaleY) + "\" />" + Environment.NewLine;
                    output += "<topshear x=\"" + data.PathShearX + "\" y=\"" + data.PathShearY + "\" />" + Environment.NewLine;
                    output += "<taper x=\"" + data.PathTaperX + "\" y=\"" + data.PathTaperY + "\" />" + Environment.NewLine;
                    output += "<revolutions val=\"" + data.PathRevolutions + "\" />" + Environment.NewLine;
                    output += "<radiusoffset val=\"" + data.PathRadiusOffset + "\" />" + Environment.NewLine;
                    output += "<skew val=\"" + data.PathSkew + "\" />" + Environment.NewLine;
                    output += "<material val=\"" + data.Material + "\" />" + Environment.NewLine;
                    // FIXME: Hollowshape. 16-21 = circle, 32-37 = square, 48-53 = triangle
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

        private void ExportLibPrims(StreamWriter stream)
        {
            lock (Prims)
            {
                stream.WriteLine("<Primitives>");

                foreach (Primitive prim in Prims.Values)
                {
                    if (prim.ParentID != 0)
                    {
                        // This prim is part of a linkset, we need to adjust it's position and rotation
                        if (Prims.ContainsKey(prim.ParentID))
                        {
                            // FIXME: Rewrite this when the xml serialization stuff is complete
                            //stream.WriteLine(prim.GetXml());
                        }
                        else if (Avatars.Contains(prim.ParentID) || Attachments.Contains(prim.ParentID))
                        {
                            // Skip this
                        }
                        else
                        {
                            // We don't have the base position for this child prim, can't render it
                            Log("Couldn't export child prim " + prim.ID.ToString() + ", parent prim is missing" +
                                Environment.NewLine);
                        }
                    }
                }

                stream.WriteLine("</Primitives>");
            }
        }

        private void cmdConnect_Click(object sender, EventArgs e)
        {
            if (cmdConnect.Text == "Connect")
            {
                cmdConnect.Text = "Disconnect";
                txtFirstName.Enabled = txtLastName.Enabled = txtPassword.Enabled = false;

                if (client.Network.Login(txtFirstName.Text, txtLastName.Text, txtPassword.Text,
                    "primexport", "jhurliman@wsu.edu"))
                {
                    ;
                }
                else
                {
                    MessageBox.Show(this, "Error logging in: " + client.Network.LoginMessage);
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

            if (radLibPrims.Checked)
            {
                save.Filter = "libprims files (*.xml)|*.xml";
            }
            else
            {
                save.Filter = "Prim.Blender files (*.prims)|*.prims";
            }

            save.RestoreDirectory = true;

            if (save.ShowDialog() == DialogResult.OK)
            {
                EntireSim = radEntireSim.Checked;
                
                Filename = save.FileName;
                cmdExport.Enabled = false;
                Invoke(new MethodInvoker(ExportPrims));
            }
        }
    }
}
