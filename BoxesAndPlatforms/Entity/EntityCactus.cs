using OpenGL;

namespace BoxesAndPlatforms {
	public class EntityCactus: Entity {
		public EntityCactus(World world, WorldLayout.Entry entry): base(entry) {
			shd = AssetLoader.getShader("entity\\shared\\colored.shd");
			msh = AssetLoader.getMesh("entity\\cactus\\cactus.modb");

			msh.prepareVAO(shd, vao = Gl.GenVertexArray());

			flags |= Flags.Collidable;

			size     = new Vector3(0.72f, 0.19f, 1.5f);
			offset   = new Vector3(0.5f, 0.5f, 0.5f);
			position = entry.pos + offset;
			physics  = world.physics.addBox(position, size, 0, 1, Physics.Filter.Static, Physics.Filter.All & ~Physics.Filter.Static);

			mat = Matrix4.CreateTranslation(position);
        }

		public override void free() {
			Gl.DeleteVertexArrays(1, new uint[] { vao });
		}

		public override void render( World world) {
			world.scene.setShader(shd);
			world.scene.setModelMatrix(mat);
			world.scene.renderMesh(msh, vao);
		}
		
		public override void update(World world) { }

		public override void renderPrepare(World world) { }
		
		Matrix4 mat;

		Shader shd;
		Mesh   msh;
		uint   vao;
	}
}
