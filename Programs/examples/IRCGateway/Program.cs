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
                _Client.Network.LoginProgress += Network_OnLogin;
                _Client.Self.ChatFromSimulator += Self_ChatFromSimulator;                
                _Client.Self.IM += Self_IM;
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

        static void Self_IM(object sender, InstantMessageEventArgs e)
        {
            if (e.IM.Dialog == InstantMessageDialog.RequestTeleport)
            {
                if (e.IM.FromAgentID == _MasterID)
                {
                    _Client.Self.TeleportLureRespond(e.IM.FromAgentID, true);
                }
            }
        }

        static void Self_ChatFromSimulator(object sender, ChatEventArgs e)
        {
            if (e.FromName != _Client.Self.Name && e.Type == ChatType.Normal && e.AudibleLevel == ChatAudibleLevel.Fully)
            {
                string str = "<" + e.FromName + "> " + e.Message;
                _IRC.SendMessage(_AutoJoinChannel, str);
                Console.WriteLine("[SL->IRC] " + str);
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

        static void Network_OnLogin(object sender, LoginProgressEventArgs e)
        {
            _IRC.SendMessage(_AutoJoinChannel, e.Message);
        }
    }
}
