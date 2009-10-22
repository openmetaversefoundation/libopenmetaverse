using System;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace OpenMetaverse.TestClient
{
    public class PlayAnimationCommand : Command
    {        
        private Dictionary<UUID, string> m_BuiltInAnimations = new Dictionary<UUID, string>();
        public PlayAnimationCommand(TestClient testClient)
        {
            Name = "play";
            Description = "Attempts to play an animation";
            Category = CommandCategory.Appearance;
            
            Type t = typeof(Animations);
            FieldInfo[] f = t.GetFields(BindingFlags.Public | BindingFlags.Static);
            foreach (FieldInfo field in f)
            {
                m_BuiltInAnimations.Add((UUID)field.GetValue(t), field.Name);                              
            }
            Console.WriteLine(f);
        }

        private string Usage()
        {
            String usage = "Usage:\n" +
                "\tplay list - list the built in animations\n" +
                "\tplay show - show any currently playing animations\n" +
                "\tplay UUID - play an animation asset\n" +
                "\tplay ANIMATION - where ANIMATION is one of the values returned from \"play list\"\n";
            return usage;
        }

        public override string Execute(string[] args, UUID fromAgentID)
        {            
            StringBuilder result = new StringBuilder();
            if (args.Length != 1)
                return Usage();

            UUID animationID;
            string arg = args[0].Trim();

            if (UUID.TryParse(args[0], out animationID))
            {
                Client.Self.AnimationStart(animationID, true);
            }
            else if (arg.ToLower().Equals("list"))
            {
                foreach (string key in m_BuiltInAnimations.Values)
                {
                    result.AppendLine(key);
                }
            }
            else if (arg.ToLower().Equals("show"))
            {
                Client.Self.SignaledAnimations.ForEach(delegate(KeyValuePair<UUID, int> kvp) {
                    if (m_BuiltInAnimations.ContainsKey(kvp.Key))
                    {
                        result.AppendFormat("The {0} System Animation is being played, sequence is {1}", m_BuiltInAnimations[kvp.Key], kvp.Value);
                    }
                    else
                    {
                        result.AppendFormat("The {0} Asset Animation is being played, sequence is {0}", kvp.Key, kvp.Value);
                    }
                });                                
            }
            else if (m_BuiltInAnimations.ContainsValue(args[0].Trim().ToUpper()))
            {
                foreach (var kvp in m_BuiltInAnimations)
                {
                    if (kvp.Value.Equals(arg.ToUpper()))
                    {
                        Client.Self.AnimationStart(kvp.Key, true);
                        break;
                    }
                }
            }
            else
            {
                return Usage();
            }

            return result.ToString();
        }
    }
}
