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
using System.Reflection;

namespace libsecondlife.Packets
{
    public abstract partial class Packet
    {
        /// <summary>
        /// Attempts to convert an LLSD structure to a known Packet type
        /// </summary>
        /// <param name="capsEventName">Event name, this must match an actual
        /// packet name for a Packet to be successfully built</param>
        /// <param name="body">LLSD to convert to a Packet</param>
        /// <returns>A Packet on success, otherwise null</returns>
        public static Packet BuildPacket(string capsEventName, Hashtable body)
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
                            ArrayList array = (ArrayList)body[field.Name];
                            Type elementType = blockType.GetElementType();
                            object[] blockArray = (object[])Array.CreateInstance(elementType, array.Count);

                            for (int i = 0; i < array.Count; i++)
                            {
                                Hashtable hashtable = (Hashtable)array[i];
                                blockArray[i] = ParseCapsBlock(hashtable, elementType);
                            }

                            field.SetValue(packet, blockArray);
                        }
                        else
                        {
                            Hashtable hashtable = (Hashtable)((ArrayList)body[field.Name])[0];
                            field.SetValue(packet, ParseCapsBlock(hashtable, blockType));
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

        public static object ParseCapsBlock(Hashtable blockData, Type blockType)
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
    }
}
