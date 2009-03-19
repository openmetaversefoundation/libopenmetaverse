/*
 * Copyright (c) 2009, openmetaverse.org
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
namespace OpenMetaverse.TestClient.Commands.Movement
{
    class TurnToCommand : Command
    {
        public TurnToCommand(TestClient client)
        {
            Name = "turnto";
            Description = "Turns the avatar looking to a specified point. Usage: turnto x y z";
            Category = CommandCategory.Movement;
        }
        public override string Execute(string[] args, UUID fromAgentID)
        {
            if (args.Length != 3)
                return "Usage: turnto x y z";
            double x, y, z;
            if (!Double.TryParse(args[0], out x) ||
                !Double.TryParse(args[1], out y) ||
                !Double.TryParse(args[2], out z))
            {
                return "Usage: turnto x y z";
            }

            Vector3 newDirection;
            newDirection.X = (float)x;
            newDirection.Y = (float)y;
            newDirection.Z = (float)z;
            Client.Self.Movement.TurnToward(newDirection);
            Client.Self.Movement.SendUpdate(false);
            return "Turned to ";
        }
    }
}