using System;
using System.Collections.Generic;
using System.Text;

namespace SLNetworkComm
{
    public enum SLChatType
    {
        Whisper,
        Say,
        Shout,
        Unknown,
        TypingNotification,
        ChatbarToggle
    };

    public enum SLSourceType
    {
        None,
        Avatar,
        Object
    };
}
