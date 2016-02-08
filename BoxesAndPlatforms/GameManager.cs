using System;
using System.Collections.Generic;
using System.Xml;

namespace BoxesAndPlatforms {
	// Keeps track of the current Level and stores the layout for each one

	public class GameManager {
		public struct Level {
			public int number;
			public string name;
			public WorldLayout world;
		}

		public GameManager() {
			levels = new List<Level>();
		}

		public GameManager(string file): this() {
			load(file);
		}

		// Loads levels from an XML file

		public void load(string file) {
			XmlDocument doc = new XmlDocument();

			doc.Load(file);

			var root = doc.FirstChild;

			while (root != null && root.Name != "levels")
				root = root.NextSibling;

			if (root == null)
				throw new Exception("Failed to load levels");

			for (var level = root.FirstChild; level != null; level = level.NextSibling) {
				if (level.Name == "level") {
					var levelFile = level.Attributes["file"].Value;
					
					levels.Add(new Level() {
						number = int.Parse(level.Attributes["number"].Value),
						name = level.Attributes["name"].Value,
						world = AssetLoader.getWorldLayout(levelFile)
					});
				}
			}

			levels.Sort((x, y) => x.number - y.number);
		}

		public void next() {
			if (++current >= levels.Count)
				current = levels.Count - 1;
		}

		public void set(int index) {
			current = Utils.clamp(index, 0, levels.Count - 1);
		}

		public WorldLayout world {
			get {
				return levels[current].world;
			}
		}

		public bool hasNext {
			get {
				return current < levels.Count - 1;
			}
		}
		
		public int current { get; private set; }

		public List<Level> levels { get; private set; }
	}
}
