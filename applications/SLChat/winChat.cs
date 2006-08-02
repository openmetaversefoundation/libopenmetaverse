/*
 * Created by SharpDevelop.
 * User: Oz
 * Date: 7/11/2006
 * Time: 8:42 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Xml;
using System.Xml.XPath;
using System.IO;
using System.Threading;
using libsecondlife;
//using libsecondlife.InventorySystem;

namespace SLChat
{
	/// <summary>
	/// Description of ChatScreen.
	/// Our chat screen shows all incoming chat, names of residents
	/// around us, possibly IMs (ala XChat irc client), and is
	/// our "main window" (although winLogin is this projects actual
	/// main window).
	/// </summary>
	public class ChatScreen : System.Windows.Forms.Form
	{
		public static winLogin winLog; //Login Window.
		public static NetCom net; //NetCom calls.
		//public static winInventory winInv; //Inventory window.
		public static winAbout winAboot; //About window.
		public static ChatScreen winCht;
		//public static Thread loginthread;
		public bool aboutVisible; //Is the about window visible?
		public bool loginVisible; //Is the login window visible? Probably a better way of checking this.
		public bool loggedin; //Are we logged in? false and true
		public bool firstlog = true; //determines if we've started a login before or not
		string newline = System.Environment.NewLine; //Used for chat.
		string fname; //First Name
		string lname; //Last Name
		string pwrd; //Password
		int ChatLeng; //Length of rChatHistory, used for buggy ChatEffects.
		//Used to toggle auto-scrolling, making it easier to
		//copy text and such from rChatHistory.
		bool chatAutoScroll = true;
		//Stores residents who have spoken Name and Key
		//used for printing key to text (and later things).
		Hashtable userHash = new Hashtable();
		IMwin[] arrIM = new IMwin[4];
		public int tabCount;
		
		public ChatScreen(winLogin Load)
		{
			winLog = Load;
			winCht = this;
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			// TODO: Remove above TODO.
		}
		
				/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChatScreen));
			this.rChatHistory = new System.Windows.Forms.RichTextBox();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.listNames = new System.Windows.Forms.ListBox();
			this.cntxListNames = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.keyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.loginToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.btnSend = new System.Windows.Forms.Button();
			this.toolStrip = new System.Windows.Forms.ToolStrip();
			this.toolStripBtnIM = new System.Windows.Forms.ToolStripButton();
			this.txtChatEntry = new System.Windows.Forms.TextBox();
			this.tabMain = new System.Windows.Forms.TabControl();
			this.tabLocalChat = new System.Windows.Forms.TabPage();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.cntxListNames.SuspendLayout();
			this.menuStrip1.SuspendLayout();
			this.toolStrip.SuspendLayout();
			this.tabMain.SuspendLayout();
			this.tabLocalChat.SuspendLayout();
			this.SuspendLayout();
			// 
			// rChatHistory
			// 
			this.rChatHistory.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
									| System.Windows.Forms.AnchorStyles.Left) 
									| System.Windows.Forms.AnchorStyles.Right)));
			this.rChatHistory.AutoWordSelection = true;
			this.rChatHistory.BackColor = System.Drawing.Color.White;
			this.rChatHistory.Location = new System.Drawing.Point(3, 3);
			this.rChatHistory.Name = "rChatHistory";
			this.rChatHistory.ReadOnly = true;
			this.rChatHistory.Size = new System.Drawing.Size(421, 329);
			this.rChatHistory.TabIndex = 3;
			this.rChatHistory.Text = "";
			this.rChatHistory.LostFocus += new System.EventHandler(this.rChatHistory_LostFocus);
			this.rChatHistory.GotFocus += new System.EventHandler(this.rChatHistory_Focused);
			this.rChatHistory.TextChanged += new System.EventHandler(this.rChatHistory_TextChanged);
			// 
			// splitContainer1
			// 
			this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
									| System.Windows.Forms.AnchorStyles.Left) 
									| System.Windows.Forms.AnchorStyles.Right)));
			this.splitContainer1.Location = new System.Drawing.Point(-4, 6);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.rChatHistory);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.listNames);
			this.splitContainer1.Size = new System.Drawing.Size(589, 340);
			this.splitContainer1.SplitterDistance = 427;
			this.splitContainer1.TabIndex = 5;
			// 
			// listNames
			// 
			this.listNames.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
									| System.Windows.Forms.AnchorStyles.Left) 
									| System.Windows.Forms.AnchorStyles.Right)));
			this.listNames.ContextMenuStrip = this.cntxListNames;
			this.listNames.FormattingEnabled = true;
			this.listNames.Location = new System.Drawing.Point(2, 3);
			this.listNames.Name = "listNames";
			this.listNames.Size = new System.Drawing.Size(152, 329);
			this.listNames.TabIndex = 2;
			// 
			// cntxListNames
			// 
			this.cntxListNames.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
									this.keyToolStripMenuItem,
									this.toolStripSeparator2,
									this.removeToolStripMenuItem});
			this.cntxListNames.Name = "cntxListNames";
			this.cntxListNames.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			this.cntxListNames.ShowImageMargin = false;
			this.cntxListNames.Size = new System.Drawing.Size(129, 54);
			// 
			// keyToolStripMenuItem
			// 
			this.keyToolStripMenuItem.Name = "keyToolStripMenuItem";
			this.keyToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
			this.keyToolStripMenuItem.Text = "Print Key (UUID)";
			this.keyToolStripMenuItem.Click += new System.EventHandler(this.KeyToolStripMenuItemClick);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(125, 6);
			// 
			// removeToolStripMenuItem
			// 
			this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
			this.removeToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
			this.removeToolStripMenuItem.Text = "Remove";
			this.removeToolStripMenuItem.Click += new System.EventHandler(this.RemoveToolStripMenuItemClick);
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
									this.fileToolStripMenuItem,
									this.helpToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			this.menuStrip1.Size = new System.Drawing.Size(589, 24);
			this.menuStrip1.TabIndex = 4;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
									this.loginToolStripMenuItem,
									this.toolStripSeparator1,
									this.exitToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
			this.fileToolStripMenuItem.Text = "&File";
			// 
			// loginToolStripMenuItem
			// 
			this.loginToolStripMenuItem.Name = "loginToolStripMenuItem";
			this.loginToolStripMenuItem.Size = new System.Drawing.Size(111, 22);
			this.loginToolStripMenuItem.Text = "&Login...";
			this.loginToolStripMenuItem.Click += new System.EventHandler(this.LoginToolStripMenuItemClick);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(108, 6);
			// 
			// exitToolStripMenuItem
			// 
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.Size = new System.Drawing.Size(111, 22);
			this.exitToolStripMenuItem.Text = "E&xit";
			this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItemClick);
			// 
			// helpToolStripMenuItem
			// 
			this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
									this.aboutToolStripMenuItem});
			this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
			this.helpToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
			this.helpToolStripMenuItem.Text = "&Help";
			// 
			// aboutToolStripMenuItem
			// 
			this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
			this.aboutToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
			this.aboutToolStripMenuItem.Text = "&About";
			this.aboutToolStripMenuItem.Click += new System.EventHandler(this.AboutToolStripMenuItemClick);
			// 
			// btnSend
			// 
			this.btnSend.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.btnSend.Location = new System.Drawing.Point(505, 352);
			this.btnSend.Name = "btnSend";
			this.btnSend.Size = new System.Drawing.Size(73, 29);
			this.btnSend.TabIndex = 1;
			this.btnSend.Text = "Say";
			this.btnSend.UseVisualStyleBackColor = true;
			this.btnSend.Click += new System.EventHandler(this.BtnSendClick);
			// 
			// toolStrip
			// 
			this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
									this.toolStripBtnIM});
			this.toolStrip.Location = new System.Drawing.Point(0, 24);
			this.toolStrip.Name = "toolStrip";
			this.toolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			this.toolStrip.Size = new System.Drawing.Size(589, 25);
			this.toolStrip.Stretch = true;
			this.toolStrip.TabIndex = 6;
			this.toolStrip.Text = "toolStrip";
			// 
			// toolStripBtnIM
			// 
			this.toolStripBtnIM.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripBtnIM.Image = ((System.Drawing.Image)(resources.GetObject("toolStripBtnIM.Image")));
			this.toolStripBtnIM.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripBtnIM.Name = "toolStripBtnIM";
			this.toolStripBtnIM.Size = new System.Drawing.Size(23, 22);
			this.toolStripBtnIM.Text = "Instant Message";
			this.toolStripBtnIM.Click += new System.EventHandler(this.ToolStripBtnIMClick);
			// 
			// txtChatEntry
			// 
			this.txtChatEntry.Location = new System.Drawing.Point(3, 357);
			this.txtChatEntry.MaxLength = 977;
			this.txtChatEntry.Name = "txtChatEntry";
			this.txtChatEntry.Size = new System.Drawing.Size(496, 21);
			this.txtChatEntry.TabIndex = 8;
			this.txtChatEntry.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtChatEntry_KeyPress);
			// 
			// tabMain
			// 
			this.tabMain.Appearance = System.Windows.Forms.TabAppearance.Buttons;
			this.tabMain.Controls.Add(this.tabLocalChat);
			this.tabMain.Location = new System.Drawing.Point(0, 52);
			this.tabMain.Name = "tabMain";
			this.tabMain.SelectedIndex = 0;
			this.tabMain.Size = new System.Drawing.Size(589, 413);
			this.tabMain.TabIndex = 9;
			// 
			// tabLocalChat
			// 
			this.tabLocalChat.Controls.Add(this.splitContainer1);
			this.tabLocalChat.Controls.Add(this.txtChatEntry);
			this.tabLocalChat.Controls.Add(this.btnSend);
			this.tabLocalChat.Location = new System.Drawing.Point(4, 25);
			this.tabLocalChat.Name = "tabLocalChat";
			this.tabLocalChat.Padding = new System.Windows.Forms.Padding(3);
			this.tabLocalChat.Size = new System.Drawing.Size(581, 384);
			this.tabLocalChat.TabIndex = 0;
			this.tabLocalChat.Text = "Local Chat";
			this.tabLocalChat.UseVisualStyleBackColor = true;
			// 
			// ChatScreen
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(589, 462);
			this.Controls.Add(this.toolStrip);
			this.Controls.Add(this.menuStrip1);
			this.Controls.Add(this.tabMain);
			this.MainMenuStrip = this.menuStrip1;
			this.MinimumSize = new System.Drawing.Size(400, 300);
			this.Name = "ChatScreen";
			this.Text = "Chat";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.ExitToolStripMenuItemClick);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.ResumeLayout(false);
			this.cntxListNames.ResumeLayout(false);
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.toolStrip.ResumeLayout(false);
			this.toolStrip.PerformLayout();
			this.tabMain.ResumeLayout(false);
			this.tabLocalChat.ResumeLayout(false);
			this.tabLocalChat.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		private System.Windows.Forms.TabPage tabLocalChat;
		public System.Windows.Forms.TabControl tabMain;
		private System.Windows.Forms.ToolStripButton toolStripBtnIM;
		private System.Windows.Forms.ToolStrip toolStrip;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripMenuItem keyToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
		private System.Windows.Forms.ContextMenuStrip cntxListNames;
		private System.Windows.Forms.RichTextBox rChatHistory;
		private System.Windows.Forms.TextBox txtChatEntry;
		private System.Windows.Forms.Button btnSend;
		private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem loginToolStripMenuItem;
		private System.Windows.Forms.ListBox listNames;
		private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.SplitContainer splitContainer1;
		
		public void callLogin(string firstname, string lastname, string password, string logLocation)
		{
			this.Text = "SLChat: "+firstname+" "+lastname;
			fname = firstname;
			lname = lastname;
			pwrd = password;
			//client = new SecondLife("keywords.txt", "protocol.txt");
			rChatHistory.Text += newline + "Logging in..." + newline;
			if(firstlog==true)
			{
				firstlog = false;
				net = new NetCom(firstname, lastname, password, logLocation, this);
				//loginthread = new Thread(new ThreadStart(net.Login));
				//loginthread.Start();
				net.Login();
			}else{
				//loginthread = new Thread(new ThreadStart(net.Login));
				//loginthread.Start();
				net.Login();
			}
		}
		
		private void rChatHistory_TextChanged(object sender, System.EventArgs e)
		{
			//TODO: Still some issues when new text is recieved. Fix.
			
			//Make sure we automaticly skip down to the last line.
			//But only if we're allowed to (i.e. chat history does
			//not have focus)
			if(chatAutoScroll==true)
			{
				rChatHistory.SelectionStart = rChatHistory.Text.Length;
				rChatHistory.ScrollToCaret();
			}
		}
		
		private void rChatHistory_Focused(object sender, System.EventArgs e)
		{
			//If we have focus, don't auto scroll.
			//Why? Because usualy the control has focus when the
			//user wishes to grab text or scroll themselves
			//and it is annoying if the chat is jumping down on you.
			chatAutoScroll = false;
		}
		
		private void rChatHistory_LostFocus(object sender, System.EventArgs e)
		{
			//See above.
			chatAutoScroll = true;
		}
		
		public void ChatEffects(int lenColon, int lenFull, int type)
 		{
			//TODO: Fix this so that it can highlight text,
			//System text should be green with "System" being bold
			//User text should be black or grey with "name" being
			//a different color for each name (ala xchat).
			
			lenFull = lenFull - 3;
    		// Determine the starting location of the word "fox".
    		//int index = rChatHistory.Text.IndexOf(searchString, ChatLeng);
    		//int index = rChatHistory.Find(searchString);
    		txtChatEntry.Text += ":/ "+ChatLeng+", "+lenColon+", "+lenFull;
    		// Determine if the word has been found and select it if it was.
    		//if (index != -1)
    		//{
      			rChatHistory.DeselectAll();
      			rChatHistory.Select(ChatLeng, lenFull);
       			rChatHistory.SelectionFont = new Font(
         					rChatHistory.SelectionFont.FontFamily, 
         					rChatHistory.SelectionFont.Size, 
         					FontStyle.Regular);
       			rChatHistory.SelectionColor = System.Drawing.Color.Black;
       			
       			txtChatEntry.Text += ":/ "+rChatHistory.SelectedText+", "+type+",#";
       			if(type==1)
       			{
       				//System message, such as MOTD
       				rChatHistory.SelectionColor = System.Drawing.Color.ForestGreen;
       				rChatHistory.SelectionFont = new Font(
         					rChatHistory.SelectionFont.FontFamily, 
         					rChatHistory.SelectionFont.Size, 
         					FontStyle.Regular);
       			}else if(type==2){
 					rChatHistory.SelectionColor = System.Drawing.Color.Black;
 					rChatHistory.SelectionFont = new Font(
         					rChatHistory.SelectionFont.FontFamily, 
         					rChatHistory.SelectionFont.Size, 
         					FontStyle.Regular);
       			}
       			rChatHistory.DeselectAll();
       			rChatHistory.Select(ChatLeng, lenColon);
       			rChatHistory.SelectionFont = new Font(
         					rChatHistory.SelectionFont.FontFamily, 
         					rChatHistory.SelectionFont.Size, 
         					FontStyle.Regular);
       			rChatHistory.SelectionColor = System.Drawing.Color.Black;
       			//int colon = searchString.IndexOf(":",0) + 1;
       			//txtChatEntry.Text += colon+",colon, "+searchString;
       			//int colon = rChatHistory.Find(":",index,RichTextBoxFinds.None);
       			//if(colon != -1)
       			//{
       				//int testLen = colon - index;
       				
       				txtChatEntry.Text += ":/ "+rChatHistory.SelectedText+", "+type+",,";
       				//System.Drawing.Font currentFont = rChatHistory.SelectionFont;
       				if(type==1){
       					rChatHistory.SelectionColor = System.Drawing.Color.ForestGreen;
       					rChatHistory.SelectionFont = new Font(
         					rChatHistory.SelectionFont.FontFamily, 
         					rChatHistory.SelectionFont.Size, 
         					FontStyle.Bold);
       				}else if(type==2){
      					//Regular chat, such as names, etc.
      					rChatHistory.SelectionColor = System.Drawing.Color.Blue;
      					rChatHistory.SelectionFont = new Font(
         					rChatHistory.SelectionFont.FontFamily, 
         					rChatHistory.SelectionFont.Size, 
         					FontStyle.Bold);
      				}
       			//}
       			//txtChatEntry.Text = searchString;
       			rChatHistory.DeselectAll();
       			rChatHistory.SelectionFont = new Font(
         					rChatHistory.SelectionFont.FontFamily, 
         					rChatHistory.SelectionFont.Size, 
         					FontStyle.Regular);
       			rChatHistory.SelectionColor = System.Drawing.Color.Black;
       			ChatLeng = rChatHistory.Text.Length;
    		//}else{
    			//txtChatEntry.Text = "Not found";
    		//}
 		}
		
		private void txtChatEntry_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			//When the enter key is pressed in the txtChatEntry
			int key = (int) e.KeyChar;
			if(key == (int) Keys.Enter)
			{
				if(btnSend.Enabled)
				{
					BtnSendClick(sender, e);
					e.Handled = true;
				}
				else
				{
					;
				}
			}
		}
		
		void ExitToolStripMenuItemClick(object sender, System.EventArgs e)
		{
			//This is our exiting function.
			if(loggedin==true)
			{
				rChatHistory.Text += newline + "Logging out...";
				net.Logout();
			}
			
			if(loggedin==false)
			{
				//Cleanup thread call just incase.
				//if(loginthread.IsAlive)
				//{
				//	loginthread.Abort();
				//}
				Application.Exit();
			}
			//System.Windows.Forms.Application.Exit()
			//System.Environment.Exit(1)
		}
		
		void LoginToolStripMenuItemClick(object sender, System.EventArgs e)
		{
			//Our login menu option from File.
			//If loggedin, we log out. (should show loginwindow too
			//after successfull log out)
			//If loggedout, show login window (winLogin).
			if(loggedin==false)
			{
				if(loginVisible==false)
				{
					winLog.chatCreated = true;
					winLog.Show();
					loginVisible = true;
				}else{
					winLog.Focus();
				}
			}else{
				net.Logout();
			}
		}
		
		void BtnSendClick(object sender, System.EventArgs e)
		{
			//SL will actualy accept "" chat, but we don't
			//want to encourage that.
			if(txtChatEntry.Text != "")
			{
				//No need to handle sending "You" text localy, this is
				//bad anyway, we want to make sure we get all text
				//even our own, from the server bouncing it back.
			
				//Takes the chat and sends it to our net function "ChatOut"
				//which handles all outgoing chat sending.
				//Text, Type (say, shout, whisper), Channel (0 == public)
				net.ChatOut(txtChatEntry.Text,0,0);
				//Clear the chat entry textbox.
				txtChatEntry.Text = "";
			}
		}
		
		void RemoveToolStripMenuItemClick(object sender, System.EventArgs e)
		{
			//Removing a user from the Names List. This should
			//perhaps be done automaticly somehow, maybe by offline
			//status or distance?
			if(this.listNames.Items.ToString() != "")
			{
				//This is basicly removing the person from our
				//userHash, hashtable, which stores Name and Key used
				//for printing key to chat.
				if(userHash.ContainsKey(listNames.SelectedItem.ToString()))
				{
					//Remove from our hashtable, just to keep things clean.
					userHash.Remove(listNames.SelectedItem.ToString());
      				//Remove from the actual list element.
					listNames.Items.Remove(listNames.SelectedItem);
				}
			}
		}
		
		void KeyToolStripMenuItemClick(object sender, System.EventArgs e)
		{
			//This prints the selected user's Key to rChatHistory
			//with the pretext of "Program:"
			if(userHash.ContainsKey(listNames.SelectedItem.ToString()))
			{
				IDictionaryEnumerator myEnum = userHash.GetEnumerator();
      			while (myEnum.MoveNext())
      			{
      				if(myEnum.Key.ToString()==listNames.SelectedItem.ToString())
      				{
      					string name = myEnum.Key.ToString();
      					string key = myEnum.Value.ToString();
      					key = key.Insert(8,"-");
						key = key.Insert(13,"-");
						key = key.Insert(18,"-");
						key = key.Insert(23,"-");
						rChatHistory.Text += newline + "Program: "+ name + " == " + key;
      				}
      			}
			}
		}
		
		
		void ToolStripBtnIMClick(object sender, System.EventArgs e)
		{
			//This will probably be removed in favor for an
			//xchat like interface with a new IM appearing in
			//a tab button at the bottom of the main window
			//instead of a new window alltogether.
			winIM IM = new winIM();
			IM.Show();
		}
		
		public void ReturnData(string data, int type, string extra1, string extra2)
		{
			//This is how we get data back from our NetCom.cs file
			//we use the returned data in a certain way to output
			//it for the interface.
			if(type==1)
			{
				//Reply Type: Login
				if(extra1!="error")
				{
					loginToolStripMenuItem.Text = "Logout";
					//string loginReply = Login(firstname, lastname, password);
					//string loginReply = "OMG WHATS: ON YUR FACE?! Yes yes yes cabam shamzaameiawml" + newline;
					rChatHistory.Text += data + newline;
					//ChatEffects(loginReply,1);
					//string text2 = "Bob Smith: AHWIWEAM W!";
					//rChatHistory.Text += text2 + newline;
					//ChatEffects(18,loginReply.Length,1);
					loggedin = true;
				}else if(extra1=="error"){
					rChatHistory.Text += data + newline;
					loggedin = false;
				}
			}else if(type==2){
				//Reply Type: Logout
				loginToolStripMenuItem.Text = "Login...";
				this.Text = "SLChat";
				rChatHistory.Text += data;
				loggedin = false;
				//Cleanup thread call just incase.
				//if(loginthread.IsAlive)
				//{
				//	loginthread.Abort();
				//}
			}else if(type==3){
				//Reply Type: Chat
				
				//Add the name if it is not already added to the list
				//Might want to check hashtable instead of listelement
				if(!listNames.Items.Contains(extra1)){
						listNames.Items.Add(extra1);
						//Add the Name (extra1) and Key (extra2) of the
						//resident who spoke. Used for printing key.
						userHash.Add(extra1,extra2);
				}
				//Printing actual chat.
				rChatHistory.Text += data;
				//ChatEffects(name.Length,output.Length,2);
			}else if(type==4){
				//Reply Type: IM
				
				for(int i=0;i<arrIM.Length;i++)
				{
					if(arrIM[i]==null)
					{
						
					}else if(arrIM[i].name==extra1)
					{
						arrIM[i].GotText(data);
						break;
					}
					if(i+1==arrIM.Length){
						CreateTab(extra1,data);
					}
				}
			}
		}
		
		void AboutToolStripMenuItemClick(object sender, System.EventArgs e)
		{
			if(aboutVisible==false)
			{
				winAboot = new winAbout();
				winAboot.Show();
			}else{
				winAboot.Show();
			}
		}
		
		public void CreateTab(string name, string text)
		{
			int i = tabCount;
			arrIM[i] = new IMwin();
			//arrIM[i].Tabs(name,text);
		}
		
		public class IMwin : System.ComponentModel.Component
		{
			public string name;
			
			public IMwin()
			{
				
			}
			//private System.ComponentModel.IContainer components = null;
			public void Tabs(string nme, string txt)
			{
				name = nme;
				//this.components = new System.ComponentModel.Container();
				this.tabIM = new System.Windows.Forms.TabPage();
				this.pnlIM = new System.Windows.Forms.Panel();
				this.txtIMEntry = new System.Windows.Forms.TextBox();
				this.btnIMSend = new System.Windows.Forms.Button();
				this.btnClose = new System.Windows.Forms.Button();
				this.rIMHistory = new System.Windows.Forms.RichTextBox();
				this.tabIM.SuspendLayout();
				this.pnlIM.SuspendLayout();
				//winCht.tabMain.SuspendLayout();
				//this.SuspendLayout();
				//winCht.SuspendLayout();
				// 
				// tabIM
				// 
				this.tabIM.Controls.Add(this.pnlIM);
				this.tabIM.Location = new System.Drawing.Point(4, 25);
				this.tabIM.Name = "tabIM";
				this.tabIM.Padding = new System.Windows.Forms.Padding(3);
				this.tabIM.Size = new System.Drawing.Size(581, 384);
				this.tabIM.TabIndex = 1;
				this.tabIM.Text = "IM";
				this.tabIM.UseVisualStyleBackColor = true;
				// 
				// pnlIM
				// 
				this.pnlIM.Controls.Add(this.txtIMEntry);
				this.pnlIM.Controls.Add(this.btnIMSend);
				this.pnlIM.Controls.Add(this.btnClose);
				this.pnlIM.Controls.Add(this.rIMHistory);
				this.pnlIM.Location = new System.Drawing.Point(0, 0);
				this.pnlIM.Name = "pnlIM";
				this.pnlIM.Size = new System.Drawing.Size(581, 384);
				this.pnlIM.TabIndex = 0;
				// 
				// txtIMEntry
				// 
				this.txtIMEntry.Location = new System.Drawing.Point(6, 354);
				this.txtIMEntry.Name = "txtIMEntry";
				this.txtIMEntry.Size = new System.Drawing.Size(496, 21);
				this.txtIMEntry.TabIndex = 3;
				// 
				// btnIMSend
				// 
				this.btnIMSend.Location = new System.Drawing.Point(508, 349);
				this.btnIMSend.Name = "btnIMSend";
				this.btnIMSend.Size = new System.Drawing.Size(67, 29);
				this.btnIMSend.TabIndex = 2;
				this.btnIMSend.Text = "Send";
				this.btnIMSend.UseVisualStyleBackColor = true;
				// 
				// btnClose
				// 
				this.btnClose.Location = new System.Drawing.Point(496, 6);
				this.btnClose.Name = "btnClose";
				this.btnClose.Size = new System.Drawing.Size(79, 29);
				this.btnClose.TabIndex = 1;
				this.btnClose.Text = "Close";
				this.btnClose.UseVisualStyleBackColor = true;
				// 
				// rIMHistory
				// 
				this.rIMHistory.BackColor = System.Drawing.Color.White;
				this.rIMHistory.Location = new System.Drawing.Point(3, 35);
				this.rIMHistory.Name = "rIMHistory";
				this.rIMHistory.ReadOnly = true;
				this.rIMHistory.Size = new System.Drawing.Size(575, 308);
				this.rIMHistory.TabIndex = 0;
				this.rIMHistory.Text = "";
				
				//winCht.tabMain.ResumeLayout(false);
				winCht.tabMain.Controls.Add(this.tabIM);
				this.tabIM.ResumeLayout(false);
				this.pnlIM.ResumeLayout(false);
				this.pnlIM.PerformLayout();
				//this.ResumeLayout(false);
				//this.PerformLayout();
				//winCht.ResumeLayout(false);
				//winCht.PerformLayout();
				winCht.rChatHistory.Text += txt + "TABS5";
			
				/*
				string title = "Oz Spade";
				TabPage myTabPage = new TabPage(title);
				tabMain.TabPages.Add(myTabPage);
				myTabPage.Controls.Add(rChatThing);*/
				winCht.tabCount++;
				GotText(txt);
			}
			private System.Windows.Forms.RichTextBox rIMHistory;
			private System.Windows.Forms.Button btnClose;
			private System.Windows.Forms.Button btnIMSend;
			private System.Windows.Forms.TextBox txtIMEntry;
			private System.Windows.Forms.Panel pnlIM;
			private System.Windows.Forms.TabPage tabIM;

			void BtnSendClick(object sender, System.EventArgs e)
			{
				//this.tabNext.Text = name;
			}
			
			void BtnCloseClick(object sender, System.EventArgs e)
			{
				winCht.tabMain.Controls.Remove(this.tabIM);
				Array.Clear(winCht.arrIM,winCht.tabMain.SelectedIndex,1);
				winCht.tabCount--;
			}
			
			public void GotText(string txt)
			{
				rIMHistory.Text += txt;
				if(!this.tabIM.Visible)
				{
					this.tabIM.Text = name + " (N)";
				}else{
					this.tabIM.Text = name;
				}
			}
		}
	}
}
