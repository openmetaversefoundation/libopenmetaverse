using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using OpenMetaverse;
using OpenMetaverse.StructuredData;

namespace Simian
{
    public struct RegionInfo
    {
        public string Name;
        public UUID ID;
        public ulong Handle;
        public bool Online;
        public IPEndPoint IPAndPort;
        public Uri HttpServer;
        public UUID MapTextureID;
        public Uri Owner;
        public Uri EnableClientCap;

        public uint X
        {
            get
            {
                uint x, y;
                OpenMetaverse.Utils.LongToUInts(Handle, out x, out y);
                return x / 256;
            }

            set
            {
                uint x, y;
                OpenMetaverse.Utils.LongToUInts(Handle, out x, out y);
                Handle = OpenMetaverse.Utils.UIntsToLong(value, y);
            }
        }

        public uint Y
        {
            get
            {
                uint x, y;
                OpenMetaverse.Utils.LongToUInts(Handle, out x, out y);
                return y / 256;
            }

            set
            {
                uint x, y;
                OpenMetaverse.Utils.LongToUInts(Handle, out x, out y);
                Handle = OpenMetaverse.Utils.UIntsToLong(x, value);
            }
        }
    }

    public delegate void RegionUpdateCallback(RegionInfo regionInfo);

    public interface IGridProvider
    {
        event RegionUpdateCallback OnRegionUpdate;

        bool TryRegisterGridSpace(RegionInfo regionInfo, X509Certificate2 regionCert, out UUID regionID);
        /// <summary>
        /// Attempts to register any available space closest to the given grid
        /// coordinates
        /// </summary>
        /// <param name="regionInfo">Information about the region to be registered.
        /// The X, Y, and Handle values may be modified if the exact grid
        /// coordinate requested is not available</param>
        /// <param name="regionCert">SSL client certificate file for the region.
        /// Must be signed by the grid server</param>
        /// <param name="isolated">If true, the registered grid space must not
        /// be adjacent to any other regions</param>
        /// <param name="regionID">The unique identifier of the registered
        /// region upon success. This will also be assigned to region.ID</param>
        /// <returns>True if the registration was successful, otherwise false</returns>
        bool TryRegisterAnyGridSpace(RegionInfo regionInfo, X509Certificate2 regionCert, bool isolated, out UUID regionID);
        bool UnregisterGridSpace(UUID regionID, X509Certificate2 regionCert);

        void RegionUpdate(RegionInfo regionInfo, X509Certificate2 regionCert);
        void RegionHeartbeat(UUID regionID, X509Certificate2 regionCert);

        bool TryGetRegion(UUID regionID, X509Certificate2 regionCert, out RegionInfo region);
        bool TryGetRegion(uint regionX, uint regionY, X509Certificate2 regionCert, out RegionInfo region);

        /// <summary>
        /// Gets the default scene running on this server
        /// </summary>
        /// <returns>A reference to the default scene on this server, or null
        /// if there are no scenes</returns>
        ISceneProvider GetDefaultLocalScene();
    }
}
