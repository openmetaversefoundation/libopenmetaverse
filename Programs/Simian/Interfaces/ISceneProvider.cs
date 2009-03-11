using System;
using System.Collections.Generic;
using HttpServer;
using OpenMetaverse;
using OpenMetaverse.StructuredData;

namespace Simian
{
    /// <summary>
    /// Specifies that fields that have been changed in a call to ISceneProvider.ObjectAddOrUpdate
    /// </summary>
    [Flags]
    public enum UpdateFlags : uint
    {
        None = 0,
        AttachmentPoint = 1 << 0,
        Material = 1 << 1,
        ClickAction = 1 << 2,
        Scale = 1 << 3,
        ParentID = 1 << 4,
        PrimFlags = 1 << 5,
        PrimData = 1 << 6,
        MediaURL = 1 << 7,
        ScratchPad = 1 << 8,
        Textures = 1 << 9,
        TextureAnim = 1 << 10,
        NameValue = 1 << 11,
        Position = 1 << 12,
        Rotation = 1 << 13,
        Velocity = 1 << 14,
        Acceleration = 1 << 15,
        AngularVelocity = 1 << 16,
        CollisionPlane = 1 << 17,
        Text = 1 << 18,
        Particles = 1 << 19,
        ExtraData = 1 << 20,
        Sound = 1 << 21,
        Joint = 1 << 22,
        FullUpdate = UInt32.MaxValue
    }

    #region Scene related classes

    public class TerrainPatch
    {
        public float[,] Height;

        public TerrainPatch(uint width, uint height)
        {
            Height = new float[height, width];
        }
    }

    public class AnimationTrigger
    {
        public UUID AnimationID;
        public int SequenceID;

        public AnimationTrigger(UUID animationID, int sequenceID)
        {
            AnimationID = animationID;
            SequenceID = sequenceID;
        }
    }

    public class ViewerEffect
    {
        public UUID EffectID;
        public EffectType Type;
        public UUID AgentID;
        public Color4 Color;
        public float Duration;
        public byte[] TypeData;

        public ViewerEffect(UUID effectID, EffectType type, UUID agentID, Color4 color, float duration, byte[] typeData)
        {
            EffectID = effectID;
            Type = type;
            AgentID = agentID;
            Color = color;
            Duration = duration;
            TypeData = typeData;
        }
    }

    #endregion Scene related classes

    public delegate void ObjectAddOrUpdateCallback(object sender, SimulationObject obj, UUID ownerID, int scriptStartParam, PrimFlags creatorFlags, UpdateFlags updateFlags);
    public delegate void ObjectRemoveCallback(object sender, SimulationObject obj);
    public delegate void ObjectSetRotationAxisCallback(object sender, SimulationObject obj, Vector3 rotationAxis);
    public delegate void ObjectApplyImpulseCallback(object sender, SimulationObject obj, Vector3 impulse);
    public delegate void ObjectApplyRotationalImpulseCallback(object sender, SimulationObject obj, Vector3 impulse);
    public delegate void ObjectSetTorqueCallback(object sender, SimulationObject obj, Vector3 torque);
    public delegate void ObjectAnimateCallback(object sender, UUID senderID, UUID objectID, AnimationTrigger[] animations);
    public delegate void ObjectChatCallback(object sender, UUID ownerID, UUID sourceID, ChatAudibleLevel audible, ChatType type, ChatSourceType SourceType, string fromName, Vector3 position, int channel, string message);
    public delegate void ObjectUndoCallback(object sender, SimulationObject obj);
    public delegate void ObjectRedoCallback(object sender, SimulationObject obj);
    public delegate void AgentAddCallback(object sender, Agent agent, PrimFlags creatorFlags);
    public delegate void AgentRemoveCallback(object sender, Agent agent);
    public delegate void AgentAppearanceCallback(object sender, Agent agent, Primitive.TextureEntry textures, byte[] visualParams);
    public delegate void TriggerSoundCallback(object sender, UUID objectID, UUID parentID, UUID ownerID, UUID soundID, Vector3 position, float gain);
    public delegate void TriggerEffectsCallback(object sender, ViewerEffect[] effects);
    public delegate void TerrainUpdateCallback(object sender, uint x, uint y, float[,] patchData);
    public delegate void WindUpdateCallback(object sender, uint x, uint y, Vector2 windSpeed);

    public interface ISceneProvider
    {
        event ObjectAddOrUpdateCallback OnObjectAddOrUpdate;
        event ObjectRemoveCallback OnObjectRemove;
        event ObjectSetRotationAxisCallback OnObjectSetRotationAxis;
        event ObjectApplyImpulseCallback OnObjectApplyImpulse;
        event ObjectApplyRotationalImpulseCallback OnObjectApplyRotationalImpulse;
        event ObjectSetTorqueCallback OnObjectSetTorque;
        event ObjectAnimateCallback OnObjectAnimate;
        event ObjectChatCallback OnObjectChat;
        event ObjectUndoCallback OnObjectUndo;
        event ObjectRedoCallback OnObjectRedo;
        event AgentAddCallback OnAgentAdd;
        event AgentRemoveCallback OnAgentRemove;
        event AgentAppearanceCallback OnAgentAppearance;
        event TriggerSoundCallback OnTriggerSound;
        event TriggerEffectsCallback OnTriggerEffects;
        event TerrainUpdateCallback OnTerrainUpdate;
        event WindUpdateCallback OnWindUpdate;

        uint RegionX { get; }
        uint RegionY { get; }
        ulong RegionHandle { get; }
        UUID RegionID { get; }
        string RegionName { get; }
        RegionFlags RegionFlags { get; }

        float WaterHeight { get; }

        uint TerrainPatchWidth { get; }
        uint TerrainPatchHeight { get; }
        uint TerrainPatchCountWidth { get; }
        uint TerrainPatchCountHeight { get; }

        void ObjectAddOrUpdate(object sender, SimulationObject obj, UUID ownerID, int scriptStartParam, PrimFlags creatorFlags, UpdateFlags updateFlags);
        bool ObjectRemove(object sender, uint localID);
        bool ObjectRemove(object sender, UUID id);
        void ObjectSetRotationAxis(object sender, SimulationObject obj, Vector3 rotationAxis);
        void ObjectApplyImpulse(object sender, SimulationObject obj, Vector3 impulse);
        void ObjectApplyRotationalImpulse(object sender, SimulationObject obj, Vector3 impulse);
        void ObjectSetTorque(object sender, SimulationObject obj, Vector3 torque);
        void ObjectAnimate(object sender, UUID senderID, UUID objectID, AnimationTrigger[] animations);
        void ObjectChat(object sender, UUID ownerID, UUID sourceID, ChatAudibleLevel audible, ChatType type, ChatSourceType sourceType, string fromName, Vector3 position, int channel, string message);
        void ObjectUndo(object sender, SimulationObject obj);
        void ObjectRedo(object sender, SimulationObject obj);
        void TriggerSound(object sender, UUID objectID, UUID parentID, UUID ownerID, UUID soundID, Vector3 position, float gain);
        void TriggerEffects(object sender, ViewerEffect[] effects);
        bool AgentAdd(object sender, Agent agent, PrimFlags creatorFlags);
        void AgentAppearance(object sender, Agent agent, Primitive.TextureEntry textures, byte[] visualParams);

        float GetTerrainHeightAt(float x, float y);
        float[,] GetTerrainPatch(uint x, uint y);
        void SetTerrainPatch(object sender, uint x, uint y, float[,] patchData);

        Vector2 GetWindSpeedAt(float x, float y);
        Vector2 GetWindSpeed(uint x, uint y);
        void SetWindSpeed(object sender, uint x, uint y, Vector2 windSpeed);

        bool ContainsObject(uint localID);
        bool ContainsObject(UUID id);
        int ObjectCount();
        bool TryGetObject(uint localID, out SimulationObject obj);
        bool TryGetObject(UUID id, out SimulationObject obj);
        void ForEachObject(Action<SimulationObject> obj);
        SimulationObject FindObject(Predicate<SimulationObject> predicate);
        int RemoveAllObjects(Predicate<SimulationObject> predicate);
        
        int AgentCount();
        bool TryGetAgent(UUID id, out Agent agent);
        void ForEachAgent(Action<Agent> action);
        Agent FindAgent(Predicate<Agent> predicate);
        int RemoveAllAgents(Predicate<Agent> predicate);

        void SendEvent(Agent agent, string name, OSDMap body);
        bool HasRunningEventQueue(Agent agent);
        bool SeedCapabilityHandler(IHttpClientContext context, IHttpRequest request, IHttpResponse response, object state);
    }
}
