using System.IO;
using System.Xml;

namespace BoxesAndPlatforms {
	// Handle settings (save and load them)

	static class Settings {
		// Load settings from an XML file

		static public void load() {
			if (File.Exists(file)) {
				XmlDocument doc = new XmlDocument();

				doc.Load(file);

				var root = doc.FirstChild;

				while (root != null && root.Name != "settings")
					root = root.NextSibling;

				if (root != null) {
					for (var setting = root.FirstChild; setting != null; setting = setting.NextSibling) {
						switch (setting.Name) {
						case "resolution":
							width = int.Parse(setting.Attributes["width"].Value);
							height = int.Parse(setting.Attributes["height"].Value);
							break;
						case "fullscreen":
							fullscreen = bool.Parse(setting.Attributes["fullscreen"].Value);
							break;
						case "maxfps":
							maxfps = int.Parse(setting.Attributes["maxfps"].Value);
							break;
						}
					}
				}
			}

			_width = width;
			_height = height;
			_fullscreen = fullscreen;
			_maxfps = maxfps;
		}

		// Store settings as a XML file

		static public void save() {
			if (hasChanged()) {
				var writer = XmlWriter.Create(file, new XmlWriterSettings() { Indent = true });

				writer.WriteStartDocument();
				writer.WriteStartElement("settings");

				writer.WriteStartElement("resolution");
				writer.WriteAttributeString("width", width.ToString());
				writer.WriteAttributeString("height", height.ToString());
				writer.WriteEndElement();

				writer.WriteStartElement("fullscreen");
				writer.WriteAttributeString("fullscreen", fullscreen.ToString());
				writer.WriteEndElement();

				writer.WriteStartElement("maxfps");
				writer.WriteAttributeString("maxfps", maxfps.ToString());
				writer.WriteEndElement();

				writer.WriteEndElement();
				writer.WriteEndDocument();

				writer.Close();
			}
		}

		static bool hasChanged() {
			if (width != _width)
				return true;

			if (height != _height)
				return true;

			if (fullscreen != _fullscreen)
				return true;

			if (maxfps != _maxfps)
				return true;

			return false;
		}
		
		const string file = "settings.xml";

		static int _width;
		static int _height;
		static bool _fullscreen;
		static int _maxfps;

		static public int width = 800;
		static public int height = 600;
		static public bool fullscreen = false;
		static public int maxfps = 30;
	}
}
