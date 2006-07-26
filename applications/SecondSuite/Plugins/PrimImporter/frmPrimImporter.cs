using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml;
using System.Threading;
using libsecondlife;

namespace SecondSuite.Plugins
{
	/// <summary>
	/// Summary description for frmPrimImporter.
	/// </summary>
	public class frmPrimImporter : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button cmdImport;
		private System.Windows.Forms.TextBox txtLog;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		// libsecondlife instance
		private SecondLife Client;
		bool WaitingOnUpdate = false;
		PrimObject CurrentPrim;
		Mutex CurrentPrimMutex = new Mutex(false, "CurrentPrimMutex");

		public frmPrimImporter(SecondLife client)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			Client = client;

			// Install our packet handlers
			Client.Network.RegisterCallback("ObjectAdd", new PacketCallback(ObjectAddHandler));
			Client.Network.RegisterCallback("ObjectUpdate", new PacketCallback(ObjectUpdateHandler));
		}

		public void ObjectAddHandler(Packet packet, Simulator simulator)
		{
			LLVector3 position = null;

			if (WaitingOnUpdate)
			{
				CurrentPrimMutex.WaitOne();

				foreach (Block block in packet.Blocks())
				{
					foreach (Field field in block.Fields)
					{
						if (field.Layout.Name == "RayEnd")
						{
							position = (LLVector3)field.Data;
						}
					}
				}

				txtLog.AppendText("Received an ObjectAdd, setting CurrentPrim position to " + position.ToString());
				CurrentPrim.Position = position;

				CurrentPrimMutex.ReleaseMutex();
			}
		}

		public void ObjectUpdateHandler(Packet packet, Simulator simulator)
		{
			uint id = 0;
			LLUUID uuid = null;

			if (WaitingOnUpdate)
			{
				CurrentPrimMutex.WaitOne();

				foreach (Block block in packet.Blocks())
				{
					foreach (Field field in block.Fields)
					{
						if (field.Layout.Name == "ID")
						{
							id = (uint)field.Data;
						}
						else if (field.Layout.Name == "FullID")
						{
							uuid = (LLUUID)field.Data;
						}
						else if (field.Layout.Name == "ObjectData")
						{
							byte[] byteArray = (byte[])field.Data;
							LLVector3 position = new LLVector3(byteArray, 0);
							if (CurrentPrim != null && position != CurrentPrim.Position)
							{
								txtLog.AppendText(position.ToString() + " doesn't match CurrentPrim.Position " + 
									CurrentPrim.Position.ToString() + "\n"/* + ", ignoring"*/);
								//return;
							}
						}
					}
				}

				CurrentPrim.ID = id;
				CurrentPrim.UUID = uuid;

				WaitingOnUpdate = false;

				CurrentPrimMutex.ReleaseMutex();
			}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		public void Connected()
		{
			;
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.cmdImport = new System.Windows.Forms.Button();
			this.txtLog = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// cmdImport
			// 
			this.cmdImport.Location = new System.Drawing.Point(376, 304);
			this.cmdImport.Name = "cmdImport";
			this.cmdImport.Size = new System.Drawing.Size(104, 24);
			this.cmdImport.TabIndex = 56;
			this.cmdImport.Text = "Import Structure";
			this.cmdImport.Click += new System.EventHandler(this.cmdImport_Click);
			// 
			// txtLog
			// 
			this.txtLog.Location = new System.Drawing.Point(16, 16);
			this.txtLog.Multiline = true;
			this.txtLog.Name = "txtLog";
			this.txtLog.Size = new System.Drawing.Size(464, 232);
			this.txtLog.TabIndex = 57;
			this.txtLog.Text = "";
			// 
			// frmPrimImporter
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(496, 349);
			this.Controls.Add(this.txtLog);
			this.Controls.Add(this.cmdImport);
			this.MaximizeBox = false;
			this.MinimumSize = new System.Drawing.Size(456, 320);
			this.Name = "frmPrimImporter";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "Prim Importer";
			this.ResumeLayout(false);

		}
		#endregion

		private void cmdImport_Click(object sender, System.EventArgs e)
		{
			OpenFileDialog openDialog = new OpenFileDialog();
			openDialog.Title = "Open Prim.Blender File";
			openDialog.Filter = "All files (*.*)|*.*|Prim files (*.prims)|*.prims" ;
			openDialog.FilterIndex = 2;

			if(openDialog.ShowDialog() != DialogResult.OK)
			{
				return;
			}

			XmlDocument xml = new XmlDocument();
			XmlNodeList list = null;

			try
			{
				// Try to load the xml file
				xml.Load(openDialog.FileName);

				//If there is a document and it has children,
				if(xml != null && xml.HasChildNodes)
				{
					//Get the children into the temp list
					list = xml.GetElementsByTagName("primitive");
				}
				else
				{
					txtLog.AppendText("ERROR: Failed to parse " + openDialog.FileName + "\n");
					return;
				}
			}
			catch (Exception err)
			{
				txtLog.AppendText("ERROR: " + err.ToString() + "\n");
				return;
			}

			foreach (XmlNode node in list)
			{
				txtLog.AppendText("Parsing primitive " + node.Attributes["key"].Value + "\n");

				XmlNode properties = node["properties"];

				PrimObject prim = new PrimObject(new LLUUID("8955674724cb43ed920b47caed15465f"));

				prim.Material = Convert.ToUInt16(properties["material"].Attributes["val"].Value);
				prim.Name = node.Attributes["key"].Value;
				// Either PathBegin/End or ProfileBegin/End should be dimple
				prim.PathBegin = PrimObject.PathBeginByte(Convert.ToSingle(properties["cut"].Attributes["x"].Value));
				prim.PathEnd = PrimObject.PathEndByte(Convert.ToSingle(properties["cut"].Attributes["y"].Value));
				prim.PathRadiusOffset = PrimObject.PathRadiusOffsetByte(Convert.ToSingle(properties["radiusoffset"].Attributes["val"].Value));
				prim.PathRevolutions = PrimObject.PathRevolutionsByte(Convert.ToSingle(properties["revolutions"].Attributes["val"].Value));
				prim.PathScaleX = PrimObject.PathScaleByte(Convert.ToSingle(properties["topsize"].Attributes["x"].Value));
				prim.PathScaleY = PrimObject.PathScaleByte(Convert.ToSingle(properties["topsize"].Attributes["y"].Value));
				prim.PathShearX = PrimObject.PathShearByte(Convert.ToSingle(properties["topshear"].Attributes["x"].Value));
				prim.PathShearY = PrimObject.PathShearByte(Convert.ToSingle(properties["topshear"].Attributes["y"].Value));
				prim.PathSkew = PrimObject.PathSkewByte(Convert.ToSingle(properties["skew"].Attributes["val"].Value));
				prim.PathTaperX = PrimObject.PathTaperByte(Convert.ToSingle(properties["taper"].Attributes["x"].Value));
				prim.PathTaperY = PrimObject.PathTaperByte(Convert.ToSingle(properties["taper"].Attributes["y"].Value));
				prim.PathTwist = PrimObject.PathTwistByte(Convert.ToSingle(properties["twist"].Attributes["y"].Value));
				prim.PathTwistBegin = PrimObject.PathTwistByte(Convert.ToSingle(properties["twist"].Attributes["x"].Value));
				prim.ProfileBegin = PrimObject.ProfileBeginByte(Convert.ToSingle(properties["cut"].Attributes["x"].Value));
				prim.ProfileEnd = PrimObject.ProfileEndByte(Convert.ToSingle(properties["cut"].Attributes["y"].Value));
				ushort curve = Convert.ToUInt16(properties["type"].Attributes["val"].Value);
				switch (curve)
				{
					case 0:
						// Box
						prim.ProfileCurve = 1;
						prim.PathCurve = 16;
						break;
					case 1:
						// Cylinder
						prim.ProfileCurve = 0;
						prim.PathCurve = 16;
						break;
					case 2:
						// Prism
						prim.ProfileCurve = 3;
						prim.PathCurve = 16;
						break;
					case 3:
						// Sphere
						prim.ProfileCurve = 5;
						prim.PathCurve = 32;
						break;
					case 4:
						// Torus
						prim.ProfileCurve = 0;
						prim.PathCurve = 32;
						break;
					case 5:
						// Tube
						prim.ProfileCurve = 1;
						prim.PathCurve = 32;
						break;
					case 6:
						// Ring
						prim.ProfileCurve = 3;
						prim.PathCurve = 16;
						break;
				}
				prim.ProfileHollow = Convert.ToUInt32(properties["hollow"].Attributes["val"].Value);
				prim.Rotation = new LLQuaternion(
					Convert.ToSingle(properties["rotation"].Attributes["x"].Value),
					Convert.ToSingle(properties["rotation"].Attributes["y"].Value),
					Convert.ToSingle(properties["rotation"].Attributes["z"].Value),
					Convert.ToSingle(properties["rotation"].Attributes["s"].Value));
				prim.Scale = new LLVector3(
					Convert.ToSingle(properties["size"].Attributes["x"].Value),
					Convert.ToSingle(properties["size"].Attributes["y"].Value),
					Convert.ToSingle(properties["size"].Attributes["z"].Value));

				LLVector3 position = new LLVector3(
					Convert.ToSingle(properties["position"].Attributes["x"].Value) + (float)Client.Avatar.Position.X,
					Convert.ToSingle(properties["position"].Attributes["y"].Value) + (float)Client.Avatar.Position.Y,
					Convert.ToSingle(properties["position"].Attributes["z"].Value) + (float)Client.Avatar.Position.Z + 50.0F);
				prim.Position = position;

				CurrentPrim = prim;
				WaitingOnUpdate = true;

				Client.CurrentRegion.RezObject(prim, position, new LLVector3(Client.Avatar.Position));

				while (WaitingOnUpdate)
				{
					System.Threading.Thread.Sleep(100);
					Application.DoEvents();
				}

				txtLog.AppendText("Rezzed primitive with UUID " + CurrentPrim.UUID + " and ID " + CurrentPrim.ID + " \n");

				Hashtable blocks = new Hashtable();
				Hashtable fields = new Hashtable();

				/*fields["ObjectLocalID"] = CurrentPrim.ID;
				blocks[fields] = "ObjectData";

				fields = new Hashtable();

				fields["AgentID"] = Client.Network.AgentID;
				blocks[fields] = "AgentData";

				Packet packet = PacketBuilder.BuildPacket("ObjectSelect", Client.Protocol, blocks, Helpers.MSG_RELIABLE);
				Client.Network.SendPacket(packet);

				System.Threading.Thread.Sleep(100);*/
				Packet packet;

				byte[] byteArray = new byte[12];
				Array.Copy(position.GetBytes(), byteArray, 12);

				fields["Data"] = byteArray;
				fields["Type"] = (byte)9;
				fields["ObjectLocalID"] = CurrentPrim.ID;
				blocks[fields] = "ObjectData";

				fields = new Hashtable();

				fields["AgentID"] = Client.Network.AgentID;
				blocks[fields] = "AgentData";

				packet = PacketBuilder.BuildPacket("MultipleObjectUpdate", Client.Protocol, blocks, Helpers.MSG_RELIABLE);
				Client.Network.SendPacket(packet);
				Client.Network.SendPacket(packet);
				Client.Network.SendPacket(packet);

				System.Threading.Thread.Sleep(500);
			}
		}
	}
}
