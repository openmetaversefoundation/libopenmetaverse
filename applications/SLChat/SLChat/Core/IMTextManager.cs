using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using SLNetworkComm;
using libsecondlife;

namespace SLChat
{
    public class IMTextManager
    {
        private ITextPrinter textPrinter;
        private SLNetCom netcom;
        private PrefsManager prefs;

        public IMTextManager(ITextPrinter textPrinter, SLNetCom netcom, PrefsManager preferences)
        {
            this.textPrinter = textPrinter;
            this.netcom = netcom;
            this.prefs = preferences;

            this.AddNetcomEvents();
        }

        private void AddNetcomEvents()
        {
            netcom.InstantMessageReceived += new EventHandler<InstantMessageEventArgs>(netcom_InstantMessageReceived);
            netcom.InstantMessageSent += new EventHandler<InstantMessageSentEventArgs>(netcom_InstantMessageSent);
        }

        private void netcom_InstantMessageSent(object sender, InstantMessageSentEventArgs e)
        {
            this.PrintIM(e.Timestamp, netcom.LoginOptions.FullName, e.Message);
        }

        private void netcom_InstantMessageReceived(object sender, InstantMessageEventArgs e)
        {
            this.PrintIM(e.Timestamp, e.FromAgentName, e.Message);
        }

        public void PrintIM(DateTime timestamp, string fromName, string message)
        {
            if (prefs.setIMTimestamps)
            {
            	timestamp = timestamp.AddHours(int.Parse(prefs.setIMTimeZ));
                textPrinter.ForeColor = Color.Gray;
                textPrinter.PrintText(timestamp.ToString(prefs.setIMStampFormat));
            }

            textPrinter.ForeColor = Color.Black;

            StringBuilder sb = new StringBuilder();

            sb.Append(fromName);
            sb.Append(": ");
            sb.Append(message);

            textPrinter.PrintTextLine(sb.ToString());
            sb = null;
        }
        
        public void PrintProgramMessage(string msg)
        {
        	textPrinter.ForeColor = Color.RoyalBlue;
        	textPrinter.PrintTextLine(msg);
        }

        public void PassIMEvent(InstantMessageEventArgs e)
        {
            this.netcom_InstantMessageReceived(netcom, e);
        }

        public ITextPrinter TextPrinter
        {
            get { return textPrinter; }
            set { textPrinter = value; }
        }
    }
}
