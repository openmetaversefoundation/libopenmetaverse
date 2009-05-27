using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace OpenMetaverse.Packets
{

    public class PacketDecoder
    {
        /// <summary>
        /// A custom decoder callback
        /// </summary>
        /// <param name="fieldName">The key of the object</param>
        /// <param name="fieldData">the data to decode</param>
        /// <returns>A string represending the fieldData</returns>
        public delegate string CustomPacketDecoder(string fieldName, object fieldData);

        private static Dictionary<string, List<CustomPacketDecoder>> Callbacks = new Dictionary<string, List<CustomPacketDecoder>>();

        public PacketDecoder()
        {
            AddCallback("Color", DecodeColorField);
            AddCallback("TextColor", DecodeColorField);
            AddCallback("Timestamp", DecodeTimeStamp);
            AddCallback("EstateCovenantReply.Data.CovenantTimestamp", DecodeTimeStamp);
            AddCallback("CreationDate", DecodeTimeStamp);
            AddCallback("BinaryBucket", DecodeBinaryBucket);
            AddCallback("ParcelData.Data", DecodeBinaryToHexString);
            AddCallback("LayerData.Data", DecodeBinaryToHexString);
            AddCallback("ImageData.Data", DecodeImageData);
            AddCallback("TransferData.Data", DecodeBinaryToHexString);
            AddCallback("ObjectData.TextureEntry", DecodeTextureEntry);
            AddCallback("ImprovedInstantMessage.MessageBlock.Dialog", DecodeDialog);

            // Inventory/Permissions
            AddCallback("BaseMask", DecodePermissionMask);
            AddCallback("OwnerMask", DecodePermissionMask);
            AddCallback("EveryoneMask", DecodePermissionMask);
            AddCallback("NextOwnerMask", DecodePermissionMask);
            AddCallback("GroupMask", DecodePermissionMask);
            
            // FetchInventoryDescendents
            AddCallback("InventoryData.SortOrder", DecodeInventorySort);

            AddCallback("WearableType", DecodeWearableType);
            //
            AddCallback("InventoryData.Type", DecodeInventoryType);
            AddCallback("InvType", DecodeInventoryInvType);
            AddCallback("InventoryData.Flags", DecodeInventoryFlags);
            // BulkUpdateInventory
            AddCallback("ItemData.Type", DecodeInventoryType);
            AddCallback("ItemData.Flags", DecodeInventoryFlags);
            
            AddCallback("SaleType", DecodeObjectSaleType);

            AddCallback("ScriptControlChange.Data.Controls", DecodeScriptControls);

            AddCallback("RegionFlags", DecodeRegionFlags);
            AddCallback("SimAccess", DecodeSimAccess);
            AddCallback("ControlFlags", DecodeControlFlags);

            // ViewerEffect TypeData
            AddCallback("ViewerEffect.Effect.TypeData", DecodeViewerEffectTypeData);
            AddCallback("ViewerEffect.Effect.Type", DecodeViewerEffectType);

            // Prim/ObjectUpdate decoders
            AddCallback("ObjectUpdate.ObjectData.PCode", DecodeObjectPCode);
            AddCallback("ObjectUpdate.ObjectData.Material", DecodeObjectMaterial);
            AddCallback("ObjectUpdate.ObjectData.ClickAction", DecodeObjectClickAction);
            AddCallback("ObjectData.UpdateFlags", DecodeObjectUpdateFlags);

            AddCallback("ObjectUpdate.ObjectData.ObjectData", DecodeObjectData);
            AddCallback("TextureAnim", DecodeObjectTextureAnim);
            AddCallback("ObjectUpdate.ObjectData.NameValue", DecodeNameValue);
            AddCallback("ObjectUpdate.ObjectData.Data", DecodeObjectData);
            
            AddCallback("ObjectUpdate.ObjectData.PSBlock", DecodeObjectParticleSystem);
            AddCallback("ParticleSys", DecodeObjectParticleSystem);
            AddCallback("ObjectUpdate.ObjectData.ExtraParams", DecodeObjectExtraParams);

            AddCallback("ImprovedTerseObjectUpdate.ObjectData.Data", DecodeTerseUpdate);
            AddCallback("ImprovedTerseObjectUpdate.ObjectData.TextureEntry", DecodeTerseTextureEntry);

            AddCallback("ObjectUpdateCompressed.ObjectData.Data", DecodeObjectCompressedData);

            // ChatFromSimulator 
            AddCallback("ChatData.SourceType", DecodeChatSourceType);
            AddCallback("ChatData.ChatType", DecodeChatChatType);
            AddCallback("ChatData.Audible", DecodeChatAudible);
            AddCallback("AttachedSound.DataBlock.Flags", DecodeAttachedSoundFlags);

            AddCallback("RequestImage.Type", DecodeImageType);

            AddCallback("EstateOwnerMessage.ParamList.Parameter", DecodeEstateParameter);

            //AddCallback("ScriptDialog.Buttons.ButtonLabel", DecodeGenericByteArrayToFormattedString);
            //AddCallback("FirstName", DecodeGenericByteArrayToFormattedString);
            //AddCallback("LastName", DecodeGenericByteArrayToFormattedString);
            //AddCallback("GroupData.Title", DecodeGenericByteArrayToFormattedString);
            AddCallback("Codec", DecodeImageCodec);
            AddCallback("Info.TeleportFlags", DecodeTeleportFlags);

            // map
            AddCallback("MapBlockRequest.AgentData.Flags", DecodeMapRequestFlags);
            AddCallback("MapItemRequest.AgentData.Flags", DecodeMapRequestFlags);
            AddCallback("MapBlockReply.Data.Access", DecodeMapAccess);
            AddCallback("FolderData.Type", DecodeFolderType);
            AddCallback("RequestData.ItemType", DecodeGridItemType);

            // TransferRequest/TransferInfo
            AddCallback("TransferInfo.Params", DecodeTransferParams);
            AddCallback("TransferInfo.ChannelType", DecodeTransferChannelType);
            AddCallback("TransferInfo.SourceType", DecodeTransferSourceType);
            AddCallback("TransferInfo.TargetType", DecodeTransferTargetType);
            AddCallback("TransferData.ChannelType", DecodeTransferChannelType);
            // Directory Manager
            AddCallback("QueryData.QueryFlags", DecodeDirQueryFlags);
            AddCallback("Category", DecodeCategory);
            AddCallback("QueryData.SearchType", SearchTypeFlags);

            AddCallback("ParcelAccessListRequest.Data.Flags", DecodeParcelACL);
            AddCallback("ParcelAccessListReply.Data.Flags", DecodeParcelACL);
            //AddCallback("ParcelAccessListReply.List.Flags", DecodeParcelACLReply);
        }

        /// <summary>
        /// Add a custom decoder callback
        /// </summary>
        /// <param name="key">The key of the field to decode</param>
        /// <param name="customPacketHandler">The custom decode handler</param>
        public void AddCallback(string key, CustomPacketDecoder customPacketHandler)
        {
            if (Callbacks.ContainsKey(key))
            {
                lock (Callbacks)
                    Callbacks[key].Add(customPacketHandler);
            }
            else
            {
                lock (Callbacks)
                    Callbacks.Add(key, new List<CustomPacketDecoder>() { customPacketHandler });
            }
        }

        /// <summary>
        /// Remove a custom decoder callback
        /// </summary>
        /// <param name="key">The key of the field to decode</param>
        /// <param name="customPacketHandler">The custom decode handler</param>
        public void RemoveCustomHandler(string key, CustomPacketDecoder customPacketHandler)
        {
            if (Callbacks.ContainsKey(key))
                lock (Callbacks)
                {
                    if (Callbacks[key].Contains(customPacketHandler))
                        Callbacks[key].Remove(customPacketHandler);
                }
        }

        #region Custom Decoders

        private static string DecodeTerseUpdate(string fieldName, object fieldData)
        {
            byte[] block = (byte[]) fieldData;
            int i = 4;
            
            StringBuilder result = new StringBuilder();

            // LocalID
            result.AppendFormat("{0,30}: {1,-40} [{2}]" + Environment.NewLine,
                                        "LocalID",
                                        Utils.BytesToUInt(block, 0),
                                        "Uint32");

            
            
            // State
            result.AppendFormat("{0,30}: {1,-40} [{2}]" + Environment.NewLine,
                                        "State",
                                        block[i++],
                                        "Byte");

            // Avatar boolean
            bool isAvatar = (block[i++] != 0);
            result.AppendFormat("{0,30}: {1,-40} [{2}]" + Environment.NewLine,
                                        "IsAvatar",
                                        isAvatar,
                                        "Boolean");
            
            // Collision normal for avatar
            if (isAvatar)
            {
                result.AppendFormat("{0,30}: {1,-40} [{2}]" + Environment.NewLine,
                                        "CollisionPlane",
                                        new Vector4(block, i),
                                        "Vector4");
            
                i += 16;
            }

            // Position
            result.AppendFormat("{0,30}: {1,-40} [{2}]" + Environment.NewLine,
                                        "Position",
                                        new Vector3(block, i),
                                        "Vector3");
            i += 12;

            // Velocity
            result.AppendFormat("{0,30}: {1,-40} [{2}]" + Environment.NewLine,
                                        "Velocity",
                                        new Vector3(
                Utils.UInt16ToFloat(block, i, -128.0f, 128.0f),
                Utils.UInt16ToFloat(block, i + 2, -128.0f, 128.0f),
                Utils.UInt16ToFloat(block, i + 4, -128.0f, 128.0f)),
                                        "Vector3");
            i += 6;

            // Acceleration
            result.AppendFormat("{0,30}: {1,-40} [{2}]" + Environment.NewLine,
                                        "Acceleration",
                                        new Vector3(
                Utils.UInt16ToFloat(block, i, -64.0f, 64.0f),
                Utils.UInt16ToFloat(block, i + 2, -64.0f, 64.0f),
                Utils.UInt16ToFloat(block, i + 4, -64.0f, 64.0f)),
                                        "Vector3");

            i += 6;
            // Rotation (theta)
            result.AppendFormat("{0,30}: {1,-40} [{2}]" + Environment.NewLine,
                                        "Rotation",
                              new Quaternion(
                Utils.UInt16ToFloat(block, i, -1.0f, 1.0f),
                Utils.UInt16ToFloat(block, i + 2, -1.0f, 1.0f),
                Utils.UInt16ToFloat(block, i + 4, -1.0f, 1.0f),
                Utils.UInt16ToFloat(block, i + 6, -1.0f, 1.0f)),
                                        "Quaternion");
            i += 8;
            // Angular velocity (omega)
            result.AppendFormat("{0,30}: {1,-40} [{2}]",
                                        "AngularVelocity",
                              new Vector3(
                Utils.UInt16ToFloat(block, i, -64.0f, 64.0f),
                Utils.UInt16ToFloat(block, i + 2, -64.0f, 64.0f),
                Utils.UInt16ToFloat(block, i + 4, -64.0f, 64.0f)),
                                        "Vector3");
            //pos += 6;
            // TODO:  What is in these 6 bytes?
            return result.ToString();
        }

        private static string DecodeObjectCompressedData(string fieldName, object fieldData)
        {
            StringBuilder result = new StringBuilder();
            byte[] block = (byte[])fieldData;
            int i = 0;

            // UUID
            result.AppendFormat("{0,30}: {1,-40} [{2}]" + Environment.NewLine,
                                        "ID",
                                        new UUID(block, 0),
                                        "UUID");
            i += 16;

            // Local ID
            uint LocalID = (uint)(block[i++] + (block[i++] << 8) +
                                   (block[i++] << 16) + (block[i++] << 24));

            result.AppendFormat("{0,30}: {1,-40} [{2}]" + Environment.NewLine,
                                        "LocalID",
                                        LocalID,
                                        "Uint32");
            // PCode
            PCode pcode = (PCode)block[i++];

            result.AppendFormat("{0,30}: {1,-2} {2,-37} [{3}]" + Environment.NewLine,
                "PCode",
                (int)pcode,
                "(" + pcode + ")",
                "PCode");

            // State
            result.AppendFormat("{0,30}: {1,-40} [{2}]" + Environment.NewLine,
                                        "State",
                                        block[i++],
                                        "Byte");

            // TODO: CRC

            i += 4;
            // Material
            result.AppendFormat("{0,30}: {1,-2} {2,-37} [{3}]" + Environment.NewLine,
                "Material",
                block[i],
                "(" + (Material)block[i++] + ")",
                "Material");

            // Click action
            result.AppendFormat("{0,30}: {1,-2} {2,-37} [{3}]" + Environment.NewLine,
                "ClickAction",
                block[i],
                "(" + (ClickAction)block[i++] + ")",
                "ClickAction");

            // Scale
            result.AppendFormat("{0,30}: {1,-40} [{2}]" + Environment.NewLine,
                                        "Scale",
                                        new Vector3(block, i),
                                        "Vector3");
            i += 12;

            // Position
            result.AppendFormat("{0,30}: {1,-40} [{2}]" + Environment.NewLine,
                                        "Position",
                                        new Vector3(block, i),
                                        "Vector3");
            i += 12;

            // Rotation
            result.AppendFormat("{0,30}: {1,-40} [{2}]" + Environment.NewLine,
                            "Rotation",
                            new Vector3(block, i),
                            "Vector3");

            i += 12;
            // Compressed flags
            CompressedFlags flags = (CompressedFlags)Utils.BytesToUInt(block, i);
            result.AppendFormat("{0,30}: {1,-10} {2,-29} [{3}]" + Environment.NewLine,
                            "CompressedFlags",
                            Utils.BytesToUInt(block, i),
                            "(" + (CompressedFlags)Utils.BytesToUInt(block, i) + ")",
                            "UInt");
            i += 4;

            // Owners ID
            result.AppendFormat("{0,30}: {1,-40} [{2}]" + Environment.NewLine,
                                        "OwnerID",
                                        new UUID(block, i),
                                        "UUID");
            i += 16;

            // Angular velocity
            if ((flags & CompressedFlags.HasAngularVelocity) != 0)
            {
                result.AppendFormat("{0,30}: {1,-40} [{2}]" + Environment.NewLine,
                "AngularVelocity",
                new Vector3(block, i),
                "Vector3");
                i += 12;
            }

            // Parent ID
            if ((flags & CompressedFlags.HasParent) != 0)
            {
                result.AppendFormat("{0,30}: {1,-40} [{2}]" + Environment.NewLine,
                "ParentID",
                (uint)(block[i++] + (block[i++] << 8) +
                                        (block[i++] << 16) + (block[i++] << 24)),
                "UInt");
            }

            // Tree data
            if ((flags & CompressedFlags.Tree) != 0)
            {
                result.AppendFormat("{0,30}: {1,-2} {2,-37} [{3}]" + Environment.NewLine,
                "TreeSpecies",
                block[i++],
                "(" + (Tree)block[i] + ")",
                "Tree");
            }

            // Scratch pad
            else if ((flags & CompressedFlags.ScratchPad) != 0)
            {
                int size = block[i++];
                byte[] scratch = new byte[size];
                Buffer.BlockCopy(block, i, scratch, 0, size);
                result.AppendFormat("{0,30}: {1,-40} [ScratchPad[]]" + Environment.NewLine,
                "ScratchPad",
                Utils.BytesToHexString(scratch, String.Format("{0,30}", "Data")));
                i += size;
            }

            // Floating text
            if ((flags & CompressedFlags.HasText) != 0)
            {
                string text = String.Empty;
                while (block[i] != 0)
                {
                    text += (char)block[i];
                    i++;
                }
                i++;

                // Floating text
                result.AppendFormat("{0,30}: {1,-40} [{2}]" + Environment.NewLine,
                "Text",
                text,
                "string");

                // Text color
                result.AppendFormat("{0,30}: {1,-40} [{2}]" + Environment.NewLine,
                "TextColor",
                new Color4(block, i, false),
                "Color4");
                i += 4;
            }

            // Media URL
            if ((flags & CompressedFlags.MediaURL) != 0)
            {
                string text = String.Empty;
                while (block[i] != 0)
                {
                    text += (char)block[i];
                    i++;
                }
                i++;

                result.AppendFormat("{0,30}: {1,-40} [{2}]" + Environment.NewLine,
                    "MediaURL",
                    text,
                    "string");
            }

            // Particle system
            if ((flags & CompressedFlags.HasParticles) != 0)
            {
                Primitive.ParticleSystem p = new Primitive.ParticleSystem(block, i);
                result.AppendLine(DecodeObjectParticleSystem("ParticleSystem", p));
                i += 86;
            }

            // Extra parameters TODO:
            Primitive prim = new Primitive();
            i += prim.SetExtraParamsFromBytes(block, i);

            //Sound data
            if ((flags & CompressedFlags.HasSound) != 0)
            {
                result.AppendFormat("{0,30}: {1,-40} [{2}]" + Environment.NewLine,
                    "SoundID",
                    new UUID(block, i),
                    "UUID");
                i += 16;

                result.AppendFormat("{0,30}: {1,-40} [{2}]" + Environment.NewLine,
                    "SoundGain",
                    Utils.BytesToFloat(block, i),
                    "Float");
                i += 4;

                result.AppendFormat("{0,30}: {1,-2} {2,-37} [{3}]" + Environment.NewLine,
                "SoundFlags",
                block[i++],
                "(" + (SoundFlags)block[i] + ")",
                "SoundFlags");

                result.AppendFormat("{0,30}: {1,-40} [{2}]" + Environment.NewLine,
                "SoundRadius",
                Utils.BytesToFloat(block, i),
                "Float");
                i += 4;
            }

            // Name values
            if ((flags & CompressedFlags.HasNameValues) != 0)
            {
                string text = String.Empty;
                while (block[i] != 0)
                {
                    text += (char)block[i];
                    i++;
                }
                i++;

                // Parse the name values
                if (text.Length > 0)
                {
                    string[] lines = text.Split('\n');
                    NameValue[] nameValues = new NameValue[lines.Length];

                    for (int j = 0; j < lines.Length; j++)
                    {
                        if (!String.IsNullOrEmpty(lines[j]))
                        {
                            NameValue nv = new NameValue(lines[j]);
                            nameValues[j] = nv;
                        }
                    }
                    DecodeNameValue("NameValues", nameValues);
                }
            }

            result.AppendFormat("{0,30}: {1,-2} {2,-37} [{3}]" + Environment.NewLine,
                "PathCurve",
                block[i],
                "(" + (PathCurve)block[i++] + ")",
                "PathCurve");

            ushort pathBegin = Utils.BytesToUInt16(block, i);
            i += 2;
            result.AppendFormat("{0,30}: {1,-40} [{2}]" + Environment.NewLine,
                "PathBegin",
                Primitive.UnpackBeginCut(pathBegin),
                "float");

            ushort pathEnd = Utils.BytesToUInt16(block, i);
            i += 2;
            result.AppendFormat("{0,30}: {1,-40} [{2}]" + Environment.NewLine,
                "PathEnd",
                Primitive.UnpackEndCut(pathEnd),
                "float");

            result.AppendFormat("{0,30}: {1,-40} [{2}]" + Environment.NewLine,
                "PathScaleX",
                Primitive.UnpackPathScale(block[i++]),
                "float");

            result.AppendFormat("{0,30}: {1,-40} [{2}]" + Environment.NewLine,
                "PathScaleY",
                Primitive.UnpackPathScale(block[i++]),
                "float");
            
            result.AppendFormat("{0,30}: {1,-40} [{2}]" + Environment.NewLine,
                            "PathShearX",
                            Primitive.UnpackPathShear((sbyte)block[i++]),
                            "float");

            result.AppendFormat("{0,30}: {1,-40} [{2}]" + Environment.NewLine,
                "PathShearY",
                Primitive.UnpackPathShear((sbyte)block[i++]),
                "float");

            result.AppendFormat("{0,30}: {1,-40} [{2}]" + Environment.NewLine,
                "PathTwist",
                Primitive.UnpackPathTwist((sbyte)block[i++]),
                "float");

            result.AppendFormat("{0,30}: {1,-40} [{2}]" + Environment.NewLine,
                "PathTwistBegin",
                Primitive.UnpackPathTwist((sbyte)block[i++]),
                "float");

            result.AppendFormat("{0,30}: {1,-40} [{2}]" + Environment.NewLine,
                "PathRadiusOffset",
                Primitive.UnpackPathTwist((sbyte)block[i++]),
                "float");

            result.AppendFormat("{0,30}: {1,-40} [{2}]" + Environment.NewLine,
                "PathTaperX",
                Primitive.UnpackPathTaper((sbyte)block[i++]),
                "float");

            result.AppendFormat("{0,30}: {1,-40} [{2}]" + Environment.NewLine,
                "PathTaperY",
                Primitive.UnpackPathTaper((sbyte)block[i++]),
                "float");

            result.AppendFormat("{0,30}: {1,-40} [{2}]" + Environment.NewLine,
                "PathRevolutions",
                Primitive.UnpackPathRevolutions(block[i++]),
                "float");

            result.AppendFormat("{0,30}: {1,-40} [{2}]" + Environment.NewLine,
                "PathSkew",
                Primitive.UnpackPathTwist((sbyte)block[i++]),
                "float");

            result.AppendFormat("{0,30}: {1,-40} [{2}]" + Environment.NewLine,
                "ProfileCurve",
                block[i++],
                "float");

            ushort profileBegin = Utils.BytesToUInt16(block, i);
            i += 2;
            result.AppendFormat("{0,30}: {1,-40} [{2}]" + Environment.NewLine,
                "ProfileBegin",
                Primitive.UnpackBeginCut(profileBegin),
                "float");

            ushort profileEnd = Utils.BytesToUInt16(block, i);
            i += 2;
            result.AppendFormat("{0,30}: {1,-40} [{2}]" + Environment.NewLine,
                "ProfileEnd",
                Primitive.UnpackEndCut(profileEnd),
                "float");

            ushort profileHollow = Utils.BytesToUInt16(block, i);
            i += 2;
            result.AppendFormat("{0,30}: {1,-40} [{2}]" + Environment.NewLine,
                "ProfileHollow",
                Primitive.UnpackProfileHollow(profileHollow),
                "float");

            int textureEntryLength = (int)Utils.BytesToUInt(block, i);
            i += 4;
            //prim.Textures = new Primitive.TextureEntry(block, i, textureEntryLength);
            String s = DecodeTextureEntry("TextureEntry", new Primitive.TextureEntry(block, i, textureEntryLength));
            result.AppendLine(s);
            i += textureEntryLength;

            // Texture animation
            if ((flags & CompressedFlags.TextureAnimation) != 0)
            {
                i += 4;
                string a = DecodeObjectTextureAnim("TextureAnimation", new Primitive.TextureAnimation(block, i));
                result.AppendLine(a);
            }

            return result.ToString();
        }

        private static string DecodeObjectData(string fieldName, object fieldData)
        {
            byte[] data = (byte[])fieldData;
            if (data.Length == 1)
            {
                return String.Format("{0,30}: {1,2} {2,-38} [{3}]",
                fieldName + " (Tree Species)",
                fieldData,
                "(" + (Tree)(byte)fieldData + ")",
                fieldData.GetType().Name);
            }
            else if (data.Length == 60)
            {
                /* TODO: these are likely useful packed fields,
                 * need to unpack them */
                return Utils.BytesToHexString((byte[])fieldData, String.Format("{0,30}", fieldName));
            }
            else
            {
                return Utils.BytesToHexString((byte[])fieldData, String.Format("{0,30}", fieldName));
            }
        }

        private static string DecodeObjectTextureAnim(string fieldName, object fieldData)
        {
            StringBuilder result = new StringBuilder();
            Primitive.TextureAnimation TextureAnim;
            if (fieldData is Primitive.TextureAnimation)
                TextureAnim = (Primitive.TextureAnimation)fieldData;
            else
                TextureAnim = new Primitive.TextureAnimation((byte[])fieldData, 0);

            result.AppendFormat("{0,30}", " <TextureAnimation>" + Environment.NewLine);
            GenericTypeDecoder(TextureAnim, ref result);
            result.AppendFormat("{0,30}", "</TextureAnimation>");

            return result.ToString();

        }

        private static string DecodeGenericByteArrayToFormattedString(string fieldName, object fieldData)
        {
                return String.Format("{0,30}: {1,-40} [String]", fieldName, Utils.BytesToString((byte[]) fieldData));   
        }

        private static string DecodeEstateParameter(string fieldName, object fieldData)
        {
            byte[] bytes = (byte[])fieldData;

            if (bytes.Length == 17)
            {
                return String.Format("{0,30}: {1,-40} [UUID]", fieldName, new UUID((byte[])fieldData, 0));
            }
            else
            {
                return String.Format("{0,30}: {1,-40} [Byte[]]", fieldName, Utils.BytesToString((byte[])fieldData));
            }
        }

        private static string DecodeNameValue(string fieldName, object fieldData)
        {
            string nameValue = Utils.BytesToString((byte[])fieldData);
            NameValue[] nameValues = null;
            if (nameValue.Length > 0)
            {
                string[] lines = nameValue.Split('\n');
                nameValues = new NameValue[lines.Length];

                for (int i = 0; i < lines.Length; i++)
                {
                    if (!String.IsNullOrEmpty(lines[i]))
                    {
                        NameValue nv = new NameValue(lines[i]);
                        nameValues[i] = nv;
                    }
                }
            }

            StringBuilder result = new StringBuilder();
            result.AppendFormat("{0,30}", " <NameValues>" + Environment.NewLine);
            if (nameValues != null)
            {
                for (int i = 0; i < nameValues.Length; i++)
                {
                    result.AppendFormat(
                        "{0,30}: Name={1} Value={2} Class={3} Type={4} Sendto={5}" + Environment.NewLine, "NameValue",
                        nameValues[i].Name, nameValues[i].Value, nameValues[i].Class, nameValues[i].Type, nameValues[i].Sendto);
                }
            }
            result.AppendFormat("{0,30}", "</NameValues>");
            return result.ToString();
        }

        private static string DecodeObjectExtraParams(string fieldName, object fieldData)
        {

            byte[] data = (byte[])fieldData;

            int i = 0;
            //int totalLength = 1;

            Primitive.FlexibleData Flexible = null;
            Primitive.LightData Light = null;
            Primitive.SculptData Sculpt = null;

            byte extraParamCount = data[i++];

            for (int k = 0; k < extraParamCount; k++)
            {
                ExtraParamType type = (ExtraParamType)Utils.BytesToUInt16(data, i);
                i += 2;

                uint paramLength = Utils.BytesToUInt(data, i);
                i += 4;

                if (type == ExtraParamType.Flexible)
                    Flexible = new Primitive.FlexibleData(data, i);
                else if (type == ExtraParamType.Light)
                    Light = new Primitive.LightData(data, i);
                else if (type == ExtraParamType.Sculpt)
                    Sculpt = new Primitive.SculptData(data, i);

                i += (int)paramLength;
                //totalLength += (int)paramLength + 6;
            }

            StringBuilder result = new StringBuilder();
            result.AppendFormat("{0,30}", "<ExtraParams>" + Environment.NewLine);
            if (Flexible != null)
            {
                result.AppendFormat("{0,30}", "<Flexible>" + Environment.NewLine);
                GenericTypeDecoder(Flexible, ref result);
                result.AppendFormat("{0,30}", "</Flexible>" + Environment.NewLine);
            }

            if (Sculpt != null)
            {
                result.AppendFormat("{0,30}", "<Sculpt>" + Environment.NewLine);
                GenericTypeDecoder(Sculpt, ref result);
                result.AppendFormat("{0,30}", "</Sculpt>" + Environment.NewLine);
            }

            if (Light != null)
            {
                result.AppendFormat("{0,30}", "<Light>" + Environment.NewLine);
                GenericTypeDecoder(Light, ref result);
                result.AppendFormat("{0,30}", "</Light>" + Environment.NewLine);
            }

            result.AppendFormat("{0,30}", "</ExtraParams>");
            return result.ToString();
        }

        private static string DecodeObjectParticleSystem(string fieldName, object fieldData)
        {
            StringBuilder result = new StringBuilder();
            Primitive.ParticleSystem ParticleSys;
            if (fieldData is Primitive.ParticleSystem)
                ParticleSys = (Primitive.ParticleSystem)fieldData;
            else
                ParticleSys = new Primitive.ParticleSystem((byte[])fieldData, 0);

            result.AppendFormat("{0,30}", "<ParticleSystem>" + Environment.NewLine);
            GenericTypeDecoder(ParticleSys, ref result);
            result.AppendFormat("{0,30}", "</ParticleSystem>");

            return result.ToString();
        }

        private static void GenericTypeDecoder(object obj, ref StringBuilder result)
        {
            FieldInfo[] fields = obj.GetType().GetFields();

            foreach (FieldInfo field in fields)
            {
                String special;
                if (SpecialDecoder("a" + "." + "b" + "." + field.Name,
                    field.GetValue(obj), out special))
                {
                    result.AppendLine(special);
                }
                else
                {
                    result.AppendFormat("{0,30}: {1,-40} [{2}]" + System.Environment.NewLine, 
                        field.Name,
                        field.GetValue(obj), 
                        field.FieldType.Name);
                }
            }
        }

        private static string DecodeObjectPCode(string fieldName, object fieldData)
        {
            return String.Format("{0,30}: {1,2} {2,-37} [PCode]",
                fieldName,
                fieldData,
                "(" + (PCode)(byte)fieldData + ")");
        }

        private static string DecodeImageType(string fieldName, object fieldData)
        {
            return String.Format("{0,30}: {1,-2} {2,-37} [ImageType]",
                fieldName,
                fieldData,
                "(" + (ImageType)(byte)fieldData + ")");
        }

        private static string DecodeImageCodec(string fieldName, object fieldData)
        {
            return String.Format("{0,30}: {1,-2} {2,-37} [ImageCodec]",
                fieldName,
                fieldData,
                "(" + (ImageCodec)(byte)fieldData + ")");
        }

        private static string DecodeObjectMaterial(string fieldName, object fieldData)
        {
            return String.Format("{0,30}: {1,-2} {2,-37} [Material]",
                fieldName,
                fieldData,
                "(" + (Material)(byte)fieldData + ")");
        }

        private static string DecodeObjectClickAction(string fieldName, object fieldData)
        {
            return String.Format("{0,30}: {1,-2} {2,-37} [ClickAction]",
                fieldName,
                fieldData,
                "(" + (ClickAction)(byte)fieldData + ")");
        }

        private static string DecodeDirQueryFlags(string fieldName, object fieldData)
        {
            return String.Format("{0,30}: {1,-10} {2,-29} [DirectoryManager.DirFindFlags]",
                fieldName,
                fieldData,
                "(" + (DirectoryManager.DirFindFlags)(uint)fieldData + ")");
        }

        private static string DecodeParcelACL(string fieldName, object fieldData)
        {
            return String.Format("{0,30}: {1,-10} {2,-29} [AccessList]",
                fieldName,
                fieldData,
                "(" + (AccessList)(uint)fieldData + ")");
        }

        private static string DecodeParcelACLReply(string fieldName, object fieldData)
        {
            return String.Format("{0,30}: {1,-10} {2,-29} [ParcelAccessFlags]",
                fieldName,
                fieldData,
                "(" + (ParcelAccessFlags)(uint)fieldData + ")");
        }
        private static string SearchTypeFlags(string fieldName, object fieldData)
        {
            return String.Format("{0,30}: {1,-10} {2,-29} [DirectoryManager.SearchTypeFlags]",
                fieldName,
                fieldData,
                "(" + (DirectoryManager.SearchTypeFlags)(uint)fieldData + ")");
        }

        private static string DecodeCategory(string fieldName, object fieldData)
        {
            return String.Format("{0,30}: {1,-2} {2,-37} [ParcelCategory]",
                fieldName,
                fieldData,
                "(" + (ParcelCategory)(uint)fieldData + ")");
        }

        private static string DecodeObjectUpdateFlags(string fieldName, object fieldData)
        {
            return String.Format("{0,30}: {1,-10} {2,-29} [PrimFlags]",
                fieldName,
                fieldData,
                "(" + (PrimFlags)(uint)fieldData + ")");
        }

        private static string DecodeTeleportFlags(string fieldName, object fieldData)
        {
            return String.Format("{0,30}: {1,-10} {2,-29} [TeleportFlags]",
                fieldName,
                fieldData,
                "(" + (TeleportFlags)(uint)fieldData + ")");
        }

        private static string DecodeScriptControls(string fieldName, object fieldData)
        {
            return String.Format("{0,30}: {1,-10} {2,-29} [AgentManager.ControlFlags]",
                fieldName,
                (uint)fieldData,
                "(" + (AgentManager.ControlFlags)(uint)fieldData + ")");
        }

        private static string DecodeColorField(string fieldName, object fieldData)
        {
            return String.Format("{0,30}: {1,-40} [Color4]",
                fieldName,
                fieldData.GetType().Name.Equals("Color4") ? (Color4)fieldData : new Color4((byte[])fieldData, 0, false));
        }

        private static string DecodeTimeStamp(string fieldName, object fieldData)
        {
            if (fieldData is Int32 && (int)fieldData > 0)
                return String.Format("{0,30}: {1,-10} {2,-29} [{3}]",
                    fieldName,
                    fieldData,
                    "(" + Utils.UnixTimeToDateTime((int)fieldData) + ")",
                    fieldData.GetType().Name);
            else if (fieldData is uint && (uint)fieldData > 0)
                return String.Format("{0,30}: {1,-10} {2,-29} [{3}]",
                    fieldName,
                    fieldData,
                    "(" + Utils.UnixTimeToDateTime((uint)fieldData) + ")",
                    fieldData.GetType().Name);
            else
                return String.Format("{0,30}: {1,-40} [{2}]",
                                     fieldName,
                                     fieldData,
                                     fieldData.GetType().Name);
        }

        private static string DecodeBinaryBucket(string fieldName, object fieldData)
        {
            byte[] bytes = (byte[])fieldData;
            string bucket = String.Empty;
            if (bytes.Length == 1)
            {
                bucket = String.Format("{0}", bytes[0]);
            }
            else if (bytes.Length == 17)
            {
                bucket = String.Format("{0} {1,-40} ({2}) [Byte[]]",
                    new UUID(bytes, 1),
                    bytes[0],
                    (AssetType)bytes[0]);
            }
            else
            {
                bucket = Utils.BytesToString(bytes);
            }
            return String.Format("{0,30}: {1,-40} [Byte[]]", fieldName, bucket);
        }

        private static string DecodeBinaryToHexString(string fieldName, object fieldData)
        {
            return String.Format("{0,30}",
                                Utils.BytesToHexString((byte[])fieldData,
                                String.Format("{0,30}", fieldName)));
        }

        private static string DecodeWearableType(string fieldName, object fieldData)
        {
            return String.Format("{0,30}: {1,-2} {2,-37} [WearableType]",
                fieldName,
                (byte)fieldData,
                "(" + (WearableType)fieldData + ")");
        }

        private static string DecodeInventoryType(string fieldName, object fieldData)
        {
            return String.Format("{0,30}: {1,-2} {2,-37} [AssetType]",
                                 fieldName,
                                 (sbyte)fieldData,
                                 "(" + (AssetType)(sbyte)fieldData + ")");
        }

        private static string DecodeInventorySort(string fieldName, object fieldData)
        {
            return String.Format("{0,30}: {1,-2} {2,-37} [InventorySortOrder]",
                                 fieldName,
                                 fieldData,
                                 "(" + (InventorySortOrder)(int)fieldData + ")");
        }

        private static string DecodeInventoryInvType(string fieldName, object fieldData)
        {
            return String.Format("{0,30}: {1,-2} {2,-37} [InventoryType]",
                                 fieldName,
                                 (sbyte)fieldData,
                                 "(" + (InventoryType)fieldData + ")");
        }

        private static string DecodeFolderType(string fieldName, object fieldData)
        {
            return String.Format("{0,30}: {1,-2} {2,-37} [AssetType]",
                                 fieldName,
                                 (sbyte)fieldData,
                                 "(" + (AssetType)fieldData + ")");
        }

        private static string DecodeInventoryFlags(string fieldName, object fieldData)
        {
            return String.Format("{0,30}: {1,-2} {2,-37} [FixMe]",
                                 fieldName,
                                  fieldData,
                                 "(" + fieldData + ")");
        }

        private static string DecodeObjectSaleType(string fieldName, object fieldData)
        {
            return String.Format("{0,30}: {1,-2} {2,-37} [SaleType]",
                                 fieldName,
                                 (byte)fieldData,
                                 "(" + (SaleType)fieldData + ")");
        }

        private static string DecodeRegionFlags(string fieldName, object fieldData)
        {
            return String.Format("{0,30}: {1,-2} {2,-37} [RegionFlags]",
                fieldName,
                fieldData,
                "(" + (RegionFlags)(uint)fieldData + ")");

        }

        private static string DecodeTransferParams(string fieldName, object fieldData)
        {
            byte[] paramData = (byte[])fieldData;
            StringBuilder result = new StringBuilder();
            result.AppendLine(" <Params>");
            if (paramData.Length == 20)
            {
                result.AppendFormat("{0,30}: {1,-40} [UUID]" + Environment.NewLine,
                                    "AssetID",
                                    new UUID(paramData, 0));

                result.AppendFormat("{0,30}: {1,-2} {2,-37} [AssetType]" + Environment.NewLine,
                "AssetType",
                (sbyte)paramData[16],
                "(" + (AssetType)(sbyte)paramData[16] + ")");

            }
            else if (paramData.Length == 100)
            {
                //UUID agentID = new UUID(info.TransferInfo.Params, 0);
                result.AppendFormat("{0,30}: {1,-40} [UUID]" + Environment.NewLine,
                                    "AgentID",
                                    new UUID(paramData, 0));

                //UUID sessionID = new UUID(info.TransferInfo.Params, 16);
                result.AppendFormat("{0,30}: {1,-40} [UUID]" + Environment.NewLine,
                                    "SessionID",
                                    new UUID(paramData, 16));
                //UUID ownerID = new UUID(info.TransferInfo.Params, 32);
                result.AppendFormat("{0,30}: {1,-40} [UUID]" + Environment.NewLine,
                                    "OwnerID",
                                    new UUID(paramData, 32));
                //UUID taskID = new UUID(info.TransferInfo.Params, 48);
                result.AppendFormat("{0,30}: {1,-40} [UUID]" + Environment.NewLine,
                                    "TaskID",
                                    new UUID(paramData, 48));
                //UUID itemID = new UUID(info.TransferInfo.Params, 64);
                result.AppendFormat("{0,30}: {1,-40} [UUID]" + Environment.NewLine,
                                    "ItemID",
                                    new UUID(paramData, 64));

                result.AppendFormat("{0,30}: {1,-40} [UUID]" + Environment.NewLine,
                                    "AssetID",
                                    new UUID(paramData, 80));

                result.AppendFormat("{0,30}: {1,-2} {2,-37} [AssetType]" + Environment.NewLine,
                    "AssetType",
                    (sbyte)paramData[96],
                    "(" + (AssetType)(sbyte)paramData[96] + ")");
            }
            else
            {
                Console.WriteLine("Oh Shit!");
            }

            result.Append("</Params>");

            return result.ToString();
        }

        private static string DecodeTransferChannelType(string fieldName, object fieldData)
        {
            return String.Format("{0,30}: {1,-2} {2,-37} [ChannelType]",
                fieldName,
                fieldData,
                "(" + (ChannelType)(int)fieldData + ")");
        }

        private static string DecodeTransferSourceType(string fieldName, object fieldData)
        {
            return String.Format("{0,30}: {1,-2} {2,-37} [SourceType]",
                fieldName,
                fieldData,
                "(" + (SourceType)(int)fieldData + ")");
        }

        private static string DecodeTransferTargetType(string fieldName, object fieldData)
        {
            return String.Format("{0,30}: {1,-2} {2,-37} [TargetType]",
                fieldName,
                fieldData,
                "(" + (TargetType)(int)fieldData + ")");
        }

        private static string DecodeMapRequestFlags(string fieldName, object fieldData)
        {
            return String.Format("{0,30}: {1,-2} {2,-37} [GridLayerType]",
                fieldName,
                fieldData,
                "(" + (GridLayerType)(uint)fieldData + ")");
        }

        private static string DecodeGridItemType(string fieldName, object fieldData)
        {
            return String.Format("{0,30}: {1,-2} {2,-37} [GridItemType]",
                fieldName,
                fieldData,
                "(" + (GridItemType)(uint)fieldData + ")");

        }

        private static string DecodeMapAccess(string fieldName, object fieldData)
        {
            return String.Format("{0,30}: {1,-2} {2,-37} [SimAccess]",
                                 fieldName,
                                 fieldData,
                                 "(" + (SimAccess)(byte)fieldData + ")");
        }

        private static string DecodeSimAccess(string fieldName, object fieldData)
        {
            return String.Format("{0,30}: {1,-2} {2,-37} [SimAccess]",
                fieldName,
                (byte)fieldData,
                "(" + (SimAccess)fieldData + ")");
        }

        private static string DecodeAttachedSoundFlags(string fieldName, object fieldData)
        {
            return String.Format("{0,30}: {1,-2} {2,-37} [SoundFlags]",
                fieldName,
                (byte)fieldData,
                "(" + (SoundFlags)fieldData + ")");
        }


        private static string DecodeChatSourceType(string fieldName, object fieldData)
        {
            return String.Format("{0,30}: {1,-2} {2,-37} [SourceType]",
                fieldName,
                fieldData,
                "(" + (SourceType)(byte)fieldData + ")");
        }

        private static string DecodeChatChatType(string fieldName, object fieldData)
        {
            return String.Format("{0,30}: {1,-2} {2,-37} [ChatType]",
                fieldName,
                (byte)fieldData,
                "(" + (ChatType)fieldData + ")");
        }

        private static string DecodeChatAudible(string fieldName, object fieldData)
        {
            return String.Format("{0,30}: {1,-2} {2,-37} [ChatAudibleLevel]",
                fieldName,
                (byte)fieldData,
                "(" + (ChatAudibleLevel)(byte)fieldData + ")");
        }

        private static string DecodeImageData(string fieldName, object fieldData)
        {
            return String.Format("{0,10}",
                                Utils.BytesToHexString((byte[])fieldData,
                                String.Format("{0,30}", fieldName)));
        }

        private string DecodeTerseTextureEntry(string fieldName, object fieldData)
        {
            byte[] block = (byte[]) fieldData;

            Primitive.TextureEntry te = new Primitive.TextureEntry(block, 4, block.Length - 4);

            StringBuilder result = new StringBuilder();

            result.AppendFormat("{0,30}", " <TextureEntry>" + Environment.NewLine);
            if (te.DefaultTexture != null)
            {
                result.AppendFormat("{0,30}", "    <DefaultTexture>" + Environment.NewLine);
                GenericFieldsDecoder(te.DefaultTexture, ref result);
                GenericPropertiesDecoder(te.DefaultTexture, ref result);
                result.AppendFormat("{0,30}", "   </DefaultTexture>" + Environment.NewLine);
            }
            result.AppendFormat("{0,30}", "    <FaceTextures>" + Environment.NewLine);
            for (int i = 0; i < te.FaceTextures.Length; i++)
            {
                if (te.FaceTextures[i] != null)
                {
                    result.AppendFormat("{0,30}[{1}]" + Environment.NewLine, "FaceTexture", i);
                    GenericFieldsDecoder(te.FaceTextures[i], ref result);
                    GenericPropertiesDecoder(te.FaceTextures[i], ref result);
                }
            }
            result.AppendFormat("{0,30}", "   </FaceTextures>" + Environment.NewLine);
            result.AppendFormat("{0,30}", "</TextureEntry>");

            return result.ToString();
        }

        private static string DecodeTextureEntry(string fieldName, object fieldData)
        {
            Primitive.TextureEntry te;
            if (fieldData is Primitive.TextureEntry)
                te = (Primitive.TextureEntry)fieldData;
            else
            {
                byte[] tebytes = (byte[])fieldData;
                te = new Primitive.TextureEntry(tebytes, 0, tebytes.Length);
            }

            StringBuilder result = new StringBuilder();

            result.AppendFormat("{0,30}", " <TextureEntry>" + Environment.NewLine);
            if (te.DefaultTexture != null)
            {
                result.AppendFormat("{0,30}", "    <DefaultTexture>" + Environment.NewLine);
                GenericFieldsDecoder(te.DefaultTexture, ref result);
                GenericPropertiesDecoder(te.DefaultTexture, ref result);
                result.AppendFormat("{0,30}", "   </DefaultTexture>" + Environment.NewLine);
            }
            result.AppendFormat("{0,30}", "    <FaceTextures>" + Environment.NewLine);
            for (int i = 0; i < te.FaceTextures.Length; i++)
            {
                if (te.FaceTextures[i] != null)
                {
                    result.AppendFormat("{0,30}[{1}]" + Environment.NewLine, "FaceTexture", i);
                    GenericFieldsDecoder(te.FaceTextures[i], ref result);
                    GenericPropertiesDecoder(te.FaceTextures[i], ref result);
                }
            }
            result.AppendFormat("{0,30}", "   </FaceTextures>" + Environment.NewLine);
            result.AppendFormat("{0,30}", "</TextureEntry>");

            return result.ToString();
        }

        private static void GenericFieldsDecoder(object obj, ref StringBuilder result)
        {
            Type parcelType = obj.GetType();
            FieldInfo[] fields = parcelType.GetFields();
            foreach (FieldInfo field in fields)
            {
                String special;
                if (SpecialDecoder("a" + "." + "b" + "." + field.Name,
                    field.GetValue(obj), out special))
                {
                    result.AppendLine(special);
                }
                else
                {
                    result.AppendFormat("{0,30}: {1,-40} [{2}]" + Environment.NewLine,
                                        field.Name,
                                        field.GetValue(obj),
                                        field.FieldType.Name);
                }
            }
        }

        private static void GenericPropertiesDecoder(object obj, ref StringBuilder result)
        {
            Type parcelType = obj.GetType();
            PropertyInfo[] propertyInfos = parcelType.GetProperties();
            foreach (PropertyInfo property in propertyInfos)
            {
                String special;
                if (SpecialDecoder("a" + "." + "b" + "." + property.Name,
                    property.GetValue(obj, null), out special))
                {
                    result.AppendLine(special);
                }
                else
                {
                    result.AppendFormat("{0,30}: {1,-40} [{2}]" + Environment.NewLine,
                                        property.Name,
                                        property.GetValue(obj, null),
                                        property.PropertyType.Name);
                }
            }
        }

        private static string DecodeDialog(string fieldName, object fieldData)
        {
            return String.Format("{0,30}: {1,-2} {2,-37} [{3}]",
                fieldName,
                (byte)fieldData,
                "(" + (InstantMessageDialog)fieldData + ")",
                fieldData.GetType().Name);
        }

        private static string DecodeControlFlags(string fieldName, object fieldData)
        {
            return String.Format("{0,30}: {1,-10} {2,-29} [{3}]",
                fieldName,
                fieldData,
                "(" + (AgentManager.ControlFlags)(uint)fieldData + ")",
                fieldData.GetType().Name);
        }

        private static string DecodePermissionMask(string fieldName, object fieldData)
        {
            return String.Format("{0,30}: {1,-10} {2,-29} [{3}]",
                                 fieldName,
                                 (uint)fieldData,
                                 "(" + (PermissionMask)fieldData + ")",
                                 fieldData.GetType().Name);
        }

        public static string DecodeViewerEffectTypeData(string fieldName, object fieldData)
        {
            byte[] data = (byte[])fieldData;
            StringBuilder sb = new StringBuilder();
            if (data.Length == 56 || data.Length == 57)
            {
                UUID sourceAvatar = new UUID(data, 0);
                UUID targetObject = new UUID(data, 16);
                Vector3d targetPos = new Vector3d(data, 32);
                sb.AppendFormat("{0,30}: {1,-40} [UUID]" + Environment.NewLine, fieldName, "Source AvatarID=" + sourceAvatar);
                sb.AppendFormat("{0,30}: {1,-40} [UUID]" + Environment.NewLine, fieldName, "Target ObjectID=" + targetObject);


                float lx, ly;
                Helpers.GlobalPosToRegionHandle((float)targetPos.X, (float)targetPos.Y, out lx, out ly);

                sb.AppendFormat("{0,30}: {1,-40} [Vector3d]", fieldName, targetPos);

                if (data.Length == 57)
                {
                    sb.AppendLine();
                    sb.AppendFormat("{0,30}: {1,-17} {2,-22} [Byte]", fieldName, "Point At Type=" + data[56],
                                    "(" + (PointAtType)data[56] + ")");
                }

                return sb.ToString();
            }
            else
            {
                return String.Format("{0,30}: (No Decoder) Length={1}" + System.Environment.NewLine, fieldName, data.Length) + Utils.BytesToHexString(data, String.Format("{0,30}", ""));
            }
        }

        public static string DecodeViewerEffectType(string fieldName, object fieldData)
        {
            return String.Format("{0,30}: {1,-2} {2,-37} [{3}]",
                                 fieldName,
                                 fieldData,
                                 "(" + (EffectType)(byte)fieldData + ")",
                                 fieldData.GetType().Name);
        }
        #endregion

        /// <summary>
        /// Creates a formatted string containing the values of a Packet
        /// </summary>
        /// <param name="packet">The Packet</param>
        /// <returns>A formatted string of values of the nested items in the Packet object</returns>
        /// should be made generic enough to decode IMessage objects too.</remarks>
        public string PacketToString(Packet packet)
        {
            StringBuilder result = new StringBuilder();

            result.AppendFormat("Packet Type: {0}" + System.Environment.NewLine, packet.Type);
            result.AppendLine("[Packet Header]");
            // payload
            result.AppendFormat("Sequence: {0}" + System.Environment.NewLine, packet.Header.Sequence);
            result.AppendFormat(" Options: {0}" + System.Environment.NewLine, InterpretOptions(packet.Header));
            result.AppendLine();

            result.AppendLine("[Packet Payload]");

            FieldInfo[] fields = packet.GetType().GetFields();

            for (int i = 0; i < fields.Length; i++)
            {
                // we're not interested in any of these here
                if (fields[i].Name == "Type" || fields[i].Name == "Header" || fields[i].Name == "HasVariableBlocks")
                    continue;

                if (fields[i].FieldType.IsArray)
                {
                    result.AppendFormat("{0,30} []" + Environment.NewLine, "-- " + fields[i].Name + " --");
                    RecursePacketArray(fields[i], packet, ref result);
                }
                else
                {
                    result.AppendFormat("{0,30}" + Environment.NewLine, "-- " + fields[i].Name + " --");
                    RecursePacketField(fields[i], packet, ref result);
                }
            }
            return result.ToString();
        }

        private static string InterpretOptions(Header header)
        {
            return "["
                 + (header.AppendedAcks ? "Ack" : "   ")
                 + " "
                 + (header.Resent ? "Res" : "   ")
                 + " "
                 + (header.Reliable ? "Rel" : "   ")
                 + " "
                 + (header.Zerocoded ? "Zer" : "   ")
                 + "]"
                 ;
        }

        private static void RecursePacketArray(FieldInfo fieldInfo, object packet, ref StringBuilder result)
        {
            var packetDataObject = fieldInfo.GetValue(packet);

            foreach (object nestedArrayRecord in packetDataObject as Array)
            {
                FieldInfo[] fields = nestedArrayRecord.GetType().GetFields();

                for (int i = 0; i < fields.Length; i++)
                {
                    String special;
                    if (SpecialDecoder(packet.GetType().Name + "." + fieldInfo.Name + "." + fields[i].Name,
                        fields[i].GetValue(nestedArrayRecord), out special))
                    {
                        result.AppendLine(special);
                    }
                    else if (fields[i].FieldType.IsArray) // default for an array (probably a byte[])
                    {
                        result.AppendFormat("{0,30}: {1,-40} [{2}]" + System.Environment.NewLine,
                            fields[i].Name,
                            fields[i].GetValue(nestedArrayRecord),
                            fields[i].GetValue(nestedArrayRecord).GetType().Name);
                    }
                    else // default for a field
                    {
                        result.AppendFormat("{0,30}: {1,-40} [{2}]" + System.Environment.NewLine,
                            fields[i].Name,
                            fields[i].GetValue(nestedArrayRecord),
                            fields[i].GetValue(nestedArrayRecord).GetType().Name);
                    }
                }

                // Handle Properties
                foreach (PropertyInfo propertyInfo in nestedArrayRecord.GetType().GetProperties())
                {
                    if (propertyInfo.Name.Equals("Length"))
                        continue;

                    string special;
                    if (SpecialDecoder(packet.GetType().Name + "." + fieldInfo.Name + "." + propertyInfo.Name,
                            propertyInfo.GetValue(nestedArrayRecord, null),
                            out special))
                    {
                        result.AppendLine(special);
                    }
                    else
                    {
                        var p = propertyInfo.GetValue(nestedArrayRecord, null);
                        string s = Utils.BytesToString((byte[])p);
                        
                        result.AppendFormat("{0, 30}: {1,-40} [{2}]c" + Environment.NewLine,
                            propertyInfo.Name,
                            Utils.BytesToString((byte[])propertyInfo.GetValue(nestedArrayRecord, null)),
                            propertyInfo.PropertyType.Name);
                        //result.AppendFormat("TEST: {0} ### ", s);
                    }
                }
                result.AppendFormat("{0,32}" + Environment.NewLine, "***");
            }
        }

        private static void RecursePacketField(FieldInfo fieldInfo, object packet, ref StringBuilder result)
        {
            object packetDataObject = fieldInfo.GetValue(packet);

            // handle Fields
            foreach (FieldInfo packetValueField in fieldInfo.GetValue(packet).GetType().GetFields())
            {
                string special;
                if (SpecialDecoder(packet.GetType().Name + "." + fieldInfo.Name + "." + packetValueField.Name,
                        packetValueField.GetValue(packetDataObject),
                        out special))
                {
                    result.AppendLine(special);
                }
                else
                {
                    result.AppendFormat("{0,30}: {1,-40} [{2}]" + Environment.NewLine,
                        packetValueField.Name, packetValueField.GetValue(packetDataObject), packetValueField.FieldType.Name);
                }
            }

            // Handle Properties
            foreach (PropertyInfo propertyInfo in packetDataObject.GetType().GetProperties())
            {
                if (propertyInfo.Name.Equals("Length"))
                    continue;

                string special;
                if (SpecialDecoder(packet.GetType().Name + "." + fieldInfo.Name + "." + propertyInfo.Name,
                        propertyInfo.GetValue(packetDataObject, null),
                        out special))
                {
                    result.AppendLine(special);
                }
                else if (propertyInfo.GetValue(packetDataObject, null).GetType() == typeof(byte[]))
                {
                    result.AppendFormat("{0, 30}: {1,-40} [{2}]" + Environment.NewLine,
                        propertyInfo.Name,
                        Utils.BytesToString((byte[])propertyInfo.GetValue(packetDataObject, null)),
                        propertyInfo.PropertyType.Name);
                }
                else
                {
                    result.AppendFormat("{0, 30}: {1,-40} [{2}]" + Environment.NewLine,
                        propertyInfo.Name,
                        propertyInfo.GetValue(packetDataObject, null),
                        propertyInfo.PropertyType.Name);
                }
            }
        }

        private static bool SpecialDecoder(string decoderKey, object fieldData, out string result)
        {
            result = string.Empty;
            string[] keys = decoderKey.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            string[] keyList = { decoderKey, decoderKey.Replace("Packet", ""), keys[1] + "." + keys[2], keys[2] };
            foreach (string key in keyList)
            {

                bool ok = true;
                if (fieldData is byte[])
                {
                    byte[] fd = (byte[])fieldData;
                    ok = fd.Length > 0;
                    if (!ok)
                    {
                        // bypass the decoder since we were passed an empty byte array
                        result = String.Format("{0,30}:", keys[2]);
                        return true;
                    }
                }

                if (ok && Callbacks.ContainsKey(key)) // fieldname e.g: Plane
                {
                    foreach (CustomPacketDecoder decoder in Callbacks[key])
                        result = decoder(keys[2], fieldData);
                    return true;
                }
            }
            return false;
        }
    }
}
