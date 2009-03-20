using System;
using System.Collections.Generic;
using System.Text;
using ExtensionLoader;
using OpenMetaverse;

namespace Simian
{
    // FIXME: Implement this class
    class XScriptEngine : IExtension<ISceneProvider>, IScriptEngine
    {
        ISceneProvider scene;

        public XScriptEngine()
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

        public bool RezScript(UUID scriptID, UUID scriptSourceAssetID, SimulationObject hostObject, int scriptStartParam)
        {
            Asset sourceAsset;
            Asset binaryAsset;

            // Try to fetch the script source code asset
            if (scene.Server.Assets.TryGetAsset(scriptSourceAssetID, out sourceAsset) && sourceAsset is AssetScriptText)
            {
                // The script binary assetID is the MD5 hash of the source to avoid lots of duplicate compiles
                UUID scriptBinaryAssetID = new UUID(Utils.MD5(sourceAsset.AssetData), 0);

                // Check if a compiled assembly already exists for this script
                if (scene.Server.Assets.TryGetAsset(scriptBinaryAssetID, out binaryAsset))
                {
                    Logger.Log("Using existing compile for scriptID " + scriptID, Helpers.LogLevel.Info);
                }
                else
                {
                    ScriptCompiler compiler = new ScriptCompiler();
                    string csText = compiler.Convert(Encoding.UTF8.GetString(sourceAsset.AssetData));
                }
            }

            return false;
        }

        public bool PostScriptEvent(UUID scriptID, EventParams parms)
        {
            return false;
        }

        public bool PostObjectEvent(UUID hostObjectID, EventParams parms)
        {
            return false;
        }

        public void SetTimerEvent(UUID scriptID, double seconds)
        {
        }

        public DetectParams GetDetectParams(UUID scriptID, int detectIndex)
        {
            DetectParams parms = new DetectParams();
            return parms;
        }

        public bool IsScriptEnabled(UUID scriptID)
        {
            return false;
        }

        public void SetScriptEnabled(UUID scriptID, bool enabled)
        {
        }

        public void SetStartParameter(UUID scriptID, int startParam)
        {
        }

        public int GetStartParameter(UUID scriptID)
        {
            return 0;
        }

        public void SetScriptMinEventDelay(UUID scriptID, double minDelay)
        {
        }

        public void TriggerState(UUID scriptID, string newState)
        {
        }

        public void ApiResetScript(UUID scriptID)
        {
        }

        public void ResetScript(UUID scriptID)
        {
        }

        public int AddListener(UUID scriptID, UUID hostObjectID, int channel, string name, UUID keyID, string message)
        {
            return 0;
        }

        public void RemoveListener(UUID scriptID, int handle)
        {
        }

        public void RemoveListeners(UUID scriptID)
        {
        }

        public void SetListenerState(UUID scriptID, int handle, bool enabled)
        {
        }

        public void SensorOnce(UUID scriptID, UUID hostObjectID, string name, UUID keyID, int type, double range, double arc)
        {
        }

        public void SensorRepeat(UUID scriptID, UUID hostObjectID, string name, UUID keyID, int type, double range, double arc, double rate)
        {
        }

        public void SensorRemove(UUID scriptID)
        {
        }
    }
}
