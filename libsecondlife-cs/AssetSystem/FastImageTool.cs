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


//See FastImageApp for an example! :)
//Please excuse my mess :/
using System;
using System.Collections;
using System.Text;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.AssetSystem.FastImageTool
{
    public delegate void ImageFinishedCallback(LLUUID id, byte[] data); //this delegate is called when an image completed.

    public class Image
    {
        public static void debug(string message)
        {
            //Console.WriteLine("FIT:IMAGE DEBUG -> " + message);
        }

        public LLUUID image_key = new LLUUID();
        public byte[] data = null; //raw image data 
        public bool[] packet_progress = null; //stores true/false depending on if the packet was recieved or not.

        public int datapacket_length = 0;
        private ImageFinishedCallback image_finished_callback;
        public uint assetdata_length = 0;
        public ushort packets_expected = 0;
        public bool is_working = false;
        public bool is_done = false;
        public bool recieved_data_packet = false;
        public int ticks_since_last_reply = 0;

        public Image(LLUUID image, ImageFinishedCallback datahandler)
        {
            if (datahandler != null) image_finished_callback = datahandler;
            else throw new Exception("ImageFinished callback is incorrect!");
            debug("Image class defined for " + image);
            ticks_since_last_reply = Environment.TickCount;
            image_key = image;

        }
        private bool IsAllDone()
        {
            foreach (bool status in packet_progress)
            {
                if (status == false) return false;
            }
            return true;
        }
        public int PacketNeeded()
        {
            if (packet_progress == null) return 0; //we didnt recieve the first packet yet!
            for (int x = 0; x < packet_progress.Length; ++x)
            {
                if (packet_progress[x] == false) return x;
            }
            return -1;
        }
        public void Update(Packet p)
        {
            if (is_done) return;

            is_working = true;
            if (p.Type == PacketType.ImageData)
            {
                ImageDataPacket reply = (ImageDataPacket)p;

                if (reply.ImageID.ID != image_key) return; //this should never be met, but a justincase

                datapacket_length = reply.ImageData.Data.Length;
                assetdata_length = reply.ImageID.Size;
                packets_expected = reply.ImageID.Packets;

                data = new byte[assetdata_length];
                packet_progress = new bool[packets_expected];

                packet_progress[0] = true; //set the first packet to true, since we get it in the initial datapacket
                Array.Copy(reply.ImageData.Data, 0, data, 0, datapacket_length);

                recieved_data_packet = true;
                debug("recieved imagedata packet ( " + image_key + " )");
            }
            else if (p.Type == PacketType.ImagePacket)
            {
                ImagePacketPacket reply = (ImagePacketPacket)p;

                if (recieved_data_packet == false) return; //ignore image packet if it was premature.

                if (reply.ImageID.ID != image_key) return; //one again, should never be met..

                long packet_id = Convert.ToInt64(reply.ImageID.Packet);
                Array.Copy(reply.ImageData.Data, 0, data, datapacket_length + (1000 * (packet_id - 1)), reply.ImageData.Data.Length);
                if (packet_id < 0 || packet_id > packets_expected) throw new Exception("Uhm, something went wrong - packet was out of bounds (" + image_key + " - " + packet_id + ")");
                packet_progress[packet_id] = true;
                //debug("recieved imagepacket packet ( " + image_key + " #" + reply.ImageID.Packet + ")");
            }
            else
            {
                is_working = false;
                throw new Exception("Invalid packet passed through Image class");
            }

            if (IsAllDone())
            {
                is_done = true;
                image_finished_callback(image_key, data);
            }

            ticks_since_last_reply = Environment.TickCount;
            is_working = false;
        }
    }

    public class ImageManager
    {
        public static void debug(string message)
        {
            //Console.WriteLine("FIT:IMAGEMANAGER DEBUG -> " + message);
        }

        public SecondLife client;
        private ImageFinishedCallback image_finished_callback;
        public Hashtable images = new Hashtable();

        public ImageManager(SecondLife slclient, ImageFinishedCallback datahandler)
        {
            if (datahandler != null) image_finished_callback = datahandler;
            else throw new Exception("ImageFinished callback is incorrect!");
            client = slclient;

            client.Network.RegisterCallback(PacketType.ImageNotInDatabase, new PacketCallback(ImagePacketHandler));
            client.Network.RegisterCallback(PacketType.ImageData, new PacketCallback(ImagePacketHandler));
            client.Network.RegisterCallback(PacketType.ImagePacket, new PacketCallback(ImagePacketHandler));
        }

        public bool Exists(LLUUID image)
        {
            if (images[image] == null) return false;
            else return true;
        }
        public void Add(LLUUID image)
        {
            if (!Exists(image)) images.Add(image, new Image(image, new ImageFinishedCallback(ImageProcessor))); //lots of images! :D
        }
        public void Remove(LLUUID image)
        {
            if (Exists(image)) images.Remove(image);
        }

        private void ImageProcessor(LLUUID id, byte[] thedata)
        {
            //this function processes the data from a completed data
            //through the defined delagate when this class was created
            //and then removes it for memory preservation! :)

            image_finished_callback(id, thedata);
            if (images[id] != null) this.Remove(id);
            debug("Removed " + id + " from the imagemanager");
        }

        private void ImagePacketHandler(Packet bundle, Simulator region)
        {
            if (bundle.Type == PacketType.ImageData)
            {
                LLUUID id = ((ImageDataPacket)bundle).ImageID.ID;
                if (images[id] != null) ((Image)(images[id])).Update(bundle);
                else return;
            }
            else if (bundle.Type == PacketType.ImagePacket)
            {
                LLUUID id = ((ImagePacketPacket)bundle).ImageID.ID;
                if (images[id] != null) ((Image)(images[id])).Update(bundle);
                else return;
            }
            else if (bundle.Type == PacketType.ImageNotInDatabase)
            {
                ImageNotInDatabasePacket reply = (ImageNotInDatabasePacket)bundle;
                if (images[reply.ImageID] != null) images.Remove(reply.ImageID);
                else return;
            }
        }

        public void Update()
        {
            Hashtable ims = new Hashtable();
            try
            {
                foreach (DictionaryEntry ent in images)
                {
                    Image tmp = (Image)ent.Value;
                    int pkt_needed = tmp.PacketNeeded();
                    if (!tmp.is_done && !tmp.is_working && (tmp.ticks_since_last_reply < (Environment.TickCount - 500)) && pkt_needed != -1)
                    {
                        ims.Add((LLUUID)ent.Key, tmp.PacketNeeded());
                    }
                }
            }
            catch
            {
                return;
            }
            if (ims.Count == 0) return; //no images to update at this time, go ahead and exit.
            RequestImagePacket rip = new RequestImagePacket();


            //go ahead and do the silly security stuff in this packet
            rip.AgentData.AgentID = client.Network.AgentID;
            rip.AgentData.SessionID = client.Network.SessionID;

            int argh = ims.Count;
            if (argh > 15) argh = 15;
            rip.RequestImage = new RequestImagePacket.RequestImageBlock[argh];

            int a = 0; //our counter in the next foreach loop.
            try
            {
                foreach (DictionaryEntry ent in ims)
                {
                    if (a < 15)
                    {
                        rip.RequestImage[a] = new RequestImagePacket.RequestImageBlock();
                        rip.RequestImage[a].DiscardLevel = 0;
                        rip.RequestImage[a].DownloadPriority = 1210000; //not sure what to set this as..
                        rip.RequestImage[a].Image = new LLUUID(ent.Key.ToString());
                        rip.RequestImage[a].Packet = Convert.ToUInt32(ent.Value);
                        rip.RequestImage[a].Type = 0; //again, not sure what it is.
                        ++a; //increment a
                    }

                }
            }
            catch
            {
                //shh...
                return;
            }

            client.Network.SendPacket((Packet)rip, client.Network.CurrentSim);
            debug("Sent packet! " + argh + " image requests sent this time..");
        }

        public bool AllImagesDone()
        {
            foreach (Image im in images.Values)
            {
                if (im.is_done == false) return false;
            }
            return true;
        }


    }
}
