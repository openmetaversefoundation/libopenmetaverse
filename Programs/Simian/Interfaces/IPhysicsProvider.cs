using System;
using OpenMetaverse;

namespace Simian
{
    public interface IPhysicsProvider
    {
        Vector3 ObjectCollisionTest(Vector3 rayStart, Vector3 rayEnd, SimulationObject obj);
        bool TryGetObjectMass(UUID objectID, out float mass);
    }
}
