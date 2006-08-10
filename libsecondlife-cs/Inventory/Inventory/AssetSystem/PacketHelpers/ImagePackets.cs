using System;
using System.Collections;

using libsecondlife;
          
namespace libsecondlife.AssetSystem.PacketHelpers
{
	/// <summary>
	/// Summary description for ImagePackets.
	/// </summary>
	public class ImagePackets
	{
		private ImagePackets() { }


		/*
			---- RequestImage ----
				-- RequestImage --
					DownloadPriority: 1215000
					DiscardLevel: 0
					Packet: 0
					Image: f252794e1b0fbe2f0f10020a437a9e40
		  
			High 00009 - RequestImage - Untrusted - Zerocoded
				0701 RequestImage (Variable)
					0193 DownloadPriority (F32 / 1)
					0257 DiscardLevel (S32 / 1)
					0785 Packet (U32 / 1)
					1184 Image (LLUUID / 1)
		*/
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
