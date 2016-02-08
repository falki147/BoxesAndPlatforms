using OpenGL;

namespace BoxesAndPlatforms {
	public class EntitySpikes: Entity {
		public EntitySpikes(World world, WorldLayout.Entry entry): base(entry) {
			shd = AssetLoader.getShader("entity\\shared\\colored.shd");
			msh = AssetLoader.getMesh("entity\\shared\\spikes.modb");

			vao = Gl.GenVertexArray();

			msh.prepareVAO(shd, vao);
			
			size     = new Vector3(0.84f, 0.84f, 0.3f);
			offset   = new Vector3(0.5f, 0.5f, 0.15f);
			position = entry.pos + offset;
			
			mat = Matrix4.CreateTranslation(position);
        }

		public override void free() {
			Gl.DeleteVertexArrays(1, new uint[] { vao });
		}

		public override void render(World world) {
			world.scene.setShader(shd);
			world.scene.setModelMatrix(mat);
			world.scene.renderMesh(msh, vao);
		}
		
		public override void update(World world) {
			if (world.player.collisionBox.isIntersecting(collisionBox))
				world.state = World.State.Restart;
		}

		public override void renderPrepare(World world) { }

		Matrix4 mat;

		Shader shd;
		Mesh   msh;
		uint   vao;
	}
}
