using OpenGL;
using System.Numerics;

namespace BoxesAndPlatforms {
	// Stores the color in an 8-Bit RGBA format

	public struct Color {
		public Color(byte r) : this(r, r, r) { }

		public Color(byte r, byte g, byte b): this(r, g, b, 0xFF) { }

		public Color(byte r, byte a) : this(r, r, r, a) { }

		public Color(byte r, byte g, byte b, byte a) {
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = a;
		}

		public Vector4 toVector() {
			return new Vector4(r, g, b, a) / 255;
		}
		
		public static Color fromUInt(uint color) {
			return new Color(
				(byte) (color & 0xFF),
				(byte) ((color >> 8) & 0xFF),
				(byte) ((color >> 16) & 0xFF),
				(byte) ((color >> 24) & 0xFF)
			);
		}

		public static Color fromUInt(uint color, byte alpha) {
			return new Color(
				(byte) (color & 0xFF),
				(byte) ((color >> 8) & 0xFF),
				(byte) ((color >> 16) & 0xFF),
				alpha
			);
		}

		public uint toUInt() {
			return ((uint) r) | (((uint)r) << 8) | (((uint)b) << 16) | (((uint)a) << 24);
		}

		public byte r, g, b, a;
	}
}
