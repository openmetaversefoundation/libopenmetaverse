namespace groupmanager
{
    partial class frmGroupInfo
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            this.tabs = new System.Windows.Forms.TabControl();
            this.tabGeneral = new System.Windows.Forms.TabPage();
            this.labelInsigniaProgress = new System.Windows.Forms.Label();
            this.grpPreferences = new System.Windows.Forms.GroupBox();
            this.chkMature = new System.Windows.Forms.CheckBox();
            this.numFee = new System.Windows.Forms.NumericUpDown();
            this.chkGroupNotices = new System.Windows.Forms.CheckBox();
            this.chkFee = new System.Windows.Forms.CheckBox();
            this.chkOpenEnrollment = new System.Windows.Forms.CheckBox();
            this.chkPublish = new System.Windows.Forms.CheckBox();
            this.chkShow = new System.Windows.Forms.CheckBox();
            this.lstMembers = new System.Windows.Forms.ListView();
            this.colName = new System.Windows.Forms.ColumnHeader();
            this.colTitle = new System.Windows.Forms.ColumnHeader();
            this.colLasLogin = new System.Windows.Forms.ColumnHeader();
            this.txtCharter = new System.Windows.Forms.TextBox();
            this.lblFoundedBy = new System.Windows.Forms.Label();
            this.lblGroupName = new System.Windows.Forms.Label();
            this.picInsignia = new System.Windows.Forms.PictureBox();
            this.tabMembersRoles = new System.Windows.Forms.TabPage();
            this.tabsMRA = new System.Windows.Forms.TabControl();
            this.tabMembers = new System.Windows.Forms.TabPage();
            this.cmdEject = new System.Windows.Forms.Button();
            this.lstMembers2 = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.chkListRoles = new System.Windows.Forms.CheckedListBox();
            this.treeAbilities = new System.Windows.Forms.TreeView();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tabRoles = new System.Windows.Forms.TabPage();
            this.tabAbilities = new System.Windows.Forms.TabPage();
            this.tabNotices = new System.Windows.Forms.TabPage();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cmdRefreshNotices = new System.Windows.Forms.Button();
            this.lstNotices = new System.Windows.Forms.ListView();
            this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader6 = new System.Windows.Forms.ColumnHeader();
            this.tabProposals = new System.Windows.Forms.TabPage();
            this.tabLand = new System.Windows.Forms.TabPage();
            this.tabsMoney = new System.Windows.Forms.TabControl();
            this.tabPlanning = new System.Windows.Forms.TabPage();
            this.txtPlanning = new System.Windows.Forms.TextBox();
            this.tabDetails = new System.Windows.Forms.TabPage();
            this.txtDetails = new System.Windows.Forms.TextBox();
            this.tabSales = new System.Windows.Forms.TabPage();
            this.txtSales = new System.Windows.Forms.TextBox();
            this.txtContribution = new System.Windows.Forms.TextBox();
            this.lblLandAvailable = new System.Windows.Forms.Label();
            this.lblLandInUse = new System.Windows.Forms.Label();
            this.lblTotalContribution = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.lstLand = new System.Windows.Forms.ListView();
            this.columnHeader7 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader8 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader9 = new System.Windows.Forms.ColumnHeader();
            this.cmdApply = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cmdOK = new System.Windows.Forms.Button();
            this.cmdRefresh = new System.Windows.Forms.Button();
            this.tabs.SuspendLayout();
            this.tabGeneral.SuspendLayout();
            this.grpPreferences.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numFee)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picInsignia)).BeginInit();
            this.tabMembersRoles.SuspendLayout();
            this.tabsMRA.SuspendLayout();
            this.tabMembers.SuspendLayout();
            this.tabNotices.SuspendLayout();
            this.tabLand.SuspendLayout();
            this.tabsMoney.SuspendLayout();
            this.tabPlanning.SuspendLayout();
            this.tabDetails.SuspendLayout();
            this.tabSales.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabs
            // 
            this.tabs.Controls.Add(this.tabGeneral);
            this.tabs.Controls.Add(this.tabMembersRoles);
            this.tabs.Controls.Add(this.tabNotices);
            this.tabs.Controls.Add(this.tabProposals);
            this.tabs.Controls.Add(this.tabLand);
            this.tabs.Location = new System.Drawing.Point(6, 7);
            this.tabs.Name = "tabs";
            this.tabs.SelectedIndex = 0;
            this.tabs.Size = new System.Drawing.Size(417, 507);
            this.tabs.TabIndex = 9;
            // 
            // tabGeneral
            // 
            this.tabGeneral.Controls.Add(this.labelInsigniaProgress);
            this.tabGeneral.Controls.Add(this.grpPreferences);
            this.tabGeneral.Controls.Add(this.lstMembers);
            this.tabGeneral.Controls.Add(this.txtCharter);
            this.tabGeneral.Controls.Add(this.lblFoundedBy);
            this.tabGeneral.Controls.Add(this.lblGroupName);
            this.tabGeneral.Controls.Add(this.picInsignia);
            this.tabGeneral.Location = new System.Drawing.Point(4, 22);
            this.tabGeneral.Name = "tabGeneral";
            this.tabGeneral.Padding = new System.Windows.Forms.Padding(3);
            this.tabGeneral.Size = new System.Drawing.Size(409, 481);
            this.tabGeneral.TabIndex = 0;
            this.tabGeneral.Text = "General";
            this.tabGeneral.UseVisualStyleBackColor = true;
            // 
            // labelInsigniaProgress
            // 
            this.labelInsigniaProgress.AutoSize = true;
            this.labelInsigniaProgress.Location = new System.Drawing.Point(23, 149);
            this.labelInsigniaProgress.Name = "labelInsigniaProgress";
            this.labelInsigniaProgress.Size = new System.Drawing.Size(54, 13);
            this.labelInsigniaProgress.TabIndex = 7;
            this.labelInsigniaProgress.Text = "Loading...";
            // 
            // grpPreferences
            // 
            this.grpPreferences.Controls.Add(this.chkMature);
            this.grpPreferences.Controls.Add(this.numFee);
            this.grpPreferences.Controls.Add(this.chkGroupNotices);
            this.grpPreferences.Controls.Add(this.chkFee);
            this.grpPreferences.Controls.Add(this.chkOpenEnrollment);
            this.grpPreferences.Controls.Add(this.chkPublish);
            this.grpPreferences.Controls.Add(this.chkShow);
            this.grpPreferences.Location = new System.Drawing.Point(10, 353);
            this.grpPreferences.Name = "grpPreferences";
            this.grpPreferences.Size = new System.Drawing.Size(393, 122);
            this.grpPreferences.TabIndex = 14;
            this.grpPreferences.TabStop = false;
            this.grpPreferences.Text = "Group Preferences";
            // 
            // chkMature
            // 
            this.chkMature.AutoSize = true;
            this.chkMature.Location = new System.Drawing.Point(162, 19);
            this.chkMature.Name = "chkMature";
            this.chkMature.Size = new System.Drawing.Size(95, 17);
            this.chkMature.TabIndex = 6;
            this.chkMature.Text = "Mature publish";
            this.chkMature.UseVisualStyleBackColor = true;
            // 
            // numFee
            // 
            this.numFee.Location = new System.Drawing.Point(162, 87);
            this.numFee.Name = "numFee";
            this.numFee.Size = new System.Drawing.Size(82, 20);
            this.numFee.TabIndex = 5;
            // 
            // chkGroupNotices
            // 
            this.chkGroupNotices.AutoSize = true;
            this.chkGroupNotices.Location = new System.Drawing.Point(250, 87);
            this.chkGroupNotices.Name = "chkGroupNotices";
            this.chkGroupNotices.Size = new System.Drawing.Size(137, 17);
            this.chkGroupNotices.TabIndex = 4;
            this.chkGroupNotices.Text = "Receive Group Notices";
            this.chkGroupNotices.UseVisualStyleBackColor = true;
            // 
            // chkFee
            // 
            this.chkFee.AutoSize = true;
            this.chkFee.Location = new System.Drawing.Point(36, 88);
            this.chkFee.Name = "chkFee";
            this.chkFee.Size = new System.Drawing.Size(114, 17);
            this.chkFee.TabIndex = 3;
            this.chkFee.Text = "Enrollment Fee: L$";
            this.chkFee.UseVisualStyleBackColor = true;
            // 
            // chkOpenEnrollment
            // 
            this.chkOpenEnrollment.AutoSize = true;
            this.chkOpenEnrollment.Location = new System.Drawing.Point(16, 65);
            this.chkOpenEnrollment.Name = "chkOpenEnrollment";
            this.chkOpenEnrollment.Size = new System.Drawing.Size(104, 17);
            this.chkOpenEnrollment.TabIndex = 2;
            this.chkOpenEnrollment.Text = "Open Enrollment";
            this.chkOpenEnrollment.UseVisualStyleBackColor = true;
            // 
            // chkPublish
            // 
            this.chkPublish.AutoSize = true;
            this.chkPublish.Location = new System.Drawing.Point(16, 42);
            this.chkPublish.Name = "chkPublish";
            this.chkPublish.Size = new System.Drawing.Size(116, 17);
            this.chkPublish.TabIndex = 1;
            this.chkPublish.Text = "Publish on the web";
            this.chkPublish.UseVisualStyleBackColor = true;
            // 
            // chkShow
            // 
            this.chkShow.AutoSize = true;
            this.chkShow.Location = new System.Drawing.Point(16, 19);
            this.chkShow.Name = "chkShow";
            this.chkShow.Size = new System.Drawing.Size(116, 17);
            this.chkShow.TabIndex = 0;
            this.chkShow.Text = "Show In Group List";
            this.chkShow.UseVisualStyleBackColor = true;
            // 
            // lstMembers
            // 
            this.lstMembers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colName,
            this.colTitle,
            this.colLasLogin});
            this.lstMembers.Location = new System.Drawing.Point(7, 221);
            this.lstMembers.Name = "lstMembers";
            this.lstMembers.Size = new System.Drawing.Size(396, 126);
            this.lstMembers.TabIndex = 13;
            this.lstMembers.UseCompatibleStateImageBehavior = false;
            this.lstMembers.View = System.Windows.Forms.View.Details;
            // 
            // colName
            // 
            this.colName.Text = "Member Name";
            this.colName.Width = 166;
            // 
            // colTitle
            // 
            this.colTitle.Text = "Title";
            this.colTitle.Width = 127;
            // 
            // colLasLogin
            // 
            this.colLasLogin.Text = "Last Login";
            this.colLasLogin.Width = 95;
            // 
            // txtCharter
            // 
            this.txtCharter.Location = new System.Drawing.Point(146, 42);
            this.txtCharter.Multiline = true;
            this.txtCharter.Name = "txtCharter";
            this.txtCharter.Size = new System.Drawing.Size(257, 173);
            this.txtCharter.TabIndex = 12;
            // 
            // lblFoundedBy
            // 
            this.lblFoundedBy.AutoSize = true;
            this.lblFoundedBy.Location = new System.Drawing.Point(7, 26);
            this.lblFoundedBy.Name = "lblFoundedBy";
            this.lblFoundedBy.Size = new System.Drawing.Size(137, 13);
            this.lblFoundedBy.TabIndex = 11;
            this.lblFoundedBy.Text = "Founded by Group Founder";
            // 
            // lblGroupName
            // 
            this.lblGroupName.AutoSize = true;
            this.lblGroupName.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblGroupName.Location = new System.Drawing.Point(7, 6);
            this.lblGroupName.Name = "lblGroupName";
            this.lblGroupName.Size = new System.Drawing.Size(99, 17);
            this.lblGroupName.TabIndex = 10;
            this.lblGroupName.Text = "Group Name";
            // 
            // picInsignia
            // 
            this.picInsignia.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picInsignia.Location = new System.Drawing.Point(10, 42);
            this.picInsignia.Name = "picInsignia";
            this.picInsignia.Size = new System.Drawing.Size(130, 130);
            this.picInsignia.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picInsignia.TabIndex = 9;
            this.picInsignia.TabStop = false;
            // 
            // tabMembersRoles
            // 
            this.tabMembersRoles.Controls.Add(this.tabsMRA);
            this.tabMembersRoles.Location = new System.Drawing.Point(4, 22);
            this.tabMembersRoles.Name = "tabMembersRoles";
            this.tabMembersRoles.Padding = new System.Windows.Forms.Padding(3);
            this.tabMembersRoles.Size = new System.Drawing.Size(409, 481);
            this.tabMembersRoles.TabIndex = 1;
            this.tabMembersRoles.Text = "Members & Roles";
            this.tabMembersRoles.UseVisualStyleBackColor = true;
            // 
            // tabsMRA
            // 
            this.tabsMRA.Controls.Add(this.tabMembers);
            this.tabsMRA.Controls.Add(this.tabRoles);
            this.tabsMRA.Controls.Add(this.tabAbilities);
            this.tabsMRA.Location = new System.Drawing.Point(6, 6);
            this.tabsMRA.Name = "tabsMRA";
            this.tabsMRA.SelectedIndex = 0;
            this.tabsMRA.Size = new System.Drawing.Size(400, 469);
            this.tabsMRA.TabIndex = 0;
            // 
            // tabMembers
            // 
            this.tabMembers.Controls.Add(this.cmdEject);
            this.tabMembers.Controls.Add(this.lstMembers2);
            this.tabMembers.Controls.Add(this.chkListRoles);
            this.tabMembers.Controls.Add(this.treeAbilities);
            this.tabMembers.Controls.Add(this.label2);
            this.tabMembers.Controls.Add(this.label1);
            this.tabMembers.Location = new System.Drawing.Point(4, 22);
            this.tabMembers.Name = "tabMembers";
            this.tabMembers.Padding = new System.Windows.Forms.Padding(3);
            this.tabMembers.Size = new System.Drawing.Size(392, 443);
            this.tabMembers.TabIndex = 0;
            this.tabMembers.Text = "Members";
            this.tabMembers.UseVisualStyleBackColor = true;
            // 
            // cmdEject
            // 
            this.cmdEject.Location = new System.Drawing.Point(258, 152);
            this.cmdEject.Name = "cmdEject";
            this.cmdEject.Size = new System.Drawing.Size(128, 23);
            this.cmdEject.TabIndex = 15;
            this.cmdEject.Text = "Eject From Group";
            this.cmdEject.UseVisualStyleBackColor = true;
            // 
            // lstMembers2
            // 
            this.lstMembers2.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.lstMembers2.Location = new System.Drawing.Point(6, 6);
            this.lstMembers2.Name = "lstMembers2";
            this.lstMembers2.Size = new System.Drawing.Size(380, 140);
            this.lstMembers2.TabIndex = 14;
            this.lstMembers2.UseCompatibleStateImageBehavior = false;
            this.lstMembers2.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Member Name";
            this.columnHeader1.Width = 152;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Donated Tier";
            this.columnHeader2.Width = 119;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Last Login";
            this.columnHeader3.Width = 96;
            // 
            // chkListRoles
            // 
            this.chkListRoles.FormattingEnabled = true;
            this.chkListRoles.Location = new System.Drawing.Point(6, 196);
            this.chkListRoles.Name = "chkListRoles";
            this.chkListRoles.Size = new System.Drawing.Size(147, 244);
            this.chkListRoles.TabIndex = 8;
            // 
            // treeAbilities
            // 
            this.treeAbilities.Location = new System.Drawing.Point(159, 196);
            this.treeAbilities.Name = "treeAbilities";
            this.treeAbilities.Size = new System.Drawing.Size(227, 244);
            this.treeAbilities.TabIndex = 7;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(156, 180);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Allowed Abilities";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 180);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Assigned Roles";
            // 
            // tabRoles
            // 
            this.tabRoles.Location = new System.Drawing.Point(4, 22);
            this.tabRoles.Name = "tabRoles";
            this.tabRoles.Padding = new System.Windows.Forms.Padding(3);
            this.tabRoles.Size = new System.Drawing.Size(392, 443);
            this.tabRoles.TabIndex = 1;
            this.tabRoles.Text = "Roles";
            this.tabRoles.UseVisualStyleBackColor = true;
            // 
            // tabAbilities
            // 
            this.tabAbilities.Location = new System.Drawing.Point(4, 22);
            this.tabAbilities.Name = "tabAbilities";
            this.tabAbilities.Size = new System.Drawing.Size(392, 443);
            this.tabAbilities.TabIndex = 2;
            this.tabAbilities.Text = "Abilities";
            this.tabAbilities.UseVisualStyleBackColor = true;
            // 
            // tabNotices
            // 
            this.tabNotices.Controls.Add(this.textBox1);
            this.tabNotices.Controls.Add(this.label3);
            this.tabNotices.Controls.Add(this.cmdRefreshNotices);
            this.tabNotices.Controls.Add(this.lstNotices);
            this.tabNotices.Location = new System.Drawing.Point(4, 22);
            this.tabNotices.Name = "tabNotices";
            this.tabNotices.Size = new System.Drawing.Size(409, 481);
            this.tabNotices.TabIndex = 2;
            this.tabNotices.Text = "Notices";
            this.tabNotices.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(3, 239);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(403, 239);
            this.textBox1.TabIndex = 18;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 223);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(83, 13);
            this.label3.TabIndex = 17;
            this.label3.Text = "Archived Notice";
            // 
            // cmdRefreshNotices
            // 
            this.cmdRefreshNotices.Location = new System.Drawing.Point(289, 194);
            this.cmdRefreshNotices.Name = "cmdRefreshNotices";
            this.cmdRefreshNotices.Size = new System.Drawing.Size(117, 23);
            this.cmdRefreshNotices.TabIndex = 16;
            this.cmdRefreshNotices.Text = "Refresh List";
            this.cmdRefreshNotices.UseVisualStyleBackColor = true;
            // 
            // lstNotices
            // 
            this.lstNotices.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6});
            this.lstNotices.Location = new System.Drawing.Point(3, 8);
            this.lstNotices.Name = "lstNotices";
            this.lstNotices.Size = new System.Drawing.Size(403, 180);
            this.lstNotices.TabIndex = 15;
            this.lstNotices.UseCompatibleStateImageBehavior = false;
            this.lstNotices.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Subject";
            this.columnHeader4.Width = 184;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "From";
            this.columnHeader5.Width = 125;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Date";
            this.columnHeader6.Width = 87;
            // 
            // tabProposals
            // 
            this.tabProposals.Location = new System.Drawing.Point(4, 22);
            this.tabProposals.Name = "tabProposals";
            this.tabProposals.Size = new System.Drawing.Size(409, 481);
            this.tabProposals.TabIndex = 3;
            this.tabProposals.Text = "Proposals";
            this.tabProposals.UseVisualStyleBackColor = true;
            // 
            // tabLand
            // 
            this.tabLand.Controls.Add(this.tabsMoney);
            this.tabLand.Controls.Add(this.txtContribution);
            this.tabLand.Controls.Add(this.lblLandAvailable);
            this.tabLand.Controls.Add(this.lblLandInUse);
            this.tabLand.Controls.Add(this.lblTotalContribution);
            this.tabLand.Controls.Add(this.label7);
            this.tabLand.Controls.Add(this.label6);
            this.tabLand.Controls.Add(this.label5);
            this.tabLand.Controls.Add(this.label4);
            this.tabLand.Controls.Add(this.lstLand);
            this.tabLand.Location = new System.Drawing.Point(4, 22);
            this.tabLand.Name = "tabLand";
            this.tabLand.Size = new System.Drawing.Size(409, 481);
            this.tabLand.TabIndex = 4;
            this.tabLand.Text = "Land & L$";
            this.tabLand.UseVisualStyleBackColor = true;
            // 
            // tabsMoney
            // 
            this.tabsMoney.Controls.Add(this.tabPlanning);
            this.tabsMoney.Controls.Add(this.tabDetails);
            this.tabsMoney.Controls.Add(this.tabSales);
            this.tabsMoney.Location = new System.Drawing.Point(3, 278);
            this.tabsMoney.Name = "tabsMoney";
            this.tabsMoney.SelectedIndex = 0;
            this.tabsMoney.Size = new System.Drawing.Size(406, 200);
            this.tabsMoney.TabIndex = 24;
            // 
            // tabPlanning
            // 
            this.tabPlanning.Controls.Add(this.txtPlanning);
            this.tabPlanning.Location = new System.Drawing.Point(4, 22);
            this.tabPlanning.Name = "tabPlanning";
            this.tabPlanning.Padding = new System.Windows.Forms.Padding(3);
            this.tabPlanning.Size = new System.Drawing.Size(398, 174);
            this.tabPlanning.TabIndex = 0;
            this.tabPlanning.Text = "Planning";
            this.tabPlanning.UseVisualStyleBackColor = true;
            // 
            // txtPlanning
            // 
            this.txtPlanning.Location = new System.Drawing.Point(6, 5);
            this.txtPlanning.Multiline = true;
            this.txtPlanning.Name = "txtPlanning";
            this.txtPlanning.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtPlanning.Size = new System.Drawing.Size(386, 163);
            this.txtPlanning.TabIndex = 13;
            // 
            // tabDetails
            // 
            this.tabDetails.Controls.Add(this.txtDetails);
            this.tabDetails.Location = new System.Drawing.Point(4, 22);
            this.tabDetails.Name = "tabDetails";
            this.tabDetails.Padding = new System.Windows.Forms.Padding(3);
            this.tabDetails.Size = new System.Drawing.Size(398, 174);
            this.tabDetails.TabIndex = 1;
            this.tabDetails.Text = "Details";
            this.tabDetails.UseVisualStyleBackColor = true;
            // 
            // txtDetails
            // 
            this.txtDetails.Location = new System.Drawing.Point(6, 6);
            this.txtDetails.Multiline = true;
            this.txtDetails.Name = "txtDetails";
            this.txtDetails.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDetails.Size = new System.Drawing.Size(386, 163);
            this.txtDetails.TabIndex = 14;
            // 
            // tabSales
            // 
            this.tabSales.Controls.Add(this.txtSales);
            this.tabSales.Location = new System.Drawing.Point(4, 22);
            this.tabSales.Name = "tabSales";
            this.tabSales.Size = new System.Drawing.Size(398, 174);
            this.tabSales.TabIndex = 2;
            this.tabSales.Text = "Sales";
            this.tabSales.UseVisualStyleBackColor = true;
            // 
            // txtSales
            // 
            this.txtSales.Location = new System.Drawing.Point(6, 6);
            this.txtSales.Multiline = true;
            this.txtSales.Name = "txtSales";
            this.txtSales.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtSales.Size = new System.Drawing.Size(386, 163);
            this.txtSales.TabIndex = 14;
            // 
            // txtContribution
            // 
            this.txtContribution.Location = new System.Drawing.Point(157, 237);
            this.txtContribution.Name = "txtContribution";
            this.txtContribution.Size = new System.Drawing.Size(94, 20);
            this.txtContribution.TabIndex = 23;
            // 
            // lblLandAvailable
            // 
            this.lblLandAvailable.AutoSize = true;
            this.lblLandAvailable.Location = new System.Drawing.Point(154, 221);
            this.lblLandAvailable.Name = "lblLandAvailable";
            this.lblLandAvailable.Size = new System.Drawing.Size(13, 13);
            this.lblLandAvailable.TabIndex = 22;
            this.lblLandAvailable.Text = "0";
            // 
            // lblLandInUse
            // 
            this.lblLandInUse.AutoSize = true;
            this.lblLandInUse.Location = new System.Drawing.Point(154, 199);
            this.lblLandInUse.Name = "lblLandInUse";
            this.lblLandInUse.Size = new System.Drawing.Size(13, 13);
            this.lblLandInUse.TabIndex = 21;
            this.lblLandInUse.Text = "0";
            // 
            // lblTotalContribution
            // 
            this.lblTotalContribution.AutoSize = true;
            this.lblTotalContribution.Location = new System.Drawing.Point(154, 176);
            this.lblTotalContribution.Name = "lblTotalContribution";
            this.lblTotalContribution.Size = new System.Drawing.Size(13, 13);
            this.lblTotalContribution.TabIndex = 20;
            this.lblTotalContribution.Text = "0";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(57, 244);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(91, 13);
            this.label7.TabIndex = 19;
            this.label7.Text = "Your Contribution:";
            this.label7.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(68, 221);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(80, 13);
            this.label6.TabIndex = 18;
            this.label6.Text = "Land Available:";
            this.label6.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(53, 199);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(95, 13);
            this.label5.TabIndex = 17;
            this.label5.Text = "Total Land In Use:";
            this.label5.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(55, 176);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(93, 13);
            this.label4.TabIndex = 16;
            this.label4.Text = "Total Contribution:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lstLand
            // 
            this.lstLand.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader7,
            this.columnHeader8,
            this.columnHeader9});
            this.lstLand.Location = new System.Drawing.Point(3, 3);
            this.lstLand.Name = "lstLand";
            this.lstLand.Size = new System.Drawing.Size(403, 140);
            this.lstLand.TabIndex = 15;
            this.lstLand.UseCompatibleStateImageBehavior = false;
            this.lstLand.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "Parcel Name";
            this.columnHeader7.Width = 180;
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "Region";
            this.columnHeader8.Width = 119;
            // 
            // columnHeader9
            // 
            this.columnHeader9.Text = "Area";
            this.columnHeader9.Width = 93;
            // 
            // cmdApply
            // 
            this.cmdApply.Location = new System.Drawing.Point(348, 520);
            this.cmdApply.Name = "cmdApply";
            this.cmdApply.Size = new System.Drawing.Size(75, 23);
            this.cmdApply.TabIndex = 10;
            this.cmdApply.Text = "Apply";
            this.cmdApply.UseVisualStyleBackColor = true;
            // 
            // cmdCancel
            // 
            this.cmdCancel.Location = new System.Drawing.Point(267, 520);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(75, 23);
            this.cmdCancel.TabIndex = 11;
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            // 
            // cmdOK
            // 
            this.cmdOK.Location = new System.Drawing.Point(186, 520);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(75, 23);
            this.cmdOK.TabIndex = 12;
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            // 
            // cmdRefresh
            // 
            this.cmdRefresh.Location = new System.Drawing.Point(6, 520);
            this.cmdRefresh.Name = "cmdRefresh";
            this.cmdRefresh.Size = new System.Drawing.Size(121, 23);
            this.cmdRefresh.TabIndex = 13;
            this.cmdRefresh.Text = "Refresh from server";
            this.cmdRefresh.UseVisualStyleBackColor = true;            
            // 
            // frmGroupInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(431, 548);
            this.Controls.Add(this.cmdRefresh);
            this.Controls.Add(this.cmdOK);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdApply);
            this.Controls.Add(this.tabs);
            this.MaximizeBox = false;
            this.Name = "frmGroupInfo";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Group Information";
            this.tabs.ResumeLayout(false);
            this.tabGeneral.ResumeLayout(false);
            this.tabGeneral.PerformLayout();
            this.grpPreferences.ResumeLayout(false);
            this.grpPreferences.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numFee)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picInsignia)).EndInit();
            this.tabMembersRoles.ResumeLayout(false);
            this.tabsMRA.ResumeLayout(false);
            this.tabMembers.ResumeLayout(false);
            this.tabMembers.PerformLayout();
            this.tabNotices.ResumeLayout(false);
            this.tabNotices.PerformLayout();
            this.tabLand.ResumeLayout(false);
            this.tabLand.PerformLayout();
            this.tabsMoney.ResumeLayout(false);
            this.tabPlanning.ResumeLayout(false);
            this.tabPlanning.PerformLayout();
            this.tabDetails.ResumeLayout(false);
            this.tabDetails.PerformLayout();
            this.tabSales.ResumeLayout(false);
            this.tabSales.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabs;
        private System.Windows.Forms.TabPage tabGeneral;
        private System.Windows.Forms.GroupBox grpPreferences;
        private System.Windows.Forms.CheckBox chkMature;
        private System.Windows.Forms.NumericUpDown numFee;
        private System.Windows.Forms.CheckBox chkGroupNotices;
        private System.Windows.Forms.CheckBox chkFee;
        private System.Windows.Forms.CheckBox chkOpenEnrollment;
        private System.Windows.Forms.CheckBox chkPublish;
        private System.Windows.Forms.CheckBox chkShow;
        private System.Windows.Forms.ListView lstMembers;
        private System.Windows.Forms.ColumnHeader colName;
        private System.Windows.Forms.ColumnHeader colTitle;
        private System.Windows.Forms.ColumnHeader colLasLogin;
        private System.Windows.Forms.TextBox txtCharter;
        private System.Windows.Forms.Label lblFoundedBy;
        private System.Windows.Forms.Label lblGroupName;
        private System.Windows.Forms.PictureBox picInsignia;
        private System.Windows.Forms.TabPage tabMembersRoles;
        private System.Windows.Forms.TabPage tabNotices;
        private System.Windows.Forms.TabPage tabProposals;
        private System.Windows.Forms.TabPage tabLand;
        private System.Windows.Forms.Button cmdApply;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Button cmdRefresh;
        private System.Windows.Forms.TabControl tabsMRA;
        private System.Windows.Forms.TabPage tabMembers;
        private System.Windows.Forms.TabPage tabRoles;
        private System.Windows.Forms.TabPage tabAbilities;
        private System.Windows.Forms.ListView lstMembers2;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.CheckedListBox chkListRoles;
        private System.Windows.Forms.TreeView treeAbilities;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button cmdEject;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button cmdRefreshNotices;
        private System.Windows.Forms.ListView lstNotices;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ListView lstLand;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.ColumnHeader columnHeader9;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TabControl tabsMoney;
        private System.Windows.Forms.TabPage tabPlanning;
        private System.Windows.Forms.TabPage tabDetails;
        private System.Windows.Forms.TextBox txtContribution;
        private System.Windows.Forms.Label lblLandAvailable;
        private System.Windows.Forms.Label lblLandInUse;
        private System.Windows.Forms.Label lblTotalContribution;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtPlanning;
        private System.Windows.Forms.TextBox txtDetails;
        private System.Windows.Forms.TabPage tabSales;
        private System.Windows.Forms.TextBox txtSales;
        private System.Windows.Forms.Label labelInsigniaProgress;

    }
}
