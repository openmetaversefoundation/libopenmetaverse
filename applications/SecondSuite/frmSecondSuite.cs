using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Reflection;
using libsecondlife;
using SecondSuite.Plugins;

namespace SecondSuite
{
	public class frmSecondSuite : System.Windows.Forms.Form
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.StatusBar statusBar;
		private System.Windows.Forms.MenuItem menuItem3;
		private System.Windows.Forms.MainMenu mainMenu;
		private System.Windows.Forms.MenuItem menuFile;
		private System.Windows.Forms.MenuItem menuExit;
		private System.Windows.Forms.MenuItem menuHelp;
		private System.Windows.Forms.MenuItem menuAbout;
		private System.Windows.Forms.MenuItem menuReload;

		//
		private frmOverview Overview;

		public frmSecondSuite()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			Overview = new frmOverview(this);
			Overview.MdiParent = this;

			FindPlugins();

			Overview.Show();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			// Kill all the clients and plugins
			DisconnectAllClients();
			ShutdownAllPlugins();

			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		public void AddClient(SecondLife client)
		{
			Global.ClientsMutex.WaitOne();
			Global.Clients.Add(client);
			Global.ClientsMutex.ReleaseMutex();

			Overview.ClientAdded(client);
		}

		public void RemoveClient(SecondLife client)
		{
			Global.ClientsMutex.WaitOne();
			client.Network.Logout();
			Global.Clients.Remove(client);
			Global.ClientsMutex.ReleaseMutex();

			Overview.ClientRemoved(client);
		}

		public void DisconnectAllClients()
		{
			Global.ClientsMutex.WaitOne();

			foreach (SecondLife client in Global.Clients)
			{
				client.Network.Logout();
			}

			Global.Clients.Clear();

			Global.ClientsMutex.ReleaseMutex();
		}

		public void ShutdownAllPlugins()
		{
			Global.PluginsMutex.WaitOne();

			foreach (SSPlugin plugin in Global.Plugins)
			{
				plugin.Shutdown();
			}

			Global.Plugins.Clear();

			Global.PluginsMutex.ReleaseMutex();
		}

		public void FindPlugins()
		{
			//First empty the collection, we're reloading them all
			Global.PluginsMutex.WaitOne();
			Global.Plugins = new PluginCollection();
			Overview.lstPlugins.Items.Clear();
			Global.PluginsMutex.ReleaseMutex();

			//Go through all the files in the plugin directory
			foreach (string filename in Directory.GetFiles(
				AppDomain.CurrentDomain.BaseDirectory + "\\plugins\\"))
			{
				FileInfo file = new FileInfo(filename);

				//Preliminary check, must be .dll
				if (file.Extension.Equals(".dll"))
				{
					//Add the 'plugin'
					this.AddPlugin(filename);
				}
			}
		}

		private void AddPlugin(string filename)
		{
			SSPlugin plugin;

			try
			{
				Assembly pluginAssembly = Assembly.LoadFrom(filename);

				foreach (Type pluginType in pluginAssembly.GetTypes())
				{
					if (pluginType.IsPublic && !pluginType.IsAbstract)
					{
						Type typeInterface = pluginType.GetInterface("SecondSuite.Plugins.SSPlugin", true);

						if (typeInterface != null)
						{
							plugin = (SSPlugin)Activator.CreateInstance(pluginAssembly.GetType(pluginType.ToString()));

							Global.PluginsMutex.WaitOne();
							Global.Plugins.Add(plugin);
							Global.PluginsMutex.ReleaseMutex();

							Overview.lstPlugins.Items.Add(plugin);
						}
					}
				}
			}
			catch (ReflectionTypeLoadException e)
			{
				MessageBox.Show(e.LoaderExceptions[0].ToString());
			}
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.statusBar = new System.Windows.Forms.StatusBar();
			this.mainMenu = new System.Windows.Forms.MainMenu();
			this.menuFile = new System.Windows.Forms.MenuItem();
			this.menuReload = new System.Windows.Forms.MenuItem();
			this.menuItem3 = new System.Windows.Forms.MenuItem();
			this.menuExit = new System.Windows.Forms.MenuItem();
			this.menuHelp = new System.Windows.Forms.MenuItem();
			this.menuAbout = new System.Windows.Forms.MenuItem();
			this.SuspendLayout();
			// 
			// statusBar
			// 
			this.statusBar.Location = new System.Drawing.Point(0, 681);
			this.statusBar.Name = "statusBar";
			this.statusBar.ShowPanels = true;
			this.statusBar.Size = new System.Drawing.Size(992, 22);
			this.statusBar.TabIndex = 1;
			// 
			// mainMenu
			// 
			this.mainMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.menuFile,
																					 this.menuHelp});
			// 
			// menuFile
			// 
			this.menuFile.Index = 0;
			this.menuFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.menuReload,
																					 this.menuItem3,
																					 this.menuExit});
			this.menuFile.Text = "File";
			// 
			// menuReload
			// 
			this.menuReload.Index = 0;
			this.menuReload.Text = "Reload Plugins";
			this.menuReload.Click += new System.EventHandler(this.menuReload_Click);
			// 
			// menuItem3
			// 
			this.menuItem3.Index = 1;
			this.menuItem3.Text = "-";
			// 
			// menuExit
			// 
			this.menuExit.Index = 2;
			this.menuExit.Text = "Exit";
			this.menuExit.Click += new System.EventHandler(this.menuExit_Click);
			// 
			// menuHelp
			// 
			this.menuHelp.Index = 1;
			this.menuHelp.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.menuAbout});
			this.menuHelp.Text = "Help";
			// 
			// menuAbout
			// 
			this.menuAbout.Index = 0;
			this.menuAbout.Text = "About";
			// 
			// frmSecondSuite
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(992, 703);
			this.Controls.Add(this.statusBar);
			this.IsMdiContainer = true;
			this.Menu = this.mainMenu;
			this.Name = "frmSecondSuite";
			this.Text = "Second Suite";
			this.Load += new System.EventHandler(this.frmSecondSuite_Load);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new frmSecondSuite());
		}

		private void frmSecondSuite_Load(object sender, System.EventArgs e)
		{
		
		}

		private void menuExit_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void menuReload_Click(object sender, System.EventArgs e)
		{
			ShutdownAllPlugins();
			FindPlugins();
		}
	}
}
