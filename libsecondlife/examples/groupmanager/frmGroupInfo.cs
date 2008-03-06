using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using libsecondlife;

namespace groupmanager
{
    public partial class frmGroupInfo : Form
    {
        Group Group;
        SecondLife Client;
        GroupProfile Profile = new GroupProfile();
        Dictionary<LLUUID, GroupMember> Members = new Dictionary<LLUUID,GroupMember>();
        Dictionary<LLUUID, GroupTitle> Titles = new Dictionary<LLUUID,GroupTitle>();
        Dictionary<LLUUID, GroupMemberData> MemberData = new Dictionary<LLUUID, GroupMemberData>();
        Dictionary<LLUUID, string> Names = new Dictionary<LLUUID, string>();
        GroupManager.GroupProfileCallback GroupProfileCallback;
        GroupManager.GroupMembersCallback GroupMembersCallback;
        GroupManager.GroupTitlesCallback GroupTitlesCallback;
        AvatarManager.AvatarNamesCallback AvatarNamesCallback;
        AssetManager.ImageReceivedCallback ImageReceivedCallback;
        
        public frmGroupInfo(Group group, SecondLife client)
        {
            InitializeComponent();

            while (!IsHandleCreated)
            {
                // Force handle creation
                IntPtr temp = Handle;
            }

            GroupProfileCallback = new GroupManager.GroupProfileCallback(GroupProfileHandler);
            GroupMembersCallback = new GroupManager.GroupMembersCallback(GroupMembersHandler);
            GroupTitlesCallback = new GroupManager.GroupTitlesCallback(GroupTitlesHandler);
            AvatarNamesCallback = new AvatarManager.AvatarNamesCallback(AvatarNamesHandler);
            ImageReceivedCallback = new AssetManager.ImageReceivedCallback(Assets_OnImageReceived);

            Group = group;
            Client = client;
            
            // Register the callbacks for this form
            Client.Assets.OnImageReceived += ImageReceivedCallback;
            Client.Groups.OnGroupProfile += GroupProfileCallback;
            Client.Groups.OnGroupMembers += GroupMembersCallback;
            Client.Groups.OnGroupTitles += GroupTitlesCallback;
            Client.Avatars.OnAvatarNames += AvatarNamesCallback;

            // Request the group information
            Client.Groups.RequestGroupProfile(Group.ID);
            Client.Groups.RequestGroupMembers(Group.ID);
            Client.Groups.RequestGroupTitles(Group.ID);
        }

        ~frmGroupInfo()
        {
            // Unregister the callbacks for this form
            Client.Assets.OnImageReceived -= ImageReceivedCallback;
            Client.Groups.OnGroupProfile -= GroupProfileCallback;
            Client.Groups.OnGroupMembers -= GroupMembersCallback;
            Client.Groups.OnGroupTitles -= GroupTitlesCallback;
            Client.Avatars.OnAvatarNames -= AvatarNamesCallback;
        }

        private void GroupProfileHandler(GroupProfile profile)
        {
            Profile = profile;

            if (Group.InsigniaID != LLUUID.Zero)
                Client.Assets.RequestImage(Group.InsigniaID, ImageType.Normal, 113000.0f, 0);

            if (this.InvokeRequired)
                this.BeginInvoke(new MethodInvoker(UpdateProfile));
        }

        void Assets_OnImageReceived(ImageDownload image, AssetTexture assetTexture)
        {
            if (image.Success)
                picInsignia.Image = OpenJPEGNet.OpenJPEG.DecodeToImage(image.AssetData);
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

            UpdateNames();
        }

        private void UpdateNames()
        {
            if (this.InvokeRequired)
            {
                Invoke(new MethodInvoker(UpdateNames));
            }
            else
            {
                lock (Names)
                {
                    if (Profile.FounderID != LLUUID.Zero && Names.ContainsKey(Profile.FounderID))
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

            UpdateMembers();
        }

        private void UpdateMembers()
        {
            if (this.InvokeRequired)
            {
                Invoke(new MethodInvoker(UpdateMembers));
            }
            else
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
                            memberData.Powers = (ulong)member.Powers;
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
        }

        private void GroupTitlesHandler(Dictionary<LLUUID, GroupTitle> titles)
        {
            Titles = titles;

            UpdateTitles();
        }

        private void UpdateTitles()
        {
            if (this.InvokeRequired)
            {
                Invoke(new MethodInvoker(UpdateTitles));
            }
            else
            {
                lock (Titles)
                {
                    foreach (KeyValuePair<LLUUID, GroupTitle> kvp in Titles)
                    {
                        Console.Write("Title: " + kvp.Value.Title + " = " + kvp.Key.ToString());
                        if (kvp.Value.Selected)
                            Console.WriteLine(" (Selected)");
                        else
                            Console.WriteLine();
                    }
                }
            }
        }
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
