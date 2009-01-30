using System;
using System.Collections.Generic;
using OpenMetaverse;

namespace Simian
{
    public delegate void ObjectAddCallback(object sender, SimulationObject obj, PrimFlags creatorFlags);
    public delegate void ObjectRemoveCallback(object sender, SimulationObject obj);
    public delegate void ObjectTransformCallback(object sender, SimulationObject obj, Vector3 position,
        Quaternion rotation, Vector3 velocity, Vector3 acceleration, Vector3 angularVelocity);
    public delegate void ObjectFlagsCallback(object sender, SimulationObject obj, PrimFlags flags);
    public delegate void ObjectImageCallback(object sender, SimulationObject obj,
        string mediaURL, Primitive.TextureEntry textureEntry);
    public delegate void ObjectModifyCallback(object sender, SimulationObject obj, Primitive.ConstructionData data);
    public delegate void AgentAddCallback(object sender, Agent agent, PrimFlags creatorFlags);
    public delegate void AgentRemoveCallback(object sender, Agent agent);
    public delegate void AgentAppearanceCallback(object sender, Agent agent, Primitive.TextureEntry textures,
        byte[] visualParams);
    // TODO: Convert terrain to a patch-based system
    public delegate void TerrainUpdatedCallback(object sender);

    public interface ISceneProvider
    {
        event ObjectAddCallback OnObjectAdd;
        event ObjectRemoveCallback OnObjectRemove;
        event ObjectTransformCallback OnObjectTransform;
        event ObjectFlagsCallback OnObjectFlags;
        event ObjectModifyCallback OnObjectModify;
        event AgentAddCallback OnAgentAdd;
        event AgentRemoveCallback OnAgentRemove;
        event AgentAppearanceCallback OnAgentAppearance;
        event TerrainUpdatedCallback OnTerrainUpdated;

        // TODO: Convert to a patch-based system, and expose terrain editing
        // through functions instead of a property
        float[] Heightmap { get; set; }
        float WaterHeight { get; }

        bool ObjectAdd(object sender, SimulationObject obj, PrimFlags creatorFlags);
        bool ObjectRemove(object sender, uint localID);
        bool ObjectRemove(object sender, UUID id);
        void ObjectTransform(object sender, uint localID, Vector3 position, Quaternion rotation, Vector3 velocity,
            Vector3 acceleration, Vector3 angularVelocity);
        void ObjectFlags(object sender, SimulationObject obj, PrimFlags flags);
        void ObjectImage(object sender, SimulationObject obj, string mediaURL, Primitive.TextureEntry textureEntry);
        void ObjectModify(object sender, uint localID, Primitive.ConstructionData data);
        bool ContainsObject(uint localID);
        bool ContainsObject(UUID id);
        bool TryGetObject(uint localID, out SimulationObject obj);
        bool TryGetObject(UUID id, out SimulationObject obj);
        void ForEachObject(Action<SimulationObject> obj);

        bool AgentAdd(object sender, Agent agent, PrimFlags creatorFlags);
        bool AgentRemove(object sender, uint localID);
        bool AgentRemove(object sender, UUID id);
        void AgentAppearance(object sender, Agent agent, Primitive.TextureEntry textures, byte[] visualParams);
        bool ContainsAgent(uint localID);
        bool ContainsAgent(UUID id);
        bool TryGetAgent(uint localID, out Agent agent);
        bool TryGetAgent(UUID id, out Agent agent);
        void ForEachAgent(Action<Agent> action);
    }
}
