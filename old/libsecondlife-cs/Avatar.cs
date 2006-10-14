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
using System.Timers;
using System.Net;
using System.Collections;

namespace libsecondlife
{
    public delegate void ChatCallback(string message, byte audible, byte chattype, byte sourcetype,
    string name, LLUUID sourceid, LLUUID ownerid, LLVector3 position);

    public delegate void InstantMessageCallback(LLUUID FromAgentID, LLUUID ToAgentID,
    uint ParentEstateID, LLUUID RegionID, LLVector3 Position, byte Offline, byte Dialog,
    LLUUID ID, uint Timestamp, string AgentName, string Message, string Bucket);

    public delegate void FriendNotificationCallback(LLUUID AgentID, bool Online);

    public delegate void TeleportCallback(string message);

    public class Avatar
    {
        public LLUUID ID;
        public uint LocalID;
        public string Name;
        public string GroupName;
        public bool Online;
        public LLVector3 Position;
        public LLQuaternion Rotation;
        public Region CurrentRegion;
    }

    public class MainAvatar
    {
        public LLUUID ID;
        public uint LocalID;
        public string FirstName;
        public string LastName;
        public string TeleportMessage;
        public LLVector3 Position;
        public LLQuaternion Rotation;
        // Should we even keep LookAt around? It's just for setting the initial
        // rotation after login AFAIK
        public LLVector3d LookAt;
        public LLVector3d HomePosition;
        public LLVector3d HomeLookAt;

        private SecondLife Client;
        private int TeleportStatus;
        private Timer TeleportTimer;
        private bool TeleportTimeout;

        public event ChatCallback OnChat;
        public event InstantMessageCallback OnInstantMessage;
        public event FriendNotificationCallback OnFriendNotification;
        public event TeleportCallback OnTeleport;

        public MainAvatar(SecondLife client)
        {
            Client = client;
            TeleportMessage = "";

            // Create emtpy vectors for now
            HomeLookAt = HomePosition = LookAt = new LLVector3d();
            Position = new LLVector3();
            Rotation = new LLQuaternion();

            // Coarse location callback
            PacketCallback callback = new PacketCallback(CoarseLocationHandler);
            Client.Network.RegisterCallback("CoarseLocationUpdate", callback);

            // Teleport callbacks
            callback = new PacketCallback(TeleportHandler);
            Client.Network.RegisterCallback("TeleportStart", callback);
            Client.Network.RegisterCallback("TeleportProgress", callback);
            Client.Network.RegisterCallback("TeleportFailed", callback);
            Client.Network.RegisterCallback("TeleportFinish", callback);

            // Instant Message callback
            callback = new PacketCallback(InstantMessageHandler);
            Client.Network.RegisterCallback("ImprovedInstantMessage", callback);

            // Chat callback
            callback = new PacketCallback(ChatHandler);
            Client.Network.RegisterCallback("ChatFromSimulator", callback);

            // Friend notification callback
            callback = new PacketCallback(FriendNotificationHandler);
            Client.Network.RegisterCallback("OnlineNotification", callback);
            Client.Network.RegisterCallback("OfflineNotification", callback);

            TeleportTimer = new Timer(8000);
            TeleportTimer.Elapsed += new ElapsedEventHandler(TeleportTimerEvent);
            TeleportTimeout = false;
        }

        private void FriendNotificationHandler(Packet packet, Simulator simulator)
        {
            // If the agent is online...
            if (packet.Layout.Name == "OnlineNotification")
            {
                LLUUID AgentID = new LLUUID();

                ArrayList blocks;

                blocks = packet.Blocks();

                foreach (Block block in blocks)
                {
                    foreach (Field field in block.Fields)
                    {
                        if (field.Layout.Name == "AgentID")
                        {
                            AgentID = (LLUUID)field.Data;

                            Client.AddAvatar(AgentID);
                            Client.AvatarsMutex.WaitOne();
                            ((Avatar)Client.Avatars[AgentID]).Online = true;
                            Client.AvatarsMutex.ReleaseMutex();

                            if (OnFriendNotification != null)
                            {
                                OnFriendNotification(AgentID, true);
                            }
                        }
                    }
                }

                return;
            }

            // If the agent is Offline...
            if (packet.Layout.Name == "OfflineNotification")
            {
                LLUUID AgentID = new LLUUID();

                ArrayList blocks;

                blocks = packet.Blocks();

                foreach (Block block in blocks)
                {
                    foreach (Field field in block.Fields)
                    {
                        if (field.Layout.Name == "AgentID")
                        {
                            AgentID = (LLUUID)field.Data;

                            Client.AddAvatar(AgentID);
                            Client.AvatarsMutex.WaitOne();
                            ((Avatar)Client.Avatars[AgentID]).Online = false;
                            Client.AvatarsMutex.ReleaseMutex();

                            if (OnFriendNotification != null)
                            {
                                OnFriendNotification(AgentID, false);
                            }
                        }
                    }
                }

                return;
            }
        }

        private void CoarseLocationHandler(Packet packet, Simulator simulator)
        {
            LLVector3d position = new LLVector3d();

            foreach (Block block in packet.Blocks())
            {
                foreach (Field field in block.Fields)
                {
                    if (field.Layout.Name == "X")
                    {
                        position.X = Convert.ToDouble((byte)field.Data);
                    }
                    else if (field.Layout.Name == "Y")
                    {
                        position.Y = Convert.ToDouble((byte)field.Data);
                    }
                    else if (field.Layout.Name == "Z")
                    {
                        position.Z = Convert.ToDouble((byte)field.Data);
                    }
                }
            }

            // Check if the avatar position hasn't been updated
            if (Position.X == 0 && Position.Y == 0 && Position.Z == 0)
            {
                Position.X = (float)position.X;
                Position.Y = (float)position.Y;
                Position.Z = (float)position.Z;

                // Send an AgentUpdate packet with the new camera location
                packet = Packets.Sim.AgentUpdate(Client.Protocol, Client.Network.AgentID, 
                    56.0F, Position, 0, 0);
                Client.Network.SendPacket(packet);

                Hashtable blocks = new Hashtable();
                Hashtable fields = new Hashtable();

                fields["ID"] = Client.Avatar.ID;
                fields["CircuitCode"] = Client.Network.CurrentSim.CircuitCode;
                fields["GenCounter"] = (uint)0;
                blocks[fields] = "Sender";

                fields = new Hashtable();
                fields["VerticalAngle"] = (float)6.28318531F;
                blocks[fields] = "FOVBlock";

                packet = PacketBuilder.BuildPacket("AgentFOV", Client.Protocol, blocks, Helpers.MSG_RELIABLE);
            }
        }

        private void InstantMessageHandler(Packet packet, Simulator simulator)
        {
            if (packet.Layout.Name == "ImprovedInstantMessage")
            {
                LLUUID FromAgentID = new LLUUID();
                LLUUID ToAgentID = new LLUUID();
                uint ParentEstateID = 0;
                LLUUID RegionID = new LLUUID();
                LLVector3 position = new LLVector3();
                byte Offline = 0;
                byte Dialog = 0;
                LLUUID ID = new LLUUID();
                uint Timestamp = 0;
                string AgentName = "";
                string Message = "";
                string Bucket = "";

                ArrayList blocks;

                blocks = packet.Blocks();

                foreach (Block block in blocks)
                {
                    foreach (Field field in block.Fields)
                    {
                        if (field.Layout.Name == "AgentID")
                        {
                            FromAgentID = (LLUUID)field.Data;
                        }
                        else if (field.Layout.Name == "ToAgentID")
                        {
                            ToAgentID = (LLUUID)field.Data;
                        }
                        else if (field.Layout.Name == "ParentEstateID")
                        {
                            ParentEstateID = (uint)field.Data;
                        }
                        else if (field.Layout.Name == "RegionID")
                        {
                            RegionID = (LLUUID)field.Data;
                        }
                        else if (field.Layout.Name == "Position")
                        {
                            position = (LLVector3)field.Data;
                        }
                        else if (field.Layout.Name == "Offline")
                        {
                            Offline = (byte)field.Data;
                        }
                        else if (field.Layout.Name == "Dialog")
                        {
                            Dialog = (byte)field.Data;
                        }
                        else if (field.Layout.Name == "ID")
                        {
                            ID = (LLUUID)field.Data;
                        }
                        else if (field.Layout.Name == "Timestamp")
                        {
                            Timestamp = (uint)field.Data;
                        }
                        else if (field.Layout.Name == "FromAgentName")
                        {
                            AgentName = System.Text.Encoding.UTF8.GetString((byte[])field.Data).Replace("\0", "");
                        }
                        else if (field.Layout.Name == "Message")
                        {
                            Message = System.Text.Encoding.UTF8.GetString((byte[])field.Data).Replace("\0", "");
                        }
                        else if (field.Layout.Name == "BinaryBucket")
                        {
                            Bucket = System.Text.Encoding.UTF8.GetString((byte[])field.Data).Replace("\0", "");
                        }
                    }
                }

                if (OnInstantMessage != null)
                {
                    OnInstantMessage(FromAgentID, ToAgentID, ParentEstateID, RegionID, position,
                            Offline, Dialog, ID, Timestamp, AgentName, Message, Bucket);
                }
            }
        }

		/**
		 As of 9/26/2006
		 ---------------
			// ChatFromSimulator
			// Chat text to appear on a user's screen
			// Position is region local.
			// Viewer can optionally use position to animate
			// If audible is CHAT_NOT_AUDIBLE, message will not be valid
			{
				ChatFromSimulator Low Trusted Unencoded
				{
					ChatData			Single
					{	FromName		Variable 1	}
					{	SourceID		LLUUID		}	// agent id or object id
					{	OwnerID			LLUUID		}	// object's owner
					{	SourceType		U8			}
					{	ChatType		U8			}
					{	Audible			U8			}
					{	Position		LLVector3	}
					{	Message			Variable 2	}	// UTF-8 text
				}
			}
		*/

        private void ChatHandler(Packet packet, Simulator simulator)
        {
            if (packet.Layout.Name == "ChatFromSimulator")
            {
				string name = "";
				LLUUID sourceID = new LLUUID();
				LLUUID ownerID = new LLUUID();
				byte sourcetype = 0;
				byte chattype = 0;
				byte audible = 0;
				LLVector3 position = new LLVector3();
				string message = "";

                ArrayList blocks;

                blocks = packet.Blocks();

                foreach (Block block in blocks)
                {
                    foreach (Field field in block.Fields)
                    {
                        if (field.Layout.Name == "SourceID")
                        {
                            sourceID = (LLUUID)field.Data;
                        }
						else if (field.Layout.Name == "OwnerID")
						{
							ownerID = (LLUUID)field.Data;
						}

                        else if (field.Layout.Name == "FromName")
                        {
                            name = System.Text.Encoding.UTF8.GetString((byte[])field.Data).Replace("\0", "");
                        }
                        else if (field.Layout.Name == "SourceType")
                        {
                            sourcetype = (byte)field.Data;
                        }
                        else if (field.Layout.Name == "ChatType")
                        {
                            chattype = (byte)field.Data;
                        }
                        else if (field.Layout.Name == "Audible")
                        {
                            audible = (byte)field.Data;
                        }
                        else if (field.Layout.Name == "Message")
                        {
                            message = System.Text.Encoding.UTF8.GetString((byte[])field.Data).Replace("\0", "");
                        }
                        else if (field.Layout.Name == "Position")
                        {
                            position = (LLVector3)field.Data;
                        }
                    }
                }

                if (OnChat != null)
                {
                    OnChat(message, audible, chattype, sourcetype, name, sourceID, ownerID, position);
                }
            }
        }

        public void InstantMessage(LLUUID target, string message)
        {
            string name = FirstName + " " + LastName;

            InstantMessage(name, LLUUID.GenerateUUID(), target, message, null);
        }

		public void InstantMessage(LLUUID target, string message, LLUUID converstationID)
		{
			string name = FirstName + " " + LastName;

			InstantMessage(name, converstationID, target, message, null);
		}

        public void InstantMessage(string fromName, LLUUID sessionID, LLUUID target, string message, LLUUID[] conferenceIDs)
        {
            TimeSpan t = (DateTime.UtcNow - new DateTime(1970, 1, 1));
            uint now = (uint)(t.TotalSeconds);

            byte[] binaryBucket;

            if (conferenceIDs != null && conferenceIDs.Length > 0)
            {
                binaryBucket = new byte[16 * conferenceIDs.Length];

                for (int i = 0; i < conferenceIDs.Length; ++i)
                {
                    Array.Copy(conferenceIDs[i].Data, 0, binaryBucket, 16 * i, 16);
                }
            }
            else
            {
                binaryBucket = new byte[0];
            }

			if( Client.CurrentRegion == null )
			{
				throw new Exception("Cannot send instant messages until CurrentRegion is not null.");
			}

            // Build the packet
            Packet packet = Packets.Communication.ImprovedInstantMessage(Client.Protocol, target, Client.Network.AgentID, 0,
                    Client.CurrentRegion.ID, new LLVector3((float)Position.X, (float)Position.Y, (float)Position.Z),
                    0, 0, sessionID, now, fromName, message, binaryBucket, Client.Network.SessionID);

            // Send the message
            Client.Network.SendPacket(packet);
        }

        public void Say(string message, int channel)
        {
            LLUUID CommandID = new LLUUID();
            LLVector3 Position = new LLVector3(0.0F, 0.0F, 0.0F);

            Packet packet = Packets.Communication.ChatFromViewer(Client.Protocol, Client.Avatar.ID, Client.Network.SessionID,
                    message, (byte)1, channel, 0, CommandID, 20, Position);

            Client.Network.SendPacket(packet);
        }

        public void Shout(string message, int channel)
        {
            LLUUID CommandID = new LLUUID();
            LLVector3 Position = new LLVector3(0.0F, 0.0F, 0.0F);

            Packet packet = Packets.Communication.ChatFromViewer(Client.Protocol, Client.Avatar.ID, Client.Network.SessionID,
                    message, (byte)2, channel, 0, CommandID, 100, Position);

            Client.Network.SendPacket(packet);
        }

        public void GiveMoney(LLUUID target, int amount, string description)
        {
            // 5001 - transaction type for av to av money transfers
            GiveMoney(target, amount, description, 5001);
        }

        public void GiveMoney(LLUUID target, int amount, string description, int transactiontype)
        {
            Hashtable blocks = new Hashtable();
            Hashtable fields = new Hashtable();

            fields["AggregatePermInventory"] = (byte)0;
            fields["AggregatePermNextOwner"] = (byte)0;
            fields["DestID"] = target;
            fields["Amount"] = amount;
            fields["Description"] = description;
            fields["Flags"] = (byte)0;
            fields["SourceID"] = Client.Network.AgentID;
            fields["TransactionType"] = transactiontype;
            blocks[fields] = "MoneyData";

            fields = new Hashtable();
            fields["AgentID"] = Client.Network.AgentID;
            fields["SessionID"] = Client.Network.SessionID;
            blocks[fields] = "AgentData";

            Packet packet = PacketBuilder.BuildPacket("MoneyTransferRequest", Client.Protocol, blocks,
                    Helpers.MSG_RELIABLE);

            Client.Network.SendPacket(packet);
        }

        public bool Teleport(U64 regionHandle, LLVector3 position)
        {
            return Teleport(regionHandle, position, new LLVector3(position.X + 1.0F, position.Y, position.Z));
        }

        public bool Teleport(U64 regionHandle, LLVector3 position, LLVector3 lookAt)
        {
            TeleportStatus = 0;
            //                      LLVector3 lookAt = new LLVector3(position.X + 1.0F, position.Y, position.Z);

            Hashtable blocks = new Hashtable();
            Hashtable fields = new Hashtable();
            fields["RegionHandle"] = regionHandle;
            fields["LookAt"] = lookAt;
            fields["Position"] = position;
            blocks[fields] = "Info";
            fields = new Hashtable();
            fields["AgentID"] = Client.Network.AgentID;
            fields["SessionID"] = Client.Network.SessionID;
            blocks[fields] = "AgentData";
            Packet packet = PacketBuilder.BuildPacket("TeleportLocationRequest", Client.Protocol, blocks,
                    Helpers.MSG_RELIABLE + Helpers.MSG_ZEROCODED);

            Client.Log("Teleporting to region " + regionHandle.ToString(), Helpers.LogLevel.Info);

            // Start the timeout check
            TeleportTimeout = false;
            TeleportTimer.Start();

            Client.Network.SendPacket(packet);

            while (TeleportStatus == 0 && !TeleportTimeout)
            {
                Client.Tick();
            }

            TeleportTimer.Stop();

            if (TeleportTimeout)
            {
                if (OnTeleport != null)
                {
                    OnTeleport("Teleport timed out.");
                }
            }
            else
            {
                if (OnTeleport != null)
                {
                    OnTeleport(TeleportMessage);
                }
            }

            return (TeleportStatus == 1);
        }

        public bool Teleport(string simName, LLVector3 position)
        {
            return Teleport(simName, position, new LLVector3(position.X + 1.0F, position.Y, position.Z));
        }

        public bool Teleport(string simName, LLVector3 position, LLVector3 lookAt)
        {
            Client.Grid.AddSim(simName);
            int attempts = 0;

            while (attempts++ < 5)
            {
                if (Client.Grid.Regions.ContainsKey(simName))
                {
                    return Teleport(((GridRegion)Client.Grid.Regions[simName]).RegionHandle, position, lookAt);
                }
                else
                {
                    System.Threading.Thread.Sleep(1000);
                    Client.Grid.AddSim(simName);
                    Client.Tick();
                }
            }
            if (OnTeleport != null)
            {
                OnTeleport("Unable to resolve name: " + simName);
            }
            return false;
        }

        private void TeleportHandler(Packet packet, Simulator simulator)
        {
            ArrayList blocks;

            if (packet.Layout.Name == "TeleportStart")
            {
                TeleportMessage = "Teleport started";
            }
            else if (packet.Layout.Name == "TeleportProgress")
            {
                blocks = packet.Blocks();

                foreach (Block block in blocks)
                {
                    foreach (Field field in block.Fields)
                    {
                        if (field.Layout.Name == "Message")
                        {
                            TeleportMessage = System.Text.Encoding.UTF8.GetString((byte[])field.Data).Replace("\0", "");
                            return;
                        }
                    }
                }
            }
            else if (packet.Layout.Name == "TeleportFailed")
            {
                blocks = packet.Blocks();

                foreach (Block block in blocks)
                {
                    foreach (Field field in block.Fields)
                    {
                        if (field.Layout.Name == "Reason")
                        {
                            TeleportMessage = System.Text.Encoding.UTF8.GetString((byte[])field.Data).Replace("\0", "");
                            TeleportStatus = -1;
                            return;
                        }
                    }
                }
            }
            else if (packet.Layout.Name == "TeleportFinish")
            {
                TeleportMessage = "Teleport finished";

                ushort port = 0;
                IPAddress ip = null;
                U64 regionHandle;

                blocks = packet.Blocks();

                foreach (Block block in blocks)
                {
                    foreach (Field field in block.Fields)
                    {
                        if (field.Layout.Name == "SimPort")
                        {
                            port = (ushort)field.Data;
                        }
                        else if (field.Layout.Name == "SimIP")
                        {
                            ip = (IPAddress)field.Data;
                        }
                        else if (field.Layout.Name == "RegionHandle")
                        {
                            regionHandle = (U64)field.Data;
                        }
                    }
                }

                if (Client.Network.Connect(ip, port, Client.Network.CurrentSim.CircuitCode, true) != null)
                {
                    // Move the avatar in to this sim
                    Packet movePacket = Packets.Sim.CompleteAgentMovement(Client.Protocol, Client.Network.AgentID,
                            Client.Network.SessionID, Client.Network.CurrentSim.CircuitCode);
                    Client.Network.SendPacket(movePacket);

                    Client.Log("Connected to new sim " + Client.Network.CurrentSim.IPEndPoint.ToString(),
                            Helpers.LogLevel.Info);

                    // Sleep a little while so we can collect parcel information
                    System.Threading.Thread.Sleep(1000);

                    Client.CurrentRegion = Client.Network.CurrentSim.Region;
                    TeleportStatus = 1;
                    return;
                }
                else
                {
                    TeleportStatus = -1;
                    return;
                }
            }
        }

        private void TeleportTimerEvent(object source, System.Timers.ElapsedEventArgs ea)
        {
            TeleportTimeout = true;
        }
    }
}
