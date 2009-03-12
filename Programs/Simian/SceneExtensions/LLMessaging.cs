using System;
using System.Collections.Generic;
using ExtensionLoader;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace Simian
{
    public class LLMessaging : IExtension<ISceneProvider>
    {
        ISceneProvider scene;

        public LLMessaging()
        {
        }

        public bool Start(ISceneProvider scene)
        {
            this.scene = scene;

            scene.UDP.RegisterPacketCallback(PacketType.ChatFromViewer, ChatFromViewerHandler);
            scene.UDP.RegisterPacketCallback(PacketType.ImprovedInstantMessage, ImprovedInstantMessageHandler);
            return true;
        }

        public void Stop()
        {
        }

        void ChatFromViewerHandler(Packet packet, Agent agent)
        {
            ChatFromViewerPacket viewerChat = (ChatFromViewerPacket)packet;

            scene.ObjectChat(this, agent.ID, agent.ID, ChatAudibleLevel.Fully, (ChatType)viewerChat.ChatData.Type,
                ChatSourceType.Agent, agent.FullName, agent.Avatar.GetSimulatorPosition(), viewerChat.ChatData.Channel,
                Utils.BytesToString(viewerChat.ChatData.Message));
        }

        void ImprovedInstantMessageHandler(Packet packet, Agent agent)
        {
            ImprovedInstantMessagePacket im = (ImprovedInstantMessagePacket)packet;
            InstantMessageDialog dialog = (InstantMessageDialog)im.MessageBlock.Dialog;

            if (dialog == InstantMessageDialog.MessageFromAgent)
            {
                // HACK: Only works for agents currently online
                Agent recipient;
                if (scene.TryGetAgent(im.MessageBlock.ToAgentID, out recipient))
                {
                    // FIXME: Look into the fields we are setting to default values
                    ImprovedInstantMessagePacket sendIM = new ImprovedInstantMessagePacket();
                    sendIM.MessageBlock.RegionID = scene.RegionID;
                    sendIM.MessageBlock.ParentEstateID = 1;
                    sendIM.MessageBlock.FromGroup = false;
                    sendIM.MessageBlock.FromAgentName = Utils.StringToBytes(agent.FullName);
                    sendIM.MessageBlock.ToAgentID = im.MessageBlock.ToAgentID;
                    sendIM.MessageBlock.Dialog = im.MessageBlock.Dialog;
                    sendIM.MessageBlock.Offline = (byte)InstantMessageOnline.Online;
                    sendIM.MessageBlock.ID = agent.ID;
                    sendIM.MessageBlock.Message = im.MessageBlock.Message;
                    sendIM.MessageBlock.BinaryBucket = Utils.EmptyBytes;
                    sendIM.MessageBlock.Timestamp = Utils.DateTimeToUnixTime(DateTime.Now);
                    sendIM.MessageBlock.Position = agent.Avatar.GetSimulatorPosition();

                    sendIM.AgentData.AgentID = agent.ID;

                    scene.UDP.SendPacket(recipient.ID, sendIM, PacketCategory.Messaging);
                }
            }
            else if (dialog == InstantMessageDialog.FriendshipOffered ||
                dialog == InstantMessageDialog.FriendshipAccepted ||
                dialog == InstantMessageDialog.FriendshipDeclined)
            {
                // HACK: Only works for agents currently online
                Agent recipient;
                if (scene.TryGetAgent(im.MessageBlock.ToAgentID, out recipient))
                {
                    ImprovedInstantMessagePacket sendIM = new ImprovedInstantMessagePacket();
                    sendIM.MessageBlock.RegionID = scene.RegionID;
                    sendIM.MessageBlock.ParentEstateID = 1;
                    sendIM.MessageBlock.FromGroup = false;
                    sendIM.MessageBlock.FromAgentName = Utils.StringToBytes(agent.FullName);
                    sendIM.MessageBlock.ToAgentID = im.MessageBlock.ToAgentID;
                    sendIM.MessageBlock.Dialog = im.MessageBlock.Dialog;
                    sendIM.MessageBlock.Offline = (byte)InstantMessageOnline.Online;
                    sendIM.MessageBlock.ID = agent.ID;
                    sendIM.MessageBlock.Message = im.MessageBlock.Message;
                    sendIM.MessageBlock.BinaryBucket = Utils.EmptyBytes;
                    sendIM.MessageBlock.Timestamp = 0;
                    sendIM.MessageBlock.Position = agent.Avatar.GetSimulatorPosition();

                    sendIM.AgentData.AgentID = agent.ID;

                    scene.UDP.SendPacket(recipient.ID, sendIM, PacketCategory.Transaction);

                    if (dialog == InstantMessageDialog.FriendshipAccepted)
                    {
                        bool receiverOnline = scene.ContainsObject(agent.ID);
                        bool senderOnline = scene.ContainsObject(recipient.ID);

                        if (receiverOnline)
                        {
                            if (senderOnline)
                            {
                                OnlineNotificationPacket notify = new OnlineNotificationPacket();
                                notify.AgentBlock = new OnlineNotificationPacket.AgentBlockBlock[0];
                                notify.AgentBlock[0] = new OnlineNotificationPacket.AgentBlockBlock();
                                notify.AgentBlock[0].AgentID = agent.ID;
                                scene.UDP.SendPacket(recipient.ID, notify, PacketCategory.State);
                            }
                            else
                            {
                                OfflineNotificationPacket notify = new OfflineNotificationPacket();
                                notify.AgentBlock = new OfflineNotificationPacket.AgentBlockBlock[0];
                                notify.AgentBlock[0] = new OfflineNotificationPacket.AgentBlockBlock();
                                notify.AgentBlock[0].AgentID = agent.ID;
                                scene.UDP.SendPacket(recipient.ID, notify, PacketCategory.State);
                            }
                        }

                        if (senderOnline)
                        {
                            if (receiverOnline)
                            {
                                OnlineNotificationPacket notify = new OnlineNotificationPacket();
                                notify.AgentBlock = new OnlineNotificationPacket.AgentBlockBlock[0];
                                notify.AgentBlock[0] = new OnlineNotificationPacket.AgentBlockBlock();
                                notify.AgentBlock[0].AgentID = recipient.ID;
                                scene.UDP.SendPacket(agent.ID, notify, PacketCategory.State);
                            }
                            else
                            {
                                OfflineNotificationPacket notify = new OfflineNotificationPacket();
                                notify.AgentBlock = new OfflineNotificationPacket.AgentBlockBlock[0];
                                notify.AgentBlock[0] = new OfflineNotificationPacket.AgentBlockBlock();
                                notify.AgentBlock[0].AgentID = recipient.ID;
                                scene.UDP.SendPacket(agent.ID, notify, PacketCategory.State);
                            }
                        }
                    }
                }
            }
        }

    }
}
