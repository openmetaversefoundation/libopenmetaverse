using System;
using System.Collections.Generic;
using ExtensionLoader;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace Simian
{
    public class ParcelManager : IExtension<ISceneProvider>, IParcelProvider
    {
        ISceneProvider scene;
        Dictionary<int, Parcel> parcels = new Dictionary<int, Parcel>();
        /// <summary>X,Y ordered 2D array of the parcelIDs for each sq. meter of a simulator</summary>
        int[] parcelOverlay = new int[64 * 64];

        public ParcelManager()
        {
        }

        public bool Start(ISceneProvider scene)
        {
            this.scene = scene;

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

            return true;
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
                            scene.UDP.SendPacket(agent.ID, overlay, PacketCategory.State);

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

        public void UpdateParcel(Parcel parcel)
        {
            lock (parcels) parcels[parcel.LocalID] = parcel;
        }

        public int GetParcelID(int x, int y)
        {
            return parcelOverlay[y * 64 + x];
        }

        public bool TryGetParcel(int parcelID, out Parcel parcel)
        {
            return parcels.TryGetValue(parcelID, out parcel);
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
    }
}
