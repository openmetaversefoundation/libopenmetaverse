using System;
using System.Collections.Generic;
using libsecondlife;

namespace libsecondlife.Utilities
{
    /// <summary>
    /// Keeps an up to date inventory of the currently seen objects in each
    /// simulator
    /// </summary>
    public class ObjectTracker
    {
        private SecondLife Client;
        private Dictionary<ulong, Dictionary<uint, PrimObject>> SimPrims = new Dictionary<ulong, Dictionary<uint, PrimObject>>();

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="client">A reference to the SecondLife client to track
        /// objects for</param>
        public ObjectTracker(SecondLife client)
        {
            Client = client;
        }
    }
}
