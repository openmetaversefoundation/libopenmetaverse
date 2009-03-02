using System;
using System.Collections.Generic;
using System.Reflection;
using ExtensionLoader;
using OpenMetaverse;

namespace Simian.Extensions
{
    public class ScriptFunction
    {
        public MethodInfo Method;
        public string Name;
        public List<Type> Parameters;
        public Type Return;
    }

    public class ScriptConsole : IExtension<Simian>
    {
        Simian server;
        IScriptApi api;
        Dictionary<string, ScriptFunction> functions;

        public ScriptConsole()
        {
        }

        public void Start(Simian server)
        {
            this.server = server;

            // Create a single local scripting API instance that we will manage
            api = new ScriptApi();
            api.Start(server, null, UUID.Zero, false, false);

            // Create a dictionary of all of the scripting functions
            functions = new Dictionary<string, ScriptFunction>();
            Type apiType = typeof(IScriptApi);
            MethodInfo[] apiMethods = apiType.GetMethods();
            for (int i = 0; i < apiMethods.Length; i++)
            {
                MethodInfo method = apiMethods[i];

                if (!method.IsConstructor && method.Name != "Start" && method.Name != "Stop")
                {
                    ScriptFunction function = new ScriptFunction();
                    function.Method = method;
                    function.Name = method.Name;
                    function.Return = method.ReturnParameter.ParameterType.UnderlyingSystemType;
                    function.Parameters = new List<Type>();

                    ParameterInfo[] parms = method.GetParameters();
                    for (int j = 0; j < parms.Length; j++)
                        function.Parameters.Add(parms[j].ParameterType.UnderlyingSystemType);

                    functions.Add(function.Name, function);
                }
            }

            server.Scene.OnObjectChat += new ObjectChatCallback(Scene_OnObjectChat);
        }

        public void Stop()
        {
            lock (api)
                api.Stop();
        }

        void PrintFunctionUsage(ScriptFunction function)
        {
            string message = "Usage: " + function.Return.Name + " " + function.Name + "(";
            for (int i = 0; i < function.Parameters.Count; i++)
            {
                message += function.Parameters[i].Name;
                if (i != function.Parameters.Count - 1)
                    message += ", ";
            }
            message += ")";

            server.Scene.ObjectChat(this, UUID.Zero, UUID.Zero, ChatAudibleLevel.Fully, ChatType.OwnerSay, ChatSourceType.Object,
                "Script Console", Vector3.Zero, 0, message);
        }

        List<string> ParseParameters(string paramsString)
        {
            List<string> parameters = new List<string>();
            int i = 0;

            while (i < paramsString.Length)
            {
                int commaPos = paramsString.IndexOf(',', i);
                int bracketPos = paramsString.IndexOf('<', i);
                int endBracketPos = paramsString.IndexOf('>', i);
                string param;

                if (bracketPos > 0 && bracketPos < commaPos)
                {
                    if (endBracketPos > bracketPos)
                        commaPos = paramsString.IndexOf(',', endBracketPos);
                    else
                        return null;
                }

                if (commaPos > 0)
                {
                    // ...,
                    param = paramsString.Substring(i, commaPos - i);
                        i = commaPos + 1;
                }
                else
                {
                    // ...
                    param = paramsString.Substring(i);
                    i = paramsString.Length;
                }

                parameters.Add(param);
            }

            return parameters;
        }

        object[] ConvertParameters(ScriptFunction function, List<string> parameters)
        {
            object[] objParameters = new object[function.Parameters.Count];

            for (int i = 0; i < objParameters.Length; i++)
            {
                Type paramType = function.Parameters[i];

                if (paramType == typeof(int))
                {
                    int value;
                    if (Int32.TryParse(parameters[i], out value))
                        objParameters[i] = value;
                    else
                        return null;
                }
                else if (paramType == typeof(double))
                {
                    double value;
                    if (Double.TryParse(parameters[i], out value))
                        objParameters[i] = value;
                    else
                        return null;
                }
                else if (paramType == typeof(ScriptTypes.LSL_Vector))
                {
                    ScriptTypes.LSL_Vector value;
                    value = (ScriptTypes.LSL_Vector)parameters[i];
                    objParameters[i] = value;
                }
                else if (paramType == typeof(ScriptTypes.LSL_Rotation))
                {
                    ScriptTypes.LSL_Rotation value;
                    value = (ScriptTypes.LSL_Rotation)parameters[i];
                    objParameters[i] = value;
                }
                else
                {
                    // String value, or something that can (hopefully) be implicitly converted
                    objParameters[i] = parameters[i];
                }
            }

            return objParameters;
        }

        void Scene_OnObjectChat(object sender, UUID ownerID, UUID sourceID, ChatAudibleLevel audible, ChatType type,
            ChatSourceType sourceType, string fromName, Vector3 position, int channel, string message)
        {
            if (sourceType == ChatSourceType.Agent && type == ChatType.Normal)
            {
                if (message.StartsWith("ll") && message.Contains("("))
                {
                    int startParam = message.IndexOf('(');
                    int endParam = message.IndexOf(')');

                    if (startParam > 2 && endParam > startParam)
                    {
                        // Try and parse this into a function call
                        string name = message.Substring(0, startParam);

                        ScriptFunction function;
                        if (functions.TryGetValue(name, out function))
                        {
                            // Parse the parameters
                            ++startParam;
                            List<string> parameters = ParseParameters(message.Substring(startParam, endParam - startParam));

                            // Parameters sanity check
                            if (parameters != null && parameters.Count == function.Parameters.Count)
                            {
                                // Convert the parameters into the required types
                                object[] objParameters = ConvertParameters(function, parameters);

                                if (objParameters != null)
                                {
                                    // Find the avatar that sent this chat
                                    SimulationObject avatar;
                                    if (server.Scene.TryGetObject(ownerID, out avatar))
                                    {
                                        // Lock the API, setup the member values, and invoke the function
                                        lock (api)
                                        {
                                            api.Start(server, avatar, UUID.Random(), false, false);

                                            object ret = function.Method.Invoke(api, objParameters);

                                            if (ret != null)
                                            {
                                                server.Scene.ObjectChat(this, UUID.Zero, UUID.Zero, ChatAudibleLevel.Fully, ChatType.OwnerSay,
                                                    ChatSourceType.Object, "Script Console", Vector3.Zero, 0, ret.ToString());
                                            }

                                            return;
                                        }
                                    }
                                }
                            }

                            PrintFunctionUsage(function);
                        }
                    }
                }
            }
        }
    }
}
