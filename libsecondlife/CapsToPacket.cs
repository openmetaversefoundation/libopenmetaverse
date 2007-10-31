/*
 * Copyright (c) 2007, Second Life Reverse Engineering Team
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
using System.Collections.Generic;
using System.Reflection;
using libsecondlife.LLSD;

namespace libsecondlife.Packets
{
    public abstract partial class Packet
    {
        public static string SerializeToXml(Packet packet)
        {
            return LLSDParser.SerializeXmlString(SerializeToLLSD(packet));
        }

        public static object SerializeToLLSD(Packet packet)
        {
            Dictionary<string, object> body = new Dictionary<string, object>();
            Type type = packet.GetType();

            foreach (FieldInfo field in type.GetFields())
            {
                if (field.IsPublic)
                {
                    Type blockType = field.FieldType;

                    if (blockType.IsArray)
                    {
                        object blockArray = field.GetValue(packet);
                        Array array = (Array)blockArray;
                        List<object> blockList = new List<object>(array.Length);
                        IEnumerator ie = array.GetEnumerator();

                        while (ie.MoveNext())
                        {
                            object block = ie.Current;
                            blockList.Add(BuildLLSDBlock(block));
                        }

                        body[field.Name] = blockList;
                    }
                    else
                    {
                        object block = field.GetValue(packet);
                        body[field.Name] = BuildLLSDBlock(block);
                    }
                }
            }

            return body;
        }

        public static string SerializeToBinary(Packet packet)
        {
            throw new NotImplementedException("Need to finish BinaryLLSD first");
        }

        /// <summary>
        /// Attempts to convert an LLSD structure to a known Packet type
        /// </summary>
        /// <param name="capsEventName">Event name, this must match an actual
        /// packet name for a Packet to be successfully built</param>
        /// <param name="body">LLSD to convert to a Packet</param>
        /// <returns>A Packet on success, otherwise null</returns>
        public static Packet BuildPacket(string capsEventName, Dictionary<string, object> body)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            // Check if we have a subclass of packet with the same name as this event
            Type type = assembly.GetType("libsecondlife.Packets." + capsEventName + "Packet", false);
            if (type == null)
                return null;

            object packet = null;

            try
            {
                // Create an instance of the object
                packet = Activator.CreateInstance(type);

                // Iterate over all of the fields in the packet class, looking for matches in the LLSD
                foreach (FieldInfo field in type.GetFields())
                {
                    if (body.ContainsKey(field.Name))
                    {
                        Type blockType = field.FieldType;

                        if (blockType.IsArray)
                        {
                            List<object> array = (List<object>)body[field.Name];
                            Type elementType = blockType.GetElementType();
                            object[] blockArray = (object[])Array.CreateInstance(elementType, array.Count);

                            for (int i = 0; i < array.Count; i++)
                            {
                                Dictionary<string, object> hashtable = (Dictionary<string, object>)array[i];
                                blockArray[i] = ParseLLSDBlock(hashtable, elementType);
                            }

                            field.SetValue(packet, blockArray);
                        }
                        else
                        {
                            Dictionary<string, object> hashtable = (Dictionary<string, object>)((List<object>)body[field.Name])[0];
                            field.SetValue(packet, ParseLLSDBlock(hashtable, blockType));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                SecondLife.LogStatic(e.ToString(), Helpers.LogLevel.Warning);
            }

            return (Packet)packet;
        }

        private static object ParseLLSDBlock(Dictionary<string, object> blockData, Type blockType)
        {
            object block = Activator.CreateInstance(blockType);

            // Iterate over each field and set the value if a match was found in the LLSD
            foreach (FieldInfo field in blockType.GetFields())
            {
                if (blockData.ContainsKey(field.Name))
                {
                    Type fieldType = field.FieldType;

                    if (fieldType == typeof(ulong))
                    {
                        // ulongs come in as a byte array, convert it manually here
                        byte[] bytes = (byte[])blockData[field.Name];
                        ulong value = Helpers.BytesToUInt64(bytes);
                        field.SetValue(block, value);
                    }
                    else if (fieldType == typeof(uint))
                    {
                        // uints come in as a byte array, convert it manually here
                        byte[] bytes = (byte[])blockData[field.Name];
                        uint value = Helpers.BytesToUIntBig(bytes);
                        field.SetValue(block, value);
                    }
                    else if (fieldType == typeof(ushort))
                    {
                        // Just need a bit of manual typecasting love here
                        field.SetValue(block, (ushort)(int)blockData[field.Name]);
                    }
                    else if (fieldType == typeof(byte))
                    {
                        // Just need a bit of manual typecasting love here
                        field.SetValue(block, (byte)(int)blockData[field.Name]);
                    }
                    else
                    {
                        field.SetValue(block, blockData[field.Name]);
                    }
                }
            }

            // String fields are properties, pick those up as well
            foreach (PropertyInfo property in blockType.GetProperties())
            {
                if (blockData.ContainsKey(property.Name))
                {
                    MethodInfo set = property.GetSetMethod();
                    set.Invoke(block, new object[] { Helpers.StringToField((string)blockData[property.Name]) });
                }
            }

            return block;
        }

        private static Dictionary<string, object> BuildLLSDBlock(object block)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            Type blockType = block.GetType();

            foreach (FieldInfo field in blockType.GetFields())
            {
                if (field.IsPublic)
                {
                    dict[field.Name] = field.GetValue(block);
                }
            }

            foreach (PropertyInfo property in blockType.GetProperties())
            {
                if (property.Name != "Length")
                {
                    dict[property.Name] = property.GetValue(block, null);
                }
            }

            return dict;
        }
    }
}
