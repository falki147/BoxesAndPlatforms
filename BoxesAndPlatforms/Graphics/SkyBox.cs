using OpenGL;
using System;

namespace BoxesAndPlatforms {
	public class SkyBox: IDisposable {
		// Handles rendering of a skybox

		public SkyBox() {
			vbo = new VertexBuffer<float>(new float[] {
				-1.0f,  1.0f, -1.0f,
				-1.0f, -1.0f, -1.0f,
				 1.0f, -1.0f, -1.0f,
				 1.0f, -1.0f, -1.0f,
				 1.0f,  1.0f, -1.0f,
				-1.0f,  1.0f, -1.0f,

				-1.0f, -1.0f,  1.0f,
				-1.0f, -1.0f, -1.0f,
				-1.0f,  1.0f, -1.0f,
				-1.0f,  1.0f, -1.0f,
				-1.0f,  1.0f,  1.0f,
				-1.0f, -1.0f,  1.0f,

				 1.0f, -1.0f, -1.0f,
				 1.0f, -1.0f,  1.0f,
				 1.0f,  1.0f,  1.0f,
				 1.0f,  1.0f,  1.0f,
				 1.0f,  1.0f, -1.0f,
				 1.0f, -1.0f, -1.0f,

				-1.0f, -1.0f,  1.0f,
				-1.0f,  1.0f,  1.0f,
				 1.0f,  1.0f,  1.0f,
				 1.0f,  1.0f,  1.0f,
				 1.0f, -1.0f,  1.0f,
				-1.0f, -1.0f,  1.0f,

				-1.0f,  1.0f, -1.0f,
				 1.0f,  1.0f, -1.0f,
				 1.0f,  1.0f,  1.0f,
				 1.0f,  1.0f,  1.0f,
				-1.0f,  1.0f,  1.0f,
				-1.0f,  1.0f, -1.0f,

				-1.0f, -1.0f, -1.0f,
				-1.0f, -1.0f,  1.0f,
				 1.0f, -1.0f, -1.0f,
				 1.0f, -1.0f, -1.0f,
				-1.0f, -1.0f,  1.0f,
				 1.0f, -1.0f,  1.0f
			});
		}

		public SkyBox(CubeMap texture, Shader shd): this() {
			this.texture = texture;
			setProgram(shd);
		}

		~SkyBox() {
			if (!disposed)
				System.Diagnostics.Debug.Fail("SkyBox was not disposed of properly.");

			Dispose(false);
		}

		// Prepare VAO for drawing
		
		public void setProgram(Shader shd) {
			if (vao == 0)
				vao = Gl.GenVertexArray();

			Gl.BindVertexArray(vao);

			vbo.bind();

			int loc;

			if ((loc = shd.getLocation(Shader.Input.Position)) >= 0) {
				Gl.EnableVertexAttribArray(loc);
				Gl.VertexAttribPointer(loc, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), (IntPtr) 0);
			}
			
			Gl.BindVertexArray(0);

			this.shd = shd;
			samplerLoc = shd.getLocation(Shader.Input.Texture);
			mvpLoc = shd.getLocation(Shader.Input.MVPMat);
        }

		// Render cube with sky texture and specified size

		public void render(Matrix4 proj, Matrix4 view, int size = 1) {
			// Set translation to zero
			view[3] = Vector4.UnitW;

			shd.use();

			Gl.UniformMatrix4fv(mvpLoc, Matrix4.CreateScaling(new Vector3(size, size, size)) * view * proj);

			Gl.ActiveTexture(TextureUnit.Texture0 + samplerLoc);
			texture.bind();

			Gl.BindVertexArray(vao);
			Gl.DrawArrays(BeginMode.Triangles, 0, 36);
			Gl.BindVertexArray(0);
		}

		void Dispose(bool disposing) {
			if (!disposed) {
				disposed = true;

				if (disposing) {
					if (vbo != null) {
						vbo.Dispose();
						vbo = null;
					}

					if (vao != 0) {
						Gl.DeleteVertexArrays(1, new uint[] { vao });
						vao = 0;
					}
				}
			}
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		bool disposed;

		VertexBuffer<float> vbo;
		uint vao;

		int samplerLoc;
		int mvpLoc;

		public Shader  shd { get; private set; }
		public CubeMap texture;
	}
}
