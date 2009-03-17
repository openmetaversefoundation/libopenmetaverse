using System;
using System.Collections.Generic;
using System.Threading;
using ExtensionLoader;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace Simian
{
    public class LLParcels : IExtension<ISceneProvider>
    {
        ISceneProvider scene;

        public LLParcels()
        {
        }

        public bool Start(ISceneProvider scene)
        {
            this.scene = scene;

            scene.UDP.RegisterPacketCallback(PacketType.ParcelPropertiesRequest, ParcelPropertiesRequestHandler);
            scene.UDP.RegisterPacketCallback(PacketType.ParcelPropertiesUpdate, ParcelPropertiesUpdateHandler);
            return true;
        }

        public void Stop()
        {
        }

        void SendParcelProperties(int parcelID, int sequenceID, bool snapSelection, ParcelResult result,
            Agent agent)
        {
            Parcel parcel;
            if (scene.Parcels.TryGetParcel(parcelID, out parcel))
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

                scene.UDP.SendPacket(agent.ID, properties, PacketCategory.Transaction);
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
                        int currentParcelID = scene.Parcels.GetParcelID(west + x, south + y);
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
            if (scene.Parcels.TryGetParcel(update.ParcelData.LocalID, out parcel))
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

                scene.Parcels.UpdateParcel(parcel);

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
