using System;
using System.IO;
using System.Numerics;
using OpenGL;

namespace BoxesAndPlatforms {
	static public class Utils {
		// Get the next power of the specified number

		static public int nextPowerOf2(int x) {
			--x;

			x |= x >> 1;
			x |= x >> 2;
			x |= x >> 4;
			x |= x >> 8;
			x |= x >> 16;
			
			return ++x;
		}

		public static void swap<T>(ref T a, ref T b) {
			T t = a;
			a = b;
			b = t;
		}

		// Get normalized path (absolute path)

		public static string normalizePath(string path) {
			return Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).ToUpperInvariant();
		}

		public static T min<T>(T x, T y) where T : IComparable {
			return x.CompareTo(y) < 0 ? x : y;
		}

		public static T max<T>(T x, T y) where T : IComparable {
			return x.CompareTo(y) > 0 ? x : y;
		}

		// Clamp vlaue to a minimum and maximum

		public static T clamp<T>(T val, T min, T max) where T : IComparable {
			return Utils.min(Utils.max(val, min), max);
		}

		// Check if a position is inside a 2D AABB

		public static bool isInside(Vector2 pos, Vector2 min, Vector2 size) {
			return min.X < pos.X && pos.X < (min.X + size.X) && min.Y < pos.Y && pos.Y < (min.Y + size.Y);
        }

		// Linear interpolation of two values

		public static float lerp(float v1, float v2, float perc) {
			return v1 + (v2 - v1) * perc;
		}

		// Linear interpolation of two angles

		public static float lerpAngle(float v1, float v2, float perc) {
			return v1 + angleDifference(v2, v1) * perc;
		}

		// Clamp angle from 0 to 2 * PI

		public static float angleClamp(float val) {
			return (val % (2 * MathF.PI) + 2 * MathF.PI) % (2 * MathF.PI);
		}

		// Clamp angle from -PI to PI

		public static float angleClamp2(float val) {
			return (val % (2 * MathF.PI) + 3 * MathF.PI) % (2 * MathF.PI) - MathF.PI;
        }

		public static float angleDifference(float angle1, float angle2) {
			return angleClamp2(angle1 - angle2);
		}
	}
}
