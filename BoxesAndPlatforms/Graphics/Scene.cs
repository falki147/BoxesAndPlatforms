using OpenGL;
using System.Numerics;

namespace BoxesAndPlatforms {
	// Handle rendering of the world

	public class Scene {
		public Scene(int width, int height) {
			matrixP = Matrix.createPerspectiveLH(1.3f, (float) width / height, 1, 400);
		}

		public void updateCamera(Vector3 from, Vector3 to, Vector3 up) {
			matrixVP = (matrixV = Matrix.createLookAtLH(cameraPos = from, to, up)) * matrixP;
		}

		// Has to be called after each render step

		public void end() {
			curShader = null;
		}

		public void setShader(Shader shader) {
			if (!ReferenceEquals(curShader, shader)) {
				(curShader = shader).use();

				curShader.setVector3(Shader.Input.CameraPos, cameraPos);
				curShader.setVector3(Shader.Input.LightDir, lightDir);
			}
		}

		public void setTexture(Texture tex) {
			curShader.setTexture(Shader.Input.Texture, tex);
		}

		public void setTexture(CubeMap tex) {
			curShader.setTexture(Shader.Input.CubeMap, tex);
		}
		
		public void renderMesh(Mesh msh, uint vao) {
			var ambient = msh.ambient + lightAmbient;

			curShader.setVector3(Shader.Input.LightAmbient, new Vector3(ambient.X, ambient.Y, ambient.Z));
			curShader.setVector3(Shader.Input.LightSpecular, new Vector3(msh.specular.X, msh.specular.Y, msh.specular.Z));

			Gl.BindVertexArray(vao);
			Gl.DrawArrays(BeginMode.Triangles, 0, msh.vertices);
			Gl.BindVertexArray(0);
		}

		public void setModelMatrix(Matrix4 matrix) {
			curShader.setMatrix3(Shader.Input.NormMat, Matrix.normalMatrix(matrix));
			curShader.setMatrix4(Shader.Input.ModelMat, matrix);
			curShader.setMatrix4(Shader.Input.MVPMat, matrix * matrixVP);
		}
		
		Shader curShader;

		public Vector3 lightDir = new Vector3(-0.959f, 0, 0.283f);
		public Vector4 lightAmbient = new Vector4(0.25f, 0.15f, 0.1f, 0);
		public Vector3 cameraPos { get; private set; }
		
		public Matrix4 matrixP { get; private set; }
		public Matrix4 matrixV { get; private set; }
		public Matrix4 matrixVP { get; private set; }
	}
}
