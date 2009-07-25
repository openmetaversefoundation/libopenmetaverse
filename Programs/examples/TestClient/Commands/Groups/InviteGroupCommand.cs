using System;
using System.Collections.Generic;
using System.Threading;
using OpenMetaverse;
using OpenMetaverse.Packets;
using System.Text;

namespace OpenMetaverse.TestClient
{
    public class InviteGroupCommand : Command
    {
        public InviteGroupCommand(TestClient testClient)
        {
            Name = "invitegroup";
            Description = "invite an avatar into a group. Usage: invitegroup AvatarUUID GroupUUID RoleUUID*";
            Category = CommandCategory.Groups;
        }

        public override string Execute(string[] args, UUID fromAgentID)
        {
            if (args.Length < 2)
                return Description;

            UUID avatar = UUID.Zero;
            UUID group = UUID.Zero;
            UUID role = UUID.Zero;
            List<UUID> roles = new List<UUID>();

            if (!UUID.TryParse(args[0], out avatar))
                    return "parse error avatar UUID";
            if (!UUID.TryParse(args[1], out group))
                    return "parse error group UUID";
            if (2 == args.Length)
                    roles.Add(UUID.Zero);
	    else
            for (int i = 2; i < args.Length; i++)
                if (UUID.TryParse(args[i], out role))
                    roles.Add(role);
                
            Client.Groups.Invite(group, roles, avatar);

            return "invited "+avatar+" to "+group;
        }
    }
}
