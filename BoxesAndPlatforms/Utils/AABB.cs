using OpenGL;
using System;
using System.Numerics;

namespace BoxesAndPlatforms {
	// Axis-Aligned Bounding Box used for fast collison detection between entities

	public struct AABB {
		public AABB(Vector3 min, Vector3 max) {
			this.min = min;
			this.max = max;
		}

		public float distanceX(AABB other) {
			return Math.Max(min.X, other.min.X) - Math.Min(max.X, other.max.X);
		}

		public float distanceY(AABB other) {
			return Math.Max(min.Y, other.min.Y) - Math.Min(max.Y, other.max.Y);
		}

		public float distanceZ(AABB other) {
			return Math.Max(min.Z, other.min.Z) - Math.Min(max.Z, other.max.Z);
		}
		
		public bool isIntersecting(AABB other) {
			if (max.X < other.min.X) return false;
			if (max.Y < other.min.Y) return false;
			if (max.Z < other.min.Z) return false;
			if (min.X > other.max.X) return false;
			if (min.Y > other.max.Y) return false;
			if (min.Z > other.max.Z) return false;

			return true;
		}

		static public AABB fromCenter(Vector3 center, Vector3 size) {
			size /= 2;
			return new AABB(center - size, center + size);
		}
		
		public Vector3 min, max;
	}
}
