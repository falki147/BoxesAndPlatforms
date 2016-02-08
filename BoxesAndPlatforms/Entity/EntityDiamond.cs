using OpenGL;
using System;

namespace BoxesAndPlatforms {
	public class EntityDiamond: Entity {
		public EntityDiamond(World world, WorldLayout.Entry entry): base(entry) {
			shd = AssetLoader.getShader("entity\\diamond\\diamond.shd");
			msh = AssetLoader.getMesh("entity\\diamond\\diamond.modb");

			msh.prepareVAO(shd, vao = Gl.GenVertexArray());
			
			size     = new Vector3(0.4f, 0.4f, 0.5f);
			offset   = new Vector3(0.5f, 0.5f, 0.5f);
			position = entry.pos + offset;

			positionTime = 2 * MathF.PI * (float) world.random.NextDouble();
        }

		public override void free() {
			Gl.DeleteVertexArrays(1, new uint[] { vao });
		}

		public override void render(World world) {
			world.scene.setShader(shd);
			world.scene.setModelMatrix(Matrix4.CreateRotationZ(rotation) * Matrix4.CreateTranslation(position + new Vector3(0, 0, MathF.Cos(positionTime) * 0.05f)));
			world.scene.setTexture(world.skybox.texture);
			world.scene.renderMesh(msh, vao);
		}
		
		public override void update(World world) {
			// Update rotation and position of diamond

			rotation = (rotation + 0.3f * world.time) % (2 * MathF.PI);
			positionTime = (positionTime + 1.2f * world.time) % (2 * MathF.PI);

			// Attract diamond to player

			var dif = position - world.player.position;
			var len = dif.Length;

			if (len < attractRadius) {
				var vel = 0.5f * dif / len * (attractRadius - len);

				position -= vel;

				if (vel.LengthSquared() > dif.LengthSquared()) {
					world.score += 10;
					remove = true;
				}
			}
		}

		public override void renderPrepare(World world) { }
		
		const float attractRadius = 1.5f;
		
		Shader shd;
		Mesh   msh;
		uint   vao;

		float rotation;
		float positionTime;
	}
}
