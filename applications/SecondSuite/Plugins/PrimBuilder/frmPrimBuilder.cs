using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using libsecondlife;

namespace SecondSuite.Plugins
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class frmPrimBuilder : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button cmdBuild;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.Label label15;
		private System.Windows.Forms.Label label16;
		private System.Windows.Forms.Label label19;
		private System.Windows.Forms.Label label20;
		private System.Windows.Forms.Label label21;
		private System.Windows.Forms.Label label22;
		private System.Windows.Forms.Label label23;
		private System.Windows.Forms.Label label24;
		private System.Windows.Forms.Label label25;
		private System.Windows.Forms.Label label28;
		private System.Windows.Forms.Label label26;
		private System.Windows.Forms.Label label27;
		private System.Windows.Forms.Label label29;
		private System.Windows.Forms.Label label30;
		private System.Windows.Forms.Label label31;
		private System.Windows.Forms.Label label32;
		private System.Windows.Forms.Label label33;
		private System.Windows.Forms.Label label34;
		private System.Windows.Forms.NumericUpDown numScaleZ;
		private System.Windows.Forms.Label label35;
		private System.Windows.Forms.NumericUpDown numScaleY;
		private System.Windows.Forms.Label label36;
		private System.Windows.Forms.NumericUpDown numScaleX;
		private System.Windows.Forms.Label label37;
		private System.Windows.Forms.NumericUpDown numRotationZ;
		private System.Windows.Forms.Label label38;
		private System.Windows.Forms.NumericUpDown numRotationY;
		private System.Windows.Forms.Label label39;
		private System.Windows.Forms.NumericUpDown numRotationX;
		private System.Windows.Forms.Label label40;
		private System.Windows.Forms.NumericUpDown numRotationS;
		private System.Windows.Forms.NumericUpDown numProfileHollow;
		private System.Windows.Forms.NumericUpDown numPathCurve;
		private System.Windows.Forms.NumericUpDown numProfileCurve;
		private System.Windows.Forms.NumericUpDown numProfileBegin;
		private System.Windows.Forms.NumericUpDown numProfileEnd;
		private System.Windows.Forms.NumericUpDown numPathTwistBegin;
		private System.Windows.Forms.NumericUpDown numPathTwist;
		private System.Windows.Forms.NumericUpDown numPathTaperX;
		private System.Windows.Forms.NumericUpDown numPathTaperY;
		private System.Windows.Forms.NumericUpDown numPathShearX;
		private System.Windows.Forms.NumericUpDown numPathShearY;
		private System.Windows.Forms.NumericUpDown numPathScaleX;
		private System.Windows.Forms.NumericUpDown numPathScaleY;
		private System.Windows.Forms.NumericUpDown numPathRevolutions;
		private System.Windows.Forms.NumericUpDown numPathRadiusOffset;
		private System.Windows.Forms.NumericUpDown numPathBegin;
		private System.Windows.Forms.NumericUpDown numPathEnd;
		private System.Windows.Forms.NumericUpDown numMaterial;
		private System.Windows.Forms.TextBox txtName;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.NumericUpDown numPositionZ;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.NumericUpDown numPositionY;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.NumericUpDown numPositionX;
		private System.Windows.Forms.Label label41;
		private System.Windows.Forms.NumericUpDown numPathSkew;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		//
		SecondLife Client;

		public frmPrimBuilder(SecondLife client)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			Client = client;
			Client.Network.RegisterCallback("CoarseLocationUpdate", new PacketCallback(LocationHandler));
		}

		private void LocationHandler(Packet packet, Circuit circuit)
		{
			if (numPositionX.Value == 0)
			{
				numPositionX.Value = (decimal)Client.Avatar.Position.X;
			}

			if (numPositionY.Value == 0)
			{
				numPositionY.Value = (decimal)Client.Avatar.Position.Y;
			}

			if (numPositionZ.Value == 0)
			{
				numPositionZ.Value = (decimal)Client.Avatar.Position.Z;
			}
		}

		public void Connected()
		{
			;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.cmdBuild = new System.Windows.Forms.Button();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.label12 = new System.Windows.Forms.Label();
			this.label13 = new System.Windows.Forms.Label();
			this.label14 = new System.Windows.Forms.Label();
			this.label15 = new System.Windows.Forms.Label();
			this.label16 = new System.Windows.Forms.Label();
			this.label19 = new System.Windows.Forms.Label();
			this.label20 = new System.Windows.Forms.Label();
			this.label21 = new System.Windows.Forms.Label();
			this.label22 = new System.Windows.Forms.Label();
			this.label23 = new System.Windows.Forms.Label();
			this.label24 = new System.Windows.Forms.Label();
			this.label25 = new System.Windows.Forms.Label();
			this.label28 = new System.Windows.Forms.Label();
			this.label26 = new System.Windows.Forms.Label();
			this.label27 = new System.Windows.Forms.Label();
			this.label29 = new System.Windows.Forms.Label();
			this.label30 = new System.Windows.Forms.Label();
			this.label31 = new System.Windows.Forms.Label();
			this.label32 = new System.Windows.Forms.Label();
			this.label33 = new System.Windows.Forms.Label();
			this.label34 = new System.Windows.Forms.Label();
			this.numScaleZ = new System.Windows.Forms.NumericUpDown();
			this.label35 = new System.Windows.Forms.Label();
			this.numScaleY = new System.Windows.Forms.NumericUpDown();
			this.label36 = new System.Windows.Forms.Label();
			this.numScaleX = new System.Windows.Forms.NumericUpDown();
			this.label37 = new System.Windows.Forms.Label();
			this.numRotationZ = new System.Windows.Forms.NumericUpDown();
			this.label38 = new System.Windows.Forms.Label();
			this.numRotationY = new System.Windows.Forms.NumericUpDown();
			this.label39 = new System.Windows.Forms.Label();
			this.numRotationX = new System.Windows.Forms.NumericUpDown();
			this.label40 = new System.Windows.Forms.Label();
			this.numRotationS = new System.Windows.Forms.NumericUpDown();
			this.numProfileHollow = new System.Windows.Forms.NumericUpDown();
			this.numPathCurve = new System.Windows.Forms.NumericUpDown();
			this.numProfileCurve = new System.Windows.Forms.NumericUpDown();
			this.numProfileBegin = new System.Windows.Forms.NumericUpDown();
			this.numProfileEnd = new System.Windows.Forms.NumericUpDown();
			this.numPathTwistBegin = new System.Windows.Forms.NumericUpDown();
			this.numPathTwist = new System.Windows.Forms.NumericUpDown();
			this.numPathTaperX = new System.Windows.Forms.NumericUpDown();
			this.numPathTaperY = new System.Windows.Forms.NumericUpDown();
			this.numPathShearX = new System.Windows.Forms.NumericUpDown();
			this.numPathShearY = new System.Windows.Forms.NumericUpDown();
			this.numPathSkew = new System.Windows.Forms.NumericUpDown();
			this.numPathScaleX = new System.Windows.Forms.NumericUpDown();
			this.numPathScaleY = new System.Windows.Forms.NumericUpDown();
			this.numPathRevolutions = new System.Windows.Forms.NumericUpDown();
			this.numPathRadiusOffset = new System.Windows.Forms.NumericUpDown();
			this.numPathBegin = new System.Windows.Forms.NumericUpDown();
			this.numPathEnd = new System.Windows.Forms.NumericUpDown();
			this.numMaterial = new System.Windows.Forms.NumericUpDown();
			this.txtName = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.numPositionZ = new System.Windows.Forms.NumericUpDown();
			this.label2 = new System.Windows.Forms.Label();
			this.numPositionY = new System.Windows.Forms.NumericUpDown();
			this.label1 = new System.Windows.Forms.Label();
			this.numPositionX = new System.Windows.Forms.NumericUpDown();
			this.label41 = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.numScaleZ)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numScaleY)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numScaleX)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numRotationZ)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numRotationY)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numRotationX)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numRotationS)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numProfileHollow)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numPathCurve)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numProfileCurve)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numProfileBegin)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numProfileEnd)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numPathTwistBegin)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numPathTwist)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numPathTaperX)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numPathTaperY)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numPathShearX)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numPathShearY)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numPathSkew)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numPathScaleX)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numPathScaleY)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numPathRevolutions)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numPathRadiusOffset)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numPathBegin)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numPathEnd)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numMaterial)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numPositionZ)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numPositionY)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numPositionX)).BeginInit();
			this.SuspendLayout();
			// 
			// cmdBuild
			// 
			this.cmdBuild.Location = new System.Drawing.Point(408, 536);
			this.cmdBuild.Name = "cmdBuild";
			this.cmdBuild.Size = new System.Drawing.Size(104, 24);
			this.cmdBuild.TabIndex = 31;
			this.cmdBuild.Text = "Build Prim";
			this.cmdBuild.Click += new System.EventHandler(this.cmdBuild_Click);
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(16, 16);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(120, 20);
			this.label4.TabIndex = 58;
			this.label4.Text = "Name:";
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(16, 48);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(120, 20);
			this.label5.TabIndex = 59;
			this.label5.Text = "Material:";
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(16, 80);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(120, 20);
			this.label6.TabIndex = 60;
			this.label6.Text = "Path:";
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(16, 112);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(120, 20);
			this.label8.TabIndex = 62;
			this.label8.Text = "Path Radius Offset:";
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(16, 144);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(120, 20);
			this.label9.TabIndex = 63;
			this.label9.Text = "Path Revolutions:";
			// 
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(16, 176);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(120, 20);
			this.label10.TabIndex = 64;
			this.label10.Text = "Path Scale:";
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(136, 80);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(96, 20);
			this.label7.TabIndex = 65;
			this.label7.Text = "Begin:";
			// 
			// label11
			// 
			this.label11.Location = new System.Drawing.Point(328, 80);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(80, 20);
			this.label11.TabIndex = 66;
			this.label11.Text = "End:";
			// 
			// label12
			// 
			this.label12.Location = new System.Drawing.Point(136, 176);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(96, 20);
			this.label12.TabIndex = 67;
			this.label12.Text = "X:";
			// 
			// label13
			// 
			this.label13.Location = new System.Drawing.Point(328, 176);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(80, 20);
			this.label13.TabIndex = 68;
			this.label13.Text = "Y:";
			// 
			// label14
			// 
			this.label14.Location = new System.Drawing.Point(328, 208);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(80, 20);
			this.label14.TabIndex = 71;
			this.label14.Text = "Y:";
			// 
			// label15
			// 
			this.label15.Location = new System.Drawing.Point(136, 208);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(96, 20);
			this.label15.TabIndex = 70;
			this.label15.Text = "X:";
			// 
			// label16
			// 
			this.label16.Location = new System.Drawing.Point(16, 208);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(120, 20);
			this.label16.TabIndex = 69;
			this.label16.Text = "Path Shear:";
			// 
			// label19
			// 
			this.label19.Location = new System.Drawing.Point(16, 336);
			this.label19.Name = "label19";
			this.label19.Size = new System.Drawing.Size(120, 20);
			this.label19.TabIndex = 72;
			this.label19.Text = "Path Skew:";
			// 
			// label20
			// 
			this.label20.Location = new System.Drawing.Point(328, 240);
			this.label20.Name = "label20";
			this.label20.Size = new System.Drawing.Size(80, 20);
			this.label20.TabIndex = 77;
			this.label20.Text = "Y:";
			// 
			// label21
			// 
			this.label21.Location = new System.Drawing.Point(136, 240);
			this.label21.Name = "label21";
			this.label21.Size = new System.Drawing.Size(96, 20);
			this.label21.TabIndex = 76;
			this.label21.Text = "X:";
			// 
			// label22
			// 
			this.label22.Location = new System.Drawing.Point(16, 240);
			this.label22.Name = "label22";
			this.label22.Size = new System.Drawing.Size(120, 20);
			this.label22.TabIndex = 75;
			this.label22.Text = "Path Taper:";
			// 
			// label23
			// 
			this.label23.Location = new System.Drawing.Point(328, 272);
			this.label23.Name = "label23";
			this.label23.Size = new System.Drawing.Size(80, 20);
			this.label23.TabIndex = 80;
			this.label23.Text = "Path Twist:";
			// 
			// label24
			// 
			this.label24.Location = new System.Drawing.Point(136, 272);
			this.label24.Name = "label24";
			this.label24.Size = new System.Drawing.Size(96, 20);
			this.label24.TabIndex = 79;
			this.label24.Text = "Path Twist Begin:";
			// 
			// label25
			// 
			this.label25.Location = new System.Drawing.Point(16, 272);
			this.label25.Name = "label25";
			this.label25.Size = new System.Drawing.Size(120, 20);
			this.label25.TabIndex = 78;
			this.label25.Text = "Twist:";
			// 
			// label28
			// 
			this.label28.Location = new System.Drawing.Point(16, 304);
			this.label28.Name = "label28";
			this.label28.Size = new System.Drawing.Size(120, 20);
			this.label28.TabIndex = 81;
			this.label28.Text = "Profile:";
			// 
			// label26
			// 
			this.label26.Location = new System.Drawing.Point(328, 304);
			this.label26.Name = "label26";
			this.label26.Size = new System.Drawing.Size(80, 20);
			this.label26.TabIndex = 83;
			this.label26.Text = "End:";
			// 
			// label27
			// 
			this.label27.Location = new System.Drawing.Point(136, 304);
			this.label27.Name = "label27";
			this.label27.Size = new System.Drawing.Size(96, 20);
			this.label27.TabIndex = 82;
			this.label27.Text = "Begin:";
			// 
			// label29
			// 
			this.label29.Location = new System.Drawing.Point(288, 336);
			this.label29.Name = "label29";
			this.label29.Size = new System.Drawing.Size(120, 20);
			this.label29.TabIndex = 84;
			this.label29.Text = "Profile Curve:";
			// 
			// label30
			// 
			this.label30.Location = new System.Drawing.Point(16, 368);
			this.label30.Name = "label30";
			this.label30.Size = new System.Drawing.Size(120, 20);
			this.label30.TabIndex = 85;
			this.label30.Text = "Path Curve:";
			// 
			// label31
			// 
			this.label31.Location = new System.Drawing.Point(288, 368);
			this.label31.Name = "label31";
			this.label31.Size = new System.Drawing.Size(120, 20);
			this.label31.TabIndex = 86;
			this.label31.Text = "Profile Hollow:";
			// 
			// label32
			// 
			this.label32.Location = new System.Drawing.Point(16, 400);
			this.label32.Name = "label32";
			this.label32.Size = new System.Drawing.Size(120, 16);
			this.label32.TabIndex = 87;
			this.label32.Text = "Rotation:";
			// 
			// label33
			// 
			this.label33.Location = new System.Drawing.Point(16, 456);
			this.label33.Name = "label33";
			this.label33.Size = new System.Drawing.Size(120, 20);
			this.label33.TabIndex = 88;
			this.label33.Text = "Scale:";
			// 
			// label34
			// 
			this.label34.Location = new System.Drawing.Point(272, 480);
			this.label34.Name = "label34";
			this.label34.Size = new System.Drawing.Size(32, 20);
			this.label34.TabIndex = 94;
			this.label34.Text = "Z:";
			// 
			// numScaleZ
			// 
			this.numScaleZ.DecimalPlaces = 5;
			this.numScaleZ.Location = new System.Drawing.Point(304, 480);
			this.numScaleZ.Maximum = new System.Decimal(new int[] {
																	  256,
																	  0,
																	  0,
																	  0});
			this.numScaleZ.Minimum = new System.Decimal(new int[] {
																	  10,
																	  0,
																	  0,
																	  -2147483648});
			this.numScaleZ.Name = "numScaleZ";
			this.numScaleZ.Size = new System.Drawing.Size(80, 20);
			this.numScaleZ.TabIndex = 27;
			this.numScaleZ.Value = new System.Decimal(new int[] {
																	5,
																	0,
																	0,
																	65536});
			// 
			// label35
			// 
			this.label35.Location = new System.Drawing.Point(144, 480);
			this.label35.Name = "label35";
			this.label35.Size = new System.Drawing.Size(32, 20);
			this.label35.TabIndex = 92;
			this.label35.Text = "Y:";
			// 
			// numScaleY
			// 
			this.numScaleY.DecimalPlaces = 5;
			this.numScaleY.Location = new System.Drawing.Point(176, 480);
			this.numScaleY.Maximum = new System.Decimal(new int[] {
																	  256,
																	  0,
																	  0,
																	  0});
			this.numScaleY.Name = "numScaleY";
			this.numScaleY.Size = new System.Drawing.Size(80, 20);
			this.numScaleY.TabIndex = 26;
			this.numScaleY.Value = new System.Decimal(new int[] {
																	5,
																	0,
																	0,
																	65536});
			// 
			// label36
			// 
			this.label36.Location = new System.Drawing.Point(16, 480);
			this.label36.Name = "label36";
			this.label36.Size = new System.Drawing.Size(32, 20);
			this.label36.TabIndex = 90;
			this.label36.Text = "X:";
			// 
			// numScaleX
			// 
			this.numScaleX.DecimalPlaces = 5;
			this.numScaleX.Location = new System.Drawing.Point(48, 480);
			this.numScaleX.Maximum = new System.Decimal(new int[] {
																	  256,
																	  0,
																	  0,
																	  0});
			this.numScaleX.Name = "numScaleX";
			this.numScaleX.Size = new System.Drawing.Size(80, 20);
			this.numScaleX.TabIndex = 25;
			this.numScaleX.Value = new System.Decimal(new int[] {
																	5,
																	0,
																	0,
																	65536});
			// 
			// label37
			// 
			this.label37.Location = new System.Drawing.Point(272, 424);
			this.label37.Name = "label37";
			this.label37.Size = new System.Drawing.Size(32, 20);
			this.label37.TabIndex = 100;
			this.label37.Text = "Z:";
			// 
			// numRotationZ
			// 
			this.numRotationZ.DecimalPlaces = 5;
			this.numRotationZ.Location = new System.Drawing.Point(304, 424);
			this.numRotationZ.Maximum = new System.Decimal(new int[] {
																		 256,
																		 0,
																		 0,
																		 0});
			this.numRotationZ.Name = "numRotationZ";
			this.numRotationZ.Size = new System.Drawing.Size(80, 20);
			this.numRotationZ.TabIndex = 23;
			// 
			// label38
			// 
			this.label38.Location = new System.Drawing.Point(144, 424);
			this.label38.Name = "label38";
			this.label38.Size = new System.Drawing.Size(32, 20);
			this.label38.TabIndex = 98;
			this.label38.Text = "Y:";
			// 
			// numRotationY
			// 
			this.numRotationY.DecimalPlaces = 5;
			this.numRotationY.Location = new System.Drawing.Point(176, 424);
			this.numRotationY.Maximum = new System.Decimal(new int[] {
																		 256,
																		 0,
																		 0,
																		 0});
			this.numRotationY.Name = "numRotationY";
			this.numRotationY.Size = new System.Drawing.Size(80, 20);
			this.numRotationY.TabIndex = 22;
			// 
			// label39
			// 
			this.label39.Location = new System.Drawing.Point(16, 424);
			this.label39.Name = "label39";
			this.label39.Size = new System.Drawing.Size(32, 20);
			this.label39.TabIndex = 96;
			this.label39.Text = "X:";
			// 
			// numRotationX
			// 
			this.numRotationX.DecimalPlaces = 5;
			this.numRotationX.Location = new System.Drawing.Point(48, 424);
			this.numRotationX.Maximum = new System.Decimal(new int[] {
																		 256,
																		 0,
																		 0,
																		 0});
			this.numRotationX.Name = "numRotationX";
			this.numRotationX.Size = new System.Drawing.Size(80, 20);
			this.numRotationX.TabIndex = 21;
			// 
			// label40
			// 
			this.label40.Location = new System.Drawing.Point(400, 424);
			this.label40.Name = "label40";
			this.label40.Size = new System.Drawing.Size(32, 20);
			this.label40.TabIndex = 102;
			this.label40.Text = "S:";
			// 
			// numRotationS
			// 
			this.numRotationS.DecimalPlaces = 5;
			this.numRotationS.Location = new System.Drawing.Point(432, 424);
			this.numRotationS.Maximum = new System.Decimal(new int[] {
																		 256,
																		 0,
																		 0,
																		 0});
			this.numRotationS.Name = "numRotationS";
			this.numRotationS.Size = new System.Drawing.Size(80, 20);
			this.numRotationS.TabIndex = 24;
			// 
			// numProfileHollow
			// 
			this.numProfileHollow.Location = new System.Drawing.Point(408, 368);
			this.numProfileHollow.Maximum = new System.Decimal(new int[] {
																			 255,
																			 0,
																			 0,
																			 0});
			this.numProfileHollow.Name = "numProfileHollow";
			this.numProfileHollow.Size = new System.Drawing.Size(80, 20);
			this.numProfileHollow.TabIndex = 20;
			// 
			// numPathCurve
			// 
			this.numPathCurve.Location = new System.Drawing.Point(136, 368);
			this.numPathCurve.Maximum = new System.Decimal(new int[] {
																		 255,
																		 0,
																		 0,
																		 0});
			this.numPathCurve.Name = "numPathCurve";
			this.numPathCurve.Size = new System.Drawing.Size(80, 20);
			this.numPathCurve.TabIndex = 19;
			this.numPathCurve.Value = new System.Decimal(new int[] {
																	   16,
																	   0,
																	   0,
																	   0});
			// 
			// numProfileCurve
			// 
			this.numProfileCurve.Location = new System.Drawing.Point(408, 336);
			this.numProfileCurve.Maximum = new System.Decimal(new int[] {
																			5,
																			0,
																			0,
																			0});
			this.numProfileCurve.Name = "numProfileCurve";
			this.numProfileCurve.Size = new System.Drawing.Size(80, 20);
			this.numProfileCurve.TabIndex = 18;
			this.numProfileCurve.Value = new System.Decimal(new int[] {
																		  1,
																		  0,
																		  0,
																		  0});
			// 
			// numProfileBegin
			// 
			this.numProfileBegin.Location = new System.Drawing.Point(232, 304);
			this.numProfileBegin.Maximum = new System.Decimal(new int[] {
																			1275,
																			0,
																			0,
																			196608});
			this.numProfileBegin.Name = "numProfileBegin";
			this.numProfileBegin.Size = new System.Drawing.Size(80, 20);
			this.numProfileBegin.TabIndex = 16;
			// 
			// numProfileEnd
			// 
			this.numProfileEnd.Location = new System.Drawing.Point(408, 304);
			this.numProfileEnd.Maximum = new System.Decimal(new int[] {
																		  5,
																		  0,
																		  0,
																		  0});
			this.numProfileEnd.Minimum = new System.Decimal(new int[] {
																		  275,
																		  0,
																		  0,
																		  -2147287040});
			this.numProfileEnd.Name = "numProfileEnd";
			this.numProfileEnd.Size = new System.Drawing.Size(80, 20);
			this.numProfileEnd.TabIndex = 17;
			// 
			// numPathTwistBegin
			// 
			this.numPathTwistBegin.Location = new System.Drawing.Point(232, 272);
			this.numPathTwistBegin.Maximum = new System.Decimal(new int[] {
																			  459,
																			  0,
																			  0,
																			  0});
			this.numPathTwistBegin.Minimum = new System.Decimal(new int[] {
																			  459,
																			  0,
																			  0,
																			  -2147483648});
			this.numPathTwistBegin.Name = "numPathTwistBegin";
			this.numPathTwistBegin.Size = new System.Drawing.Size(80, 20);
			this.numPathTwistBegin.TabIndex = 14;
			// 
			// numPathTwist
			// 
			this.numPathTwist.Location = new System.Drawing.Point(408, 272);
			this.numPathTwist.Maximum = new System.Decimal(new int[] {
																		 459,
																		 0,
																		 0,
																		 0});
			this.numPathTwist.Minimum = new System.Decimal(new int[] {
																		 459,
																		 0,
																		 0,
																		 -2147483648});
			this.numPathTwist.Name = "numPathTwist";
			this.numPathTwist.Size = new System.Drawing.Size(80, 20);
			this.numPathTwist.TabIndex = 15;
			// 
			// numPathTaperX
			// 
			this.numPathTaperX.DecimalPlaces = 2;
			this.numPathTaperX.Increment = new System.Decimal(new int[] {
																			1,
																			0,
																			0,
																			65536});
			this.numPathTaperX.Location = new System.Drawing.Point(232, 240);
			this.numPathTaperX.Maximum = new System.Decimal(new int[] {
																		  255,
																		  0,
																		  0,
																		  131072});
			this.numPathTaperX.Minimum = new System.Decimal(new int[] {
																		  255,
																		  0,
																		  0,
																		  -2147352576});
			this.numPathTaperX.Name = "numPathTaperX";
			this.numPathTaperX.Size = new System.Drawing.Size(80, 20);
			this.numPathTaperX.TabIndex = 12;
			// 
			// numPathTaperY
			// 
			this.numPathTaperY.DecimalPlaces = 2;
			this.numPathTaperY.Increment = new System.Decimal(new int[] {
																			1,
																			0,
																			0,
																			65536});
			this.numPathTaperY.Location = new System.Drawing.Point(408, 240);
			this.numPathTaperY.Maximum = new System.Decimal(new int[] {
																		  255,
																		  0,
																		  0,
																		  131072});
			this.numPathTaperY.Minimum = new System.Decimal(new int[] {
																		  255,
																		  0,
																		  0,
																		  -2147352576});
			this.numPathTaperY.Name = "numPathTaperY";
			this.numPathTaperY.Size = new System.Drawing.Size(80, 20);
			this.numPathTaperY.TabIndex = 13;
			// 
			// numPathShearX
			// 
			this.numPathShearX.DecimalPlaces = 2;
			this.numPathShearX.Increment = new System.Decimal(new int[] {
																			1,
																			0,
																			0,
																			65536});
			this.numPathShearX.Location = new System.Drawing.Point(232, 208);
			this.numPathShearX.Maximum = new System.Decimal(new int[] {
																		  255,
																		  0,
																		  0,
																		  131072});
			this.numPathShearX.Minimum = new System.Decimal(new int[] {
																		  255,
																		  0,
																		  0,
																		  -2147352576});
			this.numPathShearX.Name = "numPathShearX";
			this.numPathShearX.Size = new System.Drawing.Size(80, 20);
			this.numPathShearX.TabIndex = 8;
			// 
			// numPathShearY
			// 
			this.numPathShearY.DecimalPlaces = 2;
			this.numPathShearY.Increment = new System.Decimal(new int[] {
																			1,
																			0,
																			0,
																			65536});
			this.numPathShearY.Location = new System.Drawing.Point(408, 208);
			this.numPathShearY.Maximum = new System.Decimal(new int[] {
																		  255,
																		  0,
																		  0,
																		  131072});
			this.numPathShearY.Minimum = new System.Decimal(new int[] {
																		  255,
																		  0,
																		  0,
																		  -2147352576});
			this.numPathShearY.Name = "numPathShearY";
			this.numPathShearY.Size = new System.Drawing.Size(80, 20);
			this.numPathShearY.TabIndex = 9;
			// 
			// numPathSkew
			// 
			this.numPathSkew.DecimalPlaces = 2;
			this.numPathSkew.Increment = new System.Decimal(new int[] {
																		  1,
																		  0,
																		  0,
																		  65536});
			this.numPathSkew.Location = new System.Drawing.Point(136, 336);
			this.numPathSkew.Maximum = new System.Decimal(new int[] {
																		255,
																		0,
																		0,
																		131072});
			this.numPathSkew.Minimum = new System.Decimal(new int[] {
																		255,
																		0,
																		0,
																		-2147352576});
			this.numPathSkew.Name = "numPathSkew";
			this.numPathSkew.Size = new System.Drawing.Size(80, 20);
			this.numPathSkew.TabIndex = 10;
			// 
			// numPathScaleX
			// 
			this.numPathScaleX.DecimalPlaces = 2;
			this.numPathScaleX.Increment = new System.Decimal(new int[] {
																			1,
																			0,
																			0,
																			65536});
			this.numPathScaleX.Location = new System.Drawing.Point(232, 176);
			this.numPathScaleX.Maximum = new System.Decimal(new int[] {
																		  155,
																		  0,
																		  0,
																		  131072});
			this.numPathScaleX.Name = "numPathScaleX";
			this.numPathScaleX.Size = new System.Drawing.Size(80, 20);
			this.numPathScaleX.TabIndex = 6;
			// 
			// numPathScaleY
			// 
			this.numPathScaleY.DecimalPlaces = 2;
			this.numPathScaleY.Increment = new System.Decimal(new int[] {
																			1,
																			0,
																			0,
																			65536});
			this.numPathScaleY.Location = new System.Drawing.Point(408, 176);
			this.numPathScaleY.Maximum = new System.Decimal(new int[] {
																		  155,
																		  0,
																		  0,
																		  131072});
			this.numPathScaleY.Name = "numPathScaleY";
			this.numPathScaleY.Size = new System.Drawing.Size(80, 20);
			this.numPathScaleY.TabIndex = 7;
			// 
			// numPathRevolutions
			// 
			this.numPathRevolutions.DecimalPlaces = 2;
			this.numPathRevolutions.Increment = new System.Decimal(new int[] {
																				 1,
																				 0,
																				 0,
																				 65536});
			this.numPathRevolutions.Location = new System.Drawing.Point(136, 144);
			this.numPathRevolutions.Maximum = new System.Decimal(new int[] {
																			   486,
																			   0,
																			   0,
																			   131072});
			this.numPathRevolutions.Name = "numPathRevolutions";
			this.numPathRevolutions.Size = new System.Drawing.Size(80, 20);
			this.numPathRevolutions.TabIndex = 5;
			this.numPathRevolutions.Value = new System.Decimal(new int[] {
																			 100,
																			 0,
																			 0,
																			 131072});
			// 
			// numPathRadiusOffset
			// 
			this.numPathRadiusOffset.DecimalPlaces = 3;
			this.numPathRadiusOffset.Increment = new System.Decimal(new int[] {
																				  1,
																				  0,
																				  0,
																				  65536});
			this.numPathRadiusOffset.Location = new System.Drawing.Point(136, 112);
			this.numPathRadiusOffset.Maximum = new System.Decimal(new int[] {
																				255,
																				0,
																				0,
																				131072});
			this.numPathRadiusOffset.Minimum = new System.Decimal(new int[] {
																				255,
																				0,
																				0,
																				-2147352576});
			this.numPathRadiusOffset.Name = "numPathRadiusOffset";
			this.numPathRadiusOffset.Size = new System.Drawing.Size(80, 20);
			this.numPathRadiusOffset.TabIndex = 4;
			// 
			// numPathBegin
			// 
			this.numPathBegin.DecimalPlaces = 2;
			this.numPathBegin.Increment = new System.Decimal(new int[] {
																		   1,
																		   0,
																		   0,
																		   65536});
			this.numPathBegin.Location = new System.Drawing.Point(232, 80);
			this.numPathBegin.Maximum = new System.Decimal(new int[] {
																		 255,
																		 0,
																		 0,
																		 131072});
			this.numPathBegin.Name = "numPathBegin";
			this.numPathBegin.Size = new System.Drawing.Size(80, 20);
			this.numPathBegin.TabIndex = 2;
			// 
			// numPathEnd
			// 
			this.numPathEnd.DecimalPlaces = 2;
			this.numPathEnd.Increment = new System.Decimal(new int[] {
																		 1,
																		 0,
																		 0,
																		 65536});
			this.numPathEnd.Location = new System.Drawing.Point(408, 80);
			this.numPathEnd.Maximum = new System.Decimal(new int[] {
																	   10,
																	   0,
																	   0,
																	   65536});
			this.numPathEnd.Minimum = new System.Decimal(new int[] {
																	   155,
																	   0,
																	   0,
																	   -2147352576});
			this.numPathEnd.Name = "numPathEnd";
			this.numPathEnd.Size = new System.Drawing.Size(80, 20);
			this.numPathEnd.TabIndex = 3;
			this.numPathEnd.Value = new System.Decimal(new int[] {
																	 1,
																	 0,
																	 0,
																	 0});
			// 
			// numMaterial
			// 
			this.numMaterial.Location = new System.Drawing.Point(136, 48);
			this.numMaterial.Maximum = new System.Decimal(new int[] {
																		255,
																		0,
																		0,
																		0});
			this.numMaterial.Name = "numMaterial";
			this.numMaterial.Size = new System.Drawing.Size(80, 20);
			this.numMaterial.TabIndex = 1;
			// 
			// txtName
			// 
			this.txtName.Location = new System.Drawing.Point(136, 16);
			this.txtName.Name = "txtName";
			this.txtName.Size = new System.Drawing.Size(176, 20);
			this.txtName.TabIndex = 0;
			this.txtName.Text = "";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(272, 536);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(32, 16);
			this.label3.TabIndex = 129;
			this.label3.Text = "Z:";
			// 
			// numPositionZ
			// 
			this.numPositionZ.DecimalPlaces = 5;
			this.numPositionZ.Location = new System.Drawing.Point(304, 536);
			this.numPositionZ.Maximum = new System.Decimal(new int[] {
																		 256,
																		 0,
																		 0,
																		 0});
			this.numPositionZ.Name = "numPositionZ";
			this.numPositionZ.Size = new System.Drawing.Size(80, 20);
			this.numPositionZ.TabIndex = 30;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(144, 536);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(32, 16);
			this.label2.TabIndex = 127;
			this.label2.Text = "Y:";
			// 
			// numPositionY
			// 
			this.numPositionY.DecimalPlaces = 5;
			this.numPositionY.Location = new System.Drawing.Point(176, 536);
			this.numPositionY.Maximum = new System.Decimal(new int[] {
																		 256,
																		 0,
																		 0,
																		 0});
			this.numPositionY.Name = "numPositionY";
			this.numPositionY.Size = new System.Drawing.Size(80, 20);
			this.numPositionY.TabIndex = 29;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 536);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(32, 16);
			this.label1.TabIndex = 125;
			this.label1.Text = "X:";
			// 
			// numPositionX
			// 
			this.numPositionX.DecimalPlaces = 5;
			this.numPositionX.Location = new System.Drawing.Point(48, 536);
			this.numPositionX.Maximum = new System.Decimal(new int[] {
																		 256,
																		 0,
																		 0,
																		 0});
			this.numPositionX.Name = "numPositionX";
			this.numPositionX.Size = new System.Drawing.Size(80, 20);
			this.numPositionX.TabIndex = 28;
			// 
			// label41
			// 
			this.label41.Location = new System.Drawing.Point(16, 512);
			this.label41.Name = "label41";
			this.label41.Size = new System.Drawing.Size(120, 20);
			this.label41.TabIndex = 130;
			this.label41.Text = "Sim Position:";
			// 
			// frmPrimBuilder
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(528, 573);
			this.Controls.Add(this.label41);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.numPositionZ);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.numPositionY);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.numPositionX);
			this.Controls.Add(this.txtName);
			this.Controls.Add(this.numMaterial);
			this.Controls.Add(this.numPathEnd);
			this.Controls.Add(this.numPathBegin);
			this.Controls.Add(this.numPathRadiusOffset);
			this.Controls.Add(this.numPathRevolutions);
			this.Controls.Add(this.numPathScaleY);
			this.Controls.Add(this.numPathScaleX);
			this.Controls.Add(this.numPathSkew);
			this.Controls.Add(this.numPathShearY);
			this.Controls.Add(this.numPathShearX);
			this.Controls.Add(this.numPathTaperY);
			this.Controls.Add(this.numPathTaperX);
			this.Controls.Add(this.numPathTwist);
			this.Controls.Add(this.numPathTwistBegin);
			this.Controls.Add(this.numProfileEnd);
			this.Controls.Add(this.numProfileBegin);
			this.Controls.Add(this.numProfileCurve);
			this.Controls.Add(this.numPathCurve);
			this.Controls.Add(this.numProfileHollow);
			this.Controls.Add(this.label40);
			this.Controls.Add(this.numRotationS);
			this.Controls.Add(this.label37);
			this.Controls.Add(this.numRotationZ);
			this.Controls.Add(this.label38);
			this.Controls.Add(this.numRotationY);
			this.Controls.Add(this.label39);
			this.Controls.Add(this.numRotationX);
			this.Controls.Add(this.label34);
			this.Controls.Add(this.numScaleZ);
			this.Controls.Add(this.label35);
			this.Controls.Add(this.numScaleY);
			this.Controls.Add(this.label36);
			this.Controls.Add(this.numScaleX);
			this.Controls.Add(this.label33);
			this.Controls.Add(this.label32);
			this.Controls.Add(this.label31);
			this.Controls.Add(this.label30);
			this.Controls.Add(this.label29);
			this.Controls.Add(this.label26);
			this.Controls.Add(this.label27);
			this.Controls.Add(this.label28);
			this.Controls.Add(this.label23);
			this.Controls.Add(this.label24);
			this.Controls.Add(this.label25);
			this.Controls.Add(this.label20);
			this.Controls.Add(this.label21);
			this.Controls.Add(this.label22);
			this.Controls.Add(this.label19);
			this.Controls.Add(this.label14);
			this.Controls.Add(this.label15);
			this.Controls.Add(this.label16);
			this.Controls.Add(this.label13);
			this.Controls.Add(this.label12);
			this.Controls.Add(this.label11);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.label10);
			this.Controls.Add(this.label9);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.cmdBuild);
			this.Name = "frmPrimBuilder";
			this.Text = "Prim Builder";
			((System.ComponentModel.ISupportInitialize)(this.numScaleZ)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numScaleY)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numScaleX)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numRotationZ)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numRotationY)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numRotationX)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numRotationS)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numProfileHollow)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numPathCurve)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numProfileCurve)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numProfileBegin)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numProfileEnd)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numPathTwistBegin)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numPathTwist)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numPathTaperX)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numPathTaperY)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numPathShearX)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numPathShearY)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numPathSkew)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numPathScaleX)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numPathScaleY)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numPathRevolutions)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numPathRadiusOffset)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numPathBegin)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numPathEnd)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numMaterial)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numPositionZ)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numPositionY)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numPositionX)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		private void cmdBuild_Click(object sender, System.EventArgs e)
		{
			cmdBuild.Enabled = false;

			PrimObject prim = new PrimObject(new LLUUID("8955674724cb43ed920b47caed15465f"));

			prim.Material = (uint)numMaterial.Value;
			prim.PathBegin = PrimObject.PathBeginByte((float)numPathBegin.Value);
			prim.PathEnd = PrimObject.PathEndByte((float)numPathEnd.Value);
			prim.PathRadiusOffset = PrimObject.PathRadiusOffsetByte((float)numPathRadiusOffset.Value);
			prim.PathRevolutions = PrimObject.PathRevolutionsByte((float)numPathRevolutions.Value);
			prim.PathScaleX = PrimObject.PathScaleByte((float)numPathScaleX.Value);
			prim.PathScaleY = PrimObject.PathScaleByte((float)numPathScaleY.Value);
			prim.PathShearX = PrimObject.PathShearByte((float)numPathShearX.Value);
			prim.PathShearY = PrimObject.PathShearByte((float)numPathShearY.Value);
			prim.PathTaperX = PrimObject.PathTaperByte((float)numPathTaperX.Value);
			prim.PathTaperY = PrimObject.PathTaperByte((float)numPathTaperY.Value);
			prim.PathTwistBegin = PrimObject.PathTwistByte((float)numPathTwistBegin.Value);
			prim.PathTwist = PrimObject.PathTwistByte((float)numPathTwist.Value);
			prim.ProfileBegin = PrimObject.ProfileBeginByte((float)numProfileBegin.Value);
			prim.ProfileEnd = PrimObject.ProfileEndByte((float)numProfileEnd.Value);
			prim.PathSkew = PrimObject.PathSkewByte((float)numPathSkew.Value);
			prim.ProfileCurve = (uint)numProfileCurve.Value;
			prim.PathCurve = (uint)numPathCurve.Value;
			prim.ProfileHollow = (uint)numProfileHollow.Value;
			prim.Rotation.X = (float)numRotationX.Value;
			prim.Rotation.Y = (float)numRotationY.Value;
			prim.Rotation.Z = (float)numRotationZ.Value;
			prim.Rotation.S = (float)numRotationS.Value;
			prim.Scale.X = (float)numScaleX.Value;
			prim.Scale.Y = (float)numScaleY.Value;
			prim.Scale.Z = (float)numScaleZ.Value;
			prim.Position.X = (float)numPositionX.Value;
			prim.Position.Y = (float)numPositionY.Value;
			prim.Position.Z = (float)numPositionZ.Value;

			Client.CurrentRegion.RezObject(prim, prim.Position, new LLVector3(Client.Avatar.Position));

			// Rate limiting
			System.Threading.Thread.Sleep(250);

			cmdBuild.Enabled = true;
		}
	}
}
