using libsecondlife;
using libsecondlife.Packets;
using System;
using System.Collections.Generic;
using System.Text;

namespace libsecondlife
{
    public class LocationTracker
    {
        private SecondLife Client;
        private List<LLVector3> _Locations = new List<LLVector3>();
        private int _MyIndex;
        private int _PreyIndex;

        public delegate void OnLocationUpdateDelegate(List<LLVector3> locations, int me, int prey);

        /// <summary>
        /// Triggered when a CourseLocationUpdate packet is received from the simulator
        /// </summary>
        public event OnLocationUpdateDelegate OnLocationUpdate;

        /// <summary>
        /// Course locations used to populate the mini-map
        /// </summary>
        public List<LLVector3> Locations
        {
            get { return _Locations; }
        }

        /// <summary>
        /// Locations[MyIndex] = your avatar location
        /// </summary>
        public int MyIndex
        {
            get { return _MyIndex; }
        }

        /// <summary>
        /// Locations[MyIndex] = TrackAgent target location
        /// </summary>
        public int PreyIndex
        {
            get { return _PreyIndex; }
        }

        /// <summary>Provides access to the most recent CoarseLocationUpdate
        /// information, which can be used to populate the mini-map.
        /// </summary>
        /// <param name="client"></param>
        public LocationTracker(SecondLife client)
        {
            Client = client;
            Client.Network.RegisterCallback(PacketType.CoarseLocationUpdate, new NetworkManager.PacketCallback(Network_CoarseLocationUpdate));
            this.OnLocationUpdate += new OnLocationUpdateDelegate(LocationTracker_OnLocationUpdate);
        }

        /// <summary>
        /// Tracks an agent: Locations[MyIndex] = target location
        /// </summary>
        /// <param name="preyID">UUID of the avatar to track</param>
        public void TrackAgent(LLUUID preyID)
        {
            TrackAgentPacket p = new TrackAgentPacket();
            p.AgentData.SessionID = Client.Network.SessionID;
            p.AgentData.AgentID = Client.Network.AgentID;
            p.TargetData.PreyID = preyID;
            Client.Network.SendPacket(p);
        }

        void Network_CoarseLocationUpdate(Packet packet, Simulator simulator)
        {
            CoarseLocationUpdatePacket update = (CoarseLocationUpdatePacket)packet;
            CoarseLocationUpdatePacket.LocationBlock[] blocks = update.Location;
            List<LLVector3> locations = new List<LLVector3>();
            foreach (CoarseLocationUpdatePacket.LocationBlock block in blocks)
            {
                locations.Add(new LLVector3(block.X, block.Y, block.Z));
            }
            OnLocationUpdate(locations, update.Index.You, update.Index.Prey);
        }

        void LocationTracker_OnLocationUpdate(List<LLVector3> locations, int me, int prey)
        {
            _Locations = locations;
            _MyIndex = me;
            _PreyIndex = prey;
        }

    }
}
