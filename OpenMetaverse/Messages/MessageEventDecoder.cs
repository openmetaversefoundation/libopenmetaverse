﻿/*
 * Copyright (c) 2006-2014, openmetaverse.org
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without 
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the openmetaverse.org nor the names 
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
using OpenMetaverse.StructuredData;
using OpenMetaverse.Interfaces;
using OpenMetaverse.Messages.Linden;

namespace OpenMetaverse.Messages
{

    public static partial class MessageUtils
    {
        /// <summary>
        /// Return a decoded capabilities message as a strongly typed object
        /// </summary>
        /// <param name="eventName">A string containing the name of the capabilities message key</param>
        /// <param name="map">An <see cref="OSDMap"/> to decode</param>
        /// <returns>A strongly typed object containing the decoded information from the capabilities message, or null
        /// if no existing Message object exists for the specified event</returns>
        public static IMessage DecodeEvent(string eventName, OSDMap map)
        {
            IMessage message = null;

            switch (eventName)
            {
                case "AgentGroupDataUpdate": message = new AgentGroupDataUpdateMessage(); break;
                case "AvatarGroupsReply": message = new AgentGroupDataUpdateMessage(); break; // OpenSim sends the above with the wrong? key
                case "ParcelProperties": message = new ParcelPropertiesMessage(); break;
                case "ParcelObjectOwnersReply": message = new ParcelObjectOwnersReplyMessage(); break;
                case "TeleportFinish": message = new TeleportFinishMessage(); break;
                case "EnableSimulator": message = new EnableSimulatorMessage(); break;
                case "ParcelPropertiesUpdate": message = new ParcelPropertiesUpdateMessage(); break;
                case "EstablishAgentCommunication": message = new EstablishAgentCommunicationMessage(); break;
                case "ChatterBoxInvitation": message = new ChatterBoxInvitationMessage(); break;
                case "ChatterBoxSessionEventReply": message = new ChatterboxSessionEventReplyMessage(); break;
                case "ChatterBoxSessionStartReply": message = new ChatterBoxSessionStartReplyMessage(); break;
                case "ChatterBoxSessionAgentListUpdates": message = new ChatterBoxSessionAgentListUpdatesMessage(); break;
                case "RequiredVoiceVersion": message = new RequiredVoiceVersionMessage(); break;
                case "MapLayer": message = new MapLayerMessage(); break;
                case "ChatSessionRequest": message = new ChatSessionRequestMessage(); break;
                case "CopyInventoryFromNotecard": message = new CopyInventoryFromNotecardMessage(); break;
                case "ProvisionVoiceAccountRequest": message = new ProvisionVoiceAccountRequestMessage(); break;
                case "Viewerstats": message = new ViewerStatsMessage(); break;
                case "UpdateAgentLanguage": message = new UpdateAgentLanguageMessage(); break;
                case "RemoteParcelRequest": message = new RemoteParcelRequestMessage(); break;
                case "UpdateScriptTask": message = new UpdateScriptTaskMessage(); break;
                case "UpdateScriptAgent": message = new UpdateScriptAgentMessage(); break;
                case "SendPostcard": message = new SendPostcardMessage(); break;
                case "UpdateGestureAgentInventory": message = new UpdateGestureAgentInventoryMessage(); break;
                case "UpdateNotecardAgentInventory": message = new UpdateNotecardAgentInventoryMessage(); break;
                case "LandStatReply": message = new LandStatReplyMessage(); break;
                case "ParcelVoiceInfoRequest": message = new ParcelVoiceInfoRequestMessage(); break;
                case "ViewerStats": message = new ViewerStatsMessage(); break;
                case "EventQueueGet": message = new EventQueueGetMessage(); break;
                case "CrossedRegion": message = new CrossedRegionMessage(); break;
                case "TeleportFailed": message = new TeleportFailedMessage(); break;
                case "PlacesReply": message = new PlacesReplyMessage(); break;
                case "UpdateAgentInformation": message = new UpdateAgentInformationMessage(); break;
                case "DirLandReply": message = new DirLandReplyMessage(); break;
                case "ScriptRunningReply": message = new ScriptRunningReplyMessage(); break;
                case "SearchStatRequest": message = new SearchStatRequestMessage(); break;
                case "AgentDropGroup": message = new AgentDropGroupMessage(); break;
                case "AgentStateUpdate": message = new AgentStateUpdateMessage(); break;
                case "ForceCloseChatterBoxSession": message = new ForceCloseChatterBoxSessionMessage(); break;
                case "UploadBakedTexture": message = new UploadBakedTextureMessage(); break;
                case "RegionInfo": message = new RegionInfoMessage(); break;
                case "ObjectMediaNavigate": message = new ObjectMediaNavigateMessage(); break;
                case "ObjectMedia": message = new ObjectMediaMessage(); break;
                case "AttachmentResources": message = AttachmentResourcesMessage.GetMessageHandler(map); break;
                case "LandResources": message = LandResourcesMessage.GetMessageHandler(map); break;
                case "GetDisplayNames": message = new GetDisplayNamesMessage(); break;
                case "SetDisplayName": message = new SetDisplayNameMessage(); break;
                case "SetDisplayNameReply": message = new SetDisplayNameReplyMessage(); break;
                case "DisplayNameUpdate": message = new DisplayNameUpdateMessage(); break;
                //case "ProductInfoRequest": message = new ProductInfoRequestMessage(); break;
                case "ObjectPhysicsProperties": message = new ObjectPhysicsPropertiesMessage(); break;
                case "BulkUpdateInventory": message = new BulkUpdateInventoryMessage(); break;
                case "RenderMaterials": message = new RenderMaterialsMessage(); break;

                // Capabilities TODO:
                // DispatchRegionInfo
                // EstateChangeInfo
                // EventQueueGet
                // FetchInventoryDescendents
                // GroupProposalBallot
                // MapLayerGod
                // NewFileAgentInventory
                // RequestTextureDownload
                // SearchStatRequest
                // SearchStatTracking
                // SendUserReport
                // SendUserReportWithScreenshot
                // ServerReleaseNotes
                // StartGroupProposal
                // UpdateGestureTaskInventory
                // UpdateNotecardTaskInventory
                // ViewerStartAuction
                // UntrustedSimulatorMessage
            }

            if (message != null)
            {
                try
                {
                    message.Deserialize(map);
                    return message;
                }
                catch (Exception e)
                {
                    Logger.Log("Exception while trying to Deserialize " + eventName + ":" + e.Message + ": " + e.StackTrace, Helpers.LogLevel.Error);                    
                }

                return null;
            }
            else
            {
                return null;
            }
        }
    }
}
