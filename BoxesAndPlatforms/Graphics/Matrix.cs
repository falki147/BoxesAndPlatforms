using OpenGL;
using System.Numerics;

namespace BoxesAndPlatforms {
	// Create matrices for rendering

	public static class Matrix {
		public static Matrix4 createPerspectiveLH(float fov, float aspect, float znear, float zfar) {
			var yscale = 1 / MathF.Tan(fov / 2);
			var xscale = yscale / aspect;

			return new Matrix4(
				new Vector4(xscale, 0, 0, 0),
				new Vector4(0, yscale, 0, 0),
				new Vector4(0, 0, zfar / (zfar - znear), 1),
				new Vector4(0, 0, -znear * zfar / (zfar - znear), 0)
			);
		}

		public static Matrix4 createLookAtLH(Vector3 from, Vector3 to, Vector3 up) {
			var zaxis = (to - from).Normalize();
			var xaxis = Vector3.Cross(up, zaxis).Normalize();
			var yaxis = Vector3.Cross(zaxis, xaxis);
			
			return new Matrix4(
				new Vector4(xaxis.X, yaxis.X, zaxis.X, 0),
				new Vector4(xaxis.Y, yaxis.Y, zaxis.Y, 0),
				new Vector4(xaxis.Z, yaxis.Z, zaxis.Z, 0),
				new Vector4(-xaxis.Dot(from), -yaxis.Dot(from), -zaxis.Dot(from), 1)
			);
		}

		public static Matrix3 normalMatrix(Matrix4 mat) {
			var mat3 = new Matrix3(
				new Vector3(mat[0].X, mat[0].Y, mat[0].Z),
				new Vector3(mat[1].X, mat[1].Y, mat[1].Z),
				new Vector3(mat[2].X, mat[2].Y, mat[2].Z)
			);

			if (mat3 == Matrix3.Identity)
				return mat3;

			return mat3.Inverse();
		}
	}
}
