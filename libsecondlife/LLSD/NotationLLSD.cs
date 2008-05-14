/*
 * Copyright (c) 2007-2008, Second Life Reverse Engineering Team
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
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace libsecondlife.StructuredData
{
    /// <summary>
    /// 
    /// </summary>
    public static partial class LLSDParser
    {
        private const string notationHead = "<?llsd/notation?>\n";
        private const string baseIntend = "  ";

        private const char undefNotationValue = '!';

        private const char trueNotationValueOne = '1';
        private const char trueNotationValueTwo = 't';
        private static char[] trueNotationValueTwoFull = { 't', 'r', 'u', 'e' };
        private const char trueNotationValueThree = 'T';
        private static char[] trueNotationValueThreeFull = { 'T', 'R', 'U', 'E' };

        private const char falseNotationValueOne = '0';
        private const char falseNotationValueTwo = 'f';
        private static char[] falseNotationValueTwoFull = { 'f', 'a', 'l', 's', 'e' };
        private const char falseNotationValueThree = 'F';
        private static char[] falseNotationValueThreeFull = { 'F', 'A', 'L', 'S', 'E' };

        private const char integerNotationMarker = 'i';
        private const char realNotationMarker = 'r';
        private const char uuidNotationMarker = 'u';
        private const char binaryNotationMarker = 'b';
        private const char stringNotationMarker = 's';
        private const char uriNotationMarker = 'l';
        private const char dateNotationMarker = 'd';

        private const char arrayBeginNotationMarker = '[';
        private const char arrayEndNotationMarker = ']';

        private const char mapBeginNotationMarker = '{';
        private const char mapEndNotationMarker = '}';
        private const char kommaNotationDelimiter = ',';
        private const char keyNotationDelimiter = ':';

        private const char sizeBeginNotationMarker = '(';
        private const char sizeEndNotationMarker = ')';
        private const char doubleQuotesNotationMarker = '"';
        private const char singleQuotesNotationMarker = '\'';

        /// <summary>
        /// 
        /// </summary>
        /// <param name="notationData"></param>
        /// <returns></returns>
        public static LLSD DeserializeNotation(string notationData)
        {
            StringReader reader = new StringReader(notationData);
            LLSD llsd = DeserializeNotation(reader);
            reader.Close();
            return llsd;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static LLSD DeserializeNotation(StringReader reader)
        {
            LLSD llsd = DeserializeNotationElement(reader);
            return llsd;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="llsd"></param>
        /// <returns></returns>
        public static string SerializeNotation(LLSD llsd)
        {
            StringWriter writer = SerializeNotationStream(llsd);
            string s = writer.ToString();
            writer.Close();

            return s;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="llsd"></param>
        /// <returns></returns>
        public static StringWriter SerializeNotationStream(LLSD llsd)
        {
            StringWriter writer = new StringWriter();

            SerializeNotationElement(writer, llsd);
            return writer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="llsd"></param>
        /// <returns></returns>
        public static string SerializeNotationFormatted(LLSD llsd)
        {
            StringWriter writer = SerializeNotationStreamFormatted(llsd);
            string s = writer.ToString();
            writer.Close();

            return s;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="llsd"></param>
        /// <returns></returns>
        public static StringWriter SerializeNotationStreamFormatted(LLSD llsd)
        {
            StringWriter writer = new StringWriter();

            string intend = "";
            SerializeNotationElementFormatted(writer, intend, llsd);
            return writer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static LLSD DeserializeNotationElement(StringReader reader)
        {
            int character = ReadAndSkipWhitespace(reader);
            if (character < 0)
                return new LLSD(); // server returned an empty file, so we're going to pass along a null LLSD object

            LLSD llsd;
            int matching;
            switch ((char)character)
            {
                case undefNotationValue:
                    llsd = new LLSD();
                    break;
                case trueNotationValueOne:
                    llsd = LLSD.FromBoolean(true);
                    break;
                case trueNotationValueTwo:
                    matching = BufferCharactersEqual(reader, trueNotationValueTwoFull, 1);
                    if (matching > 1 && matching < trueNotationValueTwoFull.Length)
                        throw new LLSDException("Notation LLSD parsing: True value parsing error:");
                    llsd = LLSD.FromBoolean(true);
                    break;
                case trueNotationValueThree:
                    matching = BufferCharactersEqual(reader, trueNotationValueThreeFull, 1);
                    if (matching > 1 && matching < trueNotationValueThreeFull.Length)
                        throw new LLSDException("Notation LLSD parsing: True value parsing error:");
                    llsd = LLSD.FromBoolean(true);
                    break;
                case falseNotationValueOne:
                    llsd = LLSD.FromBoolean(false);
                    break;
                case falseNotationValueTwo:
                    matching = BufferCharactersEqual(reader, falseNotationValueTwoFull, 1);
                    if (matching > 1 && matching < falseNotationValueTwoFull.Length)
                        throw new LLSDException("Notation LLSD parsing: True value parsing error:");
                    llsd = LLSD.FromBoolean(false);
                    break;
                case falseNotationValueThree:
                    matching = BufferCharactersEqual(reader, falseNotationValueThreeFull, 1);
                    if (matching > 1 && matching < falseNotationValueThreeFull.Length)
                        throw new LLSDException("Notation LLSD parsing: True value parsing error:");
                    llsd = LLSD.FromBoolean(false);
                    break;
                case integerNotationMarker:
                    llsd = DeserializeNotationInteger(reader);
                    break;
                case realNotationMarker:
                    llsd = DeserializeNotationReal(reader);
                    break;
                case uuidNotationMarker:
                    char[] uuidBuf = new char[36];
                    if (reader.Read(uuidBuf, 0, 36) < 36)
                        throw new LLSDException("Notation LLSD parsing: Unexpected end of stream in UUID.");
                    LLUUID lluuid;
                    if (!LLUUID.TryParse(new String(uuidBuf), out lluuid))
                        throw new LLSDException("Notation LLSD parsing: Invalid UUID discovered.");
                    llsd = LLSD.FromUUID(lluuid);
                    break;
                case binaryNotationMarker:
                    byte[] bytes = new byte[0];
                    int bChar = reader.Peek();
                    if (bChar < 0)
                        throw new LLSDException("Notation LLSD parsing: Unexpected end of stream in binary.");
                    if ((char)bChar == sizeBeginNotationMarker)
                    {
                        throw new LLSDException("Notation LLSD parsing: Raw binary encoding not supported.");
                    }
                    else if (Char.IsDigit((char)bChar))
                    {
                        char[] charsBaseEncoding = new char[2];
                        if (reader.Read(charsBaseEncoding, 0, 2) < 2)
                            throw new LLSDException("Notation LLSD parsing: Unexpected end of stream in binary.");
                        int baseEncoding;
                        if (!Int32.TryParse(new String(charsBaseEncoding), out baseEncoding))
                            throw new LLSDException("Notation LLSD parsing: Invalid binary encoding base.");
                        if (baseEncoding == 64)
                        {
                            if (reader.Read() < 0)
                                throw new LLSDException("Notation LLSD parsing: Unexpected end of stream in binary.");
                            string bytes64 = GetStringDelimitedBy(reader, doubleQuotesNotationMarker);
                            bytes = Convert.FromBase64String(bytes64);
                        }
                        else
                        {
                            throw new LLSDException("Notation LLSD parsing: Encoding base" + baseEncoding + " + not supported.");
                        }
                    }
                    llsd = LLSD.FromBinary(bytes);
                    break;
                case stringNotationMarker:
                    int numChars = GetLengthInBrackets(reader);
                    if (reader.Read() < 0)
                        throw new LLSDException("Notation LLSD parsing: Unexpected end of stream in string.");
                    char[] chars = new char[numChars];
                    if (reader.Read(chars, 0, numChars) < numChars)
                        throw new LLSDException("Notation LLSD parsing: Unexpected end of stream in string.");
                    if (reader.Read() < 0)
                        throw new LLSDException("Notation LLSD parsing: Unexpected end of stream in string.");
                    llsd = LLSD.FromString(new String(chars));
                    break;
                case singleQuotesNotationMarker:
                    string sOne = GetStringDelimitedBy(reader, singleQuotesNotationMarker);
                    llsd = LLSD.FromString(sOne);
                    break;
                case doubleQuotesNotationMarker:
                    string sTwo = GetStringDelimitedBy(reader, doubleQuotesNotationMarker);
                    llsd = LLSD.FromString(sTwo);
                    break;
                case uriNotationMarker:
                    if (reader.Read() < 0)
                        throw new LLSDException("Notation LLSD parsing: Unexpected end of stream in string.");
                    string sUri = GetStringDelimitedBy(reader, doubleQuotesNotationMarker);

                    Uri uri;
                    try
                    {
                        uri = new Uri(sUri, UriKind.RelativeOrAbsolute);
                    }
                    catch
                    {
                        throw new LLSDException("Notation LLSD parsing: Invalid Uri format detected.");
                    }
                    llsd = LLSD.FromUri(uri);
                    break;
                case dateNotationMarker:
                    if (reader.Read() < 0)
                        throw new LLSDException("Notation LLSD parsing: Unexpected end of stream in date.");
                    string date = GetStringDelimitedBy(reader, doubleQuotesNotationMarker);
                    DateTime dt;
                    if (!Helpers.TryParse(date, out dt))
                        throw new LLSDException("Notation LLSD parsing: Invalid date discovered.");
                    llsd = LLSD.FromDate(dt);
                    break;
                case arrayBeginNotationMarker:
                    llsd = DeserializeNotationArray(reader);
                    break;
                case mapBeginNotationMarker:
                    llsd = DeserializeNotationMap(reader);
                    break;
                default:
                    throw new LLSDException("Notation LLSD parsing: Unknown type marker '" + (char)character + "'.");
            }
            return llsd;
        }

        private static LLSD DeserializeNotationInteger(StringReader reader)
        {
            int character;
            StringBuilder s = new StringBuilder();
            if (((character = reader.Peek()) > 0) && ((char)character == '-'))
            {
                s.Append((char)character);
                reader.Read();
            }

            while ((character = reader.Peek()) > 0 &&
                           Char.IsDigit((char)character))
            {
                s.Append((char)character);
                reader.Read();
            }
            int integer;
            if (!Helpers.TryParse(s.ToString(), out integer))
                throw new LLSDException("Notation LLSD parsing: Can't parse integer value." + s.ToString());

            return LLSD.FromInteger(integer);
        }

        private static LLSD DeserializeNotationReal(StringReader reader)
        {
            int character;
            StringBuilder s = new StringBuilder();
            if (((character = reader.Peek()) > 0) &&
                ((char)character == '-' && (char)character == '+'))
            {
                s.Append((char)character);
                reader.Read();
            }
            while (((character = reader.Peek()) > 0) &&
                   (Char.IsDigit((char)character) || (char)character == '.' ||
                    (char)character == 'e' || (char)character == 'E' ||
                    (char)character == '+' || (char)character == '-'))
            {
                s.Append((char)character);
                reader.Read();
            }
            double dbl;
            if (!Helpers.TryParse(s.ToString(), out dbl))
                throw new LLSDException("Notation LLSD parsing: Can't parse real value: " + s.ToString());

            return LLSD.FromReal(dbl);
        }

        private static LLSD DeserializeNotationArray(StringReader reader)
        {
            int character;
            LLSDArray llsdArray = new LLSDArray();
            while (((character = PeekAndSkipWhitespace(reader)) > 0) &&
                  ((char)character != arrayEndNotationMarker))
            {
                llsdArray.Add(DeserializeNotationElement(reader));

                character = ReadAndSkipWhitespace(reader);
                if (character < 0)
                    throw new LLSDException("Notation LLSD parsing: Unexpected end of array discovered.");
                else if ((char)character == kommaNotationDelimiter)
                    continue;
                else if ((char)character == arrayEndNotationMarker)
                    break;
            }
            if (character < 0)
                throw new LLSDException("Notation LLSD parsing: Unexpected end of array discovered.");

            return (LLSD)llsdArray;
        }

        private static LLSD DeserializeNotationMap(StringReader reader)
        {
            int character;
            LLSDMap llsdMap = new LLSDMap();
            while (((character = PeekAndSkipWhitespace(reader)) > 0) &&
                  ((char)character != mapEndNotationMarker))
            {
                LLSD llsdKey = DeserializeNotationElement(reader);
                if (llsdKey.Type != LLSDType.String)
                    throw new LLSDException("Notation LLSD parsing: Invalid key in map");
                string key = llsdKey.AsString();

                character = ReadAndSkipWhitespace(reader);
                if ((char)character != keyNotationDelimiter)
                    throw new LLSDException("Notation LLSD parsing: Unexpected end of stream in map.");
                if ((char)character != keyNotationDelimiter)
                    throw new LLSDException("Notation LLSD parsing: Invalid delimiter in map.");

                llsdMap[key] = DeserializeNotationElement(reader);
                character = ReadAndSkipWhitespace(reader);
                if (character < 0)
                    throw new LLSDException("Notation LLSD parsing: Unexpected end of map discovered.");
                else if ((char)character == kommaNotationDelimiter)
                    continue;
                else if ((char)character == mapEndNotationMarker)
                    break;
            }
            if (character < 0)
                throw new LLSDException("Notation LLSD parsing: Unexpected end of map discovered.");

            return (LLSD)llsdMap;
        }

        private static void SerializeNotationElement(StringWriter writer, LLSD llsd)
        {

            switch (llsd.Type)
            {
                case LLSDType.Unknown:
                    writer.Write(undefNotationValue);
                    break;
                case LLSDType.Boolean:
                    if (llsd.AsBoolean())
                        writer.Write(trueNotationValueTwo);
                    else
                        writer.Write(falseNotationValueTwo);
                    break;
                case LLSDType.Integer:
                    writer.Write(integerNotationMarker);
                    writer.Write(llsd.AsString());
                    break;
                case LLSDType.Real:
                    writer.Write(realNotationMarker);
                    writer.Write(llsd.AsString());
                    break;
                case LLSDType.UUID:
                    writer.Write(uuidNotationMarker);
                    writer.Write(llsd.AsString());
                    break;
                case LLSDType.String:
                    writer.Write(singleQuotesNotationMarker);
                    writer.Write(EscapeCharacter(llsd.AsString(), singleQuotesNotationMarker));
                    writer.Write(singleQuotesNotationMarker);
                    break;
                case LLSDType.Binary:
                    writer.Write(binaryNotationMarker);
                    writer.Write("64");
                    writer.Write(doubleQuotesNotationMarker);
                    writer.Write(llsd.AsString());
                    writer.Write(doubleQuotesNotationMarker);
                    break;
                case LLSDType.Date:
                    writer.Write(dateNotationMarker);
                    writer.Write(doubleQuotesNotationMarker);
                    writer.Write(llsd.AsString());
                    writer.Write(doubleQuotesNotationMarker);
                    break;
                case LLSDType.URI:
                    writer.Write(uriNotationMarker);
                    writer.Write(doubleQuotesNotationMarker);
                    writer.Write(EscapeCharacter(llsd.AsString(), doubleQuotesNotationMarker));
                    writer.Write(doubleQuotesNotationMarker);
                    break;
                case LLSDType.Array:
                    SerializeNotationArray(writer, (LLSDArray)llsd);
                    break;
                case LLSDType.Map:
                    SerializeNotationMap(writer, (LLSDMap)llsd);
                    break;
                default:
                    throw new LLSDException("Notation serialization: Not existing element discovered.");

            }
        }

        private static void SerializeNotationArray(StringWriter writer, LLSDArray llsdArray)
        {
            writer.Write(arrayBeginNotationMarker);
            int lastIndex = llsdArray.Count - 1;

            for (int idx = 0; idx <= lastIndex; idx++)
            {
                SerializeNotationElement(writer, llsdArray[idx]);
                if (idx < lastIndex)
                    writer.Write(kommaNotationDelimiter);
            }
            writer.Write(arrayEndNotationMarker);
        }

        private static void SerializeNotationMap(StringWriter writer, LLSDMap llsdMap)
        {
            writer.Write(mapBeginNotationMarker);
            int lastIndex = llsdMap.Count - 1;
            int idx = 0;

            foreach (KeyValuePair<string, LLSD> kvp in llsdMap)
            {
                writer.Write(singleQuotesNotationMarker);
                writer.Write(EscapeCharacter(kvp.Key, singleQuotesNotationMarker));
                writer.Write(singleQuotesNotationMarker);
                writer.Write(keyNotationDelimiter);
                SerializeNotationElement(writer, kvp.Value);
                if (idx < lastIndex)
                    writer.Write(kommaNotationDelimiter);

                idx++;
            }
            writer.Write(mapEndNotationMarker);
        }

        private static void SerializeNotationElementFormatted(StringWriter writer, string intend, LLSD llsd)
        {
            switch (llsd.Type)
            {
                case LLSDType.Unknown:
                    writer.Write(undefNotationValue);
                    break;
                case LLSDType.Boolean:
                    if (llsd.AsBoolean())
                        writer.Write(trueNotationValueTwo);
                    else
                        writer.Write(falseNotationValueTwo);
                    break;
                case LLSDType.Integer:
                    writer.Write(integerNotationMarker);
                    writer.Write(llsd.AsString());
                    break;
                case LLSDType.Real:
                    writer.Write(realNotationMarker);
                    writer.Write(llsd.AsString());
                    break;
                case LLSDType.UUID:
                    writer.Write(uuidNotationMarker);
                    writer.Write(llsd.AsString());
                    break;
                case LLSDType.String:
                    writer.Write(singleQuotesNotationMarker);
                    writer.Write(EscapeCharacter(llsd.AsString(), singleQuotesNotationMarker));
                    writer.Write(singleQuotesNotationMarker);
                    break;
                case LLSDType.Binary:
                    writer.Write(binaryNotationMarker);
                    writer.Write("64");
                    writer.Write(doubleQuotesNotationMarker);
                    writer.Write(llsd.AsString());
                    writer.Write(doubleQuotesNotationMarker);
                    break;
                case LLSDType.Date:
                    writer.Write(dateNotationMarker);
                    writer.Write(doubleQuotesNotationMarker);
                    writer.Write(llsd.AsString());
                    writer.Write(doubleQuotesNotationMarker);
                    break;
                case LLSDType.URI:
                    writer.Write(uriNotationMarker);
                    writer.Write(doubleQuotesNotationMarker);
                    writer.Write(EscapeCharacter(llsd.AsString(), doubleQuotesNotationMarker));
                    writer.Write(doubleQuotesNotationMarker);
                    break;
                case LLSDType.Array:
                    SerializeNotationArrayFormatted(writer, intend + baseIntend, (LLSDArray)llsd);
                    break;
                case LLSDType.Map:
                    SerializeNotationMapFormatted(writer, intend + baseIntend, (LLSDMap)llsd);
                    break;
                default:
                    throw new LLSDException("Notation serialization: Not existing element discovered.");

            }
        }

        private static void SerializeNotationArrayFormatted(StringWriter writer, string intend, LLSDArray llsdArray)
        {
            writer.Write(Helpers.NewLine);
            writer.Write(intend);
            writer.Write(arrayBeginNotationMarker);
            int lastIndex = llsdArray.Count - 1;

            for (int idx = 0; idx <= lastIndex; idx++)
            {
                if (llsdArray[idx].Type != LLSDType.Array && llsdArray[idx].Type != LLSDType.Map)
                    writer.Write(Helpers.NewLine);
                writer.Write(intend + baseIntend);
                SerializeNotationElementFormatted(writer, intend, llsdArray[idx]);
                if (idx < lastIndex)
                {
                    writer.Write(kommaNotationDelimiter);
                }
            }
            writer.Write(Helpers.NewLine);
            writer.Write(intend);
            writer.Write(arrayEndNotationMarker);
        }

        private static void SerializeNotationMapFormatted(StringWriter writer, string intend, LLSDMap llsdMap)
        {
            writer.Write(Helpers.NewLine);
            writer.Write(intend);
            writer.Write(mapBeginNotationMarker);
            writer.Write(Helpers.NewLine);
            int lastIndex = llsdMap.Count - 1;
            int idx = 0;

            foreach (KeyValuePair<string, LLSD> kvp in llsdMap)
            {
                writer.Write(intend + baseIntend);
                writer.Write(singleQuotesNotationMarker);
                writer.Write(EscapeCharacter(kvp.Key, singleQuotesNotationMarker));
                writer.Write(singleQuotesNotationMarker);
                writer.Write(keyNotationDelimiter);
                SerializeNotationElementFormatted(writer, intend, kvp.Value);
                if (idx < lastIndex)
                {
                    writer.Write(Helpers.NewLine);
                    writer.Write(intend + baseIntend);
                    writer.Write(kommaNotationDelimiter);
                    writer.Write(Helpers.NewLine);
                }

                idx++;
            }
            writer.Write(Helpers.NewLine);
            writer.Write(intend);
            writer.Write(mapEndNotationMarker);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static int PeekAndSkipWhitespace(StringReader reader)
        {
            int character;
            while ((character = reader.Peek()) > 0)
            {
                char c = (char)character;
                if (c == ' ' || c == '\t' || c == '\n' || c == '\r')
                {
                    reader.Read();
                    continue;
                }
                else
                    break;
            }
            return character;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static int ReadAndSkipWhitespace(StringReader reader)
        {
            int character = PeekAndSkipWhitespace(reader);
            reader.Read();
            return character;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static int GetLengthInBrackets(StringReader reader)
        {
            int character;
            StringBuilder s = new StringBuilder();
            if (((character = PeekAndSkipWhitespace(reader)) > 0) &&
                     ((char)character == sizeBeginNotationMarker))
            {
                reader.Read();
            }
            while (((character = reader.Read()) > 0) &&
                    Char.IsDigit((char)character) &&
                  ((char)character != sizeEndNotationMarker))
            {
                s.Append((char)character);
            }
            if (character < 0)
                throw new LLSDException("Notation LLSD parsing: Can't parse length value cause unexpected end of stream.");
            int length;
            if (!Helpers.TryParse(s.ToString(), out length))
                throw new LLSDException("Notation LLSD parsing: Can't parse length value.");

            return length;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        public static string GetStringDelimitedBy(StringReader reader, char delimiter)
        {
            int character;
            bool foundEscape = false;
            StringBuilder s = new StringBuilder();
            while (((character = reader.Read()) > 0) &&
                  (((char)character != delimiter) ||
                   ((char)character == delimiter && foundEscape)))
            {

                if (foundEscape)
                {
                    foundEscape = false;
                    switch ((char)character)
                    {
                        case 'a':
                            s.Append('\a');
                            break;
                        case 'b':
                            s.Append('\b');
                            break;
                        case 'f':
                            s.Append('\f');
                            break;
                        case 'n':
                            s.Append('\n');
                            break;
                        case 'r':
                            s.Append('\r');
                            break;
                        case 't':
                            s.Append('\t');
                            break;
                        case 'v':
                            s.Append('\v');
                            break;
                        default:
                            s.Append((char)character);
                            break;
                    }
                }
                else if ((char)character == '\\')
                    foundEscape = true;
                else
                    s.Append((char)character);

            }
            if (character < 0)
                throw new LLSDException("Notation LLSD parsing: Can't parse text because unexpected end of stream while expecting a '"
                                            + delimiter + "' character.");

            return s.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static int BufferCharactersEqual(StringReader reader, char[] buffer, int offset)
        {

            int character;
            int lastIndex = buffer.Length - 1;
            int crrIndex = offset;
            bool charactersEqual = true;

            while ((character = reader.Peek()) > 0 &&
                    crrIndex <= lastIndex &&
                    charactersEqual)
            {
                if (((char)character) != buffer[crrIndex])
                {
                    charactersEqual = false;
                    break;
                }
                crrIndex++;
                reader.Read();
            }

            return crrIndex;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static string UnescapeCharacter(String s, char c)
        {
            string oldOne = "\\" + c;
            string newOne = new String(c, 1);

            String sOne = s.Replace("\\\\", "\\").Replace(oldOne, newOne);
            return sOne;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static string EscapeCharacter(String s, char c)
        {
            string oldOne = new String(c, 1);
            string newOne = "\\" + c;

            String sOne = s.Replace("\\", "\\\\").Replace(oldOne, newOne);
            return sOne;
        }
    }
}
