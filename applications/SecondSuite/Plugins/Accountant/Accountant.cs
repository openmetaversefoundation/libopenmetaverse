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
using SecondSuite.Plugins;

namespace SecondSuite.Plugins
{
	/// <summary>
	/// Accountant plugin implementation
	/// </summary>
	public class Accountant : SSPlugin
	{
		public string Name { get { return "Accountant"; } }
		public string Author { get { return "John Hurliman"; } }
		public string Homepage { get { return "http://www.highenergychemistry.com/"; } }
		public string Description { get { return "Transfer money between accounts"; } }
		public bool SecondLifeClient { get { return true; } }
		public ConnectionEvent ConnectionHandler { get { return OnConnection; } }
		public override string ToString() { return Name; }

		private frmAccountant Form;
		private ConnectionEvent OnConnection;

		public void Init(SecondLife client)
		{
			Form = new frmAccountant(client);
			OnConnection = new ConnectionEvent(Form.Connected);
		}

		public System.Windows.Forms.Form Load()
		{
			return Form;
		}

		public void Shutdown()
		{
			if (Form != null)
			{
				Form.Close();
			}
		}
	}
}
