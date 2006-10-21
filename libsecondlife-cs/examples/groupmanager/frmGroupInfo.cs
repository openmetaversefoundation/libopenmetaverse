using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using libsecondlife;

namespace groupmanager
{
    public partial class frmGroupInfo : Form
    {
        Group Group;
        public frmGroupInfo(Group group)
        {
            Group = group;

            InitializeComponent();
        }
    }
}