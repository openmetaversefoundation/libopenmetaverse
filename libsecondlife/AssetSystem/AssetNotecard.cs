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

using libsecondlife;

namespace libsecondlife.AssetSystem
{
	/// <summary>
	/// Summary description for AssetNotecard.
	/// </summary>
	public class AssetNotecard : Asset
	{
		private string _Body = "";
		public string Body
		{
			get { return _Body; }
			set
			{
				_Body = value.Replace("\r", "");
				setAsset( _Body );
			}
		}

        /// <summary>
        /// </summary>
        /// <param name="assetID"></param>
        /// <param name="body"></param>
        public AssetNotecard(LLUUID assetID, string body)
            : base(assetID, (sbyte)Asset.AssetType.Notecard, false, null)
		{
			_Body = body;
			setAsset( body );
		}

        /// <summary>
        /// </summary>
        /// <param name="assetID"></param>
        /// <param name="assetData"></param>
        public AssetNotecard(LLUUID assetID, byte[] assetData)
            : base(assetID, (sbyte)Asset.AssetType.Notecard, false, null)
		{
			_AssetData = assetData;

			string temp	= System.Text.Encoding.UTF8.GetString(assetData).Trim();

			// TODO: Calculate the correct header size to look for
			// it's usually around 80 or so...
			if( temp.Length > 50 )
			{
				// Trim trailing null terminator
				temp = temp.Substring(0,temp.Length-1);

				// Remove the header
				temp = temp.Substring(temp.IndexOf("}") + 2);
				temp = temp.Substring(temp.IndexOf('\n') + 1);

				// Remove trailing close brace
				temp = temp.Substring(0,temp.Length-2);
			}
			_Body = temp;
		}

        /// <summary>
        /// </summary>
        /// <param name="body"></param>
        private void setAsset(string body)
		{
			// Format the string body into Linden text
			string lindenText = "Linden text version 1\n";
			lindenText += "{\n";
			lindenText += "LLEmbeddedItems version 1\n";
			lindenText += "{\n";
			lindenText += "count 0\n";
			lindenText += "}\n";
			lindenText += "Text length " + body.Length + "\n";
			lindenText += body;
			lindenText += "}\n";
			


			// Assume this is a string, add 1 for the null terminator
			byte[] stringBytes = System.Text.Encoding.UTF8.GetBytes((string)lindenText);
			byte[] assetData = new byte[stringBytes.Length + 1];
            Buffer.BlockCopy(stringBytes, 0, assetData, 0, stringBytes.Length);

			SetAssetData( assetData );
		}
	}
}
