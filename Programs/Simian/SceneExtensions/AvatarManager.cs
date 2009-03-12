using System;
using System.Collections.Generic;
using System.Threading;
using ExtensionLoader;
using OpenMetaverse;
using OpenMetaverse.StructuredData;
using OpenMetaverse.Packets;

namespace Simian
{
    class AvatarManager : IExtension<ISceneProvider>, IAvatarProvider
    {
        ISceneProvider scene;
        int currentWearablesSerialNum = -1;
        int currentAnimSequenceNum = 0;

        public AvatarManager()
        {
        }

        public bool Start(ISceneProvider scene)
        {
            this.scene = scene;
            return true;
        }

        public void Stop()
        {
        }

        public bool SetDefaultAnimation(Agent agent, UUID animID)
        {
            return agent.Animations.SetDefaultAnimation(animID, ref currentAnimSequenceNum);
        }

        public bool AddAnimation(Agent agent, UUID animID)
        {
            return agent.Animations.Add(animID, ref currentAnimSequenceNum);
        }

        public bool RemoveAnimation(Agent agent, UUID animID)
        {
            return agent.Animations.Remove(animID);
        }

        public bool ClearAnimations(Agent agent)
        {
            agent.Animations.Clear();
            return true;
        }

        public void SendAnimations(Agent agent)
        {
            scene.ObjectAnimate(this, agent.ID, agent.ID, agent.Animations.GetAnimations());
        }

        public void SendAlert(Agent agent, string message)
        {
            AlertMessagePacket alert = new AlertMessagePacket();
            alert.AlertData.Message = Utils.StringToBytes(message);
            scene.UDP.SendPacket(agent.ID, alert, PacketCategory.Transaction);
        }
    }
}
