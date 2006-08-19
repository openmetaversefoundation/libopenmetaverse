/*
 * Created by SharpDevelop.
 * User: Oz
 * Date: 8/12/2006
 * Time: 2:48 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Xml;
using System.Xml.XPath;
using System.IO;
using System.Collections;

namespace SLChat
{
	/// <summary>
	/// Description of PrefsManager.
	/// Loads, saves, checks, etc. preferences and files related.
	/// </summary>
	public class PrefsManager
	{
		public bool setUseFullName = false;
		public bool setListUserName = true;
		public bool setIMTimestamps = true;
		public bool setChatTimestamps = false;
		public string setChatTimeZ = "-7";
		public string setIMTimeZ = "-3"; //For some reason IM timestamps only match PDT if you use -3 instead of -7
		public string setChatStampFormat = "[HH:mm] ";
		public string setIMStampFormat = "[HH:mm] ";
		public bool setSyncTimestamps = true;
		public string[] profiles = new string[100]; //Sending out profiles
		public Hashtable settings = new Hashtable(); //sending out the collected settings
		
		public PrefsManager()
		{
			
		}
		
		public void SaveSettings(string username, string nodeparent, string nodechildren)
		{
			//Saving our login settings
			//username = name of user/profile under which settings are saved
			//nodeparent = name of node grouping to save under, i.e. "LoginSettings"
			//nodechildren = collection of nodes with pre-set values saved under parent.
			//pick whatever filename with .xml extension
			string filename = @"Profiles\\"+username+"\\settings.xml";
			SaveDirectory(@"Profiles\\"+username+"\\");
			
			XmlDocument xmlDoc = new XmlDocument();

			try
			{
				xmlDoc.Load(filename);
			}
			catch(System.IO.FileNotFoundException)
			{
				//if file is not found, create a new xml file
				XmlTextWriter xmlWriter = new XmlTextWriter(filename, System.Text.Encoding.UTF8);
				xmlWriter.Formatting = Formatting.Indented;
				xmlWriter.WriteProcessingInstruction("xml", "version='1.0' encoding='UTF-8'");
				xmlWriter.WriteStartElement("SLChatSettings");
				//If WriteProcessingInstruction is used as above,
				//Do not use WriteEndElement() here
				//xmlWriter.WriteEndElement();
				//it will cause the &ltRoot></Root> to be &ltRoot />
				xmlWriter.Close();
				xmlDoc.Load(filename);
			}
				
			XmlNode root = xmlDoc.DocumentElement;
			XmlNodeList nodeList = xmlDoc.GetElementsByTagName(nodeparent);
			if(nodeList.Count!=0){
				root.RemoveChild(nodeList[0]);
			}
			XmlElement loginSettings = xmlDoc.CreateElement(nodeparent);
			string strSettings = nodechildren;
			loginSettings.InnerXml = strSettings;
			
			root.AppendChild(loginSettings);
				
			xmlDoc.Save(filename);
		}
		
        private void SaveDirectory(string PathName)
        {
        	try
        	{
        		DirectoryInfo TheFolder = new DirectoryInfo(PathName);
                if (TheFolder.Exists)
                {
                    return;
                }

                throw new FileNotFoundException();
        	}
        	catch(FileNotFoundException )
        	{
        		DirectoryInfo TheDir = new DirectoryInfo(PathName);
        		TheDir.Create();
        		return;
        	}
        }
        
        public void DeleteProfile(string username)
        {
        	string PathName = @"Profiles\\"+username;
        	
        	try
            {
                DirectoryInfo TheFolder = new DirectoryInfo(PathName);
                if (TheFolder.Exists)
                {
                	TheFolder.Delete(true);
                	profiles[0] = "success";
                    return;
                }

                throw new FileNotFoundException();
            }
            catch(FileNotFoundException )
            {
                //If the folders are not found, inform user.
                profiles[0] = "Profile not found.";
                return;
            }
            catch(Exception e)
            {
                //An unexpected error occured.
                profiles[0] = "Problem occured: "+e.Message;
                return;
            }
        }
        
        public void LoadProfiles()
        {
        	string PathName = @"Profiles\\";
  			
            try
            {
                DirectoryInfo TheFolder = new DirectoryInfo(PathName);
                if (TheFolder.Exists)
                {
                	int i = 0;
                	DirectoryInfo[] dirs = TheFolder.GetDirectories();
                	foreach(DirectoryInfo di in dirs)
                	{
                		profiles[i] = di.Name;
                		i++;
                	}
                    return;
                }

                throw new FileNotFoundException();
            }
            catch(FileNotFoundException )
            {
                //If the folders are not found, inform user.
                profiles[0] = "No profiles found.";
                return;
            }
            catch(Exception e)
            {
                //An unexpected error occured.
                profiles[0] = "Problem occured: "+e.Message;
                return;
            }
        }
        
        public void LoadSettings(string username, string parentnode)
		{
			//Load our login settings.
			if(username==string.Empty) return;
			
			string filename = @"Profiles\\"+username+"\\settings.xml";
			
			XmlDocument xmlDoc = new XmlDocument();

			try
			{
				xmlDoc.Load(filename);
			}
			catch(System.IO.FileNotFoundException)
			{
				settings.Add("Error","File not found");
				return;
			}
				
			XmlNode root = xmlDoc.DocumentElement;
			XmlNodeList nodeList = xmlDoc.GetElementsByTagName(parentnode);
			if(nodeList.Count>0){
				for(int i = 0;i<nodeList[0].ChildNodes.Count;i++)
				{
					if(nodeList[0].ChildNodes.Item(i).Name!=null & nodeList[0].ChildNodes.Item(i).Attributes!=null)
					{
						settings.Add(nodeList[0].ChildNodes.Item(i).Name,nodeList[0].ChildNodes.Item(i).Attributes["value"].InnerText);
					}else{
						return;
					}
				}
			}
			return;
		}
		
		public void DeleteSettings(string username, string parentnode)
		{
			//Delete our login settings.
			
			string filename = @"Profiles\\"+username+"\\settings.xml";
			
			XmlDocument xmlDoc = new XmlDocument();

			try
			{
				xmlDoc.Load(filename);
			}
			catch(System.IO.FileNotFoundException)
			{
				return;
			}
			XmlNode root = xmlDoc.DocumentElement;
			XmlNodeList nodeList = xmlDoc.GetElementsByTagName(parentnode);
			if(nodeList.Count!=0){
				//Simply remove that NodeList that composes our "LoginSettings"
				root.RemoveChild(nodeList[0]);
			}
			xmlDoc.Save(filename);
			return;
		}
	}
}
