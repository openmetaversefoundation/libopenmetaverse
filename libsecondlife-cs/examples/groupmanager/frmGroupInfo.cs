using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using libsecondlife;
using libsecondlife.AssetSystem;

namespace groupmanager
{
    public partial class frmGroupInfo : Form
    {
        Group Group;
        SecondLife Client;
        GroupProfile Profile;
        Dictionary<LLUUID, GroupMember> Members;
        Dictionary<LLUUID, GroupTitle> Titles;
        
        public frmGroupInfo(Group group, SecondLife client)
        {
            Group = group;
            Client = client;

            InitializeComponent();
        }

        private void frmGroupInfo_Load(object sender, EventArgs e)
        {
            Client.Groups.BeginGetGroupProfile(Group.ID,
                new GroupManager.GroupProfileCallback(GroupProfileHandler));

            Client.Groups.BeginGetGroupMembers(Group.ID,
                new GroupManager.GroupMembersCallback(GroupMembersHandler));

            Client.Groups.BeginGetGroupTitles(Group.ID,
                new GroupManager.GroupTitlesCallback(GroupTitlesHandler));
        }

        private void GroupProfileHandler(GroupProfile profile)
        {
            Profile = profile;

            Invoke(new MethodInvoker(UpdateProfile));

            // Waterdrop: new LLUUID("c77a1c21-e604-7d2c-2c89-5539ce853466")
            ImageManager im = new ImageManager(Client);
            byte[] j2cdata = im.RequestImage(Group.InsigniaID);
            //
            JasperWrapper.jas_init();
            byte[] imagedata = JasperWrapper.jasper_decode_j2c_to_tiff(j2cdata);
            //
            MemoryStream imageStream = new MemoryStream(imagedata, false);
            Image image = Image.FromStream(imageStream, false, false);
            //
            picInsignia.Image = image;
        }

        private void UpdateProfile()
        {
            lblGroupName.Text = Profile.Name;
            lblFoundedBy.Text = "Founded by " + Profile.FounderID.ToStringHyphenated();
            txtCharter.Text = Profile.Charter;
            chkShow.Checked = Profile.ShowInList;
            chkPublish.Checked = Profile.AllowPublish;
            chkOpenEnrollment.Checked = Profile.OpenEnrollment;
            chkFee.Checked = (Profile.MembershipFee != 0);
            numFee.Value = Profile.MembershipFee;
            chkMature.Checked = Profile.MaturePublish;
            lblMemberTitle.Text = Profile.MemberTitle;
        }

        private void GroupMembersHandler(Dictionary<LLUUID, GroupMember> members)
        {
            Members = members;

            Invoke(new MethodInvoker(UpdateMembers));
        }

        private void UpdateMembers()
        {
            ;
        }

        private void GroupTitlesHandler(Dictionary<LLUUID, GroupTitle> titles)
        {
            Titles = titles;

            Invoke(new MethodInvoker(UpdateTitles));
        }

        private void UpdateTitles()
        {
            ;
        }

        //private void BytesToFile(byte[] bytes, string filename)
        //{
        //    FileStream filestream = new FileStream(filename, FileMode.Create);
        //    BinaryWriter writer = new BinaryWriter(filestream);
        //    writer.Write(bytes);
        //    writer.Close();
        //    filestream.Close();
        //}
    }
}
