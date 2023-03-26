using OpenGL;
using System.Numerics;

namespace BoxesAndPlatforms {
	public class EntityMovingSpikes: Entity {
		public EntityMovingSpikes(World world, WorldLayout.Entry entry): base(entry) {
			shd = AssetLoader.getShader("entity\\shared\\colored.shd");
			msh = AssetLoader.getMesh("entity\\shared\\spikes.modb");

			vao = Gl.GenVertexArray();

			msh.prepareVAO(shd, vao);
			
			size     = new Vector3(0.84f, 0.84f, 0.3f);
			offset   = new Vector3(0.5f, 0.5f, -0.3f);
			position = entry.pos + offset;
		}

		public override void free() {
			Gl.DeleteVertexArrays(1, new uint[] { vao });
		}

		public override void render(World world) {
			world.scene.setShader(shd);
			world.scene.setModelMatrix(Matrix4.CreateTranslation(position));
			world.scene.renderMesh(msh, vao);
		}
		
		public override void update(World world) {
			// Animate spikes

			time += world.time;

			if (time < 0.15f)
				translate(new Vector3(0, 0, (entry.pos.Z + offset.Z + 0.45f * time / 0.15f) - position.Z));
			else if (time < 2f)
				translate(new Vector3(0, 0, (entry.pos.Z + offset.Z + 0.45f * (1 - (time - 0.15f) / 1.85f)) - position.Z));
			else if (time > 2.75f)
				time -= 2.75f;

			if (world.player.collisionBox.isIntersecting(collisionBox))
				world.state = World.State.Restart;
		}

		public override void renderPrepare(World world) { }
		
		float time;
		
		Shader shd;
		Mesh   msh;
		uint   vao;
	}
}
