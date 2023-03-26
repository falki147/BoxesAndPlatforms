using OpenGL;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Xml;

namespace BoxesAndPlatforms {
	public class WorldLayout {
		public struct KeyFrame {
			public float time;
			public Vector3 pos;
		}

		public class Entry {
			public enum Activation {
				None,
				Always,
				Player
			}

			public enum Action {
				Nothing,
				Repeat,
				Reverse
			}

			// Calculate position from time
			
			public Vector3 getPosition(float time) {
				if (frames == null)
					return pos;

				if (time < 0)
					return pos;

				if (time > duration)
					return frames[frames.Length - 1].pos;

				int i;

				for (i = 0; i < frames.Length; ++i)
					if (frames[i].time > time)
						break;
				
				var beg  = i == 0 ? pos : frames[i - 1].pos;
				var begt = i == 0 ? 0 : frames[i - 1].time;

				var t = (time - begt) / (frames[i].time - begt);
				
				return beg + (frames[i].pos - beg) * t;
			}

			public string     id;
			public Vector3    pos;
			public Vector2    size;
			public string     name;
			public string     link;
			public bool       inactive;
			public Activation activation;
			public Action     onEnd;
			public KeyFrame[] frames;
			public float      duration;
		}

		public WorldLayout(string file) {
			load(file);
		}

		// Load WorldLayout from XML file saved by the LevelCreater

		public void load(string file) {
			var entryList = new List<Entry>();

			XmlDocument doc = new XmlDocument();

			doc.Load(file);

			var root = doc.FirstChild;

			while (root != null && root.Name != "entities")
				root = root.NextSibling;

			if (root == null)
				throw new Exception("Failed to load entities");

			foreach (XmlNode entity in root.SelectNodes("entity")) {
				var keyframes = new List<KeyFrame>();

				foreach (XmlNode keyframe in entity.SelectNodes("keyframe")) {
					keyframes.Add(new KeyFrame() {
						time = float.Parse(keyframe.Attributes["time"].Value),
						pos = new Vector3(
							float.Parse(keyframe.Attributes["x"].Value),
							float.Parse(keyframe.Attributes["y"].Value),
							float.Parse(keyframe.Attributes["z"].Value)
						)
					});
				}

				var attrWidth      = entity.Attributes["width"];
				var attrHeight     = entity.Attributes["height"];
				var attrName       = entity.Attributes["name"];
				var attrLink       = entity.Attributes["link"];
				var attrInactive   = entity.Attributes["inactive"];
				var attrActivation = entity.Attributes["activation"];
				var attrOnEnd      = entity.Attributes["onEnd"];

				entryList.Add(new Entry() {
					id = entity.Attributes["id"].Value,
					pos = new Vector3(
						float.Parse(entity.Attributes["x"].Value),
						float.Parse(entity.Attributes["y"].Value),
						float.Parse(entity.Attributes["z"].Value)
					),
					size = new Vector2(
						attrWidth == null ? 1 : int.Parse(attrWidth.Value),
						attrHeight == null ? 1 : int.Parse(attrHeight.Value)
					),
					name = attrName == null ? "" : attrName.Value,
					link = attrLink == null ? "" : attrLink.Value,
					inactive = attrInactive == null ? false : bool.Parse(attrInactive.Value),
                    frames = keyframes.Count == 0 ? null : keyframes.ToArray(),
					activation = attrActivation == null ? Entry.Activation.None : (
						attrActivation.Value.ToUpper() == "ALWAYS" ? Entry.Activation.Always : (
							attrActivation.Value.ToUpper() == "PLAYER" ? Entry.Activation.Player : Entry.Activation.None
						)
					),
					onEnd = attrOnEnd == null ? Entry.Action.Nothing : (
						attrOnEnd.Value.ToUpper() == "REPEAT" ? Entry.Action.Repeat : (
							attrOnEnd.Value.ToUpper() == "REVERSE" ? Entry.Action.Reverse : Entry.Action.Nothing
						)
					),
					duration = keyframes.Count > 0 ? keyframes[keyframes.Count - 1].time : 0
				});
			}

			entries = entryList.ToArray();
		}

		public Entry[] entries;
	}
}
