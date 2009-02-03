using System;
using System.Collections.Generic;
using OpenMetaverse;

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
    public delegate void TerrainUpdateCallback(object sender, uint x, uint y, float[,] patchData);

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

        float[,] GetTerrainPatch(uint x, uint y);
        void SetTerrainPatch(object sender, uint x, uint y, float[,] patchData);

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
        int ObjectCount();
        bool TryGetObject(uint localID, out SimulationObject obj);
        bool TryGetObject(UUID id, out SimulationObject obj);
        void ForEachObject(Action<SimulationObject> obj);

        bool AgentAdd(object sender, Agent agent, PrimFlags creatorFlags);
        void AgentAppearance(object sender, Agent agent, Primitive.TextureEntry textures, byte[] visualParams);
        int AgentCount();
        bool TryGetAgent(uint localID, out Agent agent);
        bool TryGetAgent(UUID id, out Agent agent);
        void ForEachAgent(Action<Agent> action);
    }
}
