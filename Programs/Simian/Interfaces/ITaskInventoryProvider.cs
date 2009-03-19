using System;
using System.Collections.Generic;
using OpenMetaverse;

namespace Simian
{
    public interface ITaskInventoryProvider
    {
        void AddTaskFile(string filename, byte[] assetData);
        bool RemoveTaskFile(string filename);
        bool TryGetTaskFile(string filename, out byte[] assetData);
    }
}
