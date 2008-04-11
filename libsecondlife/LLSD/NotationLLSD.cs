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

namespace libsecondlife.StructuredData
{
    public static partial class LLSDParser
    {
        public static LLSD DeserializeNotation(string notationData)
        {
            int unused;
            return ParseNotationElement(notationData, out unused);
        }

        private static LLSD ParseNotationElement(string notationData, out int endPos)
        {
            if (notationData.Length == 0)
            {
                endPos = 0;
                return null;
            }

            // Identify what type of object this is
            switch (notationData[0])
            {
                case '!':
                    endPos = 1;
                    return new LLSD();
                case '1':
                    endPos = 1;
                    return LLSD.FromBoolean(true);
                case '0':
                    endPos = 1;
                    return LLSD.FromBoolean(false);
                case 'i':
                {
                    if (notationData.Length < 2)
                    {
                        endPos = notationData.Length;
                        return LLSD.FromInteger(0);
                    }

                    int value;
                    endPos = FindEnd(notationData, 1);

                    if (Helpers.TryParse(notationData.Substring(1, endPos - 1), out value))
                        return LLSD.FromInteger(value);
                    else
                        return LLSD.FromInteger(0);
                }
                case 'r':
                {
                    if (notationData.Length < 2)
                    {
                        endPos = notationData.Length;
                        return LLSD.FromReal(0d);
                    }

                    double value;
                    endPos = FindEnd(notationData, 1);

                    if (Helpers.TryParse(notationData.Substring(1, endPos - 1), out value))
                        return LLSD.FromReal(value);
                    else
                        return LLSD.FromReal(0d);
                }
                case 'u':
                {
                    if (notationData.Length < 17)
                    {
                        endPos = notationData.Length;
                        return LLSD.FromUUID(LLUUID.Zero);
                    }

                    LLUUID value;
                    endPos = FindEnd(notationData, 1);

                    if (Helpers.TryParse(notationData.Substring(1, endPos - 1), out value))
                        return LLSD.FromUUID(value);
                    else
                        return LLSD.FromUUID(LLUUID.Zero);
                }
                case 'b':
                    throw new NotImplementedException("Notation binary type is unimplemented");
                case 's':
                case '"':
                case '\'':
                    if (notationData.Length < 2)
                    {
                        endPos = notationData.Length;
                        return LLSD.FromString(String.Empty);
                    }

                    endPos = FindEnd(notationData, 1);
                    return LLSD.FromString(notationData.Substring(1, endPos - 1).Trim(new char[] { '"', '\'' }));
                case 'l':
                    throw new NotImplementedException("Notation URI type is unimplemented");
                case 'd':
                    throw new NotImplementedException("Notation date type is unimplemented");
                case '[':
                {
                    if (notationData.IndexOf(']') == -1)
                        throw new LLSDException("Invalid notation array");

                    int pos = 0;
                    LLSDArray array = new LLSDArray();

                    while (notationData[pos] != ']')
                    {
                        ++pos;

                        // Advance past comma if need be
                        if (notationData[pos] == ',') ++pos;

                        // Allow a single whitespace character
                        if (pos < notationData.Length && notationData[pos] == ' ') ++pos;

                        int end;
                        array.Add(ParseNotationElement(notationData.Substring(pos), out end));
                        pos += end;
                    }

                    endPos = pos + 1;
                    return array;
                }
                case '{':
                {
                    if (notationData.IndexOf('}') == -1)
                        throw new LLSDException("Invalid notation map");

                    int pos = 0;
                    LLSDMap hashtable = new LLSDMap();

                    while (notationData[pos] != '}')
                    {
                        ++pos;

                        // Advance past comma if need be
                        if (notationData[pos] == ',') ++pos;

                        // Allow a single whitespace character
                        if (pos < notationData.Length && notationData[pos] == ' ') ++pos;

                        if (notationData[pos] != '\'')
                            throw new LLSDException("Expected a map key");

                        int endquote = notationData.IndexOf('\'', pos + 1);
                        if (endquote == -1 || (endquote + 1) >= notationData.Length || notationData[endquote + 1] != ':')
                            throw new LLSDException("Invalid map format");

                        string key = notationData.Substring(pos, endquote - pos);
                        key = key.Trim(new char[] { '"', '\'' }); //key.Replace("'", String.Empty);
                        pos += (endquote - pos) + 2;

                        int end;
                        hashtable[key] = ParseNotationElement(notationData.Substring(pos), out end);
                        pos += end;
                    }

                    endPos = pos + 1;
                    return hashtable;
                }
                default:
                    throw new LLSDException("Unknown notation value type");
            }
        }

        private static int FindEnd(string llsd, int start)
        {
            int end = llsd.IndexOfAny(new char[] { ',', ']', '}' });
            if (end == -1) end = llsd.Length - 1;
            return end;
        }
    }
}
