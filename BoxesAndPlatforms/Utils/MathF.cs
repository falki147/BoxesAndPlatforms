using System;

namespace BoxesAndPlatforms {
	// Float equivalent of Math class from System

	public static class MathF {
		static public float Atan2(float y, float x) {
			return (float) Math.Atan2(y, x);
		}

		static public float Cos(float a) {
			return (float) Math.Cos(a);
		}

		static public float Pow(float x, float y) {
			return (float) Math.Pow(x, y);
		}

		static public float Sin(float a) {
			return (float) Math.Sin(a);
		}

		static public float Sqrt(float d) {
			return (float) Math.Sqrt(d);
		}

		static public float Asin(float d) {
			return (float) Math.Asin(d);
		}

		static public float Tan(float a) {
			return (float) Math.Tan((float) a);
		}

		public const float PI = (float) Math.PI;
	}
}
