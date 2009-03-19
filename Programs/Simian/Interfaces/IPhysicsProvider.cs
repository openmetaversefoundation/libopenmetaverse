using System;
using OpenMetaverse;

namespace Simian
{
    public interface IPhysicsProvider
    {
        /// <summary>
        /// Runs a single physics frame
        /// </summary>
        /// <param name="elapsedTime">The time since Update was called last</param>
        void Update(float elapsedTime);

        Vector3 ObjectCollisionTest(Vector3 rayStart, Vector3 rayEnd, SimulationObject obj);
        bool TryGetObjectMass(UUID objectID, out float mass);
    }
}
