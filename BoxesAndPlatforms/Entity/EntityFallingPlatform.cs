using OpenGL;
using System.Numerics;

namespace BoxesAndPlatforms {
	public class EntityFallingPlatform: Entity {
		public EntityFallingPlatform(World world, WorldLayout.Entry entry): base(entry) {
			shd = AssetLoader.getShader("entity\\shared\\colored.shd");
			msh = AssetLoader.getMesh("entity\\fallingplatform\\fallingplatform.modb");

			vao = Gl.GenVertexArray();

			msh.prepareVAO(shd, vao);

			flags |= Flags.Collidable;

			size     = new Vector3(1, 1, 1);
			offset   = new Vector3(0.5f, 0.5f, 0.5f);
			position = entry.pos + offset;
			physics  = world.physics.addBox(position, size, 0, 1, Physics.Filter.Static, Physics.Filter.All & ~Physics.Filter.Static);
		}

		public override void free() {
			Gl.DeleteVertexArrays(1, new uint[] { vao });
		}

		public override void render( World world) {
			world.scene.setShader(shd);
			world.scene.setModelMatrix(Matrix4.CreateTranslation(position));
			world.scene.renderMesh(msh, vao);
		}
		
		public override void update(World world) {
			if (time < 0) {
				if (ReferenceEquals(world.player.standingOn, this))
					time = 0;
			}
			else {
				// Wait a few seconds then fall

				time += world.time;

				if (time > 0.5f && physics.mass == 0) {
					physics.mass = 0.1f;
					physics.setFilter(Physics.Filter.Default, Physics.Filter.All);
				}
			}
		}

		public override void renderPrepare(World world) { }
		
		float time = -1;

		Shader shd;
		Mesh   msh;
		uint   vao;
	}
}
