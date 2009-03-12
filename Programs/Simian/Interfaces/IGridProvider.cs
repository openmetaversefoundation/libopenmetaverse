using System;
using System.Net;
using OpenMetaverse;
using OpenMetaverse.StructuredData;

namespace Simian
{
    public class RegionInfo
    {
        public string Name;
        public UUID ID;
        public ulong Handle;
        public bool Online;
        public IPEndPoint IPAndPort;
        public Uri HttpServer;
        public UUID MapTextureID;
        public Uri Owner;

        public uint X
        {
            get
            {
                uint x, y;
                OpenMetaverse.Utils.LongToUInts(Handle, out x, out y);
                return x;
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
                return y;
            }

            set
            {
                uint x, y;
                OpenMetaverse.Utils.LongToUInts(Handle, out x, out y);
                Handle = OpenMetaverse.Utils.UIntsToLong(x, value);
            }
        }

        public OSDMap SerializeToOSD()
        {
            OSDMap osdata = new OSDMap();

            osdata["handle"] = OSD.FromULong(Handle);
            osdata["id"] = OSD.FromUUID(ID);
            osdata["map_texture_id"] = OSD.FromUUID(MapTextureID);
            osdata["name"] = OSD.FromString(Name);
            osdata["owner"] = OSD.FromUri(Owner);
            osdata["ipaddr"] = OSD.FromString(IPAndPort.Address.ToString());
            osdata["port"] = OSD.FromInteger(IPAndPort.Port);

            return osdata;
        }

        public void Deserialize(OSD osdata)
        {
            if (osdata.Type == OSDType.Map)
            {
                OSDMap map = (OSDMap)osdata;

                Handle = map["handle"].AsULong();
                ID = map["id"].AsUUID();
                MapTextureID = map["map_texture_id"].AsUUID();
                Name = map["name"].AsString();
                Owner = map["owner"].AsUri();

                IPAddress address;
                if (IPAddress.TryParse(map["ipaddr"].AsString(), out address))
                    IPAndPort = new IPEndPoint(address, map["port"].AsInteger());
            }
        }
    }

    public delegate void NeighborSimNotice(RegionInfo neighbor, bool online);

    public interface IGridProvider
    {
        bool TryRegisterGridSpace(RegionInfo region, out UUID regionID);
        /// <summary>
        /// Attempts to register any available space closest to the given grid
        /// coordinates
        /// </summary>
        /// <param name="region">Information about the region to be registered.
        /// The X, Y, and Handle values may be modified if the exact grid
        /// coordinate requested is not available</param>
        /// <param name="isolated">If true, the registered grid space must not
        /// be adjacent to any other regions</param>
        /// <param name="regionID">The unique identifier of the registered
        /// region upon success. This will also be assigned to region.ID</param>
        /// <returns>True if the registration was successful, otherwise false</returns>
        bool TryRegisterAnyGridSpace(RegionInfo region, bool isolated, out UUID regionID);
        bool UnregisterGridSpace(RegionInfo region);

        void RegionUpdate(RegionInfo region);
        void RegionHeartbeat(RegionInfo region);

        bool TryGetRegion(UUID regionID, out RegionInfo region);
        bool TryGetRegion(uint x, uint y, out RegionInfo region);

        /// <summary>
        /// Gets the default scene running on this server
        /// </summary>
        /// <returns>A reference to the default scene on this server, or null
        /// if there are no scenes</returns>
        ISceneProvider GetDefaultLocalScene();
    }
}
