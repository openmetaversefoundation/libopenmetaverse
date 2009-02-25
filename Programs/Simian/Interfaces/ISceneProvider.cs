using System;
using System.Collections.Generic;
using HttpServer;
using OpenMetaverse;
using OpenMetaverse.StructuredData;

namespace Simian
{
    public class TerrainPatch
    {
        public float[,] Height;

        public TerrainPatch(uint width, uint height)
        {
            Height = new float[height, width];
        }
    }

    public struct AnimationTrigger
    {
        public UUID AnimationID;
        public int SequenceID;

        public AnimationTrigger(UUID animationID, int sequenceID)
        {
            AnimationID = animationID;
            SequenceID = sequenceID;
        }
    }

    public struct ViewerEffect
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

    public enum SceneActionType
    {
        None = 0,
        ObjectAdd,
        ObjectRemove,
        ObjectTransform,
        ObjectFlags,
        ObjectModify,
        ObjectModifyTextures,
        ObjectAnimate,
        AgentAdd,
        AgentAppearance,
        TriggerSound,
        TriggerEffects,
        TerrainUpdate,
    }

    //public delegate void SceneActionCallback(SceneActionType type, OSD actionData);

    public delegate void ObjectAddCallback(object sender, SimulationObject obj, PrimFlags creatorFlags);
    public delegate void ObjectRemoveCallback(object sender, SimulationObject obj);
    public delegate void ObjectTransformCallback(object sender, SimulationObject obj, Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 acceleration, Vector3 angularVelocity);
    public delegate void ObjectFlagsCallback(object sender, SimulationObject obj, PrimFlags flags);
    public delegate void ObjectModifyCallback(object sender, SimulationObject obj, Primitive.ConstructionData data);
    public delegate void ObjectModifyTexturesCallback(object sender, SimulationObject obj, string mediaURL, Primitive.TextureEntry textureEntry);
    public delegate void ObjectAnimateCallback(object sender, UUID senderID, UUID objectID, AnimationTrigger[] animations);
    public delegate void ObjectUndoCallback(object sender, SimulationObject obj);
    public delegate void ObjectRedoCallback(object sender, SimulationObject obj);
    public delegate void AgentAddCallback(object sender, Agent agent, PrimFlags creatorFlags);
    public delegate void AgentRemoveCallback(object sender, Agent agent);
    public delegate void AgentAppearanceCallback(object sender, Agent agent, Primitive.TextureEntry textures, byte[] visualParams);
    public delegate void TriggerSoundCallback(object sender, UUID objectID, UUID parentID, UUID ownerID, UUID soundID, Vector3 position, float gain);
    public delegate void TriggerEffectsCallback(object sender, ViewerEffect[] effects);
    public delegate void TerrainUpdateCallback(object sender, uint x, uint y, float[,] patchData);

    public interface ISceneProvider
    {
        event ObjectAddCallback OnObjectAdd;
        event ObjectRemoveCallback OnObjectRemove;
        event ObjectTransformCallback OnObjectTransform;
        event ObjectFlagsCallback OnObjectFlags;
        event ObjectModifyCallback OnObjectModify;
        event ObjectModifyTexturesCallback OnObjectModifyTextures;
        event ObjectAnimateCallback OnObjectAnimate;
        event ObjectUndoCallback OnObjectUndo;
        event ObjectRedoCallback OnObjectRedo;
        event AgentAddCallback OnAgentAdd;
        event AgentRemoveCallback OnAgentRemove;
        event AgentAppearanceCallback OnAgentAppearance;
        event TriggerSoundCallback OnTriggerSound;
        event TriggerEffectsCallback OnTriggerEffects;
        event TerrainUpdateCallback OnTerrainUpdate;

        uint RegionX { get; }
        uint RegionY { get; }
        ulong RegionHandle { get; }
        UUID RegionID { get; }
        string RegionName { get; }
        RegionFlags RegionFlags { get; }

        float WaterHeight { get; }

        uint TerrainPatchWidth { get; }
        uint TerrainPatchHeight { get; }

        void SetTerrainPatch(object sender, uint x, uint y, float[,] patchData);
        bool ObjectAdd(object sender, SimulationObject obj, PrimFlags creatorFlags);
        bool ObjectRemove(object sender, uint localID);
        bool ObjectRemove(object sender, UUID id);
        void ObjectTransform(object sender, SimulationObject obj, Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 acceleration, Vector3 angularVelocity);
        void ObjectFlags(object sender, SimulationObject obj, PrimFlags flags);
        void ObjectModify(object sender, SimulationObject obj, Primitive.ConstructionData data);
        void ObjectModifyTextures(object sender, SimulationObject obj, string mediaURL, Primitive.TextureEntry textureEntry);
        void ObjectAnimate(object sender, UUID senderID, UUID objectID, AnimationTrigger[] animations);
        void ObjectUndo(object sender, SimulationObject obj);
        void ObjectRedo(object sender, SimulationObject obj);
        void TriggerSound(object sender, UUID objectID, UUID parentID, UUID ownerID, UUID soundID, Vector3 position, float gain);
        void TriggerEffects(object sender, ViewerEffect[] effects);
        bool AgentAdd(object sender, Agent agent, PrimFlags creatorFlags);
        void AgentAppearance(object sender, Agent agent, Primitive.TextureEntry textures, byte[] visualParams);

        float[,] GetTerrainPatch(uint x, uint y);
        bool ContainsObject(uint localID);
        bool ContainsObject(UUID id);
        int ObjectCount();
        bool TryGetObject(uint localID, out SimulationObject obj);
        bool TryGetObject(UUID id, out SimulationObject obj);
        void ForEachObject(Action<SimulationObject> obj);
        
        int AgentCount();
        bool TryGetAgent(uint localID, out Agent agent);
        bool TryGetAgent(UUID id, out Agent agent);
        void ForEachAgent(Action<Agent> action);

        void SendEvent(Agent agent, string name, OSDMap body);
        bool HasRunningEventQueue(Agent agent);
        bool SeedCapabilityHandler(IHttpClientContext context, IHttpRequest request, IHttpResponse response, object state);
    }
}
