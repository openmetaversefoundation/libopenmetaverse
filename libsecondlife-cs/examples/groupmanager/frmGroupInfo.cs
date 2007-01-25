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

    public partial class frmGroupInfo : Form
    {
        Group Group;
        SecondLife Client;
        GroupProfile Profile = new GroupProfile();
        Dictionary<LLUUID, GroupMember> Members = new Dictionary<LLUUID,GroupMember>();
        Dictionary<LLUUID, GroupTitle> Titles = new Dictionary<LLUUID,GroupTitle>();
        Dictionary<LLUUID, GroupMemberData> MemberData = new Dictionary<LLUUID, GroupMemberData>();
        Dictionary<LLUUID, string> Names = new Dictionary<LLUUID, string>();
        
        public frmGroupInfo(Group group, SecondLife client)
        {
            InitializeComponent();

            while (!IsHandleCreated)
            {
                // Force handle creation
                IntPtr temp = Handle;
            }

            Group = group;
            Client = client;

            Client.Avatars.OnAvatarNames += new AvatarManager.AvatarNamesCallback(AvatarNamesHandler);

            // Request the group information

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

            byte[] j2cdata;
            if (Group.InsigniaID != null)
            {
                j2cdata = Client.Images.RequestImage(Group.InsigniaID);
            }
            else
            {
                // ???
                j2cdata = Client.Images.RequestImage("c77a1c21-e604-7d2c-2c89-5539ce853466");
            }

            Image image = OpenJPEGNet.OpenJPEG.DecodeToImage(j2cdata);
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

            Client.Avatars.RequestAvatarName(Profile.FounderID);
        }

        private void AvatarNamesHandler(Dictionary<LLUUID, string> names)
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

                        MemberData[name.Key].Name = name.Value;
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

            Client.Avatars.RequestAvatarNames(requestids);
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
    }
}
