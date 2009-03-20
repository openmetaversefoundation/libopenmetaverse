using System;
using OpenMetaverse;

using LSL_Float = Simian.ScriptTypes.LSL_Float;
using LSL_Integer = Simian.ScriptTypes.LSL_Integer;
using LSL_Key = Simian.ScriptTypes.LSL_Key;
using LSL_List = Simian.ScriptTypes.LSL_List;
using LSL_Rotation = Simian.ScriptTypes.LSL_Rotation;
using LSL_String = Simian.ScriptTypes.LSL_String;
using LSL_Vector = Simian.ScriptTypes.LSL_Vector;

namespace Simian
{
    #region Scripting Support Classes

    /// <summary>
    /// Holds all the data required to execute a scripting event
    /// </summary>
    public class EventParams
    {
        public string EventName;
        public object[] Params;
        public DetectParams[] DetectParams;

        public EventParams(string eventName, object[] eventParams, DetectParams[] detectParams)
        {
            EventName = eventName;
            Params = eventParams;
            DetectParams = detectParams;
        }
    }

    /// <summary>
    /// Holds all of the data a script can detect about the containing object
    /// </summary>
    public class DetectParams
    {
        public LSL_Key Key;
        public LSL_Integer LinkNum;
        public LSL_Key Group;
        public LSL_String Name;
        public LSL_Key Owner;
        public LSL_Vector Offset;
        public LSL_Vector Position;
        public LSL_Rotation Rotation;
        public LSL_Vector Velocity;
        public LSL_Integer Type;
        public LSL_Vector TouchST;
        public LSL_Vector TouchNormal;
        public LSL_Vector TouchBinormal;
        public LSL_Vector TouchPos;
        public LSL_Vector TouchUV;
        public LSL_Integer TouchFace;

        public DetectParams()
        {
            Key = LSL_Key.Zero;
            LinkNum = LSL_Integer.Zero;
            Group = LSL_Key.Zero;
            Name = LSL_String.Empty;
            Owner = LSL_Key.Zero;
            Offset = LSL_Vector.Zero;
            Position = LSL_Vector.Zero;
            Rotation = LSL_Rotation.Identity;
            Velocity = LSL_Vector.Zero;
            Type = LSL_Integer.Zero;
            TouchST = ScriptTypes.TOUCH_INVALID_TEXCOORD;
            TouchNormal = LSL_Vector.Zero;
            TouchBinormal = LSL_Vector.Zero;
            TouchPos = LSL_Vector.Zero;
            TouchUV = ScriptTypes.TOUCH_INVALID_TEXCOORD;
            TouchFace = ScriptTypes.TOUCH_INVALID_FACE;
        }
    }

    public class ScriptInstance
    {
        public bool Running;
        public bool ShuttingDown;
        public string State;
        public UUID AppDomain;
        //public string PrimName;
        //public string ScriptName;
        public UUID ScriptItemID;
        public UUID ScriptAssetID;
        public UUID HostObjectID;
        public int StartParam;

        ISceneProvider scene;

        public ScriptInstance()
        {
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public void SetState(string state)
        {
        }

        public void PostEvent(EventParams data)
        {
        }

        public void ClearQueue()
        {
        }

        public void RemoveState()
        {
        }
    }

    #endregion Scripting Support Classes

    public interface IScriptEngine
    {
        bool RezScript(UUID scriptID, UUID scriptSourceAssetID, SimulationObject hostObject, int startParam);

        bool PostScriptEvent(UUID scriptID, EventParams parms);
        bool PostObjectEvent(UUID hostObjectID, EventParams parms);

        void SetTimerEvent(UUID scriptID, double seconds);

        DetectParams GetDetectParams(UUID scriptID, int detectIndex);

        bool IsScriptEnabled(UUID scriptID);
        void SetScriptEnabled(UUID scriptID, bool enabled);

        void SetStartParameter(UUID scriptID, int startParam);
        int GetStartParameter(UUID scriptID);

        void SetScriptMinEventDelay(UUID scriptID, double minDelay);

        void TriggerState(UUID scriptID, string newState);

        void ApiResetScript(UUID scriptID);
        void ResetScript(UUID scriptID);

        int AddListener(UUID scriptID, UUID hostObjectID, int channel, string name, UUID keyID, string message);
        void RemoveListener(UUID scriptID, int handle);
        void RemoveListeners(UUID scriptID);
        void SetListenerState(UUID scriptID, int handle, bool enabled);

        void SensorOnce(UUID scriptID, UUID hostObjectID, string name, UUID keyID, int type, double range, double arc);
        void SensorRepeat(UUID scriptID, UUID hostObjectID, string name, UUID keyID, int type, double range, double arc, double rate);
        void SensorRemove(UUID scriptID);
    }
}
