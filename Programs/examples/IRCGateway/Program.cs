using OpenMetaverse;
using System;

namespace IRCGateway
{
    class Program
    {
        static GridClient _Client;
        static LoginParams _ClientLogin;
        static IRCClient _IRC;
        static string _AutoJoinChannel;
        static UUID _MasterID;

        static void Main(string[] args)
        {
            int ircPort;

            if (args.Length < 7 || !UUID.TryParse(args[3], out _MasterID) || !int.TryParse(args[5], out ircPort) || args[6].IndexOf('#') == -1)
                Console.WriteLine("Usage: ircgateway.exe <firstName> <lastName> <password> <masterUUID> <ircHost> <ircPort> <#channel>");

            else
            {
                _Client = new GridClient();
                _Client.Network.OnLogin += new NetworkManager.LoginCallback(Network_OnLogin);
                _Client.Self.OnChat += new AgentManager.ChatCallback(Self_OnChat);
                _Client.Self.OnInstantMessage += new AgentManager.InstantMessageCallback(Self_OnInstantMessage);

                _ClientLogin = _Client.Network.DefaultLoginParams(args[0], args[1], args[2], "", "IRCGateway");

                _AutoJoinChannel = args[6];
                _IRC = new IRCClient(args[4], ircPort, "SLGateway", "Second Life Gateway");
                _IRC.OnConnected += new IRCClient.ConnectCallback(_IRC_OnConnected);
                _IRC.OnMessage += new IRCClient.MessageCallback(_IRC_OnMessage);

                _IRC.Connect();

                string read = Console.ReadLine();
                while (read != null) read = Console.ReadLine();                
            }
        }

        static void _IRC_OnConnected()
        {
            _IRC.JoinChannel(_AutoJoinChannel);
            _Client.Network.BeginLogin(_ClientLogin);
        }

        static void _IRC_OnMessage(string target, string name, string address, string message)
        {
            if (target == _AutoJoinChannel)
            {
                string str = "<" + name + "> " + message;
                _Client.Self.Chat(str, 0, ChatType.Normal);
                Console.WriteLine("[IRC->SL] " + str);
            }
        }

        static void Self_OnInstantMessage(InstantMessage im, Simulator simulator)
        {
            if (im.Dialog == InstantMessageDialog.RequestTeleport)
            {
                if (im.FromAgentID == _MasterID)
                {
                    _Client.Self.TeleportLureRespond(im.FromAgentID, true);
                }
            }
        }

        static void Network_OnLogin(LoginStatus login, string message)
        {
            _IRC.SendMessage(_AutoJoinChannel, message);
        }

        static void Self_OnChat(string message, ChatAudibleLevel audible, ChatType type, ChatSourceType sourceType, string fromName, UUID id, UUID ownerid, Vector3 position)
        {
            if (fromName != _Client.Self.Name &&  type == ChatType.Normal && audible == ChatAudibleLevel.Fully)
            {
                string str = "<" + fromName + "> " + message;
                _IRC.SendMessage(_AutoJoinChannel, str);
                Console.WriteLine("[SL->IRC] " + str);
            }
        }

    }
}
