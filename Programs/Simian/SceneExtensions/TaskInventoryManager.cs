using System;
using System.Collections.Generic;
using ExtensionLoader;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace Simian
{
    // FIXME: Implement this class
    class TaskInventoryManager : IExtension<ISceneProvider>, ITaskInventoryProvider
    {
        ISceneProvider scene;
        Dictionary<string, byte[]> assets = new Dictionary<string, byte[]>();

        public TaskInventoryManager()
        {
        }

        public bool Start(ISceneProvider scene)
        {
            this.scene = scene;
            return true;
        }

        public void Stop()
        {
        }

        public void AddTaskFile(string filename, byte[] assetData)
        {
            lock (assets)
                assets[filename] = assetData;
        }

        public bool RemoveTaskFile(string filename)
        {
            lock (assets)
                return assets.Remove(filename);
        }

        public bool TryGetTaskFile(string filename, out byte[] assetData)
        {
            return assets.TryGetValue(filename, out assetData);
        }
    }
}
