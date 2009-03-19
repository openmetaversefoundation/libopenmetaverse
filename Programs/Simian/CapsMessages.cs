using System;
using System.Net;
using OpenMetaverse;
using OpenMetaverse.StructuredData;

namespace Simian
{
    public static class CapsMessages
    {
        public static OSDMap TeleportFinish(UUID agentID, int locationID, ulong regionHandle, Uri seedCap, SimAccess simAccess,
            IPAddress simIP, int simPort, TeleportFlags teleportFlags)
        {
            OSDMap info = new OSDMap(8);
            info.Add("AgentID", OSD.FromUUID(agentID));
            info.Add("LocationID", OSD.FromInteger(locationID)); // Unused by the client
            info.Add("RegionHandle", OSD.FromULong(regionHandle));
            info.Add("SeedCapability", OSD.FromUri(seedCap));
            info.Add("SimAccess", OSD.FromInteger((byte)simAccess));
            info.Add("SimIP", OSD.FromBinary(simIP.GetAddressBytes()));
            info.Add("SimPort", OSD.FromInteger(simPort));
            info.Add("TeleportFlags", OSD.FromUInteger((uint)teleportFlags));

            OSDArray infoArray = new OSDArray(1);
            infoArray.Add(info);

            OSDMap teleport = new OSDMap(1);
            teleport.Add("Info", infoArray);

            return teleport;
        }

        public static OSDMap EnableSimulator(ulong regionHandle, IPAddress ip, int port)
        {
            OSDMap llsdSimInfo = new OSDMap(3);

            llsdSimInfo.Add("Handle", OSD.FromULong(regionHandle));
            llsdSimInfo.Add("IP", OSD.FromBinary(ip.GetAddressBytes()));
            llsdSimInfo.Add("Port", OSD.FromInteger(port));

            OSDArray arr = new OSDArray(1);
            arr.Add(llsdSimInfo);

            OSDMap llsdBody = new OSDMap(1);
            llsdBody.Add("SimulatorInfo", arr);

            return llsdBody;
        }

        public static OSDMap EnableClient(UUID agentID, UUID sessionID, UUID secureSessionID, int circuitCode, string firstName, string lastName, Uri callbackUri)
        {
            OSDMap map = new OSDMap(7);
            map["agent_id"] = OSD.FromUUID(agentID);
            map["session_id"] = OSD.FromUUID(sessionID);
            map["secure_session_id"] = OSD.FromUUID(secureSessionID);
            map["circuit_code"] = OSD.FromInteger(circuitCode);
            map["first_name"] = OSD.FromString(firstName);
            map["last_name"] = OSD.FromString(lastName);
            map["callback_uri"] = OSD.FromUri(callbackUri);

            return map;
        }

        public static OSDMap EnableClientComplete(UUID agentID)
        {
            OSDMap map = new OSDMap(1);
            map["agent_id"] = OSD.FromUUID(agentID);
            return map;
        }
    }
}
