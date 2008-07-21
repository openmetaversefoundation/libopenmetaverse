using System;
using System.Text.RegularExpressions;
using libsecondlife;

namespace libsecondlife.TestClient
{
    public class PrimRegexCommand : Command
    {
        public PrimRegexCommand(TestClient testClient)
        {
            Name = "primregex";
            Description = "Find prim by text predicat. " +
                "Usage: primregex [text predicat] (eg findprim .away.)";
        }

        public override string Execute(string[] args, LLUUID fromAgentID)
        {
            if (args.Length < 1)
                return "Usage: primregex [text predicat]";

            try
            {
                // Build the predicat from the args list
                string predicatPrim = string.Empty;
                for (int i = 0; i < args.Length; i++)
                    predicatPrim += args[i] + " ";
                predicatPrim = predicatPrim.TrimEnd();

                // Build Regex
                Regex regexPrimName = new Regex(predicatPrim.ToLower());

                // Print result
                Logger.Log(string.Format("Searching prim for [{0}] ({1} prims loaded in simulator)\n", predicatPrim,
                    Client.Network.CurrentSim.ObjectsPrimitives.Count), Helpers.LogLevel.Info, Client);

                Client.Network.CurrentSim.ObjectsPrimitives.ForEach(
                    delegate(Primitive prim)
                    {
                        if (prim.Text != null && regexPrimName.IsMatch(prim.Text.ToLower()))
                        {
                            Logger.Log(string.Format("\nNAME={0}\nID = {1}\nFLAGS = {2}\nTEXT = '{3}'\nDESC='{4}", prim.PropertiesFamily.Name,
                                prim.ID, prim.Flags.ToString(), prim.Text, prim.PropertiesFamily.Description), Helpers.LogLevel.Info, Client);
                        }
                        else if (prim.PropertiesFamily.Name != null && regexPrimName.IsMatch(prim.PropertiesFamily.Name.ToLower()))
                        {
                            Logger.Log(string.Format("\nNAME={0}\nID = {1}\nFLAGS = {2}\nTEXT = '{3}'\nDESC='{4}", prim.PropertiesFamily.Name,
                                prim.ID, prim.Flags.ToString(), prim.Text, prim.PropertiesFamily.Description), Helpers.LogLevel.Info, Client);
                        }
                        else if (prim.PropertiesFamily.Description != null && regexPrimName.IsMatch(prim.PropertiesFamily.Description.ToLower()))
                        {
                            Logger.Log(string.Format("\nNAME={0}\nID = {1}\nFLAGS = {2}\nTEXT = '{3}'\nDESC='{4}", prim.PropertiesFamily.Name,
                                prim.ID, prim.Flags.ToString(), prim.Text, prim.PropertiesFamily.Description), Helpers.LogLevel.Info, Client);
                        }
                    }
                );
            }
            catch (System.Exception e)
            {
                Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e);
                return "Error searching";
            }

            return "Done searching";
        }
    }
}
