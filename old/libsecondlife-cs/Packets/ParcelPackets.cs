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
using System.Collections;

namespace libsecondlife.Packets
{
       public class Parcel
       {
               // DataServer Based -------------------------------------------------
               public static Packet ParcelInfoRequest(ProtocolManager protocol, LLUUID parcelID,
                       LLUUID agentID, LLUUID sessionID)
               {
                       Hashtable blocks = new Hashtable();
                       Hashtable fields = new Hashtable();

                       fields["ParcelID"] = parcelID;
                       blocks[fields] = "Data";
                       fields = new Hashtable();
                       fields["AgentID"] = agentID;
                       fields["SessionID"] = sessionID;
                       blocks[fields] = "AgentData";

                       return PacketBuilder.BuildPacket("ParcelInfoRequest", protocol, blocks,
                               Helpers.MSG_RELIABLE + Helpers.MSG_ZEROCODED);
               }

               // Sim Based ---------------------------------------------------------
               public static Packet ParcelBuy(ProtocolManager protocol, int localID, bool groupOwned,
                       LLUUID groupID, bool final, LLUUID agentID, LLUUID sessionID)
               {
                       Hashtable blocks = new Hashtable();
                       Hashtable fields = new Hashtable();

                       fields["LocalID"] = localID;
                       fields["IsGroupOwned"] = groupOwned;
                       fields["GroupID"] = groupID;
                       fields["Final"] = final;
                       blocks[fields] = "Data";

                       fields = new Hashtable();
                       fields["AgentID"] = agentID;
                       fields["SessionID"] = sessionID;
                       blocks[fields] = "AgentData";

                       return PacketBuilder.BuildPacket("ParcelBuy", protocol, blocks, Helpers.MSG_RELIABLE);
               }

               public static Packet ParcelDeedToGroup(ProtocolManager protocol, int localID, LLUUID groupID,
                       LLUUID agentID, LLUUID sessionID)
               {
                       Hashtable blocks = new Hashtable();
                       Hashtable fields = new Hashtable();

                       fields["LocalID"] = localID;
                       fields["GroupID"] = groupID;
                       blocks[fields] = "Data";

                       fields = new Hashtable();
                       fields["AgentID"] = agentID;
                       fields["SessionID"] = sessionID;
                       blocks[fields] = "AgentData";

                       return PacketBuilder.BuildPacket("ParcelDeedToGroup", protocol, blocks, Helpers.MSG_RELIABLE);
               }

               public static Packet ParcelDwellRequest(ProtocolManager protocol, LLUUID agentID, int localID, LLUUID parcelID)
               {
                       Hashtable blocks = new Hashtable();
                       Hashtable fields = new Hashtable();

                       fields["LocalID"] = localID;
                       fields["ParcelID"] = parcelID;
                       blocks[fields] = "Data";

                       fields = new Hashtable();
                       fields["AgentID"] = agentID;
                       blocks[fields] = "AgentData";

                       return PacketBuilder.BuildPacket("ParcelDwellRequest", protocol, blocks, Helpers.MSG_RELIABLE);
               }

               public static Packet ParcelReclaim(ProtocolManager protocol, int localID,
                       LLUUID agentID, LLUUID sessionID)
               {
                       Hashtable blocks = new Hashtable();
                       Hashtable fields = new Hashtable();

                       fields["LocalID"] = localID;
                       blocks[fields] = "Data";

                       fields = new Hashtable();
                       fields["AgentID"] = agentID;
                       fields["SessionID"] = sessionID;
                       blocks[fields] = "AgentData";

                       return PacketBuilder.BuildPacket("ParcelReclaim", protocol, blocks, Helpers.MSG_RELIABLE);
               }

               public static Packet ParcelRelease(ProtocolManager protocol, int localID,
                       LLUUID agentID, LLUUID sessionID)
               {
                       Hashtable blocks = new Hashtable();
                       Hashtable fields = new Hashtable();

                       fields["LocalID"] = localID;
                       blocks[fields] = "Data";

                       fields = new Hashtable();
                       fields["AgentID"] = agentID;
                       fields["SessionID"] = sessionID;
                       blocks[fields] = "AgentData";

                       return PacketBuilder.BuildPacket("ParcelRelease", protocol, blocks, Helpers.MSG_RELIABLE);
               }

               public static Packet ParcelJoin(ProtocolManager protocol, LLUUID agentID,
                       float west, float south, float east, float north)
               {
                       Hashtable blocks = new Hashtable();
                       Hashtable fields = new Hashtable();

                       fields["AgentID"] = agentID;
                       fields["West"] = west;
                       fields["South"] = south;
                       fields["East"] = east;
                       fields["North"] = north;

                       blocks[fields] = "ParcelData";
                       return PacketBuilder.BuildPacket("ParcelJoin", protocol, blocks, Helpers.MSG_RELIABLE);
               }

               public static Packet ParcelDivide(ProtocolManager protocol, LLUUID agentID,
                       float west, float south, float east, float north)
               {
                       Hashtable blocks = new Hashtable();
                       Hashtable fields = new Hashtable();

                       fields["AgentID"] = agentID;
                       fields["West"] = west;
                       fields["South"] = south;
                       fields["East"] = east;
                       fields["North"] = north;

                       blocks[fields] = "ParcelData";
                       return PacketBuilder.BuildPacket("ParcelDivide", protocol, blocks, Helpers.MSG_RELIABLE);
               }

               public static Packet ParcelPropertiesRequest(ProtocolManager protocol, LLUUID agentID, int sequenceID,
                       float west, float south, float east, float north, bool snapSelection)
               {
                       Hashtable blocks = new Hashtable();
                       Hashtable fields = new Hashtable();

                       fields["AgentID"] = agentID;
                       fields["SequenceID"] = sequenceID;
                       fields["West"] = west;
                       fields["South"] = south;
                       fields["East"] = east;
                       fields["North"] = north;
                       fields["SnapSelection"] = snapSelection;

                       blocks[fields] = "ParcelData";
                       return PacketBuilder.BuildPacket("ParcelPropertiesRequest", protocol, blocks, Helpers.MSG_RELIABLE);
               }

               public static Packet ParcelPropertiesRequestByID(ProtocolManager protocol, LLUUID agentID, int sequenceID,
                       int localID)
               {
                       Hashtable blocks = new Hashtable();
                       Hashtable fields = new Hashtable();

                       fields["AgentID"] = agentID;
                       fields["SequenceID"] = sequenceID;
                       fields["LocalID"] = localID;

                       blocks[fields] = "ParcelData";
                       return PacketBuilder.BuildPacket("ParcelPropertiesRequestByID", protocol, blocks, Helpers.MSG_RELIABLE);
               }

               public static Packet ParcelPropertiesUpdate(ProtocolManager protocol, LLUUID agentID, LLUUID sessionID, libsecondlife.Parcel parcel)
               {
                       Hashtable blocks = new Hashtable();
                       Hashtable fields = new Hashtable();

                       fields["AgentID"] = agentID;
                       fields["SessionID"] = sessionID;
                       blocks[fields] = "AgentData";

                       fields = new Hashtable();
                       fields["LocalID"]                       = parcel.LocalID;
                       fields["Flags"]                         = 0; //TODO: !!IMPORTANT!! Find out what we are.
                       fields["ParcelFlags"]           = parcel.ParcelFlags;
                       fields["SalePrice"]                     = parcel.SalePrice;
                       fields["Name"]                          = parcel.Name;
                       fields["Desc"]                          = parcel.Desc;
                       fields["MusicURL"]                      = parcel.MusicURL;
                       fields["MediaURL"]                      = parcel.MediaURL;
                       fields["MediaID"]                       = parcel.MediaID;
                       fields["MediaAutoScale"]        = parcel.MediaAutoScale;
                       fields["GroupID"]                       = parcel.GroupID;
                       fields["PassPrice"]                     = parcel.PassPrice;
                       fields["PassHours"]                     = parcel.PassHours;
                       fields["Category"]                      = parcel.Category;
                       fields["AuthBuyerID"]           = parcel.AuthBuyerID;
                       fields["SnapshotID"]            = parcel.SnapshotID;
                       fields["UserLocation"]          = parcel.UserLocation;
                       fields["UserLookAt"]            = parcel.UserLookAt;
                       fields["LandingType"]           = parcel.LandingType;
                       blocks[fields] = "ParcelData";

                       return PacketBuilder.BuildPacket("ParcelPropertiesUpdate", protocol, blocks, Helpers.MSG_RELIABLE);
               }

               public static Packet ParcelReturnObjects(ProtocolManager protocol, LLUUID agentID, LLUUID sessionID, int localID, int returnType,
                       int otherCleanTime)
               {
                       Hashtable blocks = new Hashtable();
                       Hashtable fields = new Hashtable();

                       fields["AgentID"] = agentID;
                       fields["SessionID"] = sessionID;
                       blocks[fields] = "AgentData";

                       fields = new Hashtable();
                       fields["LocalID"] = localID;
                       fields["ReturnType"] = returnType;
                       fields["OtherCleanTime"] = otherCleanTime;
                       blocks[fields] = "ParcelData";

                       return PacketBuilder.BuildPacket("ParcelPropertiesRequestByID", protocol, blocks, Helpers.MSG_RELIABLE);
               }

               public static Packet ParcelReturnObjects(ProtocolManager protocol, LLUUID agentID, LLUUID sessionID, int localID, int returnType,
                       int otherCleanTime, LLUUID ownerID)
               {
                       Hashtable blocks = new Hashtable();
                       Hashtable fields = new Hashtable();

                       fields["AgentID"] = agentID;
                       fields["SessionID"] = sessionID;
                       blocks[fields] = "AgentData";

                       fields = new Hashtable();
                       fields["LocalID"] = localID;
                       fields["ReturnType"] = returnType;
                       fields["OtherCleanTime"] = otherCleanTime;
                       blocks[fields] = "ParcelData";

                       fields = new Hashtable();
                       fields["OwnerID"] = ownerID;

                       blocks[fields] = "OwnerIDs";

                       return PacketBuilder.BuildPacket("ParcelPropertiesRequestByID", protocol, blocks, Helpers.MSG_RELIABLE);
               }
       }
}
