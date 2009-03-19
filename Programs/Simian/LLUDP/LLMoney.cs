using System;
using System.Collections.Generic;
using ExtensionLoader;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace Simian
{
    public class LLMoney : IExtension<ISceneProvider>
    {
        ISceneProvider scene;

        public LLMoney()
        {
        }

        public bool Start(ISceneProvider scene)
        {
            this.scene = scene;

            scene.UDP.RegisterPacketCallback(PacketType.MoneyBalanceRequest, MoneyBalanceRequestHandler);
            scene.UDP.RegisterPacketCallback(PacketType.MoneyTransferRequest, MoneyTransferRequestHandler);
            return true;
        }

        public void Stop()
        {
        }

        void SendBalance(Agent agent, UUID transactionID, string message)
        {
            MoneyBalanceReplyPacket reply = new MoneyBalanceReplyPacket();
            reply.MoneyData.AgentID = agent.ID;
            reply.MoneyData.MoneyBalance = agent.Info.Balance;
            reply.MoneyData.TransactionID = transactionID;
            reply.MoneyData.Description = Utils.StringToBytes(message);

            scene.UDP.SendPacket(agent.ID, reply, PacketCategory.Transaction);
        }

        void MoneyBalanceRequestHandler(Packet packet, Agent agent)
        {
            MoneyBalanceRequestPacket request = (MoneyBalanceRequestPacket)packet;

            SendBalance(agent, request.MoneyData.TransactionID, String.Empty);
        }

        void MoneyTransferRequestHandler(Packet packet, Agent agent)
        {
            MoneyTransferRequestPacket request = (MoneyTransferRequestPacket)packet;

            if (request.MoneyData.Amount < 0 || request.MoneyData.Amount > agent.Info.Balance)
                return;

            // HACK: Only works for sending money to someone who is online
            Agent recipient;
            if (scene.TryGetAgent(request.MoneyData.DestID, out recipient))
            {
                agent.Info.Balance -= request.MoneyData.Amount;
                recipient.Info.Balance += request.MoneyData.Amount;

                SendBalance(agent, UUID.Zero, String.Format("You paid L${0} to {1}.", request.MoneyData.Amount, recipient.FullName));
                SendBalance(agent, UUID.Zero, String.Format("{1} paid you L${0}.", request.MoneyData.Amount, agent.FullName));
            }
        }
    }
}
