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
        Dictionary<LLUUID, GroupMemberData> MemberData;
        Dictionary<LLUUID, string> Names;
        
        public frmGroupInfo(Group group, SecondLife client)
        {
            Group = group;
            Client = client;
            Profile = new GroupProfile();
            MemberData = new Dictionary<LLUUID, GroupMemberData>();
            Names = new Dictionary<LLUUID, string>();

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
            txtCharter.Text = Profile.Charter;
            chkShow.Checked = Profile.ShowInList;
            chkPublish.Checked = Profile.AllowPublish;
            chkOpenEnrollment.Checked = Profile.OpenEnrollment;
            chkFee.Checked = (Profile.MembershipFee != 0);
            numFee.Value = Profile.MembershipFee;
            chkMature.Checked = Profile.MaturePublish;

            Client.Avatars.BeginGetAvatarName(Profile.FounderID, new AgentNamesCallback(AgentNamesHandler));
        }

        private void AgentNamesHandler(Dictionary<LLUUID, string> names)
        {
            lock (Names)
            {
                foreach (KeyValuePair<LLUUID, string> agent in names)
                {
                    Names[agent.Key] = agent.Value;
                }
            }

            Invoke(new MethodInvoker(UpdateNames));
        }

        private void UpdateNames()
        {
            GroupMemberData member;

            lock (Names)
            {
                if (Profile.FounderID != null && Names.ContainsKey(Profile.FounderID))
                {
                    lblFoundedBy.Text = "Founded by " + Names[Profile.FounderID];
                }

                lock (MemberData)
                {
                    foreach (KeyValuePair<LLUUID, string> name in Names)
                    {
                        if (!MemberData.ContainsKey(name.Key))
                        {
                            MemberData[name.Key] = new GroupMemberData();
                        }

                        member = MemberData[name.Key];
                        member.Name = name.Value;
                    }
                }
            }

            UpdateMemberList();
        }

        private void UpdateMemberList()
        {
            // General tab list
            lock (lstMembers)
            {
                lstMembers.Items.Clear();

                foreach (GroupMemberData entry in MemberData.Values)
                {
                    ListViewItem lvi = new ListViewItem();
                    lvi.Text = entry.Name;

                    ListViewItem.ListViewSubItem lvsi = new ListViewItem.ListViewSubItem();
                    lvsi.Text = entry.Title;
                    lvi.SubItems.Add(lvsi);

                    lvsi = new ListViewItem.ListViewSubItem();
                    lvsi.Text = entry.LastOnline;
                    lvi.SubItems.Add(lvsi);

                    lstMembers.Items.Add(lvi);
                }
            }

            // Members tab list
            lock (lstMembers2)
            {
                lstMembers2.Items.Clear();

                foreach (GroupMemberData entry in MemberData.Values)
                {
                    ListViewItem lvi = new ListViewItem();
                    lvi.Text = entry.Name;

                    ListViewItem.ListViewSubItem lvsi = new ListViewItem.ListViewSubItem();
                    lvsi.Text = entry.Contribution.ToString();
                    lvi.SubItems.Add(lvsi);

                    lvsi = new ListViewItem.ListViewSubItem();
                    lvsi.Text = entry.LastOnline;
                    lvi.SubItems.Add(lvsi);

                    lstMembers2.Items.Add(lvi);
                }
            }
        }

        private void GroupMembersHandler(Dictionary<LLUUID, GroupMember> members)
        {
            Members = members;

            Invoke(new MethodInvoker(UpdateMembers));
        }

        private void UpdateMembers()
        {
            List<LLUUID> requestids = new List<LLUUID>();

            lock (Members)
            {
                lock (MemberData)
                {
                    foreach (GroupMember member in Members.Values)
                    {
                        GroupMemberData memberData = new GroupMemberData();
                        memberData.ID = member.ID;
                        memberData.IsOwner = member.IsOwner;
                        memberData.LastOnline = member.OnlineStatus;
                        memberData.Powers = member.Powers;
                        memberData.Title = member.Title;
                        memberData.Contribution = member.Contribution;

                        MemberData[member.ID] = memberData;

                        // Add this ID to the name request batch
                        requestids.Add(member.ID);
                    }
                }
            }

            Client.Avatars.BeginGetAvatarNames(requestids, new AgentNamesCallback(AgentNamesHandler));
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

    public class GroupMemberData
    {
        public LLUUID ID;
        public string Name;
        public string Title;
        public string LastOnline;
        public ulong Powers;
        public bool IsOwner;
        public int Contribution;
    }
}
