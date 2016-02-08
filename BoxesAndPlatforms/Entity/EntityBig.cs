using OpenGL;

namespace BoxesAndPlatforms {
	public class EntityBig: Entity {
		public EntityBig(World world, WorldLayout.Entry entry): base(entry) {
			shd = AssetLoader.getShader("entity\\big\\big.shd");
			msh = AssetLoader.getMesh("entity\\shared\\platform.modb");

			msh.prepareVAO(shd, vao = Gl.GenVertexArray());

			flags |= Flags.Collidable;

			size     = new Vector3(entry.size.X, entry.size.Y, entry.pos.Z - minZ + 1);
			offset   = new Vector3(entry.size.X / 2, entry.size.Y / 2, (minZ - entry.pos.Z - 1) / 2 + 1);
            position = entry.pos + offset;
			physics  = world.physics.addBox(position, size, 0, 1, Physics.Filter.Static, Physics.Filter.All & ~Physics.Filter.Static);

			mat = Matrix4.CreateScaling(new Vector3(entry.size.X, entry.size.Y, entry.pos.Z - minZ + 1)) * Matrix4.CreateTranslation(position);
        }

		public override void free() {
			Gl.DeleteVertexArrays(1, new uint[] { vao });
		}

		public override void render(World world) {
			world.scene.setShader(shd);
			world.scene.setModelMatrix(mat);
			world.scene.renderMesh(msh, vao);
		}
		
		public override void update(World world) { }

		public override void renderPrepare(World world) { }
		
		const float minZ = -4;

		Matrix4 mat;

		Shader shd;
		Mesh   msh;
		uint   vao;
	}
}
