using OpenGL;
using System;
using System.Collections.Generic;
using System.Xml;

namespace BoxesAndPlatforms {
	// Loads and manages a OpenGL program

	public class Shader: IDisposable {
		public Shader() { }

		public Shader(string file) {
			load(file);
		}

		~Shader() {
			if (!disposed)
				System.Diagnostics.Debug.Fail("Shader was not disposed of properly.");

			Dispose(false);
		}

		public enum Input {
			None,
			Position,
			Normal,
			TexCoord,
			Color,
			Texture,
			CubeMap,
			MVPMat,
			ModelMat,
			NormMat,
			CameraPos,
			LightDir,
			LightAmbient,
			LightSpecular
		}

		public enum InputType {
			None,
			Attribute,
			Sampler,
			Uniform
		}

		public void load(string file) {
			input.Clear();
			sampler = null;

			// Load shaders and input slots from file

			XmlDocument doc = new XmlDocument();

			doc.Load(file);

			var root = doc.FirstChild;

			while (root != null && root.Name != "shader")
				root = root.NextSibling;

			if (root == null)
				throw new Exception("Failed to load shader");

			string vert = null, frag = null;
			
			for (var i = root.FirstChild; i != null; i = i.NextSibling) {
				switch (i.Name) {
				case "vertex":
					vert = i.InnerText;
					break;
				case "fragment":
					frag = i.InnerText;
					break;
				}
			}

			if (vert == null || frag == null)
				throw new Exception("Failed to load shader (missing vertex/fragment shader)");
			
			var shdVert = compileShader(ShaderType.VertexShader, vert);
			var shdFrag = compileShader(ShaderType.FragmentShader, frag);

			// Link program

			prog = Gl.CreateProgram();

			Gl.AttachShader(prog, shdVert);
			Gl.AttachShader(prog, shdFrag);

			Gl.LinkProgram(prog);

			var result = new int[1] { 0 };

			Gl.GetProgramiv(prog, ProgramParameter.LinkStatus, result);

			if (result[0] <= 0)
				throw new Exception("Failed to link program\n\nAdditional info:\n" + Gl.GetProgramInfoLog(prog));

			// Free unmanaged ressources

			Gl.DetachShader(prog, shdVert);
			Gl.DetachShader(prog, shdFrag);

			Gl.DeleteShader(shdVert);
			Gl.DeleteShader(shdFrag);

			// Handle input tags

			var ind = 0;

			for (var i = root.FirstChild; i != null; i = i.NextSibling) {
				if (i.Name == "input") {
					var type = getInputFromString(i.Attributes["type"].Value);
					
					int location;

					switch (getType(type)) {
					case InputType.Attribute:
						location = Gl.GetAttribLocation(prog, i.Attributes["name"].Value);

						if (location != -1)
							input[type] = location;

						break;
					case InputType.Sampler:
						location = Gl.GetUniformLocation(prog, i.Attributes["name"].Value);

						if (location != -1) {
							if (sampler == null)
								sampler = new List<Tuple<int, int>>();

							sampler.Add(new Tuple<int, int>(location, ind));
							input[type] = ind++;
						}
						break;
					case InputType.Uniform:
						location = Gl.GetUniformLocation(prog, i.Attributes["name"].Value);

						if (location != -1)
							input[type] = Gl.GetUniformLocation(prog, i.Attributes["name"].Value);

						break;
					}
				}
			}
		}

		uint compileShader(ShaderType type, string src) {
			var shd = Gl.CreateShader(type);

			Gl.ShaderSource(shd, src);
			Gl.CompileShader(shd);

			var result = new int[1] { 0 };

			Gl.GetShaderiv(shd, ShaderParameter.CompileStatus, result);

			if (result[0] <= 0)
				throw new Exception("Failed to compile shader\n\nAdditional info:\n" + Gl.GetShaderInfoLog(shd));

			return shd;
		}

		// Enable shader

		public void use() {
			Gl.UseProgram(prog);

			if (sampler != null) {
				foreach (var s in sampler)
					Gl.Uniform1i(s.Item1, s.Item2);

				sampler = null;
			}
		}

		// Get location of a uniform, smapler or attribute

		public int getLocation(Input input) {
			int loc;

			if (this.input.TryGetValue(input, out loc))
				return loc;

			return -1;
		}

		// Set Vector, Matrix or Texture for the specified slot

		public void setVector2(Input input, Vector2 value) {
			var loc = getLocation(input);

			if (loc >= 0)
				Gl.Uniform2f(loc, value.X, value.Y);
		}

		public void setVector3(Input input, Vector3 value) {
			var loc = getLocation(input);

			if (loc >= 0)
				Gl.Uniform3f(loc, value.X, value.Y, value.Z);
		}

		public void setVector4(Input input, Vector4 value) {
			var loc = getLocation(input);

			if (loc >= 0)
				Gl.Uniform4f(loc, value.X, value.Y, value.Z, value.W);
		}

		public void setMatrix3(Input input, Matrix3 mat) {
			var loc = getLocation(input);

			if (loc >= 0)
				Gl.UniformMatrix3fv(loc, mat);
		}

		public void setMatrix4(Input input, Matrix4 mat) {
			var loc = getLocation(input);

			if (loc >= 0)
				Gl.UniformMatrix4fv(loc, mat);
		}

		public void setTexture(Input input, Texture tex) {
			var loc = getLocation(input);

			if (loc >= 0) {
				Gl.ActiveTexture(TextureUnit.Texture0 + loc);
				Gl.BindTexture(tex);
			}
		}

		public void setTexture(Input input, CubeMap tex) {
			var loc = getLocation(input);

			if (loc >= 0) {
				Gl.ActiveTexture(TextureUnit.Texture0 + loc);
				tex.bind();
			}
		}

		Input getInputFromString(string input) {
			switch (input.ToUpper()) {
			case "POSITION":
				return Input.Position;
			case "NORMAL":
				return Input.Normal;
			case "TEXCOORD":
				return Input.TexCoord;
			case "COLOR":
				return Input.Color;
			case "TEXTURE":
				return Input.Texture;
			case "CUBEMAP":
				return Input.CubeMap;
			case "MVPMAT":
				return Input.MVPMat;
			case "MODELMAT":
				return Input.ModelMat;
			case "NORMMAT":
				return Input.NormMat;
			case "CAMERAPOS":
				return Input.CameraPos;
			case "LIGHTDIR":
				return Input.LightDir;
			case "LIGHTAMBIENT":
				return Input.LightAmbient;
			case "LIGHTSPECULAR":
				return Input.LightSpecular;
			}

			return Input.None;
		}

		InputType getType(Input input) {
			switch (input) {
			case Input.Position:
			case Input.Normal:
			case Input.TexCoord:
			case Input.Color:
				return InputType.Attribute;
			case Input.Texture:
			case Input.CubeMap:
				return InputType.Sampler;
			case Input.MVPMat:
			case Input.ModelMat:
			case Input.NormMat:
			case Input.CameraPos:
			case Input.LightDir:
			case Input.LightAmbient:
			case Input.LightSpecular:
				return InputType.Uniform;
			}

			return InputType.None;
		}

		void Dispose(bool disposing) {
			if (!disposed) {
				disposed = true;

				if (disposing)
					Gl.DeleteProgram(prog);
			}
		}
		
		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		bool disposed;

		uint prog;

		List<Tuple<int, int>> sampler;
		
		Dictionary<Input, int> input = new Dictionary<Input, int>();
	}
}
