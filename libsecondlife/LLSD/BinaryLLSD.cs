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

/* 
 * 
 * This implementation is based upon the description at
 * 
 * http://wiki.secondlife.com/wiki/LLSD
 * 
 * and (partially) tested against the (supposed) reference implementation at
 * 
 * http://svn.secondlife.com/svn/linden/release/indra/lib/python/indra/base/llsd.py
 * 
 */

using System;
using System.IO;
using System.Collections;
 using System.Collections.Generic;
 using System.Text;
 
 namespace libsecondlife.StructuredData
 {
     public static partial class LLSDParser
    {
        private const int initialBufferSize = 128;
        private const int int32Length = 4;
        private const int doubleLength = 8;
        
        private static byte[] binaryHead = Encoding.ASCII.GetBytes( "<?llsd/binary?>\n" );
        private const byte undefBinaryValue = (byte)'!';
        private const byte trueBinaryValue = (byte) '1';
        private const byte falseBinaryValue = (byte) '0';
        private const byte integerBinaryMarker = (byte) 'i';
        private const byte realBinaryMarker = (byte) 'r';
        private const byte uuidBinaryMarker = (byte) 'u';
        private const byte binaryBinaryMarker = (byte) 'b';
        private const byte stringBinaryMarker = (byte) 's';
        private const byte uriBinaryMarker = (byte) 'l';
        private const byte dateBinaryMarker = (byte) 'd';
        private const byte arrayBeginBinaryMarker = (byte) '[';
        private const byte arrayEndBinaryMarker = (byte) ']';
        private const byte mapBeginBinaryMarker = (byte) '{';
        private const byte mapEndBinaryMarker = (byte) '}';
        private const byte keyBinaryMarker = (byte) 'k';
        
        public static LLSD DeserializeBinary(byte[] binaryData)
        {
        
            MemoryStream stream = new MemoryStream( binaryData );
            LLSD llsd = DeserializeBinary( stream );
            stream.Close();
            return llsd;
        }
        
        public static LLSD DeserializeBinary( MemoryStream stream )
        {
            SkipWhiteSpace( stream );
            
            bool result = FindByteArray( stream, binaryHead );
            if ( !result )
                throw new LLSDException( "No binary encoded LLSD." );
            
            return ParseBinaryElement( stream );
         }
 
        public static byte[] SerializeBinary(LLSD llsd)
        {
            MemoryStream stream = SerializeBinaryStream( llsd );
            byte[] binaryData = stream.ToArray();
            stream.Close();
            
            return binaryData;
         }

        public static MemoryStream SerializeBinaryStream(LLSD data)
        {
            MemoryStream stream = new MemoryStream( initialBufferSize );

            stream.Write( binaryHead, 0, binaryHead.Length );
            SerializeBinaryElement( stream, data );
            return stream;
        }
        
        private static void SerializeBinaryElement( MemoryStream stream, LLSD llsd )
        {
            switch( llsd.Type )
            {
                case LLSDType.Unknown:
                    stream.WriteByte( undefBinaryValue );
                    break;
                case LLSDType.Boolean:
                    stream.Write( llsd.AsBinary(), 0, 1 );
                    break;
                case LLSDType.Integer:
                    stream.WriteByte( integerBinaryMarker );
                    stream.Write( llsd.AsBinary(), 0, int32Length );
                    break;
                case LLSDType.Real:
                    stream.WriteByte( realBinaryMarker );
                    stream.Write( llsd.AsBinary(), 0, doubleLength );
                    break;
                case LLSDType.UUID:
                    stream.WriteByte( uuidBinaryMarker );
                    stream.Write( llsd.AsBinary(), 0, 16 );
                    break;
                case LLSDType.String:
                    stream.WriteByte( stringBinaryMarker );
                    byte[] rawString = llsd.AsBinary();
                    byte[] stringLengthNetEnd = HostToNetworkIntBytes( rawString.Length );
                    stream.Write( stringLengthNetEnd, 0, int32Length );
                    stream.Write( rawString, 0, rawString.Length );
                    break;
                case LLSDType.Binary:
                    stream.WriteByte( binaryBinaryMarker );
                    byte[] rawBinary = llsd.AsBinary();
                    byte[] binaryLengthNetEnd = HostToNetworkIntBytes( rawBinary.Length );
                    stream.Write( binaryLengthNetEnd, 0, int32Length );
                    stream.Write( rawBinary, 0, rawBinary.Length );
                    break;
                case LLSDType.Date:
                    stream.WriteByte( dateBinaryMarker );
                    stream.Write( llsd.AsBinary(), 0, doubleLength );
                    break;
                case LLSDType.URI:
                    stream.WriteByte( uriBinaryMarker );
                    byte[] rawURI = llsd.AsBinary();
                    byte[] uriLengthNetEnd = HostToNetworkIntBytes( rawURI.Length );
                    stream.Write( uriLengthNetEnd, 0, int32Length );
                    stream.Write( rawURI, 0, rawURI.Length );
                    break;
                case LLSDType.Array:
                    SerializeBinaryArray( stream, (LLSDArray)llsd );
                    break;
                case LLSDType.Map:
                    SerializeBinaryMap( stream, (LLSDMap)llsd );
                    break;
                default:
                    throw new LLSDException( "Binary serialization: Not existing element discovered." );
                            
            }
        }
        
        private static void SerializeBinaryArray( MemoryStream stream, LLSDArray llsdArray )
        {
            stream.WriteByte( arrayBeginBinaryMarker );
            byte[] binaryNumElementsHostEnd = HostToNetworkIntBytes( llsdArray.Count );
            stream.Write( binaryNumElementsHostEnd, 0, int32Length );
            
            foreach( LLSD llsd in llsdArray )
            {
                SerializeBinaryElement( stream, llsd );
            }
            stream.WriteByte( arrayEndBinaryMarker );
        }
        
        private static void SerializeBinaryMap( MemoryStream stream, LLSDMap llsdMap )
        {
            stream.WriteByte( mapBeginBinaryMarker );
            byte[] binaryNumElementsNetEnd = HostToNetworkIntBytes( llsdMap.Count );
            stream.Write( binaryNumElementsNetEnd, 0, int32Length );
            
            foreach( KeyValuePair<string, LLSD> kvp in llsdMap )
            {
                stream.WriteByte( keyBinaryMarker );
                byte[] binaryKey = Encoding.UTF8.GetBytes( kvp.Key );
                byte[] binaryKeyLength = HostToNetworkIntBytes( binaryKey.Length );
                stream.Write( binaryKeyLength, 0, int32Length );
                stream.Write( binaryKey, 0, binaryKey.Length );
                SerializeBinaryElement( stream, kvp.Value );
            }
            stream.WriteByte( mapEndBinaryMarker );
        }
        
        private static LLSD ParseBinaryElement(MemoryStream stream)
        {                
            SkipWhiteSpace( stream );
            LLSD llsd;
            
            int marker = stream.ReadByte();
            if ( marker < 0 )
                throw new LLSDException( "Binary LLSD parsing:Unexpected end of stream." );
           
            switch( (byte)marker )
            {
                case undefBinaryValue:
                    llsd = new LLSD();
                    break;
                case trueBinaryValue:
                    llsd = LLSD.FromBoolean( true );
                    break;
                case falseBinaryValue:
                    llsd = LLSD.FromBoolean( false );
                    break;
                case integerBinaryMarker:
                    int integer = NetworkToHostInt( ConsumeBytes( stream, int32Length ));
                    llsd = LLSD.FromInteger( integer );
                    break;
                case realBinaryMarker:
                    double dbl = NetworkToHostDouble( ConsumeBytes( stream, doubleLength ));
                    llsd = LLSD.FromReal( dbl );
                    break;
                case uuidBinaryMarker:
                    llsd = LLSD.FromUUID( new LLUUID( ConsumeBytes( stream, 16 ), 0));
                    break;                
                case binaryBinaryMarker:
                    int binaryLength = NetworkToHostInt( ConsumeBytes( stream, int32Length )); 
                    llsd = LLSD.FromBinary( ConsumeBytes( stream, binaryLength ));
                    break;
                case stringBinaryMarker:
                    int stringLength = NetworkToHostInt( ConsumeBytes( stream, int32Length ));
                    string ss = Encoding.UTF8.GetString( ConsumeBytes( stream, stringLength ));
                    llsd = LLSD.FromString( ss );
                    break;
                case uriBinaryMarker:
                    int uriLength = NetworkToHostInt( ConsumeBytes( stream, int32Length ));
                    string sUri = Encoding.UTF8.GetString( ConsumeBytes( stream, uriLength ));
                    Uri uri;
                    try
                    {
                        uri = new Uri( sUri, UriKind.RelativeOrAbsolute );
                    } 
                    catch
                    {
                        throw new LLSDException( "Binary LLSD parsing: Invalid Uri format detected." );
                    }
                    llsd = LLSD.FromUri( uri );
                    break;
                case dateBinaryMarker:
                    double timestamp = NetworkToHostDouble( ConsumeBytes( stream, doubleLength ));
                    DateTime dateTime = DateTime.SpecifyKind( Helpers.Epoch, DateTimeKind.Utc );
                    dateTime = dateTime.AddSeconds(timestamp);
                    llsd = LLSD.FromDate( dateTime.ToLocalTime() );
                    break;
                case arrayBeginBinaryMarker:
                    llsd = ParseBinaryArray( stream );
                    break;
                case mapBeginBinaryMarker:
                    llsd = ParseBinaryMap( stream );
                    break;
                default:
                    throw new LLSDException( "Binary LLSD parsing: Unknown type marker." );
                          
            }
            return llsd;
        }
        
        private static LLSD ParseBinaryArray(MemoryStream stream)
        {
            int numElements = NetworkToHostInt( ConsumeBytes( stream, int32Length ));
            int crrElement = 0;
            LLSDArray llsdArray = new LLSDArray();
            while ( crrElement < numElements ) 
            {
                llsdArray.Add( ParseBinaryElement( stream ));
                crrElement++;
            }
                    
            if ( !FindByte( stream, arrayEndBinaryMarker ))
                throw new LLSDException( "Binary LLSD parsing: Missing end marker in array." );
                
            return (LLSD)llsdArray;
        }
        
        private static LLSD ParseBinaryMap(MemoryStream stream)
        {
            int numElements = NetworkToHostInt( ConsumeBytes( stream, int32Length ));
            int crrElement = 0;
            LLSDMap llsdMap = new LLSDMap();
            while( crrElement < numElements )
            {
                if (!FindByte( stream, keyBinaryMarker ))
                    throw new LLSDException( "Binary LLSD parsing: Missing key marker in map." );
                int keyLength = NetworkToHostInt( ConsumeBytes( stream, int32Length ));
                string key = Encoding.UTF8.GetString( ConsumeBytes( stream, keyLength ));
                llsdMap[key] = ParseBinaryElement( stream );
                crrElement++;
            }
            
            if ( !FindByte( stream, mapEndBinaryMarker ))
                throw new LLSDException( "Binary LLSD parsing: Missing end marker in map." );
            
            return (LLSD)llsdMap;
        }
        
        public static void SkipWhiteSpace(MemoryStream stream)
        {
            int bt;
            
            while ( ((bt = stream.ReadByte()) > 0) && 
                ( (byte)bt == ' ' || (byte)bt == '\t' ||
                  (byte)bt == '\n' || (byte)bt == '\r' ) 
                 )
            {
            }
            stream.Seek( -1, SeekOrigin.Current );
        }
            
        public static bool FindByte( MemoryStream stream, byte toFind )
        {
            int bt = stream.ReadByte();
            if ( bt < 0  )
                return false;
            if ( (byte)bt == toFind )
                return true;
            else 
            {
                stream.Seek( -1L, SeekOrigin.Current );                    
                return false;
            }       
        }
               
        
        public static bool FindByteArray( MemoryStream stream, byte[] toFind )
        {
            int lastIndexToFind = toFind.Length - 1;
            int crrIndex = 0;            
            bool found = true;
            int bt;
            long lastPosition = stream.Position;
            
            while( found && 
                  ((bt = stream.ReadByte()) > 0) &&
                    (crrIndex <= lastIndexToFind)
                  )
            {
                if ( toFind[crrIndex] == (byte)bt ) {
                    found = true;
                    crrIndex++;
                }
                else
                    found = false;
            }
            
            if ( found && crrIndex > lastIndexToFind ) 
            {
                stream.Seek( -1L, SeekOrigin.Current );
                return true;
            }
            else 
            {
                stream.Position = lastPosition;
                return false;
            }
        }
        
        public static byte[] ConsumeBytes( MemoryStream stream, int consumeBytes ) 
        {
            byte[] bytes = new byte[consumeBytes];
            if (stream.Read( bytes, 0, consumeBytes ) < consumeBytes )
                throw new LLSDException( "Binary LLSD parsing: Unexpected end of stream." );
            return bytes;
        }
        
        public static int NetworkToHostInt( byte[] binaryNetEnd )
        {
            if ( binaryNetEnd == null )
                return -1;
            
            int intNetEnd = BitConverter.ToInt32( binaryNetEnd, 0 );
            int intHostEnd = System.Net.IPAddress.NetworkToHostOrder( intNetEnd );
            return intHostEnd;
        }
        
        public static double NetworkToHostDouble( byte[] binaryNetEnd )
        {
            if ( binaryNetEnd == null )
                return -1d;
            long longNetEnd = BitConverter.ToInt64( binaryNetEnd, 0 );
            long longHostEnd = System.Net.IPAddress.NetworkToHostOrder( longNetEnd );
            byte[] binaryHostEnd = BitConverter.GetBytes( longHostEnd );
            double doubleHostEnd = BitConverter.ToDouble( binaryHostEnd, 0 );
            return doubleHostEnd; 
        }
                
        public static byte[] HostToNetworkIntBytes( int intHostEnd )
        {
            int intNetEnd = System.Net.IPAddress.HostToNetworkOrder( intHostEnd );
            byte[] bytesNetEnd = BitConverter.GetBytes( intNetEnd );
            return bytesNetEnd;
            
        }
                    
     }
 }