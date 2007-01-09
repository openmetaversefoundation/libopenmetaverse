using System;
using System.Collections.Generic;
using System.Threading;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.Utilities
{
    public static class Realism
    {
        public static LLUUID TypingAnimation = new LLUUID("c541c47f-e0c0-058b-ad1a-d6ae3a4584d9");

        /// <summary>
        ///  A psuedo-realistic chat function that uses the typing sound and
        /// animation, types at three characters per second, and randomly 
        /// pauses. This function will block until the message has been sent
        /// </summary>
        /// <param name="client">A reference to the client that will chat</param>
        /// <param name="message">The chat message to send</param>
        public static void Chat(SecondLife client, string message)
        {
            Chat(client, message, MainAvatar.ChatType.Normal, 3);
        }

        /// <summary>
        /// A psuedo-realistic chat function that uses the typing sound and
        /// animation, types at a given rate, and randomly pauses. This 
        /// function will block until the message has been sent
        /// </summary>
        /// <param name="client">A reference to the client that will chat</param>
        /// <param name="message">The chat message to send</param>
        /// <param name="type">The chat type (usually Normal, Whisper or Shout)</param>
        /// <param name="cps">Characters per second rate for chatting</param>
        public static void Chat(SecondLife client, string message, MainAvatar.ChatType type, int cps)
        {
            Random rand = new Random();
            int characters = 0;
            bool typing = true;

            // Start typing
            client.Self.Chat("", 0, MainAvatar.ChatType.StartTyping);
            client.Self.AnimationStart(TypingAnimation);

            while (characters < message.Length)
            {
                if (!typing)
                {
                    // Start typing again
                    client.Self.Chat("", 0, MainAvatar.ChatType.StartTyping);
                    client.Self.AnimationStart(TypingAnimation);
                    typing = true;
                }
                else
                {
                    // Randomly pause typing
                    if (rand.Next(10) >= 9)
                    {
                        client.Self.Chat("", 0, MainAvatar.ChatType.StopTyping);
                        client.Self.AnimationStop(TypingAnimation);
                        typing = false;
                    }
                }

                // Sleep for a second and increase the amount of characters we've typed
                System.Threading.Thread.Sleep(1000);
                characters += cps;
            }

            // Send the message
            client.Self.Chat(message, 0, type);

            // Stop typing
            client.Self.Chat("", 0, MainAvatar.ChatType.StopTyping);
            client.Self.AnimationStop(TypingAnimation);
        }
    }

    /// <summary>
    /// Keeps an up to date inventory of the currently seen objects in each
    /// simulator
    /// </summary>
    //public class ObjectTracker
    //{
    //    private SecondLife Client;
    //    private Dictionary<ulong, Dictionary<uint, PrimObject>> SimPrims = new Dictionary<ulong, Dictionary<uint, PrimObject>>();

    //    /// <summary>
    //    /// Default constructor
    //    /// </summary>
    //    /// <param name="client">A reference to the SecondLife client to track
    //    /// objects for</param>
    //    public ObjectTracker(SecondLife client)
    //    {
    //        Client = client;
    //    }
    //}

    /// <summary>
    /// Maintains a cache of avatars and does blocking lookups for avatar data
    /// </summary>
    public class AvatarTracker
    {
        protected SecondLife Client;
        protected Dictionary<LLUUID, Avatar> avatars = new Dictionary<LLUUID,Avatar>();
        protected Dictionary<LLUUID, ManualResetEvent> NameLookupEvents = new Dictionary<LLUUID, ManualResetEvent>();
        protected Dictionary<LLUUID, ManualResetEvent> StatisticsLookupEvents = new Dictionary<LLUUID, ManualResetEvent>();
        protected Dictionary<LLUUID, ManualResetEvent> PropertiesLookupEvents = new Dictionary<LLUUID, ManualResetEvent>();
        protected Dictionary<LLUUID, ManualResetEvent> InterestsLookupEvents = new Dictionary<LLUUID, ManualResetEvent>();
        protected Dictionary<LLUUID, ManualResetEvent> GroupsLookupEvents = new Dictionary<LLUUID, ManualResetEvent>();

        public AvatarTracker(SecondLife client)
        {
            Client = client;

            Client.Avatars.OnAvatarNames += new AvatarManager.AvatarNamesCallback(Avatars_OnAvatarNames);
            Client.Avatars.OnAvatarInterests += new AvatarManager.AvatarInterestsCallback(Avatars_OnAvatarInterests);
            Client.Avatars.OnAvatarProperties += new AvatarManager.AvatarPropertiesCallback(Avatars_OnAvatarProperties);
            Client.Avatars.OnAvatarStatistics += new AvatarManager.AvatarStatisticsCallback(Avatars_OnAvatarStatistics);
            Client.Avatars.OnAvatarGroups += new AvatarManager.AvatarGroupsCallback(Avatars_OnAvatarGroups);

            Client.Objects.OnNewAvatar += new ObjectManager.NewAvatarCallback(Objects_OnNewAvatar);
            Client.Objects.OnAvatarMoved += new ObjectManager.AvatarMovedCallback(Objects_OnAvatarMoved);
        }

        /// <summary>
        /// Check if a particular avatar is in the local cache
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Contains(LLUUID id)
        {
            return avatars.ContainsKey(id);
        }

        public Dictionary<LLUUID, Avatar> SimLocalAvatars()
        {
            Dictionary<LLUUID, Avatar> local = new Dictionary<LLUUID, Avatar>();

            lock (avatars)
            {
                foreach (Avatar avatar in avatars.Values)
                {
                    if (avatar.CurrentRegion == Client.Network.CurrentSim.Region)
                        local[avatar.ID] = avatar;
                }
            }

            return local;
        }

        /// <summary>
        /// Get an avatar's name, either from the cache or request it.
        /// This function is blocking
        /// </summary>
        /// <param name="id">Avatar key to look up</param>
        /// <returns>The avatar name, or String.Empty if the lookup failed</returns>
        public string GetAvatarName(LLUUID id)
        {
            // Short circuit the cache lookup in GetAvatarNames
            if (Contains(id))
                return LocalAvatarNameLookup(id);

            // Add to the dictionary
            lock (NameLookupEvents)
                NameLookupEvents.Add(id, new ManualResetEvent(false));

            // Call function
            Client.Avatars.RequestAvatarName(id);

            // Start blocking while we wait for this name to be fetched
            NameLookupEvents[id].WaitOne(5000, false);

            // Clean up
            lock (NameLookupEvents)
                NameLookupEvents.Remove(id);

            // Return
            return LocalAvatarNameLookup(id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        //public void BeginGetAvatarName(LLUUID id)
        //{
        //    // TODO: BeginGetAvatarNames is pretty bulky, rewrite a simple version here

        //    List<LLUUID> ids = new List<LLUUID>();
        //    ids.Add(id);
        //    BeginGetAvatarNames(ids);
        //}

        //public void BeginGetAvatarNames(List<LLUUID> ids)
        //{
        //    Dictionary<LLUUID, string> havenames = new Dictionary<LLUUID, string>();
        //    List<LLUUID> neednames = new List<LLUUID>();

        //    // Fire callbacks for the ones we already have cached
        //    foreach (LLUUID id in ids)
        //    {
        //        if (Avatars.ContainsKey(id))
        //        {
        //            havenames[id] = Avatars[id].Name;
        //            //Short circuit the lookup process
        //            if (ManualResetEvents.ContainsKey(id))
        //            {
        //                ManualResetEvents[id].Set();
        //                return;
        //            }
        //        }
        //        else
        //        {
        //            neednames.Add(id);
        //        }
        //    }

        //    if (havenames.Count > 0 && OnAgentNames != null)
        //    {
        //        OnAgentNames(havenames);
        //    }

        //    if (neednames.Count > 0)
        //    {
        //        UUIDNameRequestPacket request = new UUIDNameRequestPacket();

        //        request.UUIDNameBlock = new UUIDNameRequestPacket.UUIDNameBlockBlock[neednames.Count];

        //        for (int i = 0; i < neednames.Count; i++)
        //        {
        //            request.UUIDNameBlock[i] = new UUIDNameRequestPacket.UUIDNameBlockBlock();
        //            request.UUIDNameBlock[i].ID = neednames[i];
        //        }

        //        Client.Network.SendPacket(request);
        //    }
        //}

        public bool GetAvatarProfile(LLUUID id, out Avatar.Interests interests, out Avatar.Properties properties, 
            out Avatar.Statistics statistics, out List<LLUUID> groups)
        {
            // Do a local lookup first
            if (avatars.ContainsKey(id) && avatars[id].ProfileProperties.BornOn != null && 
                avatars[id].ProfileProperties.BornOn != String.Empty)
            {
                interests = avatars[id].ProfileInterests;
                properties = avatars[id].ProfileProperties;
                statistics = avatars[id].ProfileStatistics;
                groups = avatars[id].Groups;

                return true;
            }

            // Create the ManualResetEvents
            lock (PropertiesLookupEvents)
                if (!PropertiesLookupEvents.ContainsKey(id))
                    PropertiesLookupEvents[id] = new ManualResetEvent(false);
            lock (InterestsLookupEvents)
                if (!InterestsLookupEvents.ContainsKey(id))
                    InterestsLookupEvents[id] = new ManualResetEvent(false);
            lock (StatisticsLookupEvents)
                if (!StatisticsLookupEvents.ContainsKey(id))
                    StatisticsLookupEvents[id] = new ManualResetEvent(false);
            lock (GroupsLookupEvents)
                if (!GroupsLookupEvents.ContainsKey(id))
                    GroupsLookupEvents[id] = new ManualResetEvent(false);

            // Request the avatar profile
            Client.Avatars.RequestAvatarProperties(id);

            // Wait for all of the events to complete
            PropertiesLookupEvents[id].WaitOne(5000, false);
            InterestsLookupEvents[id].WaitOne(5000, false);
            StatisticsLookupEvents[id].WaitOne(5000, false);
            GroupsLookupEvents[id].WaitOne(5000, false);

            // Destroy the ManualResetEvents
            lock (PropertiesLookupEvents)
                PropertiesLookupEvents.Remove(id);
            lock (InterestsLookupEvents)
                InterestsLookupEvents.Remove(id);
            lock (StatisticsLookupEvents)
                StatisticsLookupEvents.Remove(id);
            lock (GroupsLookupEvents)
                GroupsLookupEvents.Remove(id);

            // If we got a filled in profile return everything
            if (avatars.ContainsKey(id) && avatars[id].ProfileProperties.BornOn != null && 
                avatars[id].ProfileProperties.BornOn != String.Empty)
            {
                interests = avatars[id].ProfileInterests;
                properties = avatars[id].ProfileProperties;
                statistics = avatars[id].ProfileStatistics;
                groups = avatars[id].Groups;

                return true;
            }
            else
            {
                interests = new Avatar.Interests();
                properties = new Avatar.Properties();
                statistics = new Avatar.Statistics();
                groups = null;

                return false;
            }
        }

        /// <summary>
        /// This function will only check if the avatar name exists locally,
        /// it will not do any networking calls to fetch the name
        /// </summary>
        /// <returns>The avatar name, or an empty string if it's not found</returns>
        protected string LocalAvatarNameLookup(LLUUID id)
        {
            lock (avatars)
            {
                if (avatars.ContainsKey(id))
                    return avatars[id].Name;
                else
                    return String.Empty;
            }
        }

        void Objects_OnAvatarMoved(Simulator simulator, AvatarUpdate avatar, ulong regionHandle, ushort timeDilation)
        {
            // TODO:
        }

        void Objects_OnNewAvatar(Simulator simulator, Avatar avatar, ulong regionHandle, ushort timeDilation)
        {
            lock (avatars)
            {
                avatars[avatar.ID] = avatar;
            }
        }

        private void Avatars_OnAvatarNames(Dictionary<LLUUID, string> names)
        {
            lock (avatars)
            {
                foreach (KeyValuePair<LLUUID, string> kvp in names)
                {
                    if (!avatars.ContainsKey(kvp.Key) || avatars[kvp.Key] == null)
                        avatars[kvp.Key] = new Avatar();

                    avatars[kvp.Key].Name = kvp.Value;

                    if (NameLookupEvents.ContainsKey(kvp.Key))
                        NameLookupEvents[kvp.Key].Set();
                }
            }
        }

        void Avatars_OnAvatarStatistics(LLUUID avatarID, Avatar.Statistics statistics)
        {
            lock (avatars)
            {
                if (!avatars.ContainsKey(avatarID))
                    avatars[avatarID] = new Avatar();

                avatars[avatarID].ProfileStatistics = statistics;
            }

            if (StatisticsLookupEvents.ContainsKey(avatarID))
                StatisticsLookupEvents[avatarID].Set();
        }

        void Avatars_OnAvatarProperties(LLUUID avatarID, Avatar.Properties properties)
        {
            lock (avatars)
            {
                if (!avatars.ContainsKey(avatarID))
                    avatars[avatarID] = new Avatar();

                avatars[avatarID].ProfileProperties = properties;
            }

            if (PropertiesLookupEvents.ContainsKey(avatarID))
                PropertiesLookupEvents[avatarID].Set();
        }

        void Avatars_OnAvatarInterests(LLUUID avatarID, Avatar.Interests interests)
        {
            lock (avatars)
            {
                if (!avatars.ContainsKey(avatarID))
                    avatars[avatarID] = new Avatar();

                avatars[avatarID].ProfileInterests = interests;
            }

            if (InterestsLookupEvents.ContainsKey(avatarID))
                InterestsLookupEvents[avatarID].Set();
        }

        void Avatars_OnAvatarGroups(LLUUID avatarID, AvatarGroupsReplyPacket.GroupDataBlock[] groups)
        {
            List<LLUUID> groupList = new List<LLUUID>();

            foreach (AvatarGroupsReplyPacket.GroupDataBlock block in groups)
            {
                // TODO: We just toss away all the other information here, seems like a waste...
                groupList.Add(block.GroupID);
            }

            lock (avatars)
            {
                if (!avatars.ContainsKey(avatarID))
                    avatars[avatarID] = new Avatar();

                avatars[avatarID].Groups = groupList;
            }

            if (GroupsLookupEvents.ContainsKey(avatarID))
                GroupsLookupEvents[avatarID].Set();
        }
    }

    public class AssetTransfer
    {
        public LLUUID ID = LLUUID.Zero;
        public ushort PacketCount = 0;
        public uint Size = 0;
        public byte[] AssetData = new byte[0];
        public int Transferred = 0;
        public bool Success = false;
    }

    public class ImageTransfer : AssetTransfer
    {
        public int Codec = 0;
        public bool NotFound = false;

        internal ManualResetEvent HeaderReceivedEvent = new ManualResetEvent(false);
        internal int InitialDataSize = 0;
    }

    public class AssetManager
    {
        private SecondLife Client;

        public AssetManager(SecondLife client)
        {
            Client = client;

            //Client.Network.RegisterCallback(PacketType.AssetUploadComplete, new NetworkManager.PacketCallback(AssetUploadCompleteHandler));
            //// Transfer Packets for downloading large assets
            //Client.Network.RegisterCallback(PacketType.TransferInfo, new NetworkManager.PacketCallback(TransferInfoHandler));
            //Client.Network.RegisterCallback(PacketType.TransferPacket, new NetworkManager.PacketCallback(TransferPacketHandler));
            //// Xfer packets for uploading large assets
            //Client.Network.RegisterCallback(PacketType.ConfirmXferPacket, new NetworkManager.PacketCallback(ConfirmXferPacketHandler));
            //Client.Network.RegisterCallback(PacketType.RequestXfer, new NetworkManager.PacketCallback(RequestXferHandler));
        }
    }

    public class ImageManager
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        public delegate void ImageReceivedCallback(ImageTransfer image);

        public event ImageReceivedCallback OnImageReceived;

        private SecondLife Client;
        private Dictionary<LLUUID, ImageTransfer> Transfers = new Dictionary<LLUUID, ImageTransfer>();

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="client">A reference to the SecondLife client to use</param>
        public ImageManager(SecondLife client)
        {
            Client = client;

            Client.Network.RegisterCallback(PacketType.ImageData, new NetworkManager.PacketCallback(ImageDataHandler));
            Client.Network.RegisterCallback(PacketType.ImagePacket, new NetworkManager.PacketCallback(ImagePacketHandler));
            Client.Network.RegisterCallback(PacketType.ImageNotInDatabase, new NetworkManager.PacketCallback(ImageNotInDatabaseHandler));
        }

        /// <summary>
        /// Initiate an image download. This is an asynchronous function
        /// </summary>
        /// <param name="imageID">The image to download</param>
        public void RequestImage(LLUUID imageID, float priority)
        {
            if (!Transfers.ContainsKey(imageID))
            {
                ImageTransfer transfer = new ImageTransfer();
                transfer.ID = imageID;

                // Add this transfer to the dictionary
                lock (Transfers) Transfers[transfer.ID] = transfer;

                // Build and send the request packet
                RequestImagePacket request = new RequestImagePacket();
                request.AgentData.AgentID = Client.Network.AgentID;
                request.AgentData.SessionID = Client.Network.SessionID;
                request.RequestImage = new RequestImagePacket.RequestImageBlock[1];
                request.RequestImage[0] = new RequestImagePacket.RequestImageBlock();
                request.RequestImage[0].DiscardLevel = 0;
                request.RequestImage[0].DownloadPriority = priority;
                request.RequestImage[0].Packet = 0;
                request.RequestImage[0].Image = imageID;
                request.RequestImage[0].Type = 0; // TODO: What is this?

                Client.Network.SendPacket(request);
            }
            else
            {
                Client.Log("RequestImage() called for an image we are already downloading, ignoring",
                    Helpers.LogLevel.Info);
            }
        }

        /// <summary>
        /// Handles the Image Data packet which includes the ID and Size of the image,
        /// along with the first block of data for the image. If the image is small enough
        /// there will be no additional packets
        /// </summary>
        public void ImageDataHandler(Packet packet, Simulator simulator)
        {
            ImageDataPacket data = (ImageDataPacket)packet;

            if (Transfers.ContainsKey(data.ImageID.ID))
            {
                ImageTransfer transfer = Transfers[data.ImageID.ID];

                transfer.Codec = data.ImageID.Codec;
                transfer.PacketCount = data.ImageID.Packets;
                transfer.Size = data.ImageID.Size;
                transfer.AssetData = new byte[transfer.Size];
                Array.Copy(data.ImageData.Data, transfer.AssetData, data.ImageData.Data.Length);
                transfer.InitialDataSize = data.ImageData.Data.Length;
                transfer.Transferred += data.ImageData.Data.Length;

                // Check if we downloaded the full image
                if (transfer.Transferred >= transfer.Size)
                {
                    lock (Transfers) Transfers.Remove(transfer.ID);
                    transfer.Success = true;

                    if (OnImageReceived != null)
                    {
                        OnImageReceived(transfer);
                    }
                }
            }
            else
            {
                Client.Log("Received an ImageData packet for an image we didn't request, ID: " + data.ImageID.ID,
                    Helpers.LogLevel.Warning);
            }
        }

        /// <summary>
        /// Handles the remaining Image data that did not fit in the initial ImageData packet
        /// </summary>
        public void ImagePacketHandler(Packet packet, Simulator simulator)
        {
            ImagePacketPacket image = (ImagePacketPacket)packet;

            if (Transfers.ContainsKey(image.ImageID.ID))
            {
                ImageTransfer transfer = Transfers[image.ImageID.ID];

                if (transfer.Size == 0)
                {
                    // We haven't received the header yet, block until it's received or times out
                    transfer.HeaderReceivedEvent.WaitOne(1000 * 20, false);

                    if (transfer.Size == 0)
                    {

                        Client.Log("Timed out while waiting for the image header to download for " +
                            transfer.ID, Helpers.LogLevel.Warning);

                        lock (Transfers) Transfers.Remove(transfer.ID);

                        // Fire the event with our transfer that contains Success = false;
                        if (OnImageReceived != null)
                        {
                            OnImageReceived(transfer);
                        }

                        return;
                    }
                }

                // The header is downloaded, we can insert this data in to the proper position
                Array.Copy(image.ImageData.Data, 0, transfer.AssetData, transfer.InitialDataSize + (1000 * (image.ImageID.Packet - 1)), image.ImageData.Data.Length);
                transfer.Transferred += image.ImageData.Data.Length;

                // Check if we downloaded the full image
                if (transfer.Transferred >= transfer.Size)
                {
                    transfer.Success = true;
                    lock (Transfers) Transfers.Remove(transfer.ID);

                    if (OnImageReceived != null)
                    {
                        OnImageReceived(transfer);
                    }
                }
            }
            else
            {
                Client.Log("Received an ImagePacket packet for an image we didn't request, ID: " + image.ImageID.ID,
                    Helpers.LogLevel.Warning);
            }
        }

        /// <summary>
        /// The requested image does not exist on the asset server
        /// </summary>
        public void ImageNotInDatabaseHandler(Packet packet, Simulator simulator)
        {
            ImageNotInDatabasePacket notin = (ImageNotInDatabasePacket)packet;

            if (Transfers.ContainsKey(notin.ImageID.ID))
            {
                ImageTransfer transfer = Transfers[notin.ImageID.ID];

                transfer.NotFound = true;
                lock (Transfers) Transfers.Remove(transfer.ID);

                // Fire the event with our transfer that contains Success = false;
                if (OnImageReceived != null)
                {
                    OnImageReceived(transfer);
                }
            }
            else
            {
                Client.Log("Received an ImageNotInDatabase packet for an image we didn't request, ID: " +
                    notin.ImageID.ID, Helpers.LogLevel.Warning);
            }
        }
    }
}
