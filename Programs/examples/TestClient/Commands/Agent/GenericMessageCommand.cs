using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace OpenMetaverse.TestClient
{
    /// <summary>
    /// Sends a packet of type GenericMessage to the simulator.
    /// </summary>
    public class GenericMessageCommand : Command
    {
        public GenericMessageCommand(TestClient testClient)
        {
            Name = "sendgeneric";
            Description = "send a generic UDP message to the simulator.";
            Category = CommandCategory.Other;        
        }

        public override string Execute(string[] args, UUID fromAgentID)
        {
            UUID target;

            if (args.Length < 1)
                return "Usage: sendgeneric method_name [value1 value2 ...]";

            string methodName = args[0];

            GenericMessagePacket gmp = new GenericMessagePacket();

            gmp.AgentData.AgentID = Client.Self.AgentID;
            gmp.AgentData.SessionID = Client.Self.SessionID;
            gmp.AgentData.TransactionID = UUID.Zero;

            gmp.MethodData.Method = Utils.StringToBytes(methodName);
            gmp.MethodData.Invoice = UUID.Zero;

            gmp.ParamList = new GenericMessagePacket.ParamListBlock[args.Length - 1];

            StringBuilder sb = new StringBuilder();

            for (int i = 1; i < args.Length; i++)
            {
                GenericMessagePacket.ParamListBlock paramBlock = new GenericMessagePacket.ParamListBlock();
                paramBlock.Parameter = Utils.StringToBytes(args[i]);
                gmp.ParamList[i - 1] = paramBlock;
                sb.AppendFormat(" {0}", args[i]);
            }

            Client.Network.SendPacket(gmp);

            return string.Format("Sent generic message with method {0}, params{1}", methodName, sb);
        }
    }
}