using OpenGL;
using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace BoxesAndPlatforms {
	// Store multiple vertices into one buffer and render it once

	public class Batch: IDisposable {
		public enum HorizontalAlign {
			Left,
			Center,
			Right
		}

		public enum VerticalAlign {
			Top,
			Middle,
			Bottom
		}

        [StructLayout(LayoutKind.Sequential)]
        struct Vertex {
			public Vertex(Vector2 pos, Vector2 tex, Color col) {
				posX = pos.X;
                posY = pos.Y;
                texX = tex.X;
                texY = tex.Y;
                this.col = col.toUInt();
			}

			public float posX;
            public float posY;
            public float texX;
            public float texY;
            public UInt32 col;
		}

		public Batch(int size = 1024) {
			vb = new VertexBuffer<Vertex>(size, BufferUsageHint.StreamDraw);
		}

		~Batch() {
			if (!disposed)
				System.Diagnostics.Debug.Fail("Batch was not disposed of properly.");

			Dispose(false);
		}

		// Begin with rendering

		public void begin() {
			if (rendered * 6 > vb.size)
				vb.resize(Utils.nextPowerOf2(rendered * 6), BufferUsageHint.StreamDraw);

			index    = 0;
			rendered = 0;

			shader.use();

			Gl.BindVertexArray(vao);

			vb.bind();
			vb.map();

			if (samplerLoc >= 0) {
				Gl.ActiveTexture(TextureUnit.Texture0 + samplerLoc);
				Gl.BindTexture(texture.TextureTarget, texture.TextureID);
			}
		}

		// Render an image

		public void drawImage(Atlas.Image img, Vector2 pos) {
			drawImage(img, pos, new Color(0xFF));
		}

		public void drawImage(Atlas.Image img, Vector2 pos, Color col) {
			drawRect(pos + img.offset, pos + img.offset + img.size, img.p1, img.p2, img.p3, img.p4, col);
		}

		public void drawImage(Atlas.Image img, Vector2 pos, Vector2 size) {
			drawImage(img, pos, size, new Color(0xFF));
		}

		public void drawImage(Atlas.Image img, Vector2 pos, Vector2 size, Color col) {
			drawRect(pos + img.offset * size, pos + (img.offset + img.size) * size, img.p1, img.p2, img.p3, img.p4, col);
		}

		// Render text

		public void drawText(Atlas.Font font, Vector2 pos, string text) {
			drawText(font, pos, text, new Color(0));
		}

		public void drawText(Atlas.Font font, Vector2 pos, string text, Color col) {
			Vector2 p = pos;

			foreach (var c in text) {
				Atlas.Glyph glyph;

				if (c == '\n') {
					p.X = pos.X;
					p.Y += font.height;
				}
				else if (font.glyphs.TryGetValue(c, out glyph)) {
					if (!glyph.empty)
						drawRect(p + glyph.offset, p + glyph.offset + glyph.size, glyph.p1, glyph.p2, glyph.p3, glyph.p4, col);

					p += glyph.advance;
				}
			}
		}

		public void drawText(Atlas.Font font, Vector2 pos, string text, HorizontalAlign halign, VerticalAlign valign) {
			drawText(font, pos, text, new Color(0), halign, valign);
		}

		public void drawText(Atlas.Font font, Vector2 pos, string text, Color col, HorizontalAlign halign, VerticalAlign valign) {
			var split = text.Split('\n');

			if (split.Length == 0)
				return;

			switch (valign) {
			case VerticalAlign.Middle:
				pos.Y -= split.Length * font.height / 2;
				break;
			case VerticalAlign.Bottom:
				pos.Y -= split.Length * font.height;
				break;
			}

			Vector2 p = pos;

			foreach (var line in split) {
				p.X = pos.X;

				if (halign != HorizontalAlign.Left) {
					var len = 0.0f;

					foreach (var c in line) {
						Atlas.Glyph glyph;
						
						if (font.glyphs.TryGetValue(c, out glyph))
							len += glyph.advance.X;
					}

					switch (halign) {
					case HorizontalAlign.Center:
						p.X -= len / 2;
						break;
					case HorizontalAlign.Right:
						p.X -= len;
						break;
					}
				}

				foreach (var c in line) {
					Atlas.Glyph glyph;

					if (font.glyphs.TryGetValue(c, out glyph)) {
						if (!glyph.empty)
							drawRect(p + glyph.offset, p + glyph.offset + glyph.size, glyph.p1, glyph.p2, glyph.p3, glyph.p4, col);

						p += glyph.advance;
					}
				}

				p.Y += font.height;
			}
		}
		
		void drawRect(Vector2 pos1, Vector2 pos2, Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, Color col) {
			if (index + 6 >= vb.size) {
				vb.unmap();
				Gl.DrawArrays(BeginMode.Triangles, 0, index);
				vb.map();
				index = 0;
			}

			vb[index++] = new Vertex(
				pos1,
				p1,
				col
			);

			vb[index++] = new Vertex(
				new Vector2(pos2.X, pos1.Y),
				p2,
				col
			);

			vb[index++] = new Vertex(
				new Vector2(pos1.X, pos2.Y),
				p3,
				col
			);

			vb[index++] = new Vertex(
				new Vector2(pos1.X, pos2.Y),
				p3,
				col
			);

			vb[index++] = new Vertex(
				new Vector2(pos2.X, pos1.Y),
				p2,
				col
			);

			vb[index++] = new Vertex(
				pos2,
				p4,
				col
			);

			++rendered;
		}

		// End rendering

		public void end() {
			vb.unmap();
			Gl.DrawArrays(BeginMode.Triangles, 0, index);
			Gl.BindVertexArray(0);
		}

		// Prepare VAO for the given shader
		
		public void setProgram(Shader shader) {
			if (vao == 0)
				vao = Gl.GenVertexArray();

			Gl.BindVertexArray(vao);

			vb.bind();

			int loc;

			if ((loc = shader.getLocation(Shader.Input.Position)) >= 0) {
				Gl.EnableVertexAttribArray(loc);
				Gl.VertexAttribPointer(loc, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf<Vertex>(), (IntPtr) 0);
			}

			if ((loc = shader.getLocation(Shader.Input.TexCoord)) >= 0) {
				Gl.EnableVertexAttribArray(loc);
				Gl.VertexAttribPointer(loc, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf<Vertex>(), (IntPtr) 8);
			}

			if ((loc = shader.getLocation(Shader.Input.Color)) >= 0) {
				Gl.EnableVertexAttribArray(loc);
				Gl.VertexAttribPointer(loc, 4, VertexAttribPointerType.UnsignedByte, true, Marshal.SizeOf<Vertex>(), (IntPtr) 16);
			}
			
			Gl.BindVertexArray(0);

			this.shader = shader;
			samplerLoc = shader.getLocation(Shader.Input.Texture);
		}

		void Dispose(bool disposing) {
			if (!disposed) {
				disposed = true;

				if (disposing) {
					vb.Dispose();

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
		
		bool disposed = false;

		int index;
		int samplerLoc;

		uint vao;
		VertexBuffer<Vertex> vb;

		public int rendered { get; private set; }
		
		public Texture texture;

		public Shader shader { get; private set; }
	}
}
