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
using System.Collections.Generic;

namespace libsecondlife
{
    public class ObjectTracker
    {
        internal Dictionary<uint, Avatar> Avatars = new Dictionary<uint, Avatar>();
        internal Dictionary<uint, Primitive> Prims = new Dictionary<uint, Primitive>();

        #region Properties

        public int AvatarCount
        {
            get { return Avatars.Count; }
        }

        public int PrimCount
        {
            get { return Prims.Count; }
        }

        #endregion Properties

        public bool TryGetValue(uint objectLocalID, out LLObject obj)
        {
            Avatar avatar;
            Primitive prim;

            if (Avatars.TryGetValue(objectLocalID, out avatar))
            {
                obj = avatar;
                return true;
            }

            if (Prims.TryGetValue(objectLocalID, out prim))
            {
                obj = prim;
                return true;
            }

            obj = null;
            return false;
        }

        public bool TryGetAvatar(uint avatarLocalID, out Avatar avatar)
        {
            return Avatars.TryGetValue(avatarLocalID, out avatar);
        }

        public bool TryGetPrimitive(uint primLocalID, out Primitive prim)
        {
            return Prims.TryGetValue(primLocalID, out prim);
        }

        public List<Primitive> FindAll(Predicate<Primitive> match)
        {
            List<Primitive> found = new List<Primitive>();
            lock (Prims)
            {
                foreach (KeyValuePair<uint, Primitive> kvp in Prims)
                {
                    if (match(kvp.Value))
                        found.Add(kvp.Value);
                }
            }
            return found;
        }

        public List<Avatar> FindAll(Predicate<Avatar> match)
        {
            List<Avatar> found = new List<Avatar>();
            lock (Avatars)
            {
                foreach (KeyValuePair<uint, Avatar> kvp in Avatars)
                {
                    if (match(kvp.Value))
                        found.Add(kvp.Value);
                }
            }
            return found;
        }

        public Primitive Find(Predicate<Primitive> match)
        {
            lock (Prims)
            {
                foreach (Primitive prim in Prims.Values)
                {
                    if (match(prim))
                        return prim;
                }
            }
            return null;
        }

        public Avatar Find(Predicate<Avatar> match)
        {
            lock (Avatars)
            {
                foreach (Avatar avatar in Avatars.Values)
                {
                    if (match(avatar))
                        return avatar;
                }
            }
            return null;
        }

        public void ForEach(Action<Primitive> action)
        {
            lock (Prims)
            {
                foreach (Primitive prim in Prims.Values)
                {
                    action(prim);
                }
            }
        }

        public void ForEach(Action<Avatar> action)
        {
            lock (Avatars)
            {
                foreach (Avatar avatar in Avatars.Values)
                {
                    action(avatar);
                }
            }
        }
    }
}
