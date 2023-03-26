using OpenGL;
using System.Numerics;

namespace BoxesAndPlatforms {
	public class EntitySmall: Entity {
		public EntitySmall(World world, WorldLayout.Entry entry): base(entry) {
			shd = AssetLoader.getShader("entity\\shared\\colored.shd");
			msh = AssetLoader.getMesh("entity\\shared\\platform.modb");

			vao = Gl.GenVertexArray();

			msh.prepareVAO(shd, vao);

			flags |= Flags.Collidable;

			size     = new Vector3(entry.size.X, entry.size.Y, 1);
			offset   = new Vector3(entry.size.X / 2, entry.size.Y / 2, 0.5f);
			position = entry.pos + offset;
            physics  = world.physics.addBox(position, size, 0, 1, Physics.Filter.Static, Physics.Filter.All & ~Physics.Filter.Static);
        }

		public override void free() {
			Gl.DeleteVertexArrays(1, new uint[] { vao });
		}

		public override void render(World world) {
			world.scene.setShader(shd);
			world.scene.setModelMatrix(Matrix4.CreateScaling(new Vector3(entry.size.X, entry.size.Y, 1)) * Matrix4.CreateTranslation(position));
			world.scene.renderMesh(msh, vao);
		}
		
		public override void update(World world) { }

		public override void renderPrepare(World world) { }
		
		Shader shd;
		Mesh   msh;
		uint   vao;
	}
}
