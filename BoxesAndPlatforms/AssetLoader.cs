using OpenGL;
using System;
using System.Collections.Generic;
using System.Xml;

namespace BoxesAndPlatforms {
	// Stores Assets. If a Asset isn't already cached it loads it and saves it to a dictionary.

	static public class AssetLoader {
		public static void dispose() {
			foreach (var cubemap in cubemaps.Values)
				cubemap.Dispose();

			foreach (var texture in textures.Values)
				texture.Dispose();

			foreach (var shader in shaders.Values)
				shader.Dispose();

			foreach (var mesh in meshes.Values)
				mesh.Dispose();
		}

		public static Atlas getAtlas(string file) {
			Atlas atlas;

			if (!atlases.TryGetValue(Utils.normalizePath(file), out atlas)) {
				Log.logf("Loading Atlas: {0}", file);
				atlases[Utils.normalizePath(file)] = atlas = new Atlas(file);
			}
			
			return atlas;
		}

		public static CubeMap getCubeMap(string file) {
			CubeMap cubemap;

			if (!cubemaps.TryGetValue(Utils.normalizePath(file), out cubemap)) {
				Log.logf("Loading CubeMap: {0}", file);
				cubemaps[Utils.normalizePath(file)] = cubemap = new CubeMap(file);
			}

			return cubemap;
		}

		public static Mesh getMesh(string file) {
			Mesh mesh;

			if (!meshes.TryGetValue(Utils.normalizePath(file), out mesh)) {
				Log.logf("Loading Mesh: {0}", file);
				meshes[Utils.normalizePath(file)] = mesh = new Mesh(file);
			}

			return mesh;
		}

		public static Texture getTexture(string file) {
			Texture texture;

			if (!textures.TryGetValue(Utils.normalizePath(file), out texture)) {
				Log.logf("Loading Texture: {0}", file);
				textures[Utils.normalizePath(file)] = texture = new Texture(file);
			}

			return texture;
		}

		public static Shader getShader(string file) {
			Shader shader;

			if (!shaders.TryGetValue(Utils.normalizePath(file), out shader)) {
				Log.logf("Loading Shader: {0}", file);
				shaders[Utils.normalizePath(file)] = shader = new Shader(file);
			}

			return shader;
		}

		public static WorldLayout getWorldLayout(string file) {
			WorldLayout worldlayout;

			if (!worldlayouts.TryGetValue(Utils.normalizePath(file), out worldlayout)) {
				Log.logf("Loading World Layout: {0}", file);
				worldlayouts[Utils.normalizePath(file)] = worldlayout = new WorldLayout(file);
			}

			return worldlayout;
		}
		
		// Preload assets from a XML file

		public static void load(string file) {
			XmlDocument doc = new XmlDocument();

			doc.Load(file);

			var root = doc.FirstChild;

			while (root != null && root.Name != "assets")
				root = root.NextSibling;

			if (root == null)
				throw new Exception("Failed to load assets");

			for (var asset = root.FirstChild; asset != null; asset = asset.NextSibling) {
				if (asset.Name == "asset") {
					var f = asset.Attributes["file"].Value;

					switch (asset.Attributes["type"].Value.ToUpper()) {
					case "ATLAS":
						Log.logf("Loading Atlas: {0}", f);
						atlases[Utils.normalizePath(f)] = new Atlas(f);
						break;
					case "CUBEMAP":
						Log.logf("Loading CubeMap: {0}", f);
						cubemaps[Utils.normalizePath(f)] = new CubeMap(f);
						break;
					case "MESH":
						Log.logf("Loading Mesh: {0}", f);
						meshes[Utils.normalizePath(f)] = new Mesh(f);
						break;
					case "SHADER":
						Log.logf("Loading Shader: {0}", f);
						shaders[Utils.normalizePath(f)] = new Shader(f);
						break;
					case "TEXTURE":
						Log.logf("Loading Texture: {0}", f);
						textures[Utils.normalizePath(f)] = new Texture(f);
						break;
					case "WORLDLAYOUT":
						Log.logf("Loading World Layout: {0}", f);
						worldlayouts[Utils.normalizePath(f)] = new WorldLayout(f);
						break;
					}
				}
			}
		}

		static Dictionary<string, Atlas> atlases            = new Dictionary<string, Atlas>();
		static Dictionary<string, CubeMap> cubemaps         = new Dictionary<string, CubeMap>();
		static Dictionary<string, Mesh> meshes              = new Dictionary<string, Mesh>();
		static Dictionary<string, Shader> shaders           = new Dictionary<string, Shader>();
		static Dictionary<string, Texture> textures         = new Dictionary<string, Texture>();
		static Dictionary<string, WorldLayout> worldlayouts = new Dictionary<string, WorldLayout>();
	}
}
