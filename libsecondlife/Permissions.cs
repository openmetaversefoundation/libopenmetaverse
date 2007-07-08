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

namespace libsecondlife
{
    /// <summary>
    /// 
    /// </summary>
    [Flags]
    public enum PermissionMask : uint
    {
        None        = 0,
        Transfer    = 1 << 13,
        Modify      = 1 << 14,
        Copy        = 1 << 15,
        [Obsolete]
        EnterParcel = 1 << 16,
        [Obsolete]
        Terraform   = 1 << 17,
        [Obsolete]
        OwnerDebit  = 1 << 18,
        Move        = 1 << 19,
        Damage      = 1 << 20,
        All         = 0x7FFFFFFF
    }

    /// <summary>
    /// 
    /// </summary>
    [Flags]
    public enum PermissionWho : byte
    {
        /// <summary></summary>
        Owner = 0x02,
        /// <summary></summary>
        Group = 0x04,
        /// <summary></summary>
        Everyone = 0x08,
        /// <summary></summary>
        NextOwner = 0x10,
        /// <summary></summary>
        All = 0x1E
    }

    /// <summary>
    /// 
    /// </summary>
    public struct Permissions
    {
        public PermissionMask BaseMask;
        public PermissionMask EveryoneMask;
        public PermissionMask GroupMask;
        public PermissionMask NextOwnerMask;
        public PermissionMask OwnerMask;

        public Permissions(uint baseMask, uint everyoneMask, uint groupMask, uint nextOwnerMask, uint ownerMask)
        {
            BaseMask = (PermissionMask)baseMask;
            EveryoneMask = (PermissionMask)everyoneMask;
            GroupMask = (PermissionMask)groupMask;
            NextOwnerMask = (PermissionMask)nextOwnerMask;
            OwnerMask = (PermissionMask)ownerMask;
        }

        public override string ToString()
        {
            return String.Format("Base: {0}, Everyone: {1}, Group: {2}, NextOwner: {3}, Owner: {4}",
                BaseMask, EveryoneMask, GroupMask, NextOwnerMask, OwnerMask);
        }
    }
}
