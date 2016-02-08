using OpenGL;

namespace BoxesAndPlatforms {
	public static class Graphics {
		public static void setAnisotropicFiltering(TextureTarget target, float value) {
			Gl.TexParameterf(target, TextureParameterName.MaxAnisotropyExt, value);
		}

		public static bool hasAnisotropicFiltering {
			get {
				return Gl.Extensions.TextureFilterAnisotropic_EXT;
			}
		}
		
		public static float maxAnisotropicFiltering {
			get {
				return Gl.GetFloat(GetPName.MaxTextureMaxAnisotropyExt);
			}
		}
    }
}
