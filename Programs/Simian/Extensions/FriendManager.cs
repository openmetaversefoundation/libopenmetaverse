using System;
using System.Collections.Generic;
using ExtensionLoader;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace Simian.Extensions
{
    public class FriendManager : IExtension
    {
        Simian Server;

        public FriendManager(Simian server)
        {
            Server = server;
        }

        public void Start()
        {
            Server.UDP.RegisterPacketCallback(PacketType.ImprovedInstantMessage, new PacketCallback(ImprovedInstantMessageHandler));
        }

        public void Stop()
        {
        }

        void ImprovedInstantMessageHandler(Packet packet, Agent agent)
        {
            ImprovedInstantMessagePacket im = (ImprovedInstantMessagePacket)packet;
            InstantMessageDialog dialog = (InstantMessageDialog)im.MessageBlock.Dialog;

            if (dialog == InstantMessageDialog.FriendshipOffered || dialog == InstantMessageDialog.FriendshipAccepted || dialog == InstantMessageDialog.FriendshipDeclined)
            {
                lock (Server.Agents)
                {
                    foreach (Agent recipient in Server.Agents.Values)
                    {
                        if (recipient.AgentID == im.MessageBlock.ToAgentID)
                        {
                            ImprovedInstantMessagePacket sendIM = new ImprovedInstantMessagePacket();
                            sendIM.MessageBlock.RegionID = UUID.Random(); //FIXME
                            sendIM.MessageBlock.ParentEstateID = 1;
                            sendIM.MessageBlock.FromGroup = false;
                            sendIM.MessageBlock.FromAgentName = Utils.StringToBytes(agent.Avatar.Name);
                            sendIM.MessageBlock.ToAgentID = im.MessageBlock.ToAgentID;
                            sendIM.MessageBlock.Dialog = im.MessageBlock.Dialog;
                            sendIM.MessageBlock.Offline = (byte)InstantMessageOnline.Online;
                            sendIM.MessageBlock.ID = agent.AgentID;
                            sendIM.MessageBlock.Message = im.MessageBlock.Message;
                            sendIM.MessageBlock.BinaryBucket = new byte[0];
                            sendIM.MessageBlock.Timestamp = 0;
                            sendIM.MessageBlock.Position = agent.Avatar.Position;

                            sendIM.AgentData.AgentID = agent.AgentID;

                            Server.UDP.SendPacket(recipient.AgentID, sendIM, PacketCategory.Transaction);

                            if (dialog == InstantMessageDialog.FriendshipAccepted)
                            {
                                bool receiverOnline = Server.Agents.ContainsKey(agent.AgentID);
                                bool senderOnline = Server.Agents.ContainsKey(recipient.AgentID);

                                if (receiverOnline)
                                {
                                    if (senderOnline)
                                    {
                                        OnlineNotificationPacket notify = new OnlineNotificationPacket();
                                        notify.AgentBlock = new OnlineNotificationPacket.AgentBlockBlock[0];
                                        notify.AgentBlock[0] = new OnlineNotificationPacket.AgentBlockBlock();
                                        notify.AgentBlock[0].AgentID = agent.AgentID;
                                        Server.UDP.SendPacket(recipient.AgentID, notify, PacketCategory.State);
                                    }
                                    else
                                    {
                                        OfflineNotificationPacket notify = new OfflineNotificationPacket();
                                        notify.AgentBlock = new OfflineNotificationPacket.AgentBlockBlock[0];
                                        notify.AgentBlock[0] = new OfflineNotificationPacket.AgentBlockBlock();
                                        notify.AgentBlock[0].AgentID = agent.AgentID;
                                        Server.UDP.SendPacket(recipient.AgentID, notify, PacketCategory.State);
                                    }
                                }

                                if (senderOnline)
                                {
                                    if (receiverOnline)
                                    {
                                        OnlineNotificationPacket notify = new OnlineNotificationPacket();
                                        notify.AgentBlock = new OnlineNotificationPacket.AgentBlockBlock[0];
                                        notify.AgentBlock[0] = new OnlineNotificationPacket.AgentBlockBlock();
                                        notify.AgentBlock[0].AgentID = recipient.AgentID;
                                        Server.UDP.SendPacket(agent.AgentID, notify, PacketCategory.State);
                                    }
                                    else
                                    {
                                        OfflineNotificationPacket notify = new OfflineNotificationPacket();
                                        notify.AgentBlock = new OfflineNotificationPacket.AgentBlockBlock[0];
                                        notify.AgentBlock[0] = new OfflineNotificationPacket.AgentBlockBlock();
                                        notify.AgentBlock[0].AgentID = recipient.AgentID;
                                        Server.UDP.SendPacket(agent.AgentID, notify, PacketCategory.State);
                                    }
                                }

                            }                            

                            break;
                        }
                    }
                }
            }
        }

    }
}
