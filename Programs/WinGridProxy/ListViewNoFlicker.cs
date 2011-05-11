/*
 * Copyright (c) 2009, openmetaverse.org
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the openmetaverse.org nor the names
 *   of its contributors may be used to endorse or promote products derived from
 *   this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace WinGridProxy
{
    class ListViewNoFlicker : ListView
    {
        public ListViewNoFlicker()
        {

            //Activate double buffering
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

            //Enable the OnNotifyMessage event so we get a chance to filter out 
            // Windows messages before they get to the form's WndProc
            this.SetStyle(ControlStyles.EnableNotifyMessage, true);
        }

        protected override void OnNotifyMessage(Message m)
        {
            //Filter out the WM_ERASEBKGND message
            if (m.Msg != 0x14)
            {
                base.OnNotifyMessage(m);
            }
        }

    }

    public class ListViewItemComparer : IComparer<object>
    {
        // Initialize the variables to default
        public int column = 0;
        public bool bAscending = true;

        // Using the Compare function of IComparer
        public int Compare(object x, object y)
        {
            // Cast the objects to ListViewItems
            ListViewItem lvi1 = (ListViewItem)x;
            ListViewItem lvi2 = (ListViewItem)y;

            // If the column is the string columns
            if (column != 2)
            {
                string lvi1String = lvi1.SubItems[column].ToString();
                string lvi2String = lvi2.SubItems[column].ToString();

                // Return the normal Compare
                if (bAscending)
                    return String.Compare(lvi1String, lvi2String);

                // Return the negated Compare
                return -String.Compare(lvi1String, lvi2String);
            }

            // The column is the Age column
            int lvi1Int = ParseListItemString(lvi1.SubItems[column].ToString());
            int lvi2Int = ParseListItemString(lvi2.SubItems[column].ToString());

            // Return the normal compare.. if x < y then return -1
            if (bAscending)
            {
                if (lvi1Int < lvi2Int)
                    return -1;
                else if (lvi1Int == lvi2Int)
                    return 0;

                return 1;
            }

            // Return the opposites for descending
            if (lvi1Int > lvi2Int)
                return -1;
            else if (lvi1Int == lvi2Int)
                return 0;

            return 1;
        }

        private int ParseListItemString(string x)
        {
            //ListViewItems are returned like this: "ListViewSubItem: {19}"
            int counter = 0;
            for (int i = x.Length - 1; i >= 0; i--, counter++)
            {
                if (x[i] == '{')
                    break;
            }

            return Int32.Parse(x.Substring(x.Length - counter, counter - 1));
        }
    }



}
