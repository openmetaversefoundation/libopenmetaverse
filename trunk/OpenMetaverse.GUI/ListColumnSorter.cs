/*
 * Copyright (c) 2007-2009, openmetaverse.org
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

using System.Collections;
using System.Windows.Forms;

namespace OpenMetaverse.GUI
{

    class ListColumnSorter : IComparer
    {
        public bool Ascending = true;
        public int SortColumn = 0;

        public int Compare(object a, object b)
        {
            ListViewItem itemA = (ListViewItem)a;
            ListViewItem itemB = (ListViewItem)b;

            if (SortColumn == 1)
            {
                int valueA = itemB.SubItems.Count > 1 ? int.Parse(itemA.SubItems[1].Text.Replace("m", "").Replace("--", "0")) : 0;
                int valueB = itemB.SubItems.Count > 1 ? int.Parse(itemB.SubItems[1].Text.Replace("m", "").Replace("--", "0")) : 0;
                if (Ascending)
                {
                    if (valueA == valueB) return 0;
                    return valueA < valueB ? -1 : 1;
                }
                else
                {
                    if (valueA == valueB) return 0;
                    return valueA < valueB ? 1 : -1;
                }
            }
            else
            {
                if (Ascending) return string.Compare(itemA.Text, itemB.Text);
                else return -string.Compare(itemA.Text, itemB.Text);
            }
        }
    }

}
