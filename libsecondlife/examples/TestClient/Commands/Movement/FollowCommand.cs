using System;
using System.Collections.Generic;
using System.Text;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestClient
{
    public class FollowCommand: Command
    {
		public FollowCommand(TestClient testClient)
		{
			Name = "follow";
			Description = "Follow another avatar. (usage: follow [FirstName LastName])  If no target is set then will follow master.";

            testClient.Network.RegisterCallback(PacketType.AlertMessage, new NetworkManager.PacketCallback(AlertMessageHandler));
		}

        public override string Execute(string[] args, LLUUID fromAgentID)
		{
			string target = String.Empty;
			for (int ct = 0; ct < args.Length; ct++)
				target = target + args[ct] + " ";
			target = target.TrimEnd();

            if (target.Length > 0)
            {
                if (Follow(target))
                    return "Following " + target;
                else
                    return "Unable to follow " + target + ".  Client may not be able to see that avatar.";
            }
            else
            {
                if (Follow(Client.MasterKey))
                    return "Following " + Client.MasterKey;
                else
                    return "No target specified and no master not found. usage: follow [FirstName LastName])";
            }
		}

        const float DISTANCE_BUFFER = 3.0f;
        uint targetLocalID = 0;

        bool Follow(string name)
        {
            lock (Client.Network.Simulators)
            {
                for (int i = 0; i < Client.Network.Simulators.Count; i++)
                {
                    Avatar target = Client.Network.Simulators[i].ObjectsAvatars.Find(
                        delegate(Avatar avatar)
                        {
                            return avatar.Name == name;
                        }
                    );

                    if (target != null)
                    {
                        targetLocalID = target.LocalID;
                        Active = true;
                        return true;
                    }
                }
            }

            Active = false;
            return false;
        }

        bool Follow(LLUUID id)
        {
            lock (Client.Network.Simulators)
            {
                for (int i = 0; i < Client.Network.Simulators.Count; i++)
                {
                    Avatar target = Client.Network.Simulators[i].ObjectsAvatars.Find(
                        delegate(Avatar avatar)
                        {
                            return avatar.ID == id;
                        }
                    );

                    if (target != null)
                    {
                        targetLocalID = target.LocalID;
                        Active = true;
                        return true;
                    }
                }
            }

            Active = false;
            return false;
        }

		public override void Think()
		{
            // Find the target position
            lock (Client.Network.Simulators)
            {
                for (int i = 0; i < Client.Network.Simulators.Count; i++)
                {
                    Avatar targetAv;

                    if (Client.Network.Simulators[i].ObjectsAvatars.TryGetValue(targetLocalID, out targetAv))
                    {
                        float distance = 0.0f;

                        if (Client.Network.Simulators[i] == Client.Network.CurrentSim)
                        {
                            distance = LLVector3.Dist(targetAv.Position, Client.Self.SimPosition);
                        }
                        else
                        {
                            // FIXME: Calculate global distances
                        }

                        if (distance > DISTANCE_BUFFER)
                        {
                            uint regionX, regionY;
                            Helpers.LongToUInts(Client.Network.Simulators[i].Handle, out regionX, out regionY);

                            double xTarget = (double)targetAv.Position.X + (double)regionX;
                            double yTarget = (double)targetAv.Position.Y + (double)regionY;
                            double zTarget = targetAv.Position.Z - 2f;

                            Client.DebugLog(String.Format("[Autopilot] {0} meters away from the target, starting autopilot to <{1},{2},{3}>",
                                distance, xTarget, yTarget, zTarget));

                            Client.Self.AutoPilot(xTarget, yTarget, zTarget);
                        }
                        else
                        {
                            // We are in range of the target and moving, stop moving
                            Client.Self.AutoPilotCancel();
                        }
                    }
                }
            }

			base.Think();
		}

        private void AlertMessageHandler(Packet packet, Simulator simulator)
        {
            AlertMessagePacket alert = (AlertMessagePacket)packet;
            string message = Helpers.FieldToUTF8String(alert.AlertData.Message);

            if (message.Contains("Autopilot cancel"))
            {
                Client.Log("Server cancelled the autopilot", Helpers.LogLevel.Info);
            }
        }
    }
}
