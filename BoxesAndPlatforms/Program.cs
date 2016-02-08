using System;
using OpenGL;

namespace BoxesAndPlatforms {
	static class Program {
		[STAThread]
		static void Main(string[] argv) {
			//try {
				// Set culture to invariant culture in order to parse floating points correctly
				System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

				if (Array.FindIndex(argv, arg => arg.ToLower() == "-log") >= 0)
					Log.enable();

				if (Array.FindIndex(argv, arg => arg.ToLower() == "-force") >= 0)
					Log.force = true;

				Log.log("Application started");

				Settings.load();

				Log.log("Creating window");
				Window.init("Boxes 'n' Platforms", Settings.width, Settings.height);
				
				Window.fullscreen = Settings.fullscreen;

				if (Settings.maxfps < 0)
					Window.limitFPS = false;
				else {
					Window.limitFPS = true;
					Window.frequency = Settings.maxfps;
				}

				Log.log("Loading OpenGL");
				Gl.ReloadFunctions();
				Log.logf("OpenGL Version {0}", Gl.GetString(StringName.Version));
				
				Window.show();

				Window.cursorVisible = false;

				Window.eventRender += RoomManager.render;
				Window.eventUpdate += RoomManager.update;
				
				Log.logf("Loading assets");
				AssetLoader.load("assets.xml");
				
				RoomManager.gotoRoom(new RoomMenu());

				Log.log("Entering Main Loop");
				Window.loop();
				
				Settings.save();

				Log.log("Terminating");
			// }
			/*catch (Exception ex) {
				#if DEBUG
					System.Diagnostics.Debug.Fail(ex.ToString());
				#else
					Window.errorMessageBox(ex.ToString(), "Boxes 'n' Platforms");
				#endif

				Log.logf("Exception caught: \r\n\r\n{0}", ex.ToString());
			}
			finally {*/
				AssetLoader.dispose();
				RoomManager.dispose();
				Window.dispose();
				Log.dispose();
			//}
		}
	}
}
