using System;
using System.Collections.Generic;
using System.Threading;

using libsecondlife;


namespace libsecondlife.TestClient
{
    public class FindObjectsCommand : Command
    {
        Dictionary<LLUUID, Primitive> PrimsWaiting = new Dictionary<LLUUID, Primitive>();
        AutoResetEvent AllPropertiesReceived = new AutoResetEvent(false);

        public FindObjectsCommand(TestClient testClient)
        {
            testClient.Objects.OnObjectProperties += new ObjectManager.ObjectPropertiesCallback(Objects_OnObjectProperties);

            Name = "findobjects";
            Description = "Finds all objects, which name contains search-string. " +
                "Usage: findobjects [radius] <search-string>";
        }

        public override string Execute(string[] args, LLUUID fromAgentID)
        {
            // *** parse arguments ***
            if ((args.Length < 1) || (args.Length > 2))
                return "Usage: findobjects [radius] <search-string>";
            float radius = float.Parse(args[0]);
            string searchString = (args.Length > 1)? args[1] : "";

            // *** get current location ***
            LLVector3 location = Client.Self.SimPosition;

            // *** find all objects in radius ***
            List<Primitive> prims = Client.Network.CurrentSim.ObjectsPrimitives.FindAll(
                delegate(Primitive prim) {
                    LLVector3 pos = prim.Position;
                    return ((prim.ParentID == 0) && (pos != LLVector3.Zero) && (LLVector3.Dist(pos, location) < radius));
                }
            );

            // *** request properties of these objects ***
            bool complete = RequestObjectProperties(prims, 250);

            foreach (Primitive p in prims) {
                string name = p.Properties.Name;
                if ((name != null) && (name.Contains(searchString)))
                    Console.WriteLine(String.Format("Object '{0}': {1}", name, p.ID.ToString()));
            }

            if (!complete) {
                Console.WriteLine("Warning: Unable to retrieve full properties for:");
                foreach (LLUUID uuid in PrimsWaiting.Keys)
                    Console.WriteLine(uuid);
            }

            return "Done searching";
        }

        private bool RequestObjectProperties(List<Primitive> objects, int msPerRequest)
        {
            // Create an array of the local IDs of all the prims we are requesting properties for
            uint[] localids = new uint[objects.Count];

            lock (PrimsWaiting) {
                PrimsWaiting.Clear();

                for (int i = 0; i < objects.Count; ++i) {
                    localids[i] = objects[i].LocalID;
                    PrimsWaiting.Add(objects[i].ID, objects[i]);
                }
            }

            Client.Objects.SelectObjects(Client.Network.CurrentSim, localids);

            return AllPropertiesReceived.WaitOne(2000 + msPerRequest * objects.Count, false);
        }

        void Objects_OnObjectProperties(Simulator simulator, LLObject.ObjectProperties properties)
        {
            lock (PrimsWaiting) {
                Primitive prim;
                if (PrimsWaiting.TryGetValue(properties.ObjectID, out prim)) {
                    prim.Properties = properties;
                }
                PrimsWaiting.Remove(properties.ObjectID);

                if (PrimsWaiting.Count == 0)
                    AllPropertiesReceived.Set();
            }
        }
    }
}
