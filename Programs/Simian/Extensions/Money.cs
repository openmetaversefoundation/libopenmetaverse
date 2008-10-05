using System;
using System.Collections.Generic;
using ExtensionLoader;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace Simian.Extensions
{
    class Money : IExtension
    {
        Simian Server;

        public Money(Simian server)
        {
            Server = server;
        }

        public void Start()
        {
            Server.UDP.RegisterPacketCallback(PacketType.MoneyBalanceRequest, new PacketCallback(MoneyBalanceRequestHandler));
            Server.UDP.RegisterPacketCallback(PacketType.MoneyTransferRequest, new PacketCallback(MoneyTransferRequestHandler));
        }

        public void Stop()
        {
        }

        void SendBalance(Agent agent, UUID transactionID, string message)
        {
            MoneyBalanceReplyPacket reply = new MoneyBalanceReplyPacket();
            reply.MoneyData.AgentID = agent.AgentID;
            reply.MoneyData.MoneyBalance = agent.Balance;
            reply.MoneyData.TransactionID = transactionID;
            reply.MoneyData.Description = Utils.StringToBytes(message);

            Server.UDP.SendPacket(agent.AgentID, reply, PacketCategory.Transaction);
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

            lock (Server.Agents)
            {
                foreach (Agent recipient in Server.Agents.Values)
                {
                    if (recipient.AgentID == request.MoneyData.DestID)
                    {
                        agent.Balance -= request.MoneyData.Amount;
                        recipient.Balance += request.MoneyData.Amount;

                        SendBalance(agent, UUID.Zero, String.Format("You paid L${0} to {1}.", request.MoneyData.Amount, recipient.Avatar.Name));
                        SendBalance(agent, UUID.Zero, String.Format("{1} paid you L${0}.", request.MoneyData.Amount, agent.Avatar.Name));

                        break;
                    }
                }
            }
        }
        
    }
}
