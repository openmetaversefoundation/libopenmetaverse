using System;
using System.Collections.Generic;
using ExtensionLoader;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace Simian.Extensions
{
    public class ParcelManager : IExtension<Simian>, IParcelProvider
    {
        Simian server;
        Dictionary<int, Parcel> parcels = new Dictionary<int, Parcel>();
        /// <summary>X,Y ordered 2D array of the parcelIDs for each sq. meter of a simulator</summary>
        int[] parcelOverlay = new int[64 * 64];

        public ParcelManager()
        {
        }

        public void Start(Simian server)
        {
            this.server = server;

            lock (parcels)
                parcels.Clear();

            // Create a default parcel
            Parcel parcel = new Parcel(0);
            parcel.Desc = "Simian Parcel";
            parcel.Flags = Parcel.ParcelFlags.AllowAPrimitiveEntry | Parcel.ParcelFlags.AllowFly | Parcel.ParcelFlags.AllowGroupScripts |
                Parcel.ParcelFlags.AllowLandmark | Parcel.ParcelFlags.AllowOtherScripts | Parcel.ParcelFlags.AllowTerraform |
                Parcel.ParcelFlags.AllowVoiceChat | Parcel.ParcelFlags.CreateGroupObjects | Parcel.ParcelFlags.CreateObjects |
                Parcel.ParcelFlags.UseBanList;
            parcel.Landing = Parcel.LandingType.Direct;
            parcel.MaxPrims = 20000;
            parcel.Name = "Simian";
            parcel.Status = Parcel.ParcelStatus.Leased;
            parcel.UserLocation = (parcel.AABBMin + parcel.AABBMax) * 0.5f;

            parcel.Bitmap = new byte[512];
            for (int i = 0; i < 512; i++)
                parcel.Bitmap[i] = Byte.MaxValue;

            // The parcelOverlay defaults to all zeroes, and the LocalID of this parcel is zero,
            // so we don't need to modify parcelOverlay before calling this
            UpdateParcelSize(ref parcel);

            // Add the default parcel to the list
            parcels[parcel.LocalID] = parcel;

            server.UDP.RegisterPacketCallback(PacketType.ParcelPropertiesRequest, ParcelPropertiesRequestHandler);
            server.UDP.RegisterPacketCallback(PacketType.ParcelPropertiesUpdate, ParcelPropertiesUpdateHandler);
        }

        public void Stop()
        {
        }

        public void SendParcelOverlay(Agent agent)
        {
            const int LAND_BLOCKS_PER_PACKET = 1024;

            byte[] byteArray = new byte[LAND_BLOCKS_PER_PACKET];
            int byteArrayCount = 0;
            int sequenceID = 0;

            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    byte tempByte = 0; // The flags for the current 4x4m parcel square

                    Parcel parcel;
                    if (parcels.TryGetValue(parcelOverlay[y * 64 + x], out parcel))
                    {
                        // Set the ownership/sale flag
                        if (parcel.OwnerID == agent.ID)
                            tempByte = (byte)ParcelOverlayType.OwnedBySelf;
                        else if (parcel.AuctionID != 0)
                            tempByte = (byte)ParcelOverlayType.Auction;
                        else if (parcel.SalePrice > 0 && (parcel.AuthBuyerID == UUID.Zero || parcel.AuthBuyerID == agent.ID))
                            tempByte = (byte)ParcelOverlayType.ForSale;
                        else if (parcel.GroupID != UUID.Zero)
                            tempByte = (byte)ParcelOverlayType.OwnedByGroup;
                        else if (parcel.OwnerID != UUID.Zero)
                            tempByte = (byte)ParcelOverlayType.OwnedByOther;
                        else
                            tempByte = (byte)ParcelOverlayType.Public;

                        // Set the border flags
                        if (x == 0)
                            tempByte |= (byte)ParcelOverlayType.BorderWest;
                        else if (parcelOverlay[y * 64 + (x - 1)] != parcel.LocalID)
                            // Parcel to the west is different from the current parcel
                            tempByte |= (byte)ParcelOverlayType.BorderWest;

                        if (y == 0)
                            tempByte |= (byte)ParcelOverlayType.BorderSouth;
                        else if (parcelOverlay[(y - 1) * 64 + x] != parcel.LocalID)
                            // Parcel to the south is different from the current parcel
                            tempByte |= (byte)ParcelOverlayType.BorderSouth;

                        byteArray[byteArrayCount] = tempByte;
                        ++byteArrayCount;
                        if (byteArrayCount >= LAND_BLOCKS_PER_PACKET)
                        {
                            // Send a ParcelOverlay packet
                            ParcelOverlayPacket overlay = new ParcelOverlayPacket();
                            overlay.ParcelData.SequenceID = sequenceID;
                            overlay.ParcelData.Data = byteArray;
                            server.UDP.SendPacket(agent.ID, overlay, PacketCategory.State);

                            byteArrayCount = 0;
                            ++sequenceID;
                        }
                    }
                    else
                    {
                        Logger.Log("Parcel overlay references missing parcel " + parcelOverlay[y * 64 + x],
                            Helpers.LogLevel.Warning);
                    }
                }
            }
        }

        void UpdateParcelSize(ref Parcel parcel)
        {
            int minX = 64;
            int minY = 64;
            int maxX = 0;
            int maxY = 0;
            int area = 0;

            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    if (parcelOverlay[y * 64 + x] == parcel.LocalID)
                    {
                        int x4 = x * 4;
                        int y4 = y * 4;

                        if (minX > x4) minX = x4;
                        if (minY > y4) minY = y4;
                        if (maxX < x4) maxX = x4;
                        if (maxX < y4) maxY = y4;
                        area += 16;
                    }
                }
            }

            parcel.AABBMin.X = minX;
            parcel.AABBMin.Y = minY;
            parcel.AABBMax.X = maxX;
            parcel.AABBMax.Y = maxY;
            parcel.Area = area;
        }

        void SendParcelProperties(int parcelID, int sequenceID, bool snapSelection, ParcelResult result,
            Agent agent)
        {
            Parcel parcel;
            if (parcels.TryGetValue(parcelID, out parcel))
            {
                ParcelPropertiesPacket properties = new ParcelPropertiesPacket();
                properties.AgeVerificationBlock.RegionDenyAgeUnverified = false;
                properties.ParcelData.AABBMax = parcel.AABBMax;
                properties.ParcelData.AABBMin = parcel.AABBMin;
                properties.ParcelData.Area = parcel.Area;
                properties.ParcelData.AuctionID = parcel.AuctionID;
                properties.ParcelData.AuthBuyerID = parcel.AuthBuyerID;
                properties.ParcelData.Bitmap = parcel.Bitmap;
                properties.ParcelData.Category = (byte)parcel.Category;
                properties.ParcelData.ClaimDate = (int)Utils.DateTimeToUnixTime(parcel.ClaimDate);
                properties.ParcelData.ClaimPrice = parcel.ClaimPrice;
                properties.ParcelData.Desc = Utils.StringToBytes(parcel.Desc);
                properties.ParcelData.GroupID = parcel.GroupID;
                properties.ParcelData.GroupPrims = parcel.GroupPrims;
                properties.ParcelData.IsGroupOwned = parcel.IsGroupOwned;
                properties.ParcelData.LandingType = (byte)parcel.Landing;
                properties.ParcelData.LocalID = parcel.LocalID;
                properties.ParcelData.MaxPrims = parcel.MaxPrims;
                properties.ParcelData.MediaAutoScale = parcel.Media.MediaAutoScale;
                properties.ParcelData.MediaID = parcel.Media.MediaID;
                properties.ParcelData.MediaURL = Utils.StringToBytes(parcel.Media.MediaURL);
                properties.ParcelData.MusicURL = Utils.StringToBytes(parcel.MusicURL);
                properties.ParcelData.Name = Utils.StringToBytes(parcel.Name);
                properties.ParcelData.OtherCleanTime = parcel.OtherCleanTime;
                properties.ParcelData.OtherCount = parcel.OtherCount;
                properties.ParcelData.OtherPrims = parcel.OtherPrims;
                properties.ParcelData.OwnerID = parcel.OwnerID;
                properties.ParcelData.OwnerPrims = parcel.OwnerPrims;
                properties.ParcelData.ParcelFlags = (uint)parcel.Flags;
                properties.ParcelData.ParcelPrimBonus = parcel.ParcelPrimBonus;
                properties.ParcelData.PassHours = parcel.PassHours;
                properties.ParcelData.PassPrice = parcel.PassPrice;
                properties.ParcelData.PublicCount = parcel.PublicCount;
                properties.ParcelData.RegionDenyAnonymous = parcel.RegionDenyAnonymous;
                properties.ParcelData.RegionDenyIdentified = false; // Deprecated
                properties.ParcelData.RegionDenyTransacted = false; // Deprecated
                properties.ParcelData.RegionPushOverride = parcel.RegionPushOverride;
                properties.ParcelData.RentPrice = parcel.RentPrice;
                properties.ParcelData.RequestResult = (int)result;
                properties.ParcelData.SalePrice = parcel.SalePrice;
                properties.ParcelData.SelectedPrims = 0; // TODO:
                properties.ParcelData.SelfCount = parcel.SelfCount;
                properties.ParcelData.SequenceID = sequenceID;
                properties.ParcelData.SimWideMaxPrims = parcel.SimWideMaxPrims;
                properties.ParcelData.SimWideTotalPrims = parcel.SimWideTotalPrims;
                properties.ParcelData.SnapSelection = snapSelection;
                properties.ParcelData.SnapshotID = parcel.SnapshotID;
                properties.ParcelData.Status = (byte)parcel.Status;
                properties.ParcelData.TotalPrims = parcel.TotalPrims;
                properties.ParcelData.UserLocation = parcel.UserLocation;
                properties.ParcelData.UserLookAt = parcel.UserLookAt;

                // HACK: Make everyone think they are the owner of this parcel
                properties.ParcelData.OwnerID = agent.ID;

                server.UDP.SendPacket(agent.ID, properties, PacketCategory.Transaction);
            }
            else
            {
                Logger.Log("SendParcelProperties() called for unknown parcel " + parcelID, Helpers.LogLevel.Warning);
            }
        }

        void ParcelPropertiesRequestHandler(Packet packet, Agent agent)
        {
            ParcelPropertiesRequestPacket request = (ParcelPropertiesRequestPacket)packet;

            // TODO: Replace with HashSet when we switch to .NET 3.5
            List<int> parcels = new List<int>();

            // Convert the boundaries to integers
            int north = (int)Math.Round(request.ParcelData.North) / 4;
            int east = (int)Math.Round(request.ParcelData.East) / 4;
            int south = (int)Math.Round(request.ParcelData.South) / 4;
            int west = (int)Math.Round(request.ParcelData.West) / 4;

            // Find all of the parcels within the given boundaries
            int xLen = east - west;
            int yLen = north - south;

            for (int x = 0; x < xLen; x++)
            {
                for (int y = 0; y < yLen; y++)
                {
                    if (west + x < 64 && south + y < 64)
                    {
                        int currentParcelID = parcelOverlay[(south + y) * 64 + (west + x)];
                        if (!parcels.Contains(currentParcelID))
                            parcels.Add(currentParcelID);
                    }
                }
            }

            ParcelResult result = ParcelResult.NoData;
            if (parcels.Count == 1)
                result = ParcelResult.Single;
            else if (parcels.Count > 1)
                result = ParcelResult.Multiple;

            for (int i = 0; i < parcels.Count; i++)
                SendParcelProperties(parcels[i], request.ParcelData.SequenceID, request.ParcelData.SnapSelection, result, agent);
        }

        void ParcelPropertiesUpdateHandler(Packet packet, Agent agent)
        {
            ParcelPropertiesUpdatePacket update = (ParcelPropertiesUpdatePacket)packet;

            Parcel parcel;
            if (parcels.TryGetValue(update.ParcelData.LocalID, out parcel))
            {
                parcel.AuthBuyerID = update.ParcelData.AuthBuyerID;
                parcel.Category = (Parcel.ParcelCategory)update.ParcelData.Category;
                parcel.Desc = Utils.BytesToString(update.ParcelData.Desc);
                parcel.Flags = (Parcel.ParcelFlags)update.ParcelData.ParcelFlags;
                parcel.GroupID = update.ParcelData.GroupID;
                parcel.Landing = (Parcel.LandingType)update.ParcelData.LandingType;
                parcel.Media.MediaAutoScale = update.ParcelData.MediaAutoScale;
                parcel.Media.MediaID = update.ParcelData.MediaID;
                parcel.Media.MediaURL = Utils.BytesToString(update.ParcelData.MediaURL);
                parcel.MusicURL = Utils.BytesToString(update.ParcelData.MusicURL);
                parcel.Name = Utils.BytesToString(update.ParcelData.Name);
                parcel.PassHours = update.ParcelData.PassHours;
                parcel.PassPrice = update.ParcelData.PassPrice;
                parcel.SalePrice = update.ParcelData.SalePrice;
                parcel.SnapshotID = update.ParcelData.SnapshotID;
                parcel.UserLocation = update.ParcelData.UserLocation;
                parcel.UserLookAt = update.ParcelData.UserLookAt;

                lock (parcels)
                    parcels[parcel.LocalID] = parcel;

                if (update.ParcelData.Flags != 0)
                    SendParcelProperties(parcel.LocalID, 0, false, ParcelResult.Single, agent);
            }
            else
            {
                Logger.Log("Got a ParcelPropertiesUpdate for an unknown parcel " + update.ParcelData.LocalID,
                    Helpers.LogLevel.Warning);
            }
        }
    }
}
