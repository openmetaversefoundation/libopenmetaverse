using System;
using libsecondlife;

namespace libsecondlife.InventorySystem.PacketHelpers
{
	/// <summary>
	/// Summary description for PacketHelpers.
	/// </summary>
	public class RequestInventoryAsset
	{
		public RequestInventoryAsset()
		{
		}

		/*
		Low 00337 - RequestInventoryAsset - Trusted - Zerocoded
			1266 QueryData (01)
				0219 AgentID (LLUUID / 1)
				0640 QueryID (LLUUID / 1)
				0719 OwnerID (LLUUID / 1)
				0968 ItemID (LLUUID / 1)
		Low 00338 - InventoryAssetResponse - Trusted - Zerocoded
			1266 QueryData (01)
				0640 QueryID (LLUUID / 1)
				0680 AssetID (LLUUID / 1)
				1058 IsReadable (BOOL / 1)
		*/
		public static Packet BuildPacket(ProtocolManager protocol
			, LLUUID agentID, LLUUID queryUD, LLUUID ownerID, LLUUID itemID )
		{
			int packetLength = 8; // header
			packetLength += 16; // AgentID (UUID)
			packetLength += 16; // QueryID (UUID)
			packetLength += 16; // OwnerID (UUID)
			packetLength += 16; // ItemID (UUID)

			Packet packet = new Packet("RequestInventoryAsset", protocol, packetLength );

			int pos = 8; // Leave room for header

			// AgentID
			Array.Copy(agentID.Data, 0, packet.Data, pos, 16);
			pos += 16;

			// QueryID
			Array.Copy(queryUD.Data, 0, packet.Data, pos, 16);
			pos += 16;

			// OwnerID
			Array.Copy(ownerID.Data, 0, packet.Data, pos, 16);
			pos += 16;

			// ItemID
			Array.Copy(itemID.Data, 0, packet.Data, pos, 16);
			pos += 16;

			// Set the packet flags
			packet.Data[0] = Helpers.MSG_ZEROCODED + Helpers.MSG_RELIABLE;

			return packet;
		}
	}
}
