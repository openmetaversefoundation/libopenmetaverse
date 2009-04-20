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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace WinGridProxy
{

    public partial class FormSessionSearch : Form
    {
        private FilterOptions FilterOpts;
        public FormSessionSearch(ref FilterOptions filterOptions)
        {

            InitializeComponent();
            filterOptions.SearchText = "Foo Bar";
            FilterOpts = filterOptions;
            this.checkBoxSearchSelected.Enabled = filterOptions.HasSelection;
        }

        private void textBoxFind_TextChanged(object sender, EventArgs e)
        {
            buttonFind.Enabled = (textBoxFind.TextLength > 0);
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            FilterOpts.SearchText = String.Empty;
        }

        private void buttonFind_Click(object sender, EventArgs e)
        {
            FilterOpts.HighlightMatches = panelColor.BackColor;
            FilterOpts.MatchCase = checkBoxMatchCase.Checked;
            FilterOpts.SearchSelected = checkBoxSearchSelected.Checked;
            FilterOpts.SearchText = textBoxFind.Text;
            FilterOpts.SearchWhat = comboBoxPacketsOrMessages.SelectedItem.ToString();
            FilterOpts.SelectResults = checkBoxSelectMatches.Checked;
            FilterOpts.UnMarkPrevious = checkBoxUnmark.Checked;
            this.Close();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                panelColor.BackColor = colorDialog1.Color;
            }
        }
    }

    public class FilterOptions
    {
        public bool HasSelection; // set to true if SessionList has sessions selected already;

        public string SearchText;
        public string SearchWhat; // Both, Messages, Packets
        public bool MatchCase;
        public bool SearchSelected;
        public bool SelectResults;
        public bool UnMarkPrevious;
        public Color HighlightMatches;

        public FilterOptions(bool hasSelection)
        {
            this.HasSelection = hasSelection;
        }
    }
}
