using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    public enum WaterType
    {
        /// <summary></summary>
        Unknown,
        /// <summary></summary>
        Dry,
        /// <summary></summary>
        Waterfront,
        /// <summary></summary>
        Underwater
    }

    public static class Realism
    {
        public readonly static LLUUID TypingAnimation = new LLUUID("c541c47f-e0c0-058b-ad1a-d6ae3a4584d9");

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
            client.Self.Chat(String.Empty, 0, MainAvatar.ChatType.StartTyping);
            client.Self.AnimationStart(TypingAnimation);

            while (characters < message.Length)
            {
                if (!typing)
                {
                    // Start typing again
                    client.Self.Chat(String.Empty, 0, MainAvatar.ChatType.StartTyping);
                    client.Self.AnimationStart(TypingAnimation);
                    typing = true;
                }
                else
                {
                    // Randomly pause typing
                    if (rand.Next(10) >= 9)
                    {
                        client.Self.Chat(String.Empty, 0, MainAvatar.ChatType.StopTyping);
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
            client.Self.Chat(String.Empty, 0, MainAvatar.ChatType.StopTyping);
            client.Self.AnimationStop(TypingAnimation);
        }
    }

    public class ConnectionManager
    {
        private SecondLife Client;
        private ulong SimHandle;
        private LLVector3 Position = LLVector3.Zero;
        private System.Timers.Timer CheckTimer;

        public ConnectionManager(SecondLife client, int timerFrequency)
        {
            Client = client;

            CheckTimer = new System.Timers.Timer(timerFrequency);
            CheckTimer.Elapsed += new System.Timers.ElapsedEventHandler(CheckTimer_Elapsed);
        }

        public static bool PersistentLogin(SecondLife client, string firstName, string lastName, string password,
            string userAgent, string start, string author)
        {
            int unknownLogins = 0;

        Start:

            if (client.Network.Login(firstName, lastName, password, userAgent, start, author))
            {
                client.Log("Logged in to " + client.Network.CurrentSim, Helpers.LogLevel.Info);
                return true;
            }
            else
            {
                if (client.Network.LoginErrorKey == "god")
                {
                    client.Log("Grid is down, waiting 10 minutes", Helpers.LogLevel.Warning);
                    LoginWait(10);
                    goto Start;
                }
                else if (client.Network.LoginErrorKey == "key")
                {
                    client.Log("Bad username or password, giving up on login", Helpers.LogLevel.Error);
                    return false;
                }
                else if (client.Network.LoginErrorKey == "presence")
                {
                    client.Log("Server is still logging us out, waiting 1 minute", Helpers.LogLevel.Warning);
                    LoginWait(1);
                    goto Start;
                }
                else if (client.Network.LoginErrorKey == "disabled")
                {
                    client.Log("This account has been banned! Giving up on login", Helpers.LogLevel.Error);
                    return false;
                }
                else if (client.Network.LoginErrorKey == "timed out")
                {
                    client.Log("Login request timed out, waiting 1 minute", Helpers.LogLevel.Warning);
                    LoginWait(1);
                    goto Start;
                }
                else
                {
                    ++unknownLogins;

                    if (unknownLogins < 5)
                    {
                        client.Log("Unknown login error, waiting 2 minutes: " + client.Network.LoginErrorKey,
                            Helpers.LogLevel.Warning);
                        LoginWait(2);
                        goto Start;
                    }
                    else
                    {
                        client.Log("Too many unknown login error codes, giving up", Helpers.LogLevel.Error);
                        return false;
                    }
                }
            }
        }

        public void StayInSim(ulong handle, LLVector3 desiredPosition)
        {
            SimHandle = handle;
            Position = desiredPosition;
            CheckTimer.Start();
        }

        private static void LoginWait(int minutes)
        {
            Thread.Sleep(1000 * 60 * minutes);
        }

        private void CheckTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (SimHandle != 0)
            {
                if (Client.Network.CurrentSim.Handle != 0 &&
                    Client.Network.CurrentSim.Handle != SimHandle)
                {
                    // Attempt to move to our target sim
                    Client.Self.Teleport(SimHandle, Position);
                }
            }
        }
    }

    /// <summary>
    /// Maintains a cache of avatars and does blocking lookups for avatar data
    /// </summary>
    public class AvatarTracker
    {
        protected SecondLife Client;
        protected Dictionary<LLUUID, Avatar> avatars = new Dictionary<LLUUID, Avatar>();
        protected Dictionary<LLUUID, ManualResetEvent> NameLookupEvents = new Dictionary<LLUUID, ManualResetEvent>();
        protected Dictionary<LLUUID, ManualResetEvent> PropertiesLookupEvents = new Dictionary<LLUUID, ManualResetEvent>();
        protected Dictionary<LLUUID, ManualResetEvent> InterestsLookupEvents = new Dictionary<LLUUID, ManualResetEvent>();
        protected Dictionary<LLUUID, ManualResetEvent> GroupsLookupEvents = new Dictionary<LLUUID, ManualResetEvent>();

        public AvatarTracker(SecondLife client)
        {
            Client = client;

            Client.Avatars.OnAvatarNames += new AvatarManager.AvatarNamesCallback(Avatars_OnAvatarNames);
            Client.Avatars.OnAvatarInterests += new AvatarManager.AvatarInterestsCallback(Avatars_OnAvatarInterests);
            Client.Avatars.OnAvatarProperties += new AvatarManager.AvatarPropertiesCallback(Avatars_OnAvatarProperties);
            Client.Avatars.OnAvatarGroups += new AvatarManager.AvatarGroupsCallback(Avatars_OnAvatarGroups);

            //Client.Objects.OnNewAvatar += new ObjectManager.NewAvatarCallback(Objects_OnNewAvatar);
            //Client.Objects.OnObjectUpdated += new ObjectManager.ObjectUpdatedCallback(Objects_OnObjectUpdated);
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
                    if (avatar.CurrentSim == Client.Network.CurrentSim)
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
            lock (NameLookupEvents) NameLookupEvents.Add(id, new ManualResetEvent(false));

            // Call function
            Client.Avatars.RequestAvatarName(id);

            // Start blocking while we wait for this name to be fetched
            NameLookupEvents[id].WaitOne(5000, false);

            // Clean up
            lock (NameLookupEvents) NameLookupEvents.Remove(id);

            // Return
            return LocalAvatarNameLookup(id);
        }

        public bool GetAvatarProfile(LLUUID id, out Avatar.Interests interests, out Avatar.AvatarProperties properties,
            out List<LLUUID> groups)
        {
            // Do a local lookup first
            if (avatars.ContainsKey(id) && avatars[id].ProfileProperties.BornOn != null &&
                avatars[id].ProfileProperties.BornOn != String.Empty)
            {
                interests = avatars[id].ProfileInterests;
                properties = avatars[id].ProfileProperties;
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
            lock (GroupsLookupEvents)
                if (!GroupsLookupEvents.ContainsKey(id))
                    GroupsLookupEvents[id] = new ManualResetEvent(false);

            // Request the avatar profile
            Client.Avatars.RequestAvatarProperties(id);

            // Wait for all of the events to complete
            PropertiesLookupEvents[id].WaitOne(5000, false);
            InterestsLookupEvents[id].WaitOne(5000, false);
            GroupsLookupEvents[id].WaitOne(5000, false);

            // Destroy the ManualResetEvents
            lock (PropertiesLookupEvents)
                PropertiesLookupEvents.Remove(id);
            lock (InterestsLookupEvents)
                InterestsLookupEvents.Remove(id);
            lock (GroupsLookupEvents)
                GroupsLookupEvents.Remove(id);

            // If we got a filled in profile return everything
            if (avatars.ContainsKey(id) && avatars[id].ProfileProperties.BornOn != null &&
                avatars[id].ProfileProperties.BornOn != String.Empty)
            {
                interests = avatars[id].ProfileInterests;
                properties = avatars[id].ProfileProperties;
                groups = avatars[id].Groups;

                return true;
            }
            else
            {
                interests = new Avatar.Interests();
                properties = new Avatar.AvatarProperties();
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

        private void Avatars_OnAvatarNames(Dictionary<LLUUID, string> names)
        {
            lock (avatars)
            {
                foreach (KeyValuePair<LLUUID, string> kvp in names)
                {
                    if (!avatars.ContainsKey(kvp.Key) || avatars[kvp.Key] == null)
                        avatars[kvp.Key] = new Avatar();

                    // FIXME: Change this to .name when we move inside libsecondlife
                    avatars[kvp.Key].Name = kvp.Value;

                    if (NameLookupEvents.ContainsKey(kvp.Key))
                        NameLookupEvents[kvp.Key].Set();
                }
            }
        }

        void Avatars_OnAvatarProperties(LLUUID avatarID, Avatar.AvatarProperties properties)
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

    /// <summary>
    /// 
    /// </summary>
    public class ParcelDownloader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator">Simulator where the parcels are located</param>
        /// <param name="Parcels">Mapping of parcel LocalIDs to Parcel objects</param>
        public delegate void ParcelsDownloadedCallback(Simulator simulator, Dictionary<int, Parcel> Parcels, int[,] map);


        /// <summary>
        /// 
        /// </summary>
        public event ParcelsDownloadedCallback OnParcelsDownloaded;

        private SecondLife Client;
        /// <summary>Dictionary of 64x64 arrays of parcels which have been successfully downloaded 
        /// for each simulator (and their LocalID's, 0 = Null)</summary>
        private Dictionary<Simulator, int[,]> ParcelMarked = new Dictionary<Simulator, int[,]>();
        private Dictionary<Simulator, Dictionary<int, Parcel>> Parcels = new Dictionary<Simulator, Dictionary<int, Parcel>>();
        private List<Simulator> active_sims = new List<Simulator>();

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="client">A reference to the SecondLife client</param>
        public ParcelDownloader(SecondLife client)
        {
            Client = client;
            Client.Parcels.OnParcelProperties += new ParcelManager.ParcelPropertiesCallback(Parcels_OnParcelProperties);
            Client.Parcels.OnAccessListReply += new ParcelManager.ParcelAccessListReplyCallback(Parcels_OnParcelAccessList);
        }

        public void DownloadSimParcels(Simulator simulator)
        {
            lock (active_sims)
            {
                if (active_sims.Contains(simulator))
                {
                    Client.Log("DownloadSimParcels(" + simulator + ") called more than once?", Helpers.LogLevel.Error);
                    return;
                }

                active_sims.Add(simulator);
            }

            lock (ParcelMarked)
            {
                if (!ParcelMarked.ContainsKey(simulator))
                {
                    ParcelMarked[simulator] = new int[64, 64];
                    Parcels[simulator] = new Dictionary<int, Parcel>();
                }
            }

            Client.Parcels.PropertiesRequest(simulator, 0.0f, 0.0f, 0.0f, 0.0f, 0, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="map"></param>
        /// <param name="localid"></param>
        /// <returns></returns>
        public float GetHeightRange(int[,] map, int localid)
        {
            float min = Single.MaxValue;
            float max = 0.0f;

            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    if (map[y, x] == localid)
                    {
                        for (int y1 = 0; y1 < 4; y1++)
                        {
                            for (int x1 = 0; x1 < 4; x1++)
                            {
                                float height;
                                int tries = 0;

                            CheckHeight:

                                if (Client.Terrain.TerrainHeightAtPoint(Client.Network.CurrentSim.Handle,
                                    x * 4 + x1, y * 4 + y1, out height))
                                {
                                    if (height < min)
                                        min = height;
                                    if (height > max)
                                        max = height;
                                }
                                else if (tries > 4)
                                {
                                    Client.Log("Too many tries on this terrain block, skipping",
                                        Helpers.LogLevel.Warning);
                                    continue;
                                }
                                else
                                {
                                    Client.Log(String.Format("Terrain height is null at {0},{1} retrying",
                                        x * 4 + x1, y * 4 + y1), Helpers.LogLevel.Info);

                                    // Terrain at this point hasn't been downloaded, move the camera to this spot
                                    // and try again
                                    Client.Self.Status.Camera.CameraCenter.X = (float)(x * 4 + x1);
                                    Client.Self.Status.Camera.CameraCenter.Y = (float)(y * 4 + y1);
                                    Client.Self.Status.Camera.CameraCenter.Z = Client.Self.Position.Z;
                                    Client.Self.Status.SendUpdate(true);

                                    Thread.Sleep(1000);
                                    goto CheckHeight;
                                }
                            }
                        }
                    }
                }
            }

            if (min != Single.MaxValue)
            {
                return max - min;
            }
            else
            {
                Client.Log("Error decoding terrain for parcel " + localid, Helpers.LogLevel.Error);
                return Single.NaN;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="map"></param>
        /// <param name="localid"></param>
        /// <returns></returns>
        public WaterType GetWaterType(int[,] map, int localid)
        {
            if (!Client.Settings.STORE_LAND_PATCHES)
            {
                Client.Log("GetWaterType() will not work without Settings.STORE_LAND_PATCHES set to true",
                    Helpers.LogLevel.Error);
                return WaterType.Unknown;
            }

            bool underwater = false;
            bool abovewater = false;

            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    if (map[y, x] == localid)
                    {
                        for (int y1 = 0; y1 < 4; y1++)
                        {
                            for (int x1 = 0; x1 < 4; x1++)
                            {
                                float height;
                                int tries = 0;

                            CheckHeight:
                                tries++;

                                if (Client.Terrain.TerrainHeightAtPoint(Client.Network.CurrentSim.Handle,
                                    x * 4 + x1, y * 4 + y1, out height))
                                {
                                    if (height < Client.Network.CurrentSim.WaterHeight)
                                    {
                                        underwater = true;
                                    }
                                    else
                                    {
                                        abovewater = true;
                                    }
                                }
                                else if (tries > 4)
                                {
                                    Client.Log("Too many tries on this terrain block, skipping", 
                                        Helpers.LogLevel.Warning);
                                    continue;
                                }
                                else
                                {
                                    Client.Log(String.Format("Terrain height is null at {0},{1} retrying",
                                        x * 4 + x1, y * 4 + y1), Helpers.LogLevel.Info);

                                    // Terrain at this point hasn't been downloaded, move the camera to this spot
                                    // and try again
                                    Client.Self.Status.Camera.CameraCenter.X = (float)(x * 4 + x1);
                                    Client.Self.Status.Camera.CameraCenter.Y = (float)(y * 4 + y1);
                                    Client.Self.Status.Camera.CameraCenter.Z = Client.Self.Position.Z;
                                    Client.Self.Status.SendUpdate(true);

                                    Thread.Sleep(1000);
                                    goto CheckHeight;
                                }
                            }
                        }
                    }
                }
            }

            if (underwater && abovewater)
            {
                return WaterType.Waterfront;
            }
            else if (abovewater)
            {
                return WaterType.Dry;
            }
            else if (underwater)
            {
                return WaterType.Underwater;
            }
            else
            {
                Client.Log("Error decoding terrain for parcel " + localid, Helpers.LogLevel.Error);
                return WaterType.Unknown;
            }
        }

        public int GetRectangularDeviation(LLVector3 aabbmin, LLVector3 aabbmax, int area)
        {
            int xlength = (int)(aabbmax.X - aabbmin.X);
            int ylength = (int)(aabbmax.Y - aabbmin.Y);
            int aabbarea = xlength * ylength;
            return (aabbarea - area) / 16;
        }

        private void Parcels_OnParcelAccessList(Simulator simulator, int sequenceID, int localID, uint flags,
                                                List<ParcelManager.ParcelAccessEntry> accessEntries)
        {
            Parcels[simulator][localID].AccessList = accessEntries;
        }

        private void Parcels_OnParcelProperties(Parcel parcel, ParcelManager.ParcelResult result, int sequenceID,
            bool snapSelection)
        {
            // Check if this is for a simulator we're concerned with
            if (!active_sims.Contains(parcel.Simulator)) return;

            // Warn about parcel property request errors and bail out
            if (result == ParcelManager.ParcelResult.NoData)
            {
                Client.Log("ParcelDownloader received a NoData response, sequenceID " + sequenceID,
                    Helpers.LogLevel.Warning);
                return;
            }

            // Warn about unexpected data and bail out
            if (!ParcelMarked.ContainsKey(parcel.Simulator))
            {
                Client.Log("ParcelDownloader received unexpected parcel data for " + parcel.Simulator,
                    Helpers.LogLevel.Warning);
                return;
            }

            int x, y, index, bit;
            int[,] markers = ParcelMarked[parcel.Simulator];

            // Add this parcel to the dictionary of LocalID -> Parcel mappings
            lock (Parcels[parcel.Simulator])
                if (!Parcels[parcel.Simulator].ContainsKey(parcel.LocalID))
                    Parcels[parcel.Simulator][parcel.LocalID] = parcel;

            // Request the access list for this parcel
            Client.Parcels.AccessListRequest(parcel.Simulator, parcel.LocalID, 
                ParcelManager.AccessList.Both, 0);

            // Mark this area as downloaded
            for (y = 0; y < 64; y++)
            {
                for (x = 0; x < 64; x++)
                {
                    if (markers[y, x] == 0)
                    {
                        index = (y * 64) + x;
                        bit = index % 8;
                        index >>= 3;

                        if ((parcel.Bitmap[index] & (1 << bit)) != 0)
                            markers[y, x] = parcel.LocalID;
                    }
                }
            }

            // Request parcel information for the next missing area
            for (y = 0; y < 64; y++)
            {
                for (x = 0; x < 64; x++)
                {
                    if (markers[y, x] == 0)
                    {
                        Client.Parcels.PropertiesRequest(parcel.Simulator,
                                                         (y + 1) * 4.0f, (x + 1) * 4.0f,
                                                         y * 4.0f, x * 4.0f, 0, false);

                        return;
                    }
                }
            }

            // If we get here, there are no more zeroes in the markers map
            lock (active_sims)
            {
                active_sims.Remove(parcel.Simulator);

                if (OnParcelsDownloaded != null)
                {
                    // This map is complete, fire callback
                    try { OnParcelsDownloaded(parcel.Simulator, Parcels[parcel.Simulator], markers); }
                    catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
                }
            }
        }
    }
}
