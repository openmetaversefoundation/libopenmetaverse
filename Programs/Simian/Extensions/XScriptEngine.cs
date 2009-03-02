using System;
using System.Collections.Generic;
using ExtensionLoader;
using OpenMetaverse;

namespace Simian.Extensions
{
    // FIXME: Implement this class
    class XScriptEngine : IExtension<Simian>, IScriptEngine
    {
        Simian server;

        public XScriptEngine()
        {
        }

        public void Start(Simian server)
        {
            this.server = server;
        }

        public void Stop()
        {
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

        public bool GetScriptState(UUID scriptID)
        {
            return false;
        }

        public void SetScriptState(UUID scriptID, bool state)
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
