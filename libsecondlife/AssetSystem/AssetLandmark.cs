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
	/// Summary description for AssetLandmark.
	/// </summary>
	public class AssetLandmark : Asset
	{
		internal int _Version = 0;
		
		public int Version
		{
			get { return _Version; }
		}
		
		internal LLVector3 _Pos = LLVector3.Zero;
		public LLVector3 Pos
		{
			get { return _Pos; }
			set {
				_Pos = value;
				setAsset();
			}
		}
		
		internal LLUUID _Region = LLUUID.Zero;
		public LLUUID Region
		{
			get { return _Region; }
			set {
				_Region = value;
				setAsset();
			}
		}
		
		private string _Body = "";
		public string Body
		{
			get { return _Body; }
		}

        /// <summary>
        /// </summary>
        /// <param name="assetID"></param>
        /// <param name="assetData"></param>
        public AssetLandmark(LLUUID assetID, byte[] assetData)
            : base(assetID, (sbyte)Asset.AssetType.Landmark, false, null)
		{
			_AssetData = assetData;

			string temp	= System.Text.Encoding.UTF8.GetString(assetData).Trim();
			processLandmark(temp);
			_Body = temp;
		}
		
		private void processLandmark(string temp)
		{
			Console.Write(temp + "\n");
			string[] parts = temp.Split('\n');
			int.TryParse(parts[0].Substring(17, 1), out _Version);
			LLUUID.TryParse(parts[1].Substring(10, 36), out _Region);
			LLVector3.TryParse(parts[2].Substring(11, parts[2].Length - 11), out _Pos);
		}

	        private void setAsset()
		{
			string body = "Landmark version " + _Version.ToString() + "\n";
			body += "region_id " + _Region.ToStringHyphenated() + "\n";
			body += "local_pos " + _Pos.X.ToString() + " " + _Pos.Y.ToString() + " " + _Pos.Z.ToString();
			// Assume this is a string, add 1 for the null terminator
			byte[] stringBytes = System.Text.Encoding.UTF8.GetBytes((string)body);
			byte[] assetData = new byte[stringBytes.Length + 1];
	                Buffer.BlockCopy(stringBytes, 0, assetData, 0, stringBytes.Length);

			SetAssetData( assetData );
		}
	}
}
