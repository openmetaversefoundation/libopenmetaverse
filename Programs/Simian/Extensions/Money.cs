using System;
using System.Collections.Generic;
using ExtensionLoader;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace Simian.Extensions
{
    class Money : IExtension<Simian>
    {
        Simian server;

        public Money()
        {
            
        }

        public void Start(Simian server)
        {
            this.server = server;

            server.UDP.RegisterPacketCallback(PacketType.MoneyBalanceRequest, new PacketCallback(MoneyBalanceRequestHandler));
            server.UDP.RegisterPacketCallback(PacketType.MoneyTransferRequest, new PacketCallback(MoneyTransferRequestHandler));
        }

        public void Stop()
        {
        }

        void SendBalance(Agent agent, UUID transactionID, string message)
        {
            MoneyBalanceReplyPacket reply = new MoneyBalanceReplyPacket();
            reply.MoneyData.AgentID = agent.ID;
            reply.MoneyData.MoneyBalance = agent.Balance;
            reply.MoneyData.TransactionID = transactionID;
            reply.MoneyData.Description = Utils.StringToBytes(message);

            server.UDP.SendPacket(agent.ID, reply, PacketCategory.Transaction);
        }

        void MoneyBalanceRequestHandler(Packet packet, Agent agent)
        {
            MoneyBalanceRequestPacket request = (MoneyBalanceRequestPacket)packet;

            SendBalance(agent, request.MoneyData.TransactionID, String.Empty);
        }

        void MoneyTransferRequestHandler(Packet packet, Agent agent)
        {
            MoneyTransferRequestPacket request = (MoneyTransferRequestPacket)packet;

            if (request.MoneyData.Amount < 0 || request.MoneyData.Amount > agent.Balance)
                return;

            // HACK: Only works for sending money to someone who is online
            Agent recipient;
            if (server.Scene.TryGetAgent(request.MoneyData.DestID, out recipient))
            {
                agent.Balance -= request.MoneyData.Amount;
                recipient.Balance += request.MoneyData.Amount;

                SendBalance(agent, UUID.Zero, String.Format("You paid L${0} to {1}.", request.MoneyData.Amount, recipient.FullName));
                SendBalance(agent, UUID.Zero, String.Format("{1} paid you L${0}.", request.MoneyData.Amount, agent.FullName));
            }
        }
    }
}
