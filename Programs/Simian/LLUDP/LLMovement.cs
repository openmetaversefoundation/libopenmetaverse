using System;
using System.Collections.Generic;
using System.Threading;
using ExtensionLoader;
using OpenMetaverse;
using OpenMetaverse.Packets;
using OpenMetaverse.Rendering;

namespace Simian
{
    public class LLMovement : IExtension<ISceneProvider>
    {
        //static readonly UUID BIG_SPLASH_SOUND = new UUID("486475b9-1460-4969-871e-fad973b38015");
        //static readonly Vector3 SEATING_FUDGE = new Vector3(0.3f, 0.0f, 0.0f);

        ISceneProvider scene;

        public LLMovement()
        {
        }

        public bool Start(ISceneProvider scene)
        {
            this.scene = scene;

            scene.UDP.RegisterPacketCallback(PacketType.AgentRequestSit, AgentRequestSitHandler);
            scene.UDP.RegisterPacketCallback(PacketType.AgentSit, AgentSitHandler);
            scene.UDP.RegisterPacketCallback(PacketType.AgentUpdate, AgentUpdateHandler);
            scene.UDP.RegisterPacketCallback(PacketType.SetAlwaysRun, SetAlwaysRunHandler);

            return true;
        }

        public void Stop()
        {
        }

        void AgentRequestSitHandler(Packet packet, Agent agent)
        {
            AgentRequestSitPacket request = (AgentRequestSitPacket)packet;

            SimulationObject obj;
            if (scene.TryGetObject(request.TargetObject.TargetID, out obj))
            {
                agent.RequestedSitTarget = request.TargetObject.TargetID;
                agent.RequestedSitOffset = request.TargetObject.Offset;

                AvatarSitResponsePacket response = new AvatarSitResponsePacket();
                response.SitObject.ID = request.TargetObject.TargetID;
                response.SitTransform.AutoPilot = true;
                response.SitTransform.CameraAtOffset = Vector3.Zero;
                response.SitTransform.CameraEyeOffset = Vector3.Zero;
                response.SitTransform.ForceMouselook = false;
                response.SitTransform.SitPosition = request.TargetObject.Offset;
                response.SitTransform.SitRotation = obj.SitRotation;

                scene.UDP.SendPacket(agent.ID, response, PacketCategory.State);
            }
            else
            {
                //TODO: send error
            }
        }

        void AgentSitHandler(Packet packet, Agent agent)
        {
            AgentSitPacket sit = (AgentSitPacket)packet;

            if (agent.RequestedSitTarget != UUID.Zero)
            {
                SimulationObject obj;
                SimulationObject avObj;
                if (scene.TryGetObject(agent.RequestedSitTarget, out obj) && scene.TryGetObject(agent.ID, out avObj))
                {
                    agent.Avatar.Prim.Flags &= ~PrimFlags.Physics;
                    agent.Avatar.Prim.ParentID = obj.Prim.LocalID;
                    agent.Avatar.Prim.Position = new Vector3(
                        obj.Prim.Scale.X * 0.5f,
                        obj.Prim.Scale.Z * 0.5f,
                        agent.Avatar.Prim.Scale.Z * 0.33f);

                    scene.ObjectAddOrUpdate(this, avObj, avObj.Prim.OwnerID, PrimFlags.None,
                        UpdateFlags.PrimFlags | UpdateFlags.ParentID | UpdateFlags.Position);
                    scene.Avatars.SetDefaultAnimation(agent, Animations.SIT);
                    scene.Avatars.SendAnimations(agent);
                }
                else
                {
                    //TODO: send error
                }

                agent.RequestedSitTarget = UUID.Zero;
                agent.RequestedSitOffset = Vector3.Zero;
            }
        }

        void AgentUpdateHandler(Packet packet, Agent agent)
        {
            AgentUpdatePacket update = (AgentUpdatePacket)packet;

            SimulationObject obj;
            if (scene.TryGetObject(agent.ID, out obj))
            {
                if (agent.Avatar.Prim.ParentID == 0)
                    agent.Avatar.Prim.Rotation = update.AgentData.BodyRotation;

                agent.ControlFlags = (AgentManager.ControlFlags)update.AgentData.ControlFlags;
                agent.State = (AgentState)update.AgentData.State;
                agent.HideTitle = update.AgentData.Flags != 0;

                // Check for standing up
                SimulationObject parent;
                if (scene.TryGetObject(agent.Avatar.Prim.ParentID, out parent) &&
                    agent.Avatar.Prim.ParentID > 0 &&
                    (agent.ControlFlags & AgentManager.ControlFlags.AGENT_CONTROL_STAND_UP) == AgentManager.ControlFlags.AGENT_CONTROL_STAND_UP)
                {
                    agent.Avatar.Prim.Position = parent.Prim.Position
                        + Vector3.Transform(parent.SitPosition, Matrix4.CreateFromQuaternion(parent.SitRotation))
                        + Vector3.UnitZ;

                    agent.Avatar.Prim.ParentID = 0;
                    
                    scene.Avatars.SetDefaultAnimation(agent, Animations.STAND);
                    scene.Avatars.SendAnimations(agent);

                    agent.Avatar.Prim.Flags |= PrimFlags.Physics;
                }

                scene.ObjectAddOrUpdate(this, obj, obj.Prim.OwnerID, PrimFlags.None, UpdateFlags.Position | UpdateFlags.Rotation);
            }
        }

        void SetAlwaysRunHandler(Packet packet, Agent agent)
        {
            SetAlwaysRunPacket run = (SetAlwaysRunPacket)packet;

            agent.Running = run.AgentData.AlwaysRun;
        }
    }
}
