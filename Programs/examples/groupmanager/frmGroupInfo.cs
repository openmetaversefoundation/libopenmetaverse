using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using OpenMetaverse;
using OpenMetaverse.Imaging;
using OpenMetaverse.Assets;

namespace groupmanager
{
    public partial class frmGroupInfo : Form
    {
        Group Group;
        GridClient Client;
        Group Profile = new Group();
        Dictionary<UUID, GroupMember> Members = new Dictionary<UUID,GroupMember>();
        Dictionary<UUID, GroupTitle> Titles = new Dictionary<UUID,GroupTitle>();
        Dictionary<UUID, GroupMemberData> MemberData = new Dictionary<UUID, GroupMemberData>();
        Dictionary<UUID, string> Names = new Dictionary<UUID, string>();
        GroupManager.GroupProfileCallback GroupProfileCallback;
        GroupManager.GroupMembersCallback GroupMembersCallback;
        GroupManager.GroupTitlesCallback GroupTitlesCallback;
        AvatarManager.AvatarNamesCallback AvatarNamesCallback;
        
        public frmGroupInfo(Group group, GridClient client)
        {
            InitializeComponent();

            while (!IsHandleCreated)
            {
                // Force handle creation
                // warning CS0219: The variable `temp' is assigned but its value is never used
                IntPtr temp = Handle;
            }

            GroupProfileCallback = new GroupManager.GroupProfileCallback(GroupProfileHandler);
            GroupMembersCallback = new GroupManager.GroupMembersCallback(GroupMembersHandler);
            GroupTitlesCallback = new GroupManager.GroupTitlesCallback(GroupTitlesHandler);
            AvatarNamesCallback = new AvatarManager.AvatarNamesCallback(AvatarNamesHandler);

            Group = group;
            Client = client;
            
            // Register the callbacks for this form
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
            Client.Groups.OnGroupProfile -= GroupProfileCallback;
            Client.Groups.OnGroupMembers -= GroupMembersCallback;
            Client.Groups.OnGroupTitles -= GroupTitlesCallback;
            Client.Avatars.OnAvatarNames -= AvatarNamesCallback;
        }

        private void GroupProfileHandler(Group profile)
        {
            Profile = profile;

            if (Group.InsigniaID != UUID.Zero)
                Client.Assets.RequestImage(Group.InsigniaID, ImageType.Normal,
                    delegate(TextureRequestState state, AssetTexture assetTexture)
                        {
                            ManagedImage imgData;
                            Image bitmap;

                            if (state != TextureRequestState.Timeout || state != TextureRequestState.NotFound)
                            {
                                OpenJPEG.DecodeToImage(assetTexture.AssetData, out imgData, out bitmap);
                                picInsignia.Image = bitmap;
                                UpdateInsigniaProgressText("Progress...");
                            }
                            if (state == TextureRequestState.Finished)
                            {
                                UpdateInsigniaProgressText("");
                            }
                        }, true);

            if (this.InvokeRequired)
                this.BeginInvoke(new MethodInvoker(UpdateProfile));
        }

        private void UpdateInsigniaProgressText(string resultText)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new MethodInvoker(delegate()
                {
                    UpdateInsigniaProgressText(resultText);
                }));
            }   
            else
                labelInsigniaProgress.Text = resultText;
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

        private void AvatarNamesHandler(Dictionary<UUID, string> names)
        {
            lock (Names)
            {
                foreach (KeyValuePair<UUID, string> agent in names)
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
                    if (Profile.FounderID != UUID.Zero && Names.ContainsKey(Profile.FounderID))
                    {
                        lblFoundedBy.Text = "Founded by " + Names[Profile.FounderID];
                    }

                    lock (MemberData)
                    {
                        foreach (KeyValuePair<UUID, string> name in Names)
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

        private void GroupMembersHandler(UUID requestID, UUID groupID, Dictionary<UUID, GroupMember> members)
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
                List<UUID> requestids = new List<UUID>();

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

        private void GroupTitlesHandler(UUID requestID, UUID groupID, Dictionary<UUID, GroupTitle> titles)
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
                    foreach (KeyValuePair<UUID, GroupTitle> kvp in Titles)
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
        public UUID ID;
        public string Name;
        public string Title;
        public string LastOnline;
        public ulong Powers;
        public bool IsOwner;
        public int Contribution;
    }
}
