/*
 * Copyright (c) 2006, Second Life Reverse Engineering Team
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without 
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the Second Life Reverse Engineering Team nor the names 
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
using System.Collections;

using libsecondlife;
using libsecondlife.AssetSystem;
using libsecondlife.Packets;


namespace libsecondlife.InventorySystem
{

	/// <summary>
	/// Summary description for Inventory.
	/// </summary>
	public class InventoryManager
	{
		// Reference to the SLClient Library
		private SecondLife slClient;

		// Reference to the Asset Manager
		private static AssetManager slAssetManager;
		internal AssetManager AssetManager
		{
			get{ return slAssetManager; }
		}


		// UUID of Root Inventory Folder
		private LLUUID uuidRootFolder;

		// Setup a hashtable to easily lookup folders by UUID
		private Hashtable htFoldersByUUID        = new Hashtable();
	
		// Setup a Hashtable to track download progress
		private Hashtable htFolderDownloadStatus;
		private ArrayList alFolderRequestQueue;

		private int iLastPacketRecieved;

		private class DescendentRequest
		{
			public LLUUID FolderID;

			public int Expected		= int.MaxValue;
			public int Received		= 0;
			public int LastReceived = 0;

			public bool FetchFolders = true;
			public bool FetchItems   = true;

			public DescendentRequest( LLUUID folderID )
			{
				FolderID = folderID;
				LastReceived = InventoryManager.getUnixtime();
			}

			public DescendentRequest( LLUUID folderID, bool fetchFolders, bool fetchItems )
			{
				FolderID = folderID;
				FetchFolders = fetchFolders;
				FetchItems   = fetchItems;
				LastReceived = InventoryManager.getUnixtime();
			}

		}

		// Each InventorySystem needs to be initialized with a client (for network access to SL)
		// and root folder.  The root folder can be the root folder of an object OR an agent.
		public InventoryManager( SecondLife client, LLUUID rootFolder )
		{
			slClient = client;
			if( slAssetManager == null )
			{
				slAssetManager = new AssetManager( slClient );
			}

			uuidRootFolder = rootFolder;
			
			resetFoldersByUUID();
			
			// Setup the callback
			PacketCallback InventoryDescendentsCallback = new PacketCallback(InventoryDescendentsHandler);
			slClient.Network.RegisterCallback("InventoryDescendents", InventoryDescendentsCallback);
		}


		public AssetManager getAssetManager()
		{
			Console.WriteLine("It is not recommended that you access the asset manager directly");
			return AssetManager;
		}

		private void resetFoldersByUUID()
		{
			// Init folder structure with root
			htFoldersByUUID = new Hashtable();
			
			InventoryFolder ifRootFolder = new InventoryFolder(this, "My Inventory", uuidRootFolder, null);
			htFoldersByUUID[uuidRootFolder] = ifRootFolder;
		}

		public InventoryFolder getRootFolder()
		{
			return (InventoryFolder)htFoldersByUUID[uuidRootFolder];
		}

		public InventoryFolder getFolder( LLUUID folderID )
		{
			return (InventoryFolder)htFoldersByUUID[folderID];
		}

		public InventoryFolder getFolder( String sFolderPath )
		{
			string sSecretConst = "+@#%$#$%^%^%$^$%SV$#%FR$G";
			sFolderPath = sFolderPath.Replace("//",sSecretConst);

			char[] seperators = new char[1];
			seperators[0] = '/';
			string[] sFolderPathParts = sFolderPath.Split(seperators);
			for( int i = 0; i<sFolderPathParts.Length; i++ )
			{
				sFolderPathParts[i] = sFolderPathParts[i].Replace(sSecretConst,"/");
			}

			return getFolder(new Queue(sFolderPathParts));
		}
		private InventoryFolder getFolder( Queue qFolderPath )
		{
			return getFolder( qFolderPath, getRootFolder() );
		}

		private InventoryFolder getFolder( Queue qFolderPath, InventoryFolder ifRoot )
		{
			string sCurFolder = (string)qFolderPath.Dequeue();
			
			foreach( InventoryBase ibFolder in ifRoot.alContents )
			{
				if( ibFolder is libsecondlife.InventorySystem.InventoryFolder )
				{
					if( ((InventoryFolder)ibFolder).Name.Equals( sCurFolder ) )
					{
						if( qFolderPath.Count == 0 )
						{
							return (InventoryFolder)ibFolder;
						} 
						else 
						{
							return getFolder( qFolderPath, (InventoryFolder)ibFolder );
						}
					}
				}
			}
			
			return null;
		}

		private void RequestFolder( DescendentRequest dr )
		{
			Packet packet = InventoryPackets.FetchInventoryDescendents(slClient.Protocol
							, slClient.Network.AgentID
							, dr.FolderID
							, slClient.Network.AgentID
							, dr.FetchFolders
							, dr.FetchItems);

			htFolderDownloadStatus[dr.FolderID] = dr;

			slClient.Network.SendPacket(packet);

		}

		internal InventoryFolder FolderCreate( String name, LLUUID parentid )
		{
			InventoryFolder ifolder = new InventoryFolder( this, name, LLUUID.GenerateUUID(), parentid );
			ifolder._Type = -1;

			if( htFoldersByUUID.Contains(ifolder.ParentID) )
			{
				if( ((InventoryFolder)htFoldersByUUID[ifolder.ParentID]).alContents.Contains(ifolder) == false)
				{
					// Add new folder to the contents of the parent folder.
					((InventoryFolder)htFoldersByUUID[ifolder.ParentID]).alContents.Add( ifolder );
				}
			} else {
				throw new Exception("Parent Folder " + ifolder.ParentID + " does not exist in this Inventory Manager.");
			}

			if( htFoldersByUUID.Contains( ifolder.FolderID ) == false )
			{
				htFoldersByUUID[ifolder.FolderID] = ifolder;
			}

			Packet packet = InventoryPackets.CreateInventoryFolder( slClient.Protocol, slClient.Network.AgentID, ifolder.Name, ifolder.ParentID, ifolder.Type, ifolder.FolderID );
			slClient.Network.SendPacket(packet);

			return ifolder;
		}

		internal void FolderRemove( InventoryFolder ifolder )
		{
			FolderRemove( ifolder.FolderID );
		}
			
		internal void FolderRemove( LLUUID folderID )
		{
			htFoldersByUUID.Remove( folderID );
			Packet packet = InventoryPackets.RemoveInventoryFolder( slClient.Protocol, slClient.Network.AgentID, folderID );
			slClient.Network.SendPacket(packet);
		}

		internal void FolderMove( InventoryFolder iFolder, LLUUID newParentID )
		{
			Packet packet = InventoryPackets.MoveInventoryFolder( slClient.Protocol, slClient.Network.AgentID, newParentID, iFolder.FolderID );
			slClient.Network.SendPacket(packet);
		}

		internal void FolderRename( InventoryFolder ifolder )
		{
			Packet packet = InventoryPackets.UpdateInventoryFolder( slClient.Protocol, slClient.Network.AgentID, ifolder.Name, ifolder.ParentID, ifolder.Type, ifolder.FolderID );
			slClient.Network.SendPacket(packet);
		}

		internal void ItemUpdate( InventoryItem iitem )
		{
			Packet packet = InventorySystem.PacketHelpers.UpdateInventoryItem.BuildPacket( slClient.Protocol, iitem, slClient.Network.AgentID );
			slClient.Network.SendPacket(packet);
		}

		internal void ItemCopy( LLUUID ItemID, LLUUID TargetFolderID )
		{
			Packet packet = InventoryPackets.CopyInventoryItem( slClient.Protocol, slClient.Network.AgentID, ItemID, TargetFolderID );
			slClient.Network.SendPacket(packet);
		}

		internal void ItemGiveTo( InventoryItem iitem, LLUUID ToAgentID )
		{

			LLUUID MessageID = LLUUID.GenerateUUID();

			Packet packet = InventoryPackets.ImprovedInstantMessage( slClient.Protocol
				, MessageID
				, ToAgentID
				, slClient.Network.AgentID
				, slClient.Avatar.FirstName + " " + slClient.Avatar.LastName
				, new LLVector3(slClient.Avatar.Position)
				, iitem
				);

			slClient.Network.SendPacket(packet);

		}

		internal void ItemRemove( InventoryItem iitem )
		{
			InventoryFolder ifolder = getFolder( iitem.FolderID );
			ifolder.alContents.Remove( iitem );

			Packet packet = InventoryPackets.RemoveInventoryItem( slClient.Protocol, slClient.Network.AgentID, iitem.ItemID );
			slClient.Network.SendPacket(packet);
		}

		internal InventoryNotecard NewNotecard( string Name, string Description, string Body, LLUUID FolderID )
		{
			LLUUID ItemID = LLUUID.GenerateUUID();
			InventoryNotecard iNotecard = new InventoryNotecard( this, Name, Description, ItemID, FolderID, slClient.Network.AgentID );

			// Create this notecard on the server.
			ItemUpdate( iNotecard );

			if( (Body != null) && (Body.Equals("") != true) )
			{
				iNotecard.Body = Body;
			}

			return iNotecard;
		}

		internal InventoryImage NewImage( string Name, string Description, byte[] j2cdata, LLUUID FolderID )
		{
			LLUUID ItemID = LLUUID.GenerateUUID();
			InventoryImage iImage = new InventoryImage( this, Name, Description, ItemID, FolderID, slClient.Network.AgentID );

			// Create this notecard on the server.
			ItemUpdate( iImage );

			if( (j2cdata != null) && (j2cdata.Length != 0) )
			{
				iImage.J2CData = j2cdata;
			}

			return iImage;
		}

		public void DownloadInventory()
		{
			resetFoldersByUUID();

			if( htFolderDownloadStatus == null )
			{
				// Create status table
				htFolderDownloadStatus = new Hashtable();
			} else {
				if( htFolderDownloadStatus.Count != 0 )
				{
					throw new Exception("Inventory Download requested while previous download in progress.");
				}
			}

			if( alFolderRequestQueue == null )
			{
				alFolderRequestQueue = new ArrayList();
			}

			// Set last packet received to now, just so out time-out timer works
			iLastPacketRecieved = getUnixtime();

			// Send Packet requesting the root Folder, 
			// this should recurse through all folders
			RequestFolder( new DescendentRequest(uuidRootFolder) );

			while ( (htFolderDownloadStatus.Count > 0) || (alFolderRequestQueue.Count > 0) )
			{
				if( htFolderDownloadStatus.Count == 0 )
				{
					DescendentRequest dr = (DescendentRequest)alFolderRequestQueue[0];
					alFolderRequestQueue.RemoveAt(0);
					RequestFolder( dr );
				}

				if( (getUnixtime() - iLastPacketRecieved) > 10 )
				{
					Console.WriteLine("Time-out while waiting for packets (" + (getUnixtime() - iLastPacketRecieved) + " seconds since last packet)");
					Console.WriteLine("Current Status:");

					// have to make a seperate list otherwise we run into modifying the original array
					// while still enumerating it.
					ArrayList alRestartList = new ArrayList();

					Console.WriteLine( htFolderDownloadStatus[0].GetType() );
					foreach( DescendentRequest dr in htFolderDownloadStatus )
					{
						Console.WriteLine( dr.FolderID + " " + dr.Expected + " / " + dr.Received + " / " + dr.LastReceived );
						
						alRestartList.Add( dr );
					}

					iLastPacketRecieved = getUnixtime();
					foreach( DescendentRequest dr in alRestartList )
					{
						RequestFolder( dr );
					}

				}
				slClient.Tick();

			}		
		}





		/*
		Low 00333 - InventoryDescendents - Untrusted - Unencoded
			1044 ItemData (Variable)
				0047 GroupOwned (BOOL / 1)
				0149 CRC (U32 / 1)
				0159 CreationDate (S32 / 1)
				0345 SaleType (U8 / 1)
				0395 BaseMask (U32 / 1)
				0506 Name (Variable / 1)
				0562 InvType (S8 / 1)
				0630 Type (S8 / 1)
				0680 AssetID (LLUUID / 1)
				0699 GroupID (LLUUID / 1)
				0716 SalePrice (S32 / 1)
				0719 OwnerID (LLUUID / 1)
				0736 CreatorID (LLUUID / 1)
				0968 ItemID (LLUUID / 1)
				1025 FolderID (LLUUID / 1)
				1084 EveryoneMask (U32 / 1)
				1101 Description (Variable / 1)
				1189 Flags (U32 / 1)
				1348 NextOwnerMask (U32 / 1)
				1452 GroupMask (U32 / 1)
				1505 OwnerMask (U32 / 1)
			1297 AgentData (01)
				0219 AgentID (LLUUID / 1)
				0366 Descendents (S32 / 1)
				0418 Version (S32 / 1)
				0719 OwnerID (LLUUID / 1)
				1025 FolderID (LLUUID / 1)
			1298 FolderData (Variable)
				0506 Name (Variable / 1)
				0558 ParentID (LLUUID / 1)
				0630 Type (S8 / 1)
				1025 FolderID (LLUUID / 1)
		*/
		public void InventoryDescendentsHandler(Packet packet, Simulator simulator)
		{
//			Console.WriteLine("Status|Queue :: " + htFolderDownloadStatus.Count + "/" + qFolderRequestQueue.Count);
			iLastPacketRecieved = getUnixtime();

			ArrayList blocks = packet.Blocks();
			
			InventoryItem   invItem;
			InventoryFolder invFolder;

			LLUUID uuidFolderID = new LLUUID();

			int iDescendentsExpected = int.MaxValue;
			int iDescendentsReceivedThisBlock = 0;

			foreach (Block block in blocks)
			{
				if( block.Layout.Name.Equals("ItemData") )
				{
					invItem = new InventoryItem(this);

					foreach (Field field in block.Fields )
					{
						switch( field.Layout.Name )
						{
							case "Name":
								invItem._Name = System.Text.Encoding.UTF8.GetString( (byte[])field.Data).Trim();
								invItem._Name = invItem.Name.Substring(0,invItem.Name.Length-1);
								break;
							case "Description":
								invItem._Description = System.Text.Encoding.UTF8.GetString( (byte[])field.Data).Trim();
								invItem._Description = invItem.Description.Substring(0,invItem.Description.Length-1);
								break;

							case "InvType":
								invItem._InvType = sbyte.Parse(field.Data.ToString());
								break;
							case "Type":
								invItem._Type = sbyte.Parse(field.Data.ToString());
								break;

							case "SaleType":
								invItem._SaleType = byte.Parse(field.Data.ToString());
								break;
						
							case "GroupOwned":
								invItem._GroupOwned = bool.Parse(field.Data.ToString());
								break;
						
							case "FolderID":
								invItem._FolderID = new LLUUID(field.Data.ToString());
								break;
							case "ItemID":
								invItem._ItemID = new LLUUID(field.Data.ToString());
								break;
							case "AssetID":
								invItem._AssetID = new LLUUID(field.Data.ToString());
								break;
							case "GroupID":
								invItem._GroupID = new LLUUID(field.Data.ToString());
								break;
							case "OwnerID":
								invItem._OwnerID = new LLUUID(field.Data.ToString());
								break;
							case "CreatorID":
								invItem._CreatorID = new LLUUID(field.Data.ToString());
								break;

							case "CRC":
								invItem._CRC = uint.Parse(field.Data.ToString());
								break;
							case "Flags":
								invItem._Flags = uint.Parse(field.Data.ToString());
								break;

							case "BaseMask":
								invItem._BaseMask = uint.Parse(field.Data.ToString());
								break;
							case "EveryoneMask":
								invItem._EveryoneMask = uint.Parse(field.Data.ToString());
								break;
							case "NextOwnerMask":
								invItem._NextOwnerMask = uint.Parse(field.Data.ToString());
								break;
							case "GroupMask":
								invItem._GroupMask = uint.Parse(field.Data.ToString());
								break;
							case "OwnerMask":
								invItem._OwnerMask = uint.Parse(field.Data.ToString());
								break;

							case "CreationDate":
								invItem._CreationDate = int.Parse(field.Data.ToString());
								break;
							case "SalePrice":
								invItem._SalePrice = int.Parse(field.Data.ToString());
								break;

							default:
								break;
						}
					}

					// There is always an item block, even if there isn't any items
					// the "filler" block will not have a name
					if( (invItem.Name != null) && !invItem.Name.Equals("") )
					{
						iDescendentsReceivedThisBlock++;

						InventoryFolder ifolder = (InventoryFolder)htFoldersByUUID[invItem.FolderID];
						
						if( ifolder.alContents.Contains( invItem ) == false )
						{
							if( (invItem.InvType == 7) && (invItem.Type == Asset.ASSET_TYPE_NOTECARD) )
							{
								InventoryItem temp = new InventoryNotecard( this, invItem );
								invItem = temp;
							}

							if( (invItem.InvType == 0) && (invItem.Type == Asset.ASSET_TYPE_IMAGE) )
							{
								InventoryItem temp = new InventoryImage( this, invItem );
								invItem = temp;
							}

							ifolder.alContents.Add(invItem);
						}
					}
				}

				// Count number of folder descendents received
				if( block.Layout.Name.Equals("FolderData") )
				{
					String name		= "";
					LLUUID folderid = new LLUUID();
					LLUUID parentid = new LLUUID();
					sbyte  type		= 0;

					foreach (Field field in block.Fields )
					{
						switch( field.Layout.Name )
						{
							case "Name":
								name = System.Text.Encoding.UTF8.GetString( (byte[])field.Data).Trim();
								name = name.Substring(0,name.Length-1);
								break;
							case "FolderID":
								folderid = new LLUUID(field.Data.ToString());
								break;
							case "ParentID":
								parentid = new LLUUID(field.Data.ToString());
								break;
							case "Type":
								type = sbyte.Parse(field.Data.ToString());
								break;
							default:
								break;
						}
					}

					invFolder = new InventoryFolder(this, name, folderid, parentid);

					// There is always an folder block, even if there isn't any folders
					// the "filler" block will not have a name
					if( (invFolder.Name != null) && !invFolder.Name.Equals("") )
					{
						iDescendentsReceivedThisBlock++;

						// Add folder to Parent
						InventoryFolder ifolder = (InventoryFolder)htFoldersByUUID[invFolder.ParentID];
						if( ifolder.alContents.Contains(invFolder) == false )
						{
							ifolder.alContents.Add(invFolder);
						}


						// Add folder to UUID Lookup
						htFoldersByUUID[invFolder.FolderID] = invFolder;
						

						// It's not the root, should be safe to "recurse"
						if( !invFolder.FolderID.Equals( uuidRootFolder ) )
						{
							bool alreadyQueued = false;
							foreach( DescendentRequest dr in alFolderRequestQueue )
							{
								if( dr.FolderID == invFolder.FolderID )
								{
									alreadyQueued = true;
									break;
								}
							}
							
							if( !alreadyQueued )
							{
								alFolderRequestQueue.Add( new DescendentRequest( invFolder.FolderID ) );
							}
						}
					}
				}

				// Check how many descendents we're actually supposed to receive
				if( block.Layout.Name.Equals("AgentData") )
				{
					foreach (Field field in block.Fields )
					{
						if( field.Layout.Name.Equals("Descendents") )
						{
							iDescendentsExpected    = int.Parse(field.Data.ToString());
						}
						if( field.Layout.Name.Equals("FolderID") )
						{
							uuidFolderID = field.Data.ToString();
//							Console.WriteLine("Recieved a packet for : " + uuidFolderID);
						}
					}
				}
			}

			// Update download status for this folder
			if( iDescendentsReceivedThisBlock >= iDescendentsExpected )
			{
				// We received all the descendents we're expecting for this folder
				// in this packet, so go ahead and remove folder from status list.
				htFolderDownloadStatus.Remove(uuidFolderID);
			} 
			else 
			{

				// This one packet didn't have all the descendents we're expecting
				// so update the total we're expecting, and update the total downloaded
				DescendentRequest dr = (DescendentRequest)htFolderDownloadStatus[uuidFolderID];
				dr.Expected  = iDescendentsExpected;
				dr.Received += iDescendentsReceivedThisBlock;
				dr.LastReceived = getUnixtime();

				if( dr.Received >= dr.Expected )
				{
					// Looks like after updating, we have all the descendents, 
					// remove from folder status.
					htFolderDownloadStatus.Remove(uuidFolderID);
				} 
				else 
				{
					htFolderDownloadStatus[uuidFolderID] = dr;
//					Console.WriteLine( uuidFolderID + " is expecting " + (iDescendentsExpected - iStatus[1]) + " more packets." );
				}
			}
		}

		public static int getUnixtime()
		{
			TimeSpan ts = (DateTime.UtcNow - new DateTime(1970,1,1,0,0,0));
			return (int)ts.TotalSeconds;
		}
	}
}
