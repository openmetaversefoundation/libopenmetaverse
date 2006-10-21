using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SLNetworkComm;
using libsecondlife;

namespace SLChat
{
    public class ChatTextManager
    {
        private ITextPrinter textPrinter;
        private SLNetCom netcom;
        private PrefsManager prefs;
        //Our color set for the different textes.
        private Color colorProgram = Color.RoyalBlue;
        private Color colorSLSystem = Color.Teal;
        private Color colorUserText = Color.Black;
        private Color colorObjectText = Color.DarkGreen;
        private Color colorLindenText = Color.Chocolate;

        public ChatTextManager(ITextPrinter textPrinter, SLNetCom netcom, PrefsManager preferences)
        {
            this.textPrinter = textPrinter;
            this.netcom = netcom;
            this.prefs = preferences;

            this.AddNetcomEvents();
        }

        private void AddNetcomEvents()
        {
            netcom.ClientLoggedIn += new EventHandler<ClientLoginEventArgs>(netcom_ClientLoggedIn);
            netcom.ClientLoggedOut += new EventHandler<ClientLoginEventArgs>(netcom_ClientLoggedOut);
            netcom.ClientLoginError += new EventHandler<ClientLoginEventArgs>(netcom_ClientLoginError);
            netcom.ChatReceived += new EventHandler<ChatEventArgs>(netcom_ChatReceived);
        }

        private void netcom_ChatReceived(object sender, ChatEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Message)) return;

            if (prefs.setChatTimestamps)
            {
            	
            	TimeSpan t = (DateTime.UtcNow - new DateTime(1970, 1, 1));
                DateTime tr = (new DateTime(1970,1,1)).AddSeconds(t.TotalSeconds);
                tr = tr.AddHours(int.Parse(prefs.setChatTimeZ));
                textPrinter.ForeColor = Color.Gray;
                textPrinter.PrintText(tr.ToString(prefs.setChatStampFormat));
            }
            
            StringBuilder sb = new StringBuilder();

            //If the name is the users, the source is an agent,
            //and a "/me" command is not being done, then we append "you"
            //otherwise we append the full name.
            if (e.FromName == netcom.LoginOptions.FullName & e.SourceType == SLSourceType.Avatar & !e.Message.ToLower().StartsWith("/me ") & !prefs.setUseFullName)
                	sb.Append("You");
            else
                sb.Append(e.FromName);

            switch (e.Type)
            {
                case SLChatType.Say:
                    //sb.Append(": ");
                    break;

                case SLChatType.Whisper:
                    sb.Append(" whisper");
                    break;

                case SLChatType.Shout:
                    sb.Append(" shout");
                    break;
            }
            
            //Setting us up for proper grammar, so we don't get "You shouts"
            //but rather "You shout"
            if(e.FromName != netcom.LoginOptions.FullName & e.Type != SLChatType.Say)
            	sb.Append("s");
            
            //Checking for a /me command
            string Mess = e.Message;
            if(!e.Message.ToLower().StartsWith("/me "))
            {
            	sb.Append(": ");
            }else {
            	Mess = e.Message.Remove(0,3);
            }

            sb.Append(Mess);

            switch (e.SourceType)
            {
                case SLSourceType.Avatar:
            		if(e.FromName.Split(' ')[1]=="Linden")
            		{
            			textPrinter.ForeColor = colorLindenText;
            		}else{
                    	textPrinter.ForeColor = colorUserText;
            		}
                    break;

                case SLSourceType.Object:
                    textPrinter.ForeColor = colorObjectText;
                    break;
            }

            textPrinter.PrintTextLine(sb.ToString());
            sb = null;
        }

        private void netcom_ClientLoginError(object sender, ClientLoginEventArgs e)
        {
            textPrinter.ForeColor = colorSLSystem;
            textPrinter.PrintTextLine("Login error: " + e.LoginReply);
        }

        private void netcom_ClientLoggedOut(object sender, ClientLoginEventArgs e)
        {
            textPrinter.ForeColor = colorProgram;
            textPrinter.PrintTextLine("Logged out of Second Life.");

            textPrinter.ForeColor = colorSLSystem;
            textPrinter.PrintTextLine("Logout reply: " + e.LoginReply);
        }

        private void netcom_ClientLoggedIn(object sender, ClientLoginEventArgs e)
        {
            textPrinter.ForeColor = colorProgram;
            textPrinter.PrintTextLine("Logged into Second Life as " + netcom.LoginOptions.FullName + ".");

            textPrinter.ForeColor = colorSLSystem;
            textPrinter.PrintTextLine("Login reply: " + e.LoginReply);
        }
        
        public void PrintProgramMessage(string msg)
        {
        	textPrinter.ForeColor = colorProgram;
        	textPrinter.PrintTextLine(msg);
        }

        public ITextPrinter TextPrinter
        {
            get { return textPrinter; }
            set { textPrinter = value; }
        }
    }
}
