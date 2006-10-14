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

using System;
using System.Timers;
using System.Collections;

namespace libsecondlife
{
       // Class for parcels used by the dataserver
       // Such as the "For-Sale" listings
       public class DirectoryParcel
       {
               public LLUUID ID;
               public LLUUID OwnerID;
               public LLUUID SnapshotID;
               public U64 RegionHandle;
               public string Name;
               public string SimName;
               public string Desc;
               public int SalePrice;
               public int ActualArea;
               public LLVector3 GlobalPosition;
               public LLVector3 SimPosition;
               public float Dwell;

               public DirectoryParcel()
               {
                       GlobalPosition = new LLVector3();
                       SimPosition = new LLVector3();
               }
       }

       // Class for parcels within a sim
       public class Parcel
       {
               public int RequestResult;
               public int SequenceID;
               public bool SnapSelection;
               public int SelfCount;
               public int OtherCount;
               public int PublicCount;
               public int LocalID;
               public LLUUID OwnerID;
               public bool IsGroupOwned;
               public uint AuctionID;
               public bool ReservedNewbie;
               public int ClaimDate;
               public int ClaimPrice;
               public int RentPrice;
               public LLVector3 AABBMin;
               public LLVector3 AABBMax;
               public byte[] Bitmap;
               public int Area;
               public byte Status;
               public int SimWideMaxObjects;
               public int SimWideTotalObjects;
               public int MaxObjects;
               public int TotalObjects;
               public int OwnerObjects;
               public int GroupObjects;
               public int OtherObjects;
               public float ParcelObjectBonus;
               public int OtherCleanTime;
               public uint ParcelFlags;
               public int SalePrice;
               public string Name;
               public string Desc;
               public string MusicURL;
               public string MediaURL;
               public LLUUID MediaID;
               public byte MediaAutoScale;
               public LLUUID GroupID;
               public int PassPrice;
               public float PassHours;
               public byte Category;
               public LLUUID AuthBuyerID;
               public LLUUID SnapshotID;
               public LLVector3 UserLocation;
               public LLVector3 UserLookAt;
               public byte LandingType;
               public float Dwell;

               private Simulator Sim; // Using Sim instead of Region since it references both

               private void init()
               {
                       Dwell                           = 0.0f;
                       RequestResult           = 0;
                       SequenceID                      = 0;
                       SnapSelection           = false;
                       SelfCount                       = 0;
                       OtherCount                      = 0;
                       PublicCount                     = 0;
                       LocalID                         = 0;
                       OwnerID                         = new LLUUID();
                       IsGroupOwned            = false;
                       AuctionID                       = 0;
                       ReservedNewbie          = false;
                       ClaimDate                       = 0;
                       ClaimPrice                      = 0;
                       RentPrice                       = 0;
                       AABBMin                         = new LLVector3();
                       AABBMax                         = new LLVector3();
                       Bitmap                          = new byte[512];
                       Area                            = 0;
                       Status                          = 0;
                       SimWideMaxObjects       = 0;
                       SimWideTotalObjects     = 0;
                       MaxObjects                      = 0;
                       TotalObjects            = 0;
                       OwnerObjects            = 0;
                       GroupObjects            = 0;
                       OtherObjects            = 0;
                       ParcelObjectBonus       = 0.0f;
                       OtherCleanTime          = 0;
                       ParcelFlags                     = 0;
                       SalePrice                       = 0;
                       Name                            = "";
                       Desc                            = "";
                       MusicURL                        = "";
                       MediaURL                        = "";
                       MediaID                         = new LLUUID();
                       MediaAutoScale          = 0;
                       GroupID                         = new LLUUID();
                       PassPrice                       = 0;
                       PassHours                       = 0.0f;
                       Category                        = 0;
                       AuthBuyerID                     = new LLUUID();
                       SnapshotID                      = new LLUUID();
                       UserLocation            = new LLVector3();
                       UserLookAt                      = new LLVector3();
                       LandingType                     = 0;
               }

               public Parcel()
               {
                       init();
               }

               public Parcel(Simulator simulator)
               {
                       Sim = simulator;
                       init();
               }

               public void GetDwell(SecondLife client)
               {
                       Packet DwellPacket = Packets.Parcel.ParcelDwellRequest(client.Protocol,client.Avatar.ID,LocalID,new LLUUID());
                       Sim.SendPacket(DwellPacket,true);
               }

               public bool Buy(SecondLife client, bool forGroup, LLUUID groupID)
               {
                       Packet buyPacket = Packets.Parcel.ParcelBuy(client.Protocol,LocalID,forGroup,groupID,true,client.Avatar.ID,client.Network.SessionID);
                       Sim.SendPacket(buyPacket,true);

                       return false;
               }

               public bool Reclaim(SecondLife client)
               {
                       Packet reclaimPacket = Packets.Parcel.ParcelReclaim(client.Protocol,LocalID,client.Avatar.ID,client.Network.SessionID);
                       Sim.SendPacket(reclaimPacket,true);

                       return false;
               }

               public bool Deed(SecondLife client, LLUUID groupID)
               {
                       Packet deedPacket = Packets.Parcel.ParcelDeedToGroup(client.Protocol,LocalID,groupID,client.Avatar.ID,client.Network.SessionID);
                       Sim.SendPacket(deedPacket,true);

                       return false;
               }

               public void Update(SecondLife client)
               {
                       Packet updatePacket = Packets.Parcel.ParcelPropertiesUpdate(client.Protocol,client.Avatar.ID,client.Network.SessionID,this);
                       Sim.SendPacket(updatePacket,true);
               }

               public void ReturnObjects(SecondLife client, int returnType, int otherCleanTime)
               {
                       Packet returnPacket = Packets.Parcel.ParcelReturnObjects(client.Protocol,client.Avatar.ID,client.Network.SessionID,LocalID,
                               returnType,otherCleanTime);
                       Sim.SendPacket(returnPacket,true);
               }

               public void ReturnObjects(SecondLife client, int returnType, int otherCleanTime, LLUUID ownerID)
               {
                       Packet returnPacket = Packets.Parcel.ParcelReturnObjects(client.Protocol,client.Avatar.ID,client.Network.SessionID,LocalID,
                               returnType,otherCleanTime,ownerID);
                       Sim.SendPacket(returnPacket,true);
               }
       }

       public class ParcelManager
       {
               public ArrayList ParcelsForSale;

               private SecondLife Client;
               private bool ReservedNewbie;
               private bool ForSale;
               private bool Auction;
               private bool Finished;
               private Timer DirLandTimer;
               private bool DirLandTimeout;
               private bool ParcelInfoTimeout;
               private DirectoryParcel ParcelInfoParcel;

               public ParcelManager(SecondLife client)
               {
                       Client = client;
                       ParcelsForSale = new ArrayList();

                       // Setup the callbacks
                       PacketCallback callback = new PacketCallback(DirLandReplyHandler);
                       Client.Network.RegisterCallback("DirLandReply", callback);
                       callback = new PacketCallback(ParcelInfoReplyHandler);
                       Client.Network.RegisterCallback("ParcelInfoReply", callback);
                       callback = new PacketCallback(ParcelPropertiesHandler);
                       Client.Network.RegisterCallback("ParcelProperties",callback);
                       callback = new PacketCallback(ParcelDwellReplyHandler);
                       Client.Network.RegisterCallback("ParcelDwellReply",callback);
               }

               public void ParcelDwellReplyHandler(Packet packet, Simulator simulator)
               {
                       int LocalID                      = 0;
                       LLUUID ParcelID          = new LLUUID();
                       float Dwell                      = 0.0f;

                       foreach( Block block in packet.Blocks())
                       {
                               foreach(Field field in block.Fields)
                               {
                                       if(field.Layout.Name == "LocalID")
                                               LocalID = (int)field.Data;
                                       else if(field.Layout.Name == "ParcelID")
                                               ParcelID = (LLUUID)field.Data;
                                       else if(field.Layout.Name == "Dwell")
                                               Dwell = (float)field.Data;
                               }
                       }
                       if(Dwell != 0.0f && simulator.Region.Parcels.ContainsKey(LocalID))
                               ((Parcel)simulator.Region.Parcels[LocalID]).Dwell = Dwell;
               }

               public bool RequestParcelInfo(DirectoryParcel parcel)
               {
                       int attempts = 0;

                       Beginning:
                               if (attempts++ > 3)
                               {
                                       return false;
                               }

                       Finished = false;
                       ParcelInfoTimeout = false;
                       ParcelInfoParcel = parcel;

                       // Setup the timer
                       Timer ParcelInfoTimer = new Timer(5000);
                       ParcelInfoTimer.Elapsed += new ElapsedEventHandler(ParcelInfoTimerEvent);
                       ParcelInfoTimeout = false;

                       // Build the ParcelInfoRequest packet
                       Packet parcelInfoPacket = Packets.Parcel.ParcelInfoRequest(Client.Protocol, parcel.ID,
                               Client.Network.AgentID, Client.Network.SessionID);

                       // Start the timer
                       ParcelInfoTimer.Start();

                       Client.Network.SendPacket(parcelInfoPacket);

                       while (!Finished)
                       {
                               if (ParcelInfoTimeout)
                               {
                                       goto Beginning;
                               }

                               Client.Tick();
                       }

                       return true;
               }

               public void ParcelPropertiesHandler(Packet packet, Simulator simulator)
               {
                       // Marked == Added to Parcel Class specifically for this Packet
                       // -> XYZ == Equivilent to property XYZ in Packet.
                       int RequestResult               = 0;
                       int SequenceID                  = 0;
                       bool SnapSelection              = false;
                       int SelfCount                   = 0;
                       int OtherCount                  = 0;
                       int PublicCount                 = 0;
                       int LocalID                             = 0;                                            // Marked
                       LLUUID OwnerID                  = new LLUUID();                         // -> OwnerID
                       bool IsGroupOwned               = false;                                        // Marked
                       uint AuctionID                  = 0;
                       bool ReservedNewbie             = false;                                        // Marked -> FirstLand
                       int ClaimDate                   = 0;                                            // Marked
                       int ClaimPrice                  = 0;
                       int RentPrice                   = 0;
                       LLVector3 AABBMin               = new LLVector3();
                       LLVector3 AABBMax               = new LLVector3();
                       byte[] Bitmap                   = new byte[512];                        // Marked
                       int Area                                = 0;                                            // -> ActualArea
                       byte Status                             = 0;
                       int SimWideMaxObjects   = 0;
                       int SimWideTotalObjects = 0;
                       int MaxObjects                  = 0;
                       int TotalObjects                = 0;
                       int OwnerObjects                = 0;
                       int GroupObjects                = 0;
                       int OtherObjects                = 0;
                       float ParcelObjectBonus = 0.0f;
                       int OtherCleanTime              = 0;
                       uint ParcelFlags                = 0;
                       int SalePrice                   = 0;                                            // -> SalePrice
                       string Name                             = "";
                       string Desc                             = "";
                       string MusicURL                 = "";
                       string MediaURL                 = "";
                       LLUUID MediaID                  = new LLUUID();
                       byte MediaAutoScale             = 0;
                       LLUUID GroupID                  = new LLUUID();
                       int PassPrice                   = 0;
                       float PassHours                 = 0.0f;
                       byte Category                   = 0;
                       LLUUID AuthBuyerID              = new LLUUID();                         // Marked
                       LLUUID SnapshotID               = new LLUUID();                         // -> SnapshotID
                       LLVector3 UserLocation  = new LLVector3();
                       LLVector3 UserLookAt    = new LLVector3();
                       byte LandingType                = 0;

                       foreach( Block block in packet.Blocks())
                       {
                               foreach(Field field in block.Fields)
                               {
                                       if(field.Layout.Name == "RequestResult")
                                               RequestResult = (int)field.Data;
                                       else if(field.Layout.Name == "SequenceID")
                                               SequenceID = (int)field.Data;
                                       else if(field.Layout.Name == "SnapSelection")
                                               SnapSelection = (bool)field.Data;
                                       else if(field.Layout.Name == "SelfCount")
                                               SelfCount = (int)field.Data;
                                       else if(field.Layout.Name == "OtherCount")
                                               OtherCount = (int)field.Data;
                                       else if(field.Layout.Name == "PublicCount")
                                               PublicCount = (int)field.Data;
                                       else if(field.Layout.Name == "LocalID")
                                               LocalID = (int)field.Data;
                                       else if(field.Layout.Name == "OwnerID")
                                               OwnerID = (LLUUID)field.Data;
                                       else if(field.Layout.Name == "IsGroupOwned")
                                               IsGroupOwned = (bool)field.Data;
                                       else if(field.Layout.Name == "AuctionID")
                                               AuctionID = (uint)field.Data;
                                       else if(field.Layout.Name == "ReservedNewbie")
                                               ReservedNewbie = (bool)field.Data;
                                       else if(field.Layout.Name == "ClaimDate")
                                               ClaimDate = (int)field.Data;
                                       else if(field.Layout.Name == "ClaimPrice")
                                               ClaimPrice = (int)field.Data;
                                       else if(field.Layout.Name == "RentPrice")
                                               RentPrice = (int)field.Data;
                                       else if(field.Layout.Name == "AABBMin")
                                               AABBMin = (LLVector3)field.Data;
                                       else if(field.Layout.Name == "AABBMax")
                                               AABBMax = (LLVector3)field.Data;
                                       else if(field.Layout.Name == "Bitmap")
                                               Bitmap = (byte[])field.Data;
                                       else if(field.Layout.Name == "Area")
                                               Area = (int)field.Data;
                                       else if(field.Layout.Name == "Status")
                                               Status = (byte)field.Data;
                                       else if(field.Layout.Name == "SimWideMaxObjects")
                                               SimWideMaxObjects = (int)field.Data;
                                       else if(field.Layout.Name == "SimWideTotalObjects")
                                               SimWideTotalObjects = (int)field.Data;
                                       else if(field.Layout.Name == "MaxObjects")
                                               MaxObjects = (int)field.Data;
                                       else if(field.Layout.Name == "TotalObjects")
                                               TotalObjects = (int)field.Data;
                                       else if(field.Layout.Name == "OwnerObjects")
                                               OwnerObjects = (int)field.Data;
                                       else if(field.Layout.Name == "GroupObjects")
                                               GroupObjects = (int)field.Data;
                                       else if(field.Layout.Name == "OtherObjects")
                                               OtherObjects = (int)field.Data;
                                       else if(field.Layout.Name == "ParcelObjectBonus")
                                               ParcelObjectBonus = (float)field.Data;
                                       else if(field.Layout.Name == "OtherCleanTime")
                                               OtherCleanTime = (int)field.Data;
                                       else if(field.Layout.Name == "ParcelFlags")
                                               ParcelFlags = (uint)field.Data;
                                       else if(field.Layout.Name == "SalePrice")
                                               SalePrice = (int)field.Data;
                                       else if(field.Layout.Name == "Name")
                                               Name = System.Text.Encoding.UTF8.GetString((byte[])field.Data).Replace("\0", "");
                                       else if(field.Layout.Name == "Desc")
                                               Desc = System.Text.Encoding.UTF8.GetString((byte[])field.Data).Replace("\0", "");
                                       else if(field.Layout.Name == "MusicURL")
                                               MusicURL = System.Text.Encoding.UTF8.GetString((byte[])field.Data).Replace("\0", "");
                                       else if(field.Layout.Name == "MediaURL")
                                               MediaURL = System.Text.Encoding.UTF8.GetString((byte[])field.Data).Replace("\0", "");
                                       else if(field.Layout.Name == "MediaID")
                                               MediaID = (LLUUID)field.Data;
                                       else if(field.Layout.Name == "MediaAutoScale")
                                               MediaAutoScale = (byte)field.Data;
                                       else if(field.Layout.Name == "GroupID")
                                               GroupID = (LLUUID)field.Data;
                                       else if(field.Layout.Name == "PassPrice")
                                               PassPrice = (int)field.Data;
                                       else if(field.Layout.Name == "PassHours")
                                               PassHours = (float)field.Data;
                                       else if(field.Layout.Name == "Category")
                                               Category = (byte)field.Data;
                                       else if(field.Layout.Name == "AuthBuyerID")
                                               AuthBuyerID = (LLUUID)field.Data;
                                       else if(field.Layout.Name == "SnapshotID")
                                               SnapshotID = (LLUUID)field.Data;
                                       else if(field.Layout.Name == "UserLocation")
                                               UserLocation = (LLVector3)field.Data;
                                       else if(field.Layout.Name == "UserLookAt")
                                               UserLookAt = (LLVector3)field.Data;
                                       else if(field.Layout.Name == "LandingType")
                                               LandingType = (byte)field.Data;
                                       //                                      else
                                       //                                              Client.Log("Unknown field type '" + field.Layout.Name + "' in ParcelProperties",Helpers.LogLevel.Warning);

                               }
                       }

                       /* Mark this area as downloaded */
                       int x, y, index, subindex;
                       byte val;

                       for(x = 0; x < 64; x++)
                       {
                               for(y = 0; y < 64; y++)
                               {
                                       if(simulator.Region.ParcelMarked[y,x] == 0)
                                       {
                                               index = ((x * 64) + y);
                                               subindex = index % 8;
                                               index /= 8;

                                               val = Bitmap[index];

                                               simulator.Region.ParcelMarked[y,x] = ((val >> subindex) & 1) == 1 ? LocalID : 0;
                                       }
                               }
                       }

                       /* Fire off the next request, if we are downloading the whole sim */
                       bool hasTriggered = false;
                       if(simulator.Region.ParcelDownloading == true)
                       {
                               for(x = 0; x < 64; x++)
                               {
                                       for(y = 0; y < 64; y++)
                                       {
                                               if(simulator.Region.ParcelMarked[x,y] == 0)
                                               {
                                                       Client.Network.SendPacket(libsecondlife.Packets.Parcel.ParcelPropertiesRequest(Client.Protocol,Client.Avatar.ID, -10000 - (x*64) - y,
                                                               (x*4.0f),(y*4.0f),(x*4.0f) + 4.0f,(y*4.0f) + 4.0f, false));
                                                       hasTriggered = true;

                                                       goto exit;
                                               }
                                       }
                               }
                       exit:;
                       }

                       /* This map is complete, fire callback */
                       if(hasTriggered == false)
                       {
                               simulator.Region.FilledParcels();
                       }

                       /* Save this parcels data */
                       // TODO: Lots of values are not being stored, Parcel needs to be expanded to take all the data.
                       simulator.Region.ParcelsMutex.WaitOne();

                       if(!simulator.Region.Parcels.ContainsKey(LocalID))
                       {
                               simulator.Region.Parcels[LocalID] = new Parcel(simulator);
                       }

                       // God help me should I have to type this out again... argh.
                       ((Parcel)simulator.Region.Parcels[LocalID]).RequestResult               = RequestResult;
                       ((Parcel)simulator.Region.Parcels[LocalID]).SequenceID                  = SequenceID;
                       ((Parcel)simulator.Region.Parcels[LocalID]).SnapSelection               = SnapSelection;
                       ((Parcel)simulator.Region.Parcels[LocalID]).SelfCount                   = SelfCount;
                       ((Parcel)simulator.Region.Parcels[LocalID]).OtherCount                  = OtherCount;
                       ((Parcel)simulator.Region.Parcels[LocalID]).PublicCount                 = PublicCount;
                       ((Parcel)simulator.Region.Parcels[LocalID]).LocalID                             = LocalID;
                       ((Parcel)simulator.Region.Parcels[LocalID]).OwnerID                             = OwnerID;
                       ((Parcel)simulator.Region.Parcels[LocalID]).IsGroupOwned                = IsGroupOwned;
                       ((Parcel)simulator.Region.Parcels[LocalID]).AuctionID                   = AuctionID;
                       ((Parcel)simulator.Region.Parcels[LocalID]).ReservedNewbie              = ReservedNewbie;
                       ((Parcel)simulator.Region.Parcels[LocalID]).ClaimDate                   = ClaimDate;
                       ((Parcel)simulator.Region.Parcels[LocalID]).ClaimPrice                  = ClaimPrice;
                       ((Parcel)simulator.Region.Parcels[LocalID]).RentPrice                   = RentPrice;
                       ((Parcel)simulator.Region.Parcels[LocalID]).AABBMin                             = AABBMin;
                       ((Parcel)simulator.Region.Parcels[LocalID]).AABBMax                             = AABBMax;
                       ((Parcel)simulator.Region.Parcels[LocalID]).Bitmap                              = Bitmap;
                       ((Parcel)simulator.Region.Parcels[LocalID]).Area                                = Area;
                       ((Parcel)simulator.Region.Parcels[LocalID]).Status                              = Status;
                       ((Parcel)simulator.Region.Parcels[LocalID]).SimWideMaxObjects   = SimWideMaxObjects;
                       ((Parcel)simulator.Region.Parcels[LocalID]).SimWideTotalObjects = SimWideTotalObjects;
                       ((Parcel)simulator.Region.Parcels[LocalID]).MaxObjects                  = MaxObjects;
                       ((Parcel)simulator.Region.Parcels[LocalID]).TotalObjects                = TotalObjects;
                       ((Parcel)simulator.Region.Parcels[LocalID]).OwnerObjects                = OwnerObjects;
                       ((Parcel)simulator.Region.Parcels[LocalID]).GroupObjects                = GroupObjects;
                       ((Parcel)simulator.Region.Parcels[LocalID]).OtherObjects                = OtherObjects;
                       ((Parcel)simulator.Region.Parcels[LocalID]).ParcelObjectBonus   = ParcelObjectBonus;
                       ((Parcel)simulator.Region.Parcels[LocalID]).OtherCleanTime              = OtherCleanTime;
                       ((Parcel)simulator.Region.Parcels[LocalID]).ParcelFlags                 = ParcelFlags;
                       ((Parcel)simulator.Region.Parcels[LocalID]).SalePrice                   = SalePrice;
                       ((Parcel)simulator.Region.Parcels[LocalID]).Name                                = Name;
                       ((Parcel)simulator.Region.Parcels[LocalID]).Desc                                = Desc;
                       ((Parcel)simulator.Region.Parcels[LocalID]).MusicURL                    = MusicURL;
                       ((Parcel)simulator.Region.Parcels[LocalID]).MediaURL                    = MediaURL;
                       ((Parcel)simulator.Region.Parcels[LocalID]).MediaID                             = MediaID;
                       ((Parcel)simulator.Region.Parcels[LocalID]).MediaAutoScale              = MediaAutoScale;
                       ((Parcel)simulator.Region.Parcels[LocalID]).GroupID                             = GroupID;
                       ((Parcel)simulator.Region.Parcels[LocalID]).PassPrice                   = PassPrice;
                       ((Parcel)simulator.Region.Parcels[LocalID]).PassHours                   = PassHours;
                       ((Parcel)simulator.Region.Parcels[LocalID]).Category                    = Category;
                       ((Parcel)simulator.Region.Parcels[LocalID]).AuthBuyerID                 = AuthBuyerID;
                       ((Parcel)simulator.Region.Parcels[LocalID]).SnapshotID                  = SnapshotID;
                       ((Parcel)simulator.Region.Parcels[LocalID]).UserLocation                = UserLocation;
                       ((Parcel)simulator.Region.Parcels[LocalID]).UserLookAt                  = UserLookAt;
                       ((Parcel)simulator.Region.Parcels[LocalID]).LandingType                 = LandingType;

                       simulator.Region.ParcelsMutex.ReleaseMutex();
               }



               private void ParcelInfoReplyHandler(Packet packet, Simulator simulator)
               {
                       string simName = "";
                       int actualArea = 0;
                       float globalX = 0.0F;
                       float globalY = 0.0F;
                       float globalZ = 0.0F;
                       LLUUID parcelID = null;
                       string name = "";
                       string desc = "";
                       int salePrice = 0;
                       LLUUID ownerID = null;
                       LLUUID snapshotID = null;
                       float dwell = 0.0F;

                       try
                       {
                               foreach (Block block in packet.Blocks())
                               {
                                       foreach (Field field in block.Fields)
                                       {
                                               if (field.Layout.Name == "ParcelID")
                                               {
                                                       parcelID = (LLUUID)field.Data;

                                                       if (!parcelID.Equals(ParcelInfoParcel.ID))
                                                       {
                                                               Client.Log("Received a ParcelInfoReply for " + parcelID.ToString() +
                                                                       ", looking for " + ParcelInfoParcel.ID.ToString(), Helpers.LogLevel.Warning);

                                                               // Build and resend the ParcelInfoRequest packet
                                                               Packet parcelInfoPacket = Packets.Parcel.ParcelInfoRequest(Client.Protocol, ParcelInfoParcel.ID,
                                                                       Client.Network.AgentID, Client.Network.SessionID);

                                                               Client.Network.SendPacket(parcelInfoPacket);

                                                               return;
                                                       }
                                               }
                                               else if (field.Layout.Name == "ActualArea")
                                               {
                                                       actualArea = (int)field.Data;
                                               }
                                               else if (field.Layout.Name == "SalePrice")
                                               {
                                                       salePrice = (int)field.Data;
                                               }
                                               else if (field.Layout.Name == "Name")
                                               {
                                                       name = System.Text.Encoding.UTF8.GetString((byte[])field.Data).Replace("\0", "");
                                               }
                                               else if (field.Layout.Name == "SimName")
                                               {
                                                       simName = System.Text.Encoding.UTF8.GetString((byte[])field.Data).Replace("\0", "");
                                               }
                                               else if (field.Layout.Name == "GlobalX")
                                               {
                                                       globalX = (float)field.Data;
                                               }
                                               else if (field.Layout.Name == "GlobalY")
                                               {
                                                       globalY = (float)field.Data;
                                               }
                                               else if (field.Layout.Name == "GlobalZ")
                                               {
                                                       globalZ = (float)field.Data;
                                               }
                                               else if (field.Layout.Name == "Desc")
                                               {
                                                       desc = System.Text.Encoding.UTF8.GetString((byte[])field.Data).Replace("\0", "");
                                               }
                                               else if (field.Layout.Name == "OwnerID")
                                               {
                                                       ownerID = (LLUUID)field.Data;
                                               }
                                               else if (field.Layout.Name == "SnapshotID")
                                               {
                                                       snapshotID = (LLUUID)field.Data;
                                               }
                                               else if (field.Layout.Name == "Dwell")
                                               {
                                                       dwell = (float)field.Data;
                                               }
                                       }
                               }

                               ParcelInfoParcel.SimName = simName;
                               ParcelInfoParcel.ActualArea = actualArea;
                               ParcelInfoParcel.GlobalPosition.X = globalX;
                               ParcelInfoParcel.GlobalPosition.Y = globalY;
                               ParcelInfoParcel.GlobalPosition.Z = globalZ;
                               ParcelInfoParcel.Name = name;
                               ParcelInfoParcel.Desc = desc;
                               ParcelInfoParcel.SalePrice = salePrice;
                               ParcelInfoParcel.OwnerID = ownerID;
                               ParcelInfoParcel.SnapshotID = snapshotID;
                               ParcelInfoParcel.Dwell = dwell;

                               // Get RegionHandle from GlobalX/GlobalY
                               uint handleX = (uint)Math.Floor(ParcelInfoParcel.GlobalPosition.X / 256.0F);
                               handleX *= 256;
                               uint handleY = (uint)Math.Floor(ParcelInfoParcel.GlobalPosition.Y / 256.0F);
                               handleY *= 256;
                               ParcelInfoParcel.RegionHandle = new U64(handleX, handleY);

                               // Get SimPosition from GlobalX/GlobalY and RegionHandle
                               ParcelInfoParcel.SimPosition.X = ParcelInfoParcel.GlobalPosition.X - (float)handleX;
                               ParcelInfoParcel.SimPosition.Y = ParcelInfoParcel.GlobalPosition.Y - (float)handleY;
                               ParcelInfoParcel.SimPosition.Z = ParcelInfoParcel.GlobalPosition.Z;
                       }
                       catch (Exception e)
                       {
                               Client.Log(e.ToString(), Helpers.LogLevel.Error);
                       }

                       Finished = true;
               }

               private void ParcelInfoTimerEvent(object source, System.Timers.ElapsedEventArgs ea)
               {
                       ParcelInfoTimeout = true;
               }

               public int DirLandRequest(bool reservedNewbie, bool forSale, bool auction)
               {
                       // Set the class-wide variables so the callback has them
                       ReservedNewbie = reservedNewbie;
                       ForSale = forSale;
                       Auction = auction;

                       // Clear the list
                       ParcelsForSale.Clear();

                       // Setup the timer
                       DirLandTimer = new Timer(15000);
                       DirLandTimer.Elapsed += new ElapsedEventHandler(DirLandTimerEvent);
                       DirLandTimeout = false;
                       DirLandTimer.Start();

                       LLUUID queryID = new LLUUID();
                       Packet landQuery = Packets.Sim.DirLandQuery(Client.Protocol, ReservedNewbie, ForSale, queryID,
                               Auction, 0, Client.Network.AgentID, Client.Network.SessionID);
                       Client.Network.SendPacket(landQuery);

                       while (!DirLandTimeout)
                       {
                               Client.Tick();
                       }

                       // Double check the timer is actually stopped
                       DirLandTimer.Stop();

                       return ParcelsForSale.Count;
               }

               private void DirLandReplyHandler(Packet packet, Simulator simulator)
               {
                       if (!DirLandTimeout)
                       {
                               // Reset the timer
                               DirLandTimer.Stop();
                               DirLandTimer.Start();

                               foreach (Block block in packet.Blocks())
                               {
                                       DirectoryParcel parcel = new DirectoryParcel();

                                       if (block.Layout.Name == "QueryReplies")
                                       {
                                               foreach (Field field in block.Fields)
                                               {
                                                       if (field.Layout.Name == "ReservedNewbie")
                                                       {
                                                               if ((bool)field.Data != ReservedNewbie)
                                                               {
                                                                       goto Skip;
                                                               }
                                                       }
                                                       else if (field.Layout.Name == "Auction")
                                                       {
                                                               if ((bool)field.Data != Auction)
                                                               {
                                                                       goto Skip;
                                                               }
                                                       }
                                                       else if (field.Layout.Name == "ForSale")
                                                       {
                                                               if ((bool)field.Data != ForSale)
                                                               {
                                                                       goto Skip;
                                                               }
                                                       }
                                                       else if (field.Layout.Name == "ParcelID")
                                                       {
                                                               parcel.ID = (LLUUID)field.Data;
                                                       }
                                                       else if (field.Layout.Name == "Name")
                                                       {
                                                               parcel.Name = System.Text.Encoding.UTF8.GetString((byte[])field.Data).Replace("\0", "");
                                                       }
                                                       else if (field.Layout.Name == "ActualArea")
                                                       {
                                                               parcel.ActualArea = (int)field.Data;
                                                       }
                                                       else if (field.Layout.Name == "SalePrice")
                                                       {
                                                               parcel.SalePrice = (int)field.Data;
                                                       }
                                               }

                                               if (parcel.ID != null)
                                               {
                                                       ParcelsForSale.Add(parcel);
                                               }
                                               else
                                               {
                                                       Client.Log("Parcel with no ID found in DirLandReply, skipping", Helpers.LogLevel.Warning);
                                               }
                                       }

                               Skip:
                                       ;
                               }
                       }
                       else
                       {
                               Console.WriteLine("Received a DirLandReply after the timeout!");
                       }
               }

               private void DirLandTimerEvent(object source, System.Timers.ElapsedEventArgs ea)
               {
                       DirLandTimer.Stop();
                       DirLandTimeout = true;
               }
       }
}
