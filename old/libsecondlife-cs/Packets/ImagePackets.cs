using System;
using System.Collections;

using libsecondlife;
          
namespace libsecondlife.Packets
{
	/// <summary>
	/// Summary description for ImagePackets.
	/// </summary>
	public class ImagePackets
	{
		private ImagePackets() { }


		public static Packet RequestImage(ProtocolManager protocol, LLUUID imageID)
		{
			Hashtable blocks = new Hashtable();
			Hashtable fields = new Hashtable();

			fields["DownloadPriority"]	= (float)1215000.0;
			fields["DiscardLevel"]		= (int)0;
			fields["Packet"]			= (uint)0;
			fields["Image"]				= imageID;

			blocks[fields]		= "RequestImage";

			return PacketBuilder.BuildPacket("RequestImage", protocol, blocks, Helpers.MSG_RELIABLE | Helpers.MSG_ZEROCODED);
		}

	}
}
