using System;
using System.Collections;
using System.Threading;
using SecondSuite.Plugins;

namespace SecondSuite
{
	public class Global
	{
		public Global()
		{
		}
		
		public static PluginCollection Plugins = new PluginCollection();
		public static Mutex PluginsMutex = new Mutex(false, "PluginsMutex");
		public static ArrayList Clients = new ArrayList();
		public static Mutex ClientsMutex = new Mutex(false, "ClientsMutex");
	}
}
