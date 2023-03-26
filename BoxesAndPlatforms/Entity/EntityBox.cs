using OpenGL;
using System.Numerics;

namespace BoxesAndPlatforms {
	public class EntityBox: Entity {
		public EntityBox(World world, WorldLayout.Entry entry): base(entry) {
			shd = AssetLoader.getShader("entity\\shared\\colored.shd");
			msh = AssetLoader.getMesh("entity\\box\\box.modb");

			vao = Gl.GenVertexArray();

			msh.prepareVAO(shd, vao);

			flags |= Flags.Collidable | Flags.Movable;

			size     = new Vector3(1, 1, 1);
			position = entry.pos + new Vector3(0.5f, 0.5f, 0.5f);
            physics  = world.physics.addBox(position, size, 0.1f, 1, Physics.Filter.Default, Physics.Filter.All);
		}

		public override void free() {
			Gl.DeleteVertexArrays(1, new uint[] { vao });
		}

		public override void render(World world) {
			world.scene.setShader(shd);
			world.scene.setModelMatrix(Matrix4.CreateTranslation(position));
			world.scene.renderMesh(msh, vao);
		}
		
		public override void update(World world) { }

		public override void renderPrepare(World world) { }
		
		Shader shd;
		Mesh   msh;
		uint   vao;
	}
}
