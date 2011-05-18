/*
 * Copyright (c) 2007-2008, openmetaverse.org
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
using System.Collections.Generic;
using System.Text;

namespace OpenMetaverse
{
    /// <summary>
    /// A Name Value pair with additional settings, used in the protocol
    /// primarily to transmit avatar names and active group in object packets
    /// </summary>
    public struct NameValue
    {
        #region Enums

        /// <summary>Type of the value</summary>
        public enum ValueType
        {
            /// <summary>Unknown</summary>
            Unknown = -1,
            /// <summary>String value</summary>
            String,
            /// <summary></summary>
            F32,
            /// <summary></summary>
            S32,
            /// <summary></summary>
            VEC3,
            /// <summary></summary>
            U32,
            /// <summary>Deprecated</summary>
            [Obsolete]
            CAMERA,
            /// <summary>String value, but designated as an asset</summary>
            Asset,
            /// <summary></summary>
            U64
        }

        /// <summary>
        /// 
        /// </summary>
        public enum ClassType
        {
            /// <summary></summary>
            Unknown = -1,
            /// <summary></summary>
            ReadOnly,
            /// <summary></summary>
            ReadWrite,
            /// <summary></summary>
            Callback
        }

        /// <summary>
        /// 
        /// </summary>
        public enum SendtoType
        {
            /// <summary></summary>
            Unknown = -1,
            /// <summary></summary>
            Sim,
            /// <summary></summary>
            DataSim,
            /// <summary></summary>
            SimViewer,
            /// <summary></summary>
            DataSimViewer
        }

        #endregion Enums


        /// <summary></summary>
        public string Name;
        /// <summary></summary>
        public ValueType Type;
        /// <summary></summary>
        public ClassType Class;
        /// <summary></summary>
        public SendtoType Sendto;
        /// <summary></summary>
        public object Value;


        private static readonly string[] TypeStrings = new string[]
        {
            "STRING",
            "F32",
            "S32",
            "VEC3",
            "U32",
            "ASSET",
            "U64"
        };
        private static readonly string[] ClassStrings = new string[]
        {
            "R",    // Read-only
            "RW",   // Read-write
            "CB"    // Callback
        };
        private static readonly string[] SendtoStrings = new string[]
        {
            "S",    // Sim
            "DS",   // Data Sim
            "SV",   // Sim Viewer
            "DSV"   // Data Sim Viewer
        };
        private static readonly char[] Separators = new char[]
        {
            ' ',
            '\n',
            '\t',
            '\r'
        };

        /// <summary>
        /// Constructor that takes all the fields as parameters
        /// </summary>
        /// <param name="name"></param>
        /// <param name="valueType"></param>
        /// <param name="classType"></param>
        /// <param name="sendtoType"></param>
        /// <param name="value"></param>
        public NameValue(string name, ValueType valueType, ClassType classType, SendtoType sendtoType, object value)
        {
            Name = name;
            Type = valueType;
            Class = classType;
            Sendto = sendtoType;
            Value = value;
        }

        /// <summary>
        /// Constructor that takes a single line from a NameValue field
        /// </summary>
        /// <param name="data"></param>
        public NameValue(string data)
        {
            int i;

            // Name
            i = data.IndexOfAny(Separators);
            if (i < 1)
            {
                Name = String.Empty;
                Type = ValueType.Unknown;
                Class = ClassType.Unknown;
                Sendto = SendtoType.Unknown;
                Value = null;
                return;
            }
            Name = data.Substring(0, i);
            data = data.Substring(i + 1);

            // Type
            i = data.IndexOfAny(Separators);
            if (i > 0)
            {
                Type = GetValueType(data.Substring(0, i));
                data = data.Substring(i + 1);

                // Class
                i = data.IndexOfAny(Separators);
                if (i > 0)
                {
                    Class = GetClassType(data.Substring(0, i));
                    data = data.Substring(i + 1);

                    // Sendto
                    i = data.IndexOfAny(Separators);
                    if (i > 0)
                    {
                        Sendto = GetSendtoType(data.Substring(0, 1));
                        data = data.Substring(i + 1);
                    }
                }
            }

            // Value
            Type = ValueType.String;
            Class = ClassType.ReadOnly;
            Sendto = SendtoType.Sim;
            Value = null;
            SetValue(data);
        }

        public static string NameValuesToString(NameValue[] values)
        {
            if (values == null || values.Length == 0)
                return String.Empty;

            StringBuilder output = new StringBuilder();

            for (int i = 0; i < values.Length; i++)
            {
                NameValue value = values[i];

                if (value.Value != null)
                {
                    string newLine = (i < values.Length - 1) ? "\n" : String.Empty;
                    output.AppendFormat("{0} {1} {2} {3} {4}{5}", value.Name, TypeStrings[(int)value.Type],
                        ClassStrings[(int)value.Class], SendtoStrings[(int)value.Sendto], value.Value.ToString(), newLine);
                }
            }

            return output.ToString();
        }

        private void SetValue(string value)
        {
            switch (Type)
            {
                case ValueType.Asset:
                case ValueType.String:
                    Value = value;
                    break;
                case ValueType.F32:
                {
                    float temp;
                    Utils.TryParseSingle(value, out temp);
                    Value = temp;
                    break;
                }
                case ValueType.S32:
                {
                    int temp;
                    Int32.TryParse(value, out temp);
                    Value = temp;
                    break;
                }
                case ValueType.U32:
                {
                    uint temp;
                    UInt32.TryParse(value, out temp);
                    Value = temp;
                    break;
                }
                case ValueType.U64:
                {
                    ulong temp;
                    UInt64.TryParse(value, out temp);
                    Value = temp;
                    break;
                }
                case ValueType.VEC3:
                {
                    Vector3 temp;
                    Vector3.TryParse(value, out temp);
                    Value = temp;
                    break;
                }
                default:
                    Value = null;
                    break;
            }
        }

        private static ValueType GetValueType(string value)
        {
            ValueType type = ValueType.Unknown;

            for (int i = 0; i < TypeStrings.Length; i++)
            {
                if (value == TypeStrings[i])
                {
                    type = (ValueType)i;
                    break;
                }
            }

            if (type == ValueType.Unknown)
                type = ValueType.String;

            return type;
        }

        private static ClassType GetClassType(string value)
        {
            ClassType type = ClassType.Unknown;

            for (int i = 0; i < ClassStrings.Length; i++)
            {
                if (value == ClassStrings[i])
                {
                    type = (ClassType)i;
                    break;
                }
            }

            if (type == ClassType.Unknown)
                type = ClassType.ReadOnly;

            return type;
        }

        private static SendtoType GetSendtoType(string value)
        {
            SendtoType type = SendtoType.Unknown;

            for (int i = 0; i < SendtoStrings.Length; i++)
            {
                if (value == SendtoStrings[i])
                {
                    type = (SendtoType)i;
                    break;
                }
            }

            if (type == SendtoType.Unknown)
                type = SendtoType.Sim;

            return type;
        }
    }
}
