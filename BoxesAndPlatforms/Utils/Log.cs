using System.IO;

namespace BoxesAndPlatforms {
	// Small static log class for logging

	static public class Log {
		static public void enable() {
			enabled = true;
			stream = new StreamWriter(File.OpenWrite("log.txt"));
		}

		static public void dispose() {
			if (stream != null)
				stream.Close();
		}

		static public void log(string str) {
			if (enabled) {
				stream.WriteLine(str);

				if (force)
					stream.Flush();
			}
		}

		static public void logf(string str, params object[] arg) {
			if (enabled) {
				stream.WriteLine(str, arg);

				if (force)
					stream.Flush();
			}
		}

		static public bool enabled { get; private set; }

		static StreamWriter stream;

		static public bool force;
	}
}
