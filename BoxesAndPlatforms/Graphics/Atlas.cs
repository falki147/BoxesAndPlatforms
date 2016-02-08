using OpenGL;
using System;
using System.Collections.Generic;
using System.Xml;

namespace BoxesAndPlatforms {
	// Stores the location of images and fonts on a texture

	public class Atlas {
		public Atlas() { }

		public Atlas(string file) {
			load(file);
		}

		public struct Image {
			public Vector2 offset;
			public Vector2 size;
			public Vector2 p1;
			public Vector2 p2;
			public Vector2 p3;
			public Vector2 p4;
		}

		public struct Glyph {
			public bool empty;
			public Vector2 advance;
			public Vector2 offset;
			public Vector2 size;
			public Vector2 p1;
			public Vector2 p2;
			public Vector2 p3;
			public Vector2 p4;
		}
		
		public struct Font {
			public float height;
			public Dictionary<char, Glyph> glyphs;
		}

		// Parse XML file that contains an atlas

		public void load(string file) {
			XmlDocument doc = new XmlDocument();

			doc.Load(file);

			var root = doc.FirstChild;

			while (root != null && root.Name != "atlas")
				root = root.NextSibling;

			if (root == null)
				throw new Exception("Failed to load texture atlas");

			var width  = float.Parse(root.Attributes["width"].Value);
			var height = float.Parse(root.Attributes["height"].Value);
			
			images = new Dictionary<string, Atlas.Image>();
			fonts  = new Dictionary<string, Atlas.Font>();

			for (var i = root.FirstChild; i != null; i = i.NextSibling) {
				switch (i.Name) {
				case "images":
					for (var j = i.FirstChild; j != null; j = j.NextSibling) {
						if (j.Name == "image") {
							var flipped = bool.Parse(j.Attributes["flipped"].Value);

							var w = float.Parse(j.Attributes["w"].Value);
							var h = float.Parse(j.Attributes["h"].Value);

							var nx = float.Parse(j.Attributes["x"].Value) / width;
							var ny = float.Parse(j.Attributes["y"].Value) / height;
							var nw = w / width;
							var nh = h / height;

							if (!flipped)
								images[j.Attributes["name"].Value] = new Atlas.Image() {
									offset = new Vector2(
										float.Parse(j.Attributes["offsetX"].Value),
										float.Parse(j.Attributes["offsetY"].Value)
									),
									size = new Vector2(w, h),
									p1 = new Vector2(nx, ny),
									p2 = new Vector2(nx + nw, ny),
									p3 = new Vector2(nx, ny - nh),
									p4 = new Vector2(nx + nw, ny - nh)
								};
							else
								images[j.Attributes["name"].Value] = new Atlas.Image() {
									offset = new Vector2(
										float.Parse(j.Attributes["offsetX"].Value),
										float.Parse(j.Attributes["offsetY"].Value)
									),
									size = new Vector2(h, w),
									p1 = new Vector2(nx, ny - nh),
									p2 = new Vector2(nx, ny),
									p3 = new Vector2(nx + nw, ny - nh),
									p4 = new Vector2(nx + nw, ny)
								};
						}
					}
					break;
				case "fonts":
					for (var j = i.FirstChild; j != null; j = j.NextSibling) {
						if (j.Name == "font") {
							var font = new Atlas.Font() {
								height = float.Parse(j.Attributes["height"].Value),
								glyphs = new Dictionary<char, Atlas.Glyph>()
							};

							for (var k = j.FirstChild; k != null; k = k.NextSibling) {
								if (k.Name == "glyph") {
									var flipped = bool.Parse(k.Attributes["flipped"].Value);

									var w = float.Parse(k.Attributes["w"].Value);
									var h = float.Parse(k.Attributes["h"].Value);

									var nx = float.Parse(k.Attributes["x"].Value) / width;
									var ny = float.Parse(k.Attributes["y"].Value) / height;
									var nw = w / width;
									var nh = h / height;

									if (w != 0 && h != 0) {
										if (!flipped)
											font.glyphs[(char) int.Parse(k.Attributes["char"].Value)] = new Atlas.Glyph() {
												empty = false,
												advance = new Vector2(
													float.Parse(k.Attributes["advX"].Value),
													float.Parse(k.Attributes["advY"].Value)
												),
												offset = new Vector2(
													float.Parse(k.Attributes["offsetX"].Value),
													float.Parse(k.Attributes["offsetY"].Value)
												),
												size = new Vector2(w, h),
												p1 = new Vector2(nx, ny),
												p2 = new Vector2(nx + nw, ny),
												p3 = new Vector2(nx, ny - nh),
												p4 = new Vector2(nx + nw, ny - nh)
											};
										else
											font.glyphs[(char) int.Parse(k.Attributes["char"].Value)] = new Atlas.Glyph() {
												empty = false,
												advance = new Vector2(
													float.Parse(k.Attributes["advX"].Value),
													float.Parse(k.Attributes["advY"].Value)
												),
												offset = new Vector2(
													float.Parse(k.Attributes["offsetX"].Value),
													float.Parse(k.Attributes["offsetY"].Value)
												),
												size = new Vector2(h, w),
												p1 = new Vector2(nx, ny - nh),
												p2 = new Vector2(nx, ny),
												p3 = new Vector2(nx + nw, ny - nh),
												p4 = new Vector2(nx + nw, ny)
											};
									}
									else {
										font.glyphs[(char) int.Parse(k.Attributes["char"].Value)] = new Atlas.Glyph() {
											empty = true,
											advance = new Vector2(
												float.Parse(k.Attributes["advX"].Value),
												float.Parse(k.Attributes["advY"].Value)
											)
										};
									}
								}
							}

							fonts[j.Attributes["name"].Value] = font;
						}
					}
					break;
				}
			}
		}

		public Dictionary<string, Image> images;
		public Dictionary<string, Font>  fonts;
	}
}
