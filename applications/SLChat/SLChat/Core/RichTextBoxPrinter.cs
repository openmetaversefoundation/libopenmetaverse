using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SLChat
{
    public class RichTextBoxPrinter : ITextPrinter
    {
        private RichTextBox rtb;

        public RichTextBoxPrinter(RichTextBox textBox)
        {
            rtb = textBox;
        }

        #region ITextPrinter Members

        public void PrintText(string text)
        {
            rtb.AppendText(text);
        }

        public void PrintTextLine(string text)
        {
            rtb.AppendText(text + Environment.NewLine);
        }

        public string Content
        {
            get
            {
                return rtb.Text;
            }
            set
            {
                rtb.Text = value;
            }
        }

        public System.Drawing.Color ForeColor
        {
            get
            {
                return rtb.SelectionColor;
            }
            set
            {
                rtb.SelectionColor = value;
            }
        }

        public System.Drawing.Color BackColor
        {
            get
            {
                return rtb.SelectionBackColor;
            }
            set
            {
                rtb.SelectionBackColor = value;
            }
        }

        public System.Drawing.Font Font
        {
            get
            {
                return rtb.SelectionFont;
            }
            set
            {
                rtb.SelectionFont = value;
            }
        }

        #endregion
    }
}
