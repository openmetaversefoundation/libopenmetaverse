using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace SLChat
{
    public interface ITextPrinter
    {
        void PrintText(string text);
        void PrintTextLine(string text);

        string Content { get; set; }
        Color ForeColor { get; set; }
        Color BackColor { get; set; }
        Font Font { get; set; }
    }
}
