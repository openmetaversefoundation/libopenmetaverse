using System;
using OpenMetaverse;

namespace Simian
{
    public delegate bool ObjectAddedCallback(SimulationObject obj);
    public delegate bool ObjectRemovedCallback(SimulationObject obj);
    public delegate void ObjectUpdatedCallback(SimulationObject obj);
    // TODO: ObjectImpulseAppliedCallback

    public interface ISceneProvider
    {
        event ObjectAddedCallback OnObjectAdded;
        event ObjectRemovedCallback OnObjectRemoved;
        event ObjectUpdatedCallback OnObjectUpdated;

        void AddObject(Agent creator, SimulationObject obj);
        void RemoveObject(SimulationObject obj);
        bool TryGetObject(uint localID, out SimulationObject obj);
        bool TryGetObject(UUID id, out SimulationObject obj);
        void ObjectUpdate(SimulationObject obj, byte state, PrimFlags flags);
    }
}
