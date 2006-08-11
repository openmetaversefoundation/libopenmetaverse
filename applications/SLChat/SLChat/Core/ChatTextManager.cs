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
        //Our color set for the different textes.
        private Color colorProgram = Color.RoyalBlue;
        private Color colorSLSystem = Color.Teal;
        private Color colorUserText = Color.Black;
        private Color colorObjectText = Color.DarkGreen;
        private Color colorLindenText = Color.Chocolate;

        public ChatTextManager(ITextPrinter textPrinter, SLNetCom netcom)
        {
            this.textPrinter = textPrinter;
            this.netcom = netcom;

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

            StringBuilder sb = new StringBuilder();

            if (e.FromName == netcom.LoginOptions.FullName)
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
            if(e.FromName != netcom.LoginOptions.FullName)
            	sb.Append("s");
            
            //Checking for a /me command
            string Mess = e.Message;
            if(!e.Message.ToLower().StartsWith("/me "))
            {
            	sb.Append(": ");
            }else if(e.Message.StartsWith("/ME ")){
            	char[] MeChar = {'/','M','E',' '};
            	Mess = e.Message.TrimStart(MeChar);
            	sb.Append(" ");
            }else {
            	char[] MeChar = {'/','m','e',' '};
            	Mess = e.Message.TrimStart(MeChar);
            	sb.Append(" ");
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
