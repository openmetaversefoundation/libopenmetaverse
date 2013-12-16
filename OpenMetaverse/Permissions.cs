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
using OpenMetaverse.StructuredData;

namespace OpenMetaverse
{
    /// <summary>
    /// 
    /// </summary>
    [Flags]
    public enum PermissionMask : uint
    {
        None = 0,
        Transfer = 1 << 13,
        Modify = 1 << 14,
        Copy = 1 << 15,
        Export = 1 << 16,
        Move = 1 << 19,
        Damage = 1 << 20,
        // All does not contain Export, which is special and must be
        // explicitly given
        All = (1 << 13) | (1 << 14) | (1 << 15) | (1 << 19)
    }

    /// <summary>
    /// 
    /// </summary>
    [Flags]
    public enum PermissionWho : byte
    {
        /// <summary></summary>
        Base = 0x01,
        /// <summary></summary>
        Owner = 0x02,
        /// <summary></summary>
        Group = 0x04,
        /// <summary></summary>
        Everyone = 0x08,
        /// <summary></summary>
        NextOwner = 0x10,
        /// <summary></summary>
        All = 0x1F
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable()]
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

        public Permissions GetNextPermissions()
        {
            uint nextMask = (uint)NextOwnerMask;

            return new Permissions(
                (uint)BaseMask & nextMask,
                (uint)EveryoneMask & nextMask,
                (uint)GroupMask & nextMask,
                (uint)NextOwnerMask,
                (uint)OwnerMask & nextMask
                );
        }

        public OSD GetOSD()
        {
            OSDMap permissions = new OSDMap(5);
            permissions["base_mask"] = OSD.FromInteger((uint)BaseMask);
            permissions["everyone_mask"] = OSD.FromInteger((uint)EveryoneMask);
            permissions["group_mask"] = OSD.FromInteger((uint)GroupMask);
            permissions["next_owner_mask"] = OSD.FromInteger((uint)NextOwnerMask);
            permissions["owner_mask"] = OSD.FromInteger((uint)OwnerMask);
            return permissions;
        }

        public static Permissions FromOSD(OSD llsd)
        {
            Permissions permissions = new Permissions();
            OSDMap map = llsd as OSDMap;

            if (map != null)
            {
                permissions.BaseMask = (PermissionMask)map["base_mask"].AsUInteger();
                permissions.EveryoneMask = (PermissionMask)map["everyone_mask"].AsUInteger();
                permissions.GroupMask = (PermissionMask)map["group_mask"].AsUInteger();
                permissions.NextOwnerMask = (PermissionMask)map["next_owner_mask"].AsUInteger();
                permissions.OwnerMask = (PermissionMask)map["owner_mask"].AsUInteger();
            }

            return permissions;
        }

        public override string ToString()
        {
            return String.Format("Base: {0}, Everyone: {1}, Group: {2}, NextOwner: {3}, Owner: {4}",
                BaseMask, EveryoneMask, GroupMask, NextOwnerMask, OwnerMask);
        }

        public override int GetHashCode()
        {
            return BaseMask.GetHashCode() ^ EveryoneMask.GetHashCode() ^ GroupMask.GetHashCode() ^
                NextOwnerMask.GetHashCode() ^ OwnerMask.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return (obj is Permissions) ? this == (Permissions)obj : false;
        }

        public bool Equals(Permissions other)
        {
            return this == other;
        }

        public static bool operator ==(Permissions lhs, Permissions rhs)
        {
            return (lhs.BaseMask == rhs.BaseMask) && (lhs.EveryoneMask == rhs.EveryoneMask) &&
                (lhs.GroupMask == rhs.GroupMask) && (lhs.NextOwnerMask == rhs.NextOwnerMask) &&
                (lhs.OwnerMask == rhs.OwnerMask);
        }

        public static bool operator !=(Permissions lhs, Permissions rhs)
        {
            return !(lhs == rhs);
        }

        public static bool HasPermissions(PermissionMask perms, PermissionMask checkPerms)
        {
            return (perms & checkPerms) == checkPerms;
        }

        public static readonly Permissions NoPermissions = new Permissions();
        public static readonly Permissions FullPermissions = new Permissions((uint)PermissionMask.All, (uint)PermissionMask.All,
            (uint)PermissionMask.All, (uint)PermissionMask.All, (uint)PermissionMask.All);
    }
}
