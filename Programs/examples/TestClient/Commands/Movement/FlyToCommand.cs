using System;

namespace OpenMetaverse.TestClient.Commands.Movement
{
    class FlyToCommand : Command
    {

        Vector3 myPos = new Vector3();
        Vector2 myPos0 = new Vector2();
        Vector3 target = new Vector3();
        Vector2 target0 = new Vector2();
        float diff, olddiff, saveolddiff;
        int startTime = 0;
        int duration = 10000;
        bool running = false;

        public FlyToCommand(TestClient Client)
        {
            Name = "FlyTo";
            Description = "Fly the avatar toward the specified position for a maximum of seconds. Usage: FlyTo x y z [seconds]";
            Category = CommandCategory.Movement;
        }

        public override string Execute(string[] args, UUID fromAgentID)
        {
            if (args.Length > 4 || args.Length < 3)
                return "Usage: FlyTo x y z [seconds]";

            if (!float.TryParse(args[0], out target.X) ||
                !float.TryParse(args[1], out target.Y) ||
                !float.TryParse(args[2], out target.Z))
            {
                return "Usage: FlyTo x y z [seconds]";
            }

            if (running)
                return "Already in progress, wait for the previous FlyTo to finish";

            running = true;

            // Subscribe to terse update events while this command is running
            Client.Objects.TerseObjectUpdate += Objects_OnObjectUpdated;

            target0.X = target.X;
            target0.Y = target.Y;

            if (args.Length == 4 && Int32.TryParse(args[3], out duration))
                duration *= 1000;

            startTime = Environment.TickCount;
            Client.Self.Movement.Fly = true;
            Client.Self.Movement.AtPos = true;
            Client.Self.Movement.AtNeg = false;
            ZMovement();
            Client.Self.Movement.TurnToward(target);

            return string.Format("flying to {0} in {1} seconds", target.ToString(), duration / 1000);
        }

        private void Objects_OnObjectUpdated(object sender, TerseObjectUpdateEventArgs e)
        {
            if (startTime == 0) return;
            if (e.Update.LocalID == Client.Self.LocalID)
            {
                XYMovement();
                ZMovement();
                if (Client.Self.Movement.AtPos || Client.Self.Movement.AtNeg)
                {
                    Client.Self.Movement.TurnToward(target);
                    Debug("Flyxy ");
                }
                else if (Client.Self.Movement.UpPos || Client.Self.Movement.UpNeg)
                {
                    Client.Self.Movement.TurnToward(target);
                    //Client.Self.Movement.SendUpdate(false);
                    Debug("Fly z ");
                }
                else if (Vector3.Distance(target, Client.Self.SimPosition) <= 2.0)
                {
                    EndFlyto();
                    Debug("At Target");
                }
            }
            if (Environment.TickCount - startTime > duration)
            {
                EndFlyto();
                Debug("End Flyto");
            }
        }

        private bool XYMovement()
        {
            bool res = false;

            myPos = Client.Self.SimPosition;
            myPos0.X = myPos.X;
            myPos0.Y = myPos.Y;
            diff = Vector2.Distance(target0, myPos0);
            Vector2 vvel = new Vector2(Client.Self.Velocity.X, Client.Self.Velocity.Y);
            float vel = vvel.Length();
            if (diff >= 10.0)
            {
                Client.Self.Movement.AtPos = true;
                
                res = true;
            }
            else if (diff >= 2 && vel < 5)
            {
                Client.Self.Movement.AtPos = true;
            }
            else
            {
                Client.Self.Movement.AtPos = false;
                Client.Self.Movement.AtNeg = false;
            }
            saveolddiff = olddiff;
            olddiff = diff;
            return res;
        }

        private void ZMovement()
        {
            Client.Self.Movement.UpPos = false;
            Client.Self.Movement.UpNeg = false;
            float diffz = (target.Z - Client.Self.SimPosition.Z);
            if (diffz >= 20.0)
                Client.Self.Movement.UpPos = true;
            else if (diffz <= -20.0)
                Client.Self.Movement.UpNeg = true;
            else if (diffz >= +5.0 && Client.Self.Velocity.Z < +4.0)
                Client.Self.Movement.UpPos = true;
            else if (diffz <= -5.0 && Client.Self.Velocity.Z > -4.0)
                Client.Self.Movement.UpNeg = true;
            else if (diffz >= +2.0 && Client.Self.Velocity.Z < +1.0)
                Client.Self.Movement.UpPos = true;
            else if (diffz <= -2.0 && Client.Self.Velocity.Z > -1.0)
                Client.Self.Movement.UpNeg = true;
        }

        private void EndFlyto()
        {
            // Unsubscribe from terse update events
            Client.Objects.TerseObjectUpdate -= Objects_OnObjectUpdated;

            startTime = 0;
            Client.Self.Movement.AtPos = false;
            Client.Self.Movement.AtNeg = false;
            Client.Self.Movement.UpPos = false;
            Client.Self.Movement.UpNeg = false;
            Client.Self.Movement.SendUpdate(false);

            running = false;
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private void Debug(string x)
        {
            Console.WriteLine(x + " {0,3:##0} {1,3:##0} {2,3:##0} diff {3,5:##0.0} olddiff {4,5:##0.0}  At:{5,5} {6,5}  Up:{7,5} {8,5}  v: {9} w: {10}",
                myPos.X, myPos.Y, myPos.Z, diff, saveolddiff,
                Client.Self.Movement.AtPos, Client.Self.Movement.AtNeg, Client.Self.Movement.UpPos, Client.Self.Movement.UpNeg,
                Client.Self.Velocity.ToString(), Client.Self.AngularVelocity.ToString());
        }
    }
}
