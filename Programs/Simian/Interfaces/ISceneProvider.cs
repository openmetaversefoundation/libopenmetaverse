using System;
using OpenMetaverse;

namespace Simian
{
    public delegate bool ObjectAddedCallback(object sender, SimulationObject obj);
    public delegate bool ObjectRemovedCallback(object sender, SimulationObject obj);
    public delegate void ObjectUpdatedCallback(object sender, SimulationObject obj);
    public delegate void TerrainUpdatedCallback(object sender);
    // TODO: ObjectImpulseAppliedCallback

    public interface ISceneProvider
    {
        event ObjectAddedCallback OnObjectAdded;
        event ObjectRemovedCallback OnObjectRemoved;
        event ObjectUpdatedCallback OnObjectUpdated;
        event TerrainUpdatedCallback OnTerrainUpdated;

        float[] Heightmap { get; set; }

        void AddObject(object sender, Agent creator, SimulationObject obj);
        void RemoveObject(object sender, SimulationObject obj);
        void ObjectUpdate(object sender, SimulationObject obj, byte state, PrimFlags flags);
        bool TryGetObject(uint localID, out SimulationObject obj);
        bool TryGetObject(UUID id, out SimulationObject obj);
    }
}
