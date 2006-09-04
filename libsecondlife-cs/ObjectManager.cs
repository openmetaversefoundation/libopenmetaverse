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
using System.Collections;

namespace libsecondlife
{
    public delegate void NewPrimCallback(Simulator simulator, PrimObject prim);
    public delegate void NewAvatarCallback(Simulator simulator, Avatar avatar);
    public delegate void PrimMovedCallback(Simulator simulator, PrimObject prim);
    public delegate void AvatarMovedCallback(Simulator simulator, Avatar avatar);

	/// <summary>
	/// Tracks all the objects (avatars and prims) in a region
	/// </summary>
	public class ObjectManager
    {
        private SecondLife Client;

        public ArrayList Prims;
        public ArrayList Avatars;

        public ObjectManager(SecondLife client)
        {
            Client = client;

            Prims = new ArrayList();
            Avatars = new ArrayList();

            Client.Network.RegisterCallback("ObjectUpdate", new PacketCallback(UpdateHandler));
            Client.Network.RegisterCallback("ImprovedTerseObjectUpdate", new PacketCallback(TerseUpdateHandler));
        }

        private void UpdateHandler(Packet packet, Simulator simulator)
        {
            // Create a PrimObject or Avatar and add it to Prims/Avatars then fire the callback
        }

        private void TerseUpdateHandler(Packet packet, Simulator simulator)
        {
            // Find the referenced PrimObject or Avatar and update it, then fire the callback.
            // If no PrimObject/Avatar exists with the given LocalID request an update
        }
    }
}
