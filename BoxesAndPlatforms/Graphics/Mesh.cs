using OpenGL;
using System;
using System.IO;
using System.Numerics;

namespace BoxesAndPlatforms {
	// Stores a mesh

	public class Mesh: IDisposable {
		[Flags]
		enum Flags {
			Normals   = 1, // Mesh has normals
			TexCoords = 2, // Mesh has texture coordinates
			Colors    = 4  // Mesh has colors
		}

		public Mesh() { }

		public Mesh(string file) {
			load(file);
		}

		~Mesh() {
			if (!disposed)
				System.Diagnostics.Debug.Fail("Mesh was not disposed of properly.");

			Dispose(false);
		}

		// Loads a MODB file and stores it's content on the GPU

		public void load(string file) {
			var reader = new BinaryReader(File.OpenRead(file));

			if (reader.ReadUInt32() != 0x42444F4D)
				throw new Exception("Invalid MODB file");

			vertices = reader.ReadInt32();
			flags    = (Flags) reader.ReadInt32();
			ambient  = Color.fromUInt(reader.ReadUInt32()).toVector();
			specular = Color.fromUInt(reader.ReadUInt32()).toVector();

			size = 12 + ((flags & Flags.Normals) != 0 ? 12 : 0) + ((flags & Flags.TexCoords) != 0 ? 8 : 0) + ((flags & Flags.Colors) != 0 ? 4 : 0);
			vbo  = new VertexBuffer<byte>(reader.ReadBytes(vertices * size));
		}

		// Prepare VAO for shader

		public void prepareVAO(Shader shader, uint vao) {
			Gl.BindVertexArray(vao);

			vbo.bind();

			int offset = 0;

			int loc;
			
			if ((loc = shader.getLocation(Shader.Input.Position)) >= 0) {
				Gl.EnableVertexAttribArray(loc);
				Gl.VertexAttribPointer(loc, 3, VertexAttribPointerType.Float, false, size, (IntPtr) 0);
				offset += 12;
            }

			if ((flags & Flags.Normals) != 0 && (loc = shader.getLocation(Shader.Input.Normal)) >= 0) {
				Gl.EnableVertexAttribArray(loc);
				Gl.VertexAttribPointer(loc, 3, VertexAttribPointerType.Float, false, size, (IntPtr) offset);
				offset += 12;
			}

			if ((flags & Flags.TexCoords) != 0 && (loc = shader.getLocation(Shader.Input.TexCoord)) >= 0) {
				Gl.EnableVertexAttribArray(loc);
				Gl.VertexAttribPointer(loc, 2, VertexAttribPointerType.Float, false, size, (IntPtr) offset);
				offset += 8;
			}

			if ((flags &  Flags.Colors) != 0 && (loc = shader.getLocation(Shader.Input.Color)) >= 0) {
				Gl.EnableVertexAttribArray(loc);
				Gl.VertexAttribPointer(loc, 4, VertexAttribPointerType.UnsignedByte, true, size, (IntPtr) offset);
				offset += 4;
			}

			Gl.BindVertexArray(0);
		}

		void Dispose(bool disposing) {
			if (!disposed) {
				disposed = true;

				if (disposing)
					vbo.Dispose();
			}
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		bool disposed;

		Flags flags;
		int size;

		public Vector4 ambient { get; private set; }
		public Vector4 specular { get; private set; }
		
		public int vertices { get; private set; }
		public VertexBuffer<byte> vbo { get; private set; }
	}
}
