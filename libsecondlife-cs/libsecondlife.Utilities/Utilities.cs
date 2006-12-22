using System;
using System.Collections.Generic;
using System.Threading;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.Utilities
{
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

        public AvatarTracker(SecondLife client)
        {
            Client = client;

            Client.Avatars.OnAvatarNames += new AvatarManager.AvatarNamesCallback(Avatars_OnAvatarNames);
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

        /// <summary>
        /// Get an avatar's name, either from the cache or request it.
        /// This function is blocking
        /// </summary>
        /// <param name="key">Avatar key to look up</param>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ids"></param>
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

                    avatars[kvp.Key].Name = kvp.Value;

                    lock (NameLookupEvents)
                    {
                        if (NameLookupEvents.ContainsKey(kvp.Key))
                            NameLookupEvents[kvp.Key].Set();
                    }
                }
            }
        }
    }
}
