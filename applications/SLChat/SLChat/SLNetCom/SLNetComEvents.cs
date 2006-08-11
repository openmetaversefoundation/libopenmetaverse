using System;
using System.Collections.Generic;
using System.Text;
using libsecondlife;

namespace SLNetworkComm
{
    public partial class SLNetCom
    {
        // For the NetcomSync stuff
        private delegate void OnClientLoginRaise(ClientLoginEventArgs e);
        private delegate void OnChatRaise(ChatEventArgs e);
        private delegate void OnInstantMessageRaise(InstantMessageEventArgs e);

        public event EventHandler<ClientLoginEventArgs> ClientLoggedIn;
        public event EventHandler<ClientLoginEventArgs> ClientLoginError;
        public event EventHandler<ClientLoginEventArgs> ClientLoggedOut;
        public event EventHandler<ChatEventArgs> ChatReceived;
        public event EventHandler<ChatSentEventArgs> ChatSent;
        public event EventHandler<InstantMessageEventArgs> InstantMessageReceived;
        public event EventHandler<InstantMessageSentEventArgs> InstantMessageSent;

        protected virtual void OnClientLoggedIn(ClientLoginEventArgs e)
        {
            if (ClientLoggedIn != null) ClientLoggedIn(this, e);
        }

        protected virtual void OnClientLoginError(ClientLoginEventArgs e)
        {
            if (ClientLoginError != null) ClientLoginError(this, e);
        }

        protected virtual void OnClientLoggedOut(ClientLoginEventArgs e)
        {
            if (ClientLoggedOut != null) ClientLoggedOut(this, e);
        }

        protected virtual void OnChatReceived(ChatEventArgs e)
        {
            if (ChatReceived != null) ChatReceived(this, e);
        }

        protected virtual void OnChatSent(ChatSentEventArgs e)
        {
            if (ChatSent != null) ChatSent(this, e);
        }

        protected virtual void OnInstantMessageReceived(InstantMessageEventArgs e)
        {
            if (InstantMessageReceived != null) InstantMessageReceived(this, e);
        }

        protected virtual void OnInstantMessageSent(InstantMessageSentEventArgs e)
        {
            if (InstantMessageSent != null) InstantMessageSent(this, e);
        }
    }

    public class ClientLoginEventArgs : EventArgs
    {
        private string _loginReply;

        public ClientLoginEventArgs(string loginReply)
        {
            _loginReply = loginReply;
        }

        public string LoginReply
        {
            get { return _loginReply; }
        }
    }

    public class ChatEventArgs : EventArgs
    {
        private string _message;
        private SLChatType _type;
        private LLVector3 _sourcePos;
        private SLSourceType _sourceType;
        private LLUUID _sourceId;
        private LLUUID _ownerId;
        private string _fromName;
        private bool _audible;
        private byte _command;
        private LLUUID _commandId;

        public ChatEventArgs(
            string message, SLChatType type, LLVector3 sourcePos, SLSourceType sourceType,
            LLUUID sourceId, LLUUID ownerId, string fromName,
            bool audible, byte command, LLUUID commandId)
        {
            _message = message;
            _type = type;
            _sourcePos = sourcePos;
            _sourceType = sourceType;
            _sourceId = sourceId;
            _ownerId = ownerId;
            _fromName = fromName;
            _audible = audible;
            _command = command;
            _commandId = commandId;
        }

        public string Message
        {
            get { return _message; }
        }

        public SLChatType Type
        {
            get { return _type; }
        }

        public LLVector3 SourcePosition
        {
            get { return _sourcePos; }
        }

        public SLSourceType SourceType
        {
            get { return _sourceType; }
        }

        public LLUUID SourceId
        {
            get { return _sourceId; }
        }

        public LLUUID OwnerId
        {
            get { return _ownerId; }
        }

        public string FromName
        {
            get { return _fromName; }
        }

        public bool Audible
        {
            get { return _audible; }
        }

        public byte Command
        {
            get { return _command; }
        }

        public LLUUID CommandId
        {
            get { return _commandId; }
        }
    }

    public class ChatSentEventArgs : EventArgs
    {
        private string _message;
        private SLChatType _messageType;
        private int _channel;

        public ChatSentEventArgs(string message, SLChatType messageType, int channel)
        {
            _message = message;
            _messageType = messageType;
            _channel = channel;
        }

        public string Message
        {
            get { return _message; }
        }

        public SLChatType MessageType
        {
            get { return _messageType; }
        }

        public int Channel
        {
            get { return _channel; }
        }
    }

    public class InstantMessageEventArgs : EventArgs
    {
        private LLUUID _fromAgentId;
        private LLUUID _toAgentId;
        private uint _parentEstateId;
        private LLUUID _regionId;
        private LLVector3 _position;
        private bool _offline;
        private byte _dialog;
        private LLUUID _id;
        private DateTime _timestamp;
        private string _fromAgentName;
        private string _message;
        private string _binaryBucket;

        public InstantMessageEventArgs(
            LLUUID fromAgentId, LLUUID toAgentId, uint parentEstateId, LLUUID regionId,
            LLVector3 position, bool offline, byte dialog, LLUUID id,
            DateTime timestamp, string fromAgentName, string message, string binaryBucket)
        {
            _fromAgentId = fromAgentId;
            _toAgentId = toAgentId;
            _parentEstateId = parentEstateId;
            _regionId = regionId;
            _position = position;
            _offline = offline;
            _dialog = dialog;
            _id = id;
            _timestamp = timestamp;
            _fromAgentName = fromAgentName;
            _message = message;
            _binaryBucket = binaryBucket;
        }

        public LLUUID FromAgentId
        {
            get { return _fromAgentId; }
        }

        public LLUUID ToAgentId
        {
            get { return _toAgentId; }
        }

        public uint ParentEstateId
        {
            get { return _parentEstateId; }
        }

        public LLUUID RegionId
        {
            get { return _regionId; }
        }

        public LLVector3 Position
        {
            get { return _position; }
        }

        public bool Offline
        {
            get { return _offline; }
        }

        public byte Dialog
        {
            get { return _dialog; }
        }

        public LLUUID Id
        {
            get { return _id; }
        }

        public DateTime Timestamp
        {
            get { return _timestamp; }
        }

        public string FromAgentName
        {
            get { return _fromAgentName; }
        }

        public string Message
        {
            get { return _message; }
        }

        public string BinaryBucket
        {
            get { return _binaryBucket; }
        }
    }

    public class InstantMessageSentEventArgs : EventArgs
    {
        private string _message;
        private LLUUID _targetId;
        private LLUUID _sessionId;
        private DateTime _timestamp;

        public InstantMessageSentEventArgs(string message, LLUUID targetId, LLUUID sessionId, DateTime timestamp)
        {
            _message = message;
            _targetId = targetId;
            _sessionId = sessionId;
            _timestamp = timestamp;
        }

        public string Message
        {
            get { return _message; }
        }

        public LLUUID TargetId
        {
            get { return _targetId; }
        }

        public LLUUID SessionId
        {
            get { return _sessionId; }
        }

        public DateTime Timestamp
        {
            get { return _timestamp; }
        }
    }
}
