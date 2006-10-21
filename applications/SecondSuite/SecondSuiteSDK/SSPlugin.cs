using System;
using libsecondlife;

namespace SecondSuite.Plugins
{
	public delegate void ConnectionEvent();

	/// <summary>
	/// A public interface for Second Suite plugins
	/// </summary>
	public interface SSPlugin
	{
		string Name{get;}
		string Author{get;}
		string Homepage{get;}
		string Description{get;}
		bool SecondLifeClient{get;}

		ConnectionEvent ConnectionHandler{get;}

		void Init(SecondLife client);
		System.Windows.Forms.Form Load();
		void Shutdown();
	}
}
