using System;
using System.Collections.Generic;
using System.Collections;
using System.Net;
using OpenMetaverse;
using OpenMetaverse.Packets;
using NUnit.Framework;

namespace OpenMetaverse.Tests
{
    public class MockNetworkManager : INetworkManager
    {
        public delegate void PacketSent(Packet packet, Simulator sim);
        public event PacketSent OnPacketSent;

        #region INetworkManager Members

        public event NetworkManager.LoginCallback OnLogin;

        public event NetworkManager.ConnectedCallback OnConnected;

        public event NetworkManager.DisconnectedCallback OnDisconnected;


        public Simulator CurrentSim
        {
            get { return null; }
        }

        public uint CircuitCode
        {
            get { return 0; }
        }

        private bool _Connected;
        public bool Connected
        {
            get { return _Connected; }
        }

        public UUID SessionID
        {
            get { return UUID.Zero; }
        }

        public UUID SecureSessionID
        {
            get { return UUID.Zero; }
        }

        public UUID AgentID
        {
            get { return UUID.Zero; }
        }


        public void RegisterCallback(PacketType type, NetworkManager.PacketCallback callback)
        {
            return;
        }

        public void RegisterEventCallback(string capsEvent, Caps.EventQueueCallback callback)
        {
            return;
        }

        public void RegisterLoginResponseCallback(NetworkManager.LoginResponseCallback callback, string[] options)
        {
            return;
        }

        public void SendPacket(Packet packet, Simulator sim)
        {
            if (OnPacketSent != null)
            {
                OnPacketSent(packet, sim);
            }
        }

        public void SendPacket(Packet packet)
        {
            if (OnPacketSent != null)
            {
                OnPacketSent(packet, CurrentSim);
            }
        }

        public bool Login(LoginParams loginParams)
        {
            _Connected = true;
            OnLogin(LoginStatus.Success, "Testing");
            OnConnected(null);
            return true;
        }

        public void Logout()
        {
            return;
        }

        #endregion
    }
}
