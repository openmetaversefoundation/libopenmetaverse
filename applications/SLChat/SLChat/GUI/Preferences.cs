/*
 * Created by SharpDevelop.
 * User: Oz
 * Date: 8/12/2006
 * Time: 12:31 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;

namespace SLChat
{
	/// <summary>
	/// Description of Preferences.
	/// </summary>
	public partial class frmPrefs
	{
		private frmMain MainForm;
		private PrefsManager prefs;
		//The name of the user whose profiles to load
		//NOTE: format is first_last i.e. "Bob_Smith"
		string user;
		Hashtable settings = new Hashtable();
		
		public frmPrefs(frmMain main, string username, PrefsManager preferences)
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			lbxChoices.SelectedIndex = lbxChoices.FindString("General");
			user = username;
			prefs = preferences;
			MainForm = main;
			LoadDefaults();
			//our private load settings to handle how they turn out
			//LoadSettings("GeneralSettings");
			LoadSettings("ChatSettings");
			LoadSettings("TimestampSettings");
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
		}
		
		private void LoadDefaults()
		{
			//Loads the defaults settings before anything else
			//Chat
			chkYouName.Checked = false;
			chkListUserName.Checked = true;
			//Timestamps
			chkIMTimestamps.Checked = true;
			chkChatTimestamps.Checked = false;
			numIMTimeZ.Text = "-3";
			numChatTimeZ.Text = "-7";
			txtIMStampFormat.Text = "[HH:mm] ";
			txtChatStampFormat.Text = "[HH:mm] ";
			chkSyncStamps.Checked = true;
		}
		
		private void LoadSettings(string parentnode)
		{
			prefs.LoadSettings(user,parentnode);
        	
        	settings = prefs.settings;
        	
        	if(!settings.ContainsKey("Error"))
        	{
        		IDictionaryEnumerator myEnum = settings.GetEnumerator();
      			while (myEnum.MoveNext())
      			{
      				if(parentnode=="ChatSettings")
      				{
      					if(myEnum.Key.ToString()=="UseFullName")
      					{
      						if(myEnum.Value.ToString()=="true"){
      							chkYouName.Checked = prefs.setUseFullName = true;
      						}else{
      							chkYouName.Checked = prefs.setUseFullName = false;
      						}
      					}else if(myEnum.Key.ToString()=="ListUserName"){
      						if(myEnum.Value.ToString()=="true"){
      							chkListUserName.Checked = prefs.setListUserName = true;
      						}else{
      							chkListUserName.Checked = prefs.setListUserName = false;
      						}
      					}
      				}else if(parentnode=="TimestampSettings"){
      					if(myEnum.Key.ToString()=="ShowIMTimestamps")
      					{
      						if(myEnum.Value.ToString()=="true")
      						{
      							chkIMTimestamps.Checked = prefs.setIMTimestamps = true;
      						}else{
      							chkIMTimestamps.Checked = prefs.setIMTimestamps = false;
      						}
      					}else if(myEnum.Key.ToString()=="ShowChatTimestamps"){
      						if(myEnum.Value.ToString()=="true"){
      							chkChatTimestamps.Checked = prefs.setChatTimestamps = true;
      						}else{
      							chkChatTimestamps.Checked = prefs.setChatTimestamps = false;
      						}
      					}else if(myEnum.Key.ToString()=="IMTimeZone"){
      						numIMTimeZ.Text = prefs.setIMTimeZ = myEnum.Value.ToString();
      					}else if(myEnum.Key.ToString()=="ChatTimeZone"){
      						numChatTimeZ.Text = prefs.setChatTimeZ = myEnum.Value.ToString();
      					}else if(myEnum.Key.ToString()=="IMStampFormat"){
      						txtIMStampFormat.Text = prefs.setIMStampFormat = myEnum.Value.ToString();
      					}else if(myEnum.Key.ToString()=="ChatStampFormat"){
      						txtChatStampFormat.Text = prefs.setChatStampFormat = myEnum.Value.ToString();
      					}else if(myEnum.Key.ToString()=="SyncStampSettings"){
      						if(myEnum.Value.ToString()=="true"){
      							chkSyncStamps.Checked = prefs.setSyncTimestamps = true;
      						}else{
      							chkSyncStamps.Checked = prefs.setSyncTimestamps = false;
      						}
      					}
      				}
      			}
        	}else{
        		IDictionaryEnumerator myEnum = settings.GetEnumerator();
      			while (myEnum.MoveNext())
      			{
      				if(myEnum.Key.ToString()=="Error")
      				{
      					//rtbStatus.Text = "Error loading settings: "+myEnum.Value.ToString();
      				}
      			}
        	}
        	settings.Clear();
        	prefs.settings.Clear();
		}
		
		private void SaveSettings()
		{
			if(chkYouName.Checked != prefs.setUseFullName 
			   | chkListUserName.Checked != prefs.setListUserName)
			{
				string strChat = "<UseFullName value=\""+chkYouName.Checked.ToString().ToLower()+"\"/>"+
					"<ListUserName value=\""+chkListUserName.Checked.ToString().ToLower()+"\"/>";
				prefs.SaveSettings(user,"ChatSettings",strChat);
				MainForm.LoadSettings("ChatSettings");
			}
			
			if(chkIMTimestamps.Checked != prefs.setIMTimestamps | chkChatTimestamps.Checked != prefs.setChatTimestamps |
			   numIMTimeZ.Text != prefs.setIMTimeZ | numChatTimeZ.Text != prefs.setChatTimeZ | 
			   txtIMStampFormat.Text != prefs.setIMStampFormat | txtChatStampFormat.Text != prefs.setChatStampFormat |
			   chkSyncStamps.Checked != prefs.setSyncTimestamps)
			{
				string strStamps = "<ShowIMTimestamps value=\""+chkIMTimestamps.Checked.ToString().ToLower()+"\"/>"+
									"<ShowChatTimestamps value=\""+chkChatTimestamps.Checked.ToString().ToLower()+"\"/>"+
									"<IMTimeZone value=\""+numIMTimeZ.Text+"\"/>"+
									"<ChatTimeZone value=\""+numChatTimeZ.Text+"\"/>"+
									"<IMStampFormat value=\""+txtIMStampFormat.Text+"\"/>"+
									"<ChatStampFormat value=\""+txtChatStampFormat.Text+"\"/>"+
									"<SyncStampSettings value=\""+chkSyncStamps.Checked.ToString().ToLower()+"\"/>";
				prefs.SaveSettings(user,"TimestampSettings",strStamps);
				MainForm.LoadSettings("TimestampSettings");
			}
		}
		
		public void LbxChoicesSelectedIndexChanged(object sender, System.EventArgs e)
		{
			gbxGeneral.Visible = false;
			gbxChat.Visible = false;
			gbxIM.Visible = false;
			gbxTimestamps.Visible = false;
			gbxProfiles.Visible = false;
			if(lbxChoices.SelectedItem.ToString() == "General")
			{
				gbxGeneral.Visible = true;
			}else if(lbxChoices.SelectedItem.ToString() == "Chat"){
				gbxChat.Visible = true;
			}else if(lbxChoices.SelectedItem.ToString() == "IM"){
				gbxIM.Visible = true;
			}else if(lbxChoices.SelectedItem.ToString() == "Timestamps"){
				gbxTimestamps.Visible = true;
			}else if(lbxChoices.SelectedItem.ToString() == "Profiles"){
				gbxProfiles.Visible = true;
				prefs.LoadProfiles();
				//Load the profiles to our combobox.
				cbxProfiles.Items.Clear();
				string[] profiles = new string[100];
				profiles = prefs.profiles;
				for(int i=0;i<profiles.Length;i++)
				{
					if(profiles[i]!=null)
					{
						cbxProfiles.Items.Add(profiles[i].ToString());
					}else{
						//Exit when we hit a null profile.
						break;
					}
				}
			}
		}
		
		private void BtnCancelClick(object sender, System.EventArgs e)
		{
			MainForm.prefsCreated = false;
			this.Close();
		}
		
		private void BtnApplyClick(object sender, System.EventArgs e)
		{
			SaveSettings();
		}
		
		private void BtnOkClick(object sender, System.EventArgs e)
		{
			SaveSettings();
			MainForm.prefsCreated = false;
			this.Close();
		}
		
		private void BtnPDeleteClick(object sender, System.EventArgs e)
		{
			prefs.DeleteProfile(cbxProfiles.Text);
			if(prefs.profiles[0]=="success")
			{
				//Successfully deleted profile, remove from combo
				cbxProfiles.Items.Remove(cbxProfiles.Text);
				cbxProfiles.Text = "";
			}else{
				//Failed event.
			}
		}
		
		private void BtnDefaultsClick(object sender, System.EventArgs e)
		{
			LoadDefaults();
		}
	}
}
