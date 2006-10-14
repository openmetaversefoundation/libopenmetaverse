/*
 * Copyright (c) 2006, Second Life Reverse Engineering Team
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without 
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the Second Life Reverse Engineering Team nor the names 
 *   of its contributors may be used to endorse or promote products derived from
 *   this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF 
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
 * POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections;

using libsecondlife;
using libsecondlife.Packets;

using libsecondlife.InventorySystem;

namespace libsecondlife.AssetSystem
{
	/// <summary>
	/// Manages the uploading and downloading of Images from SecondLife
	/// </summary>
	public class ImageManager
	{
		private SecondLife slClient;

        private ImagePacketHelpers ImagePacketHelper;

		private Hashtable htDownloadRequests = new Hashtable();

		private class TransferRequest
		{
			public bool Completed;
			public bool Status;
			public string StatusMsg;

			public uint Size;
			public uint Received;
			public uint LastPacket;
			public byte[] AssetData;

            public int BaseDataReceived;

			public TransferRequest()
			{
				Completed = false;

				Status		= false;
				StatusMsg	= "";

				AssetData	= null;
                BaseDataReceived = 0;
			}
		}

        /// <summary>
        /// </summary>
        /// <param name="client"></param>
        public ImageManager(SecondLife client)
		{
			slClient = client;

            ImagePacketHelper = new ImagePacketHelpers(client.Network.AgentID, client.Network.SessionID);

			PacketCallback ImageDataCallback = new PacketCallback(ImageDataCallbackHandler);
			slClient.Network.RegisterCallback(PacketType.ImageData, ImageDataCallback);

            PacketCallback ImagePacketCallback = new PacketCallback(ImagePacketCallbackHandler);
            slClient.Network.RegisterCallback(PacketType.ImagePacket, ImagePacketCallback);

            PacketCallback ImageNotInDatabaseCallback = new PacketCallback(ImageNotInDatabaseCallbackHandler);
            slClient.Network.RegisterCallback(PacketType.ImageNotInDatabase, ImageNotInDatabaseCallback);
        }

        /// <summary>
        /// Requests an image from SecondLife and blocks until it's received.
        /// </summary>
        /// <param name="ImageID">The Image's AssetID</param>
        public byte[] RequestImage(LLUUID ImageID)
		{
			TransferRequest tr = new TransferRequest();
			tr.Completed  = false;
			tr.Size		  = int.MaxValue; // Number of bytes expected
			tr.Received   = 0; // Number of bytes received
			tr.LastPacket = Helpers.GetUnixTime(); // last time we recevied a packet for this request

			htDownloadRequests[ImageID] = tr;

            Packet packet = ImagePacketHelper.RequestImage(ImageID);
			slClient.Network.SendPacket(packet);
            Console.WriteLine(packet);

			while( tr.Completed == false )
			{
				slClient.Tick();
			}

			if( tr.Status == true )
			{
				return tr.AssetData;
			} 
			else 
			{
				throw new Exception( "RequestImage: " + tr.StatusMsg );
			}

		}


        /// <summary>
        /// Handles the Image Data packet, which includes the ID, and Size of the image, 
        /// along with the first block of data for the image.  If the image is small enough
        /// there will be no additional packets.
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="simulator"></param>
        public void ImageDataCallbackHandler(Packet packet, Simulator simulator)
		{
            Console.WriteLine(packet);
            ImageDataPacket reply = (ImageDataPacket)packet;

			LLUUID ImageID = reply.ImageID.ID;
			ushort Packets = reply.ImageID.Packets;
			uint   Size    = reply.ImageID.Size;
			byte[] Data    = reply.ImageData.Data;

            // Lookup the request that this packet is for
			TransferRequest tr = (TransferRequest)htDownloadRequests[ImageID];
			if( tr == null )
			{
                // Received a packet for an image we didn't request...
				return;
			}

            // Initialize the request so that it's data buffer is the right size for the image
			tr.Size = Size;
			tr.AssetData = new byte[tr.Size];
            tr.BaseDataReceived = Data.Length;

            // Copy the first block of image data into the request.
			Array.Copy(Data, 0, tr.AssetData, tr.Received, Data.Length);
			tr.Received += (uint)Data.Length;

			// If we've gotten all the data, mark it completed.
			if( tr.Received >= tr.Size )
			{
				tr.Completed = true;
				tr.Status	 = true;
			}
		}

        /// <summary>
        /// Handles the remaining Image data that did not fit in the initial ImageData packet
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="simulator"></param>
        public void ImagePacketCallbackHandler(Packet packet, Simulator simulator)
		{
            Console.WriteLine(packet);
            ImagePacketPacket reply = (ImagePacketPacket)packet;
            

            // Lookup the request for this packet
			TransferRequest tr = (TransferRequest)htDownloadRequests[reply.ImageID.ID];
			if( tr == null )
			{
                // Received a packet that doesn't belong to any requests in our queue, strange...
				return;
			}


            // TODO: Received data should probably be put into a temporary collection that's indected by ImageID.Packet
            // then once we've received all data packets, it should be re-assembled into a complete array and marked
            // completed.


            // Add this packet's data to the request.
            Array.Copy(reply.ImageData.Data, 0, tr.AssetData, tr.BaseDataReceived + (1000 * (reply.ImageID.Packet - 1)), reply.ImageData.Data.Length);
            tr.Received += (uint)reply.ImageData.Data.Length;

			// If we've gotten all the data, mark it completed.
			if( tr.Received >= tr.Size )
			{
				tr.Completed = true;
				tr.Status	 = true;
			}		
		}

        /// <summary>
        ///
        /// </summary>
        public void ImageNotInDatabaseCallbackHandler(Packet packet, Simulator simulator)
        {
            Console.WriteLine(packet);
        }
	}
}