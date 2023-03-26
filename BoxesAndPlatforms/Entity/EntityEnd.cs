using OpenGL;
using System;
using System.Numerics;

namespace BoxesAndPlatforms {
	public class EntityEnd: Entity {
		public EntityEnd(World world, WorldLayout.Entry entry): base(entry) {
			shdColored = AssetLoader.getShader("entity\\shared\\colored.shd");

			mshEnd  = AssetLoader.getMesh("entity\\end\\end.modb");
			mshDrop = AssetLoader.getMesh("entity\\end\\drop.modb");

			mshEnd.prepareVAO(shdColored, vaoEnd = Gl.GenVertexArray());
			mshDrop.prepareVAO(shdColored, vaoDrop = Gl.GenVertexArray());

			size     = new Vector3(0.5f, 0.5f, 0.5f);
			offset   = new Vector3(0.5f, 0.5f, 0.5f);
			position = entry.pos + offset;
        }

		public override void free() {
			Gl.DeleteVertexArrays(1, new uint[] { vaoEnd, vaoDrop });
		}

		public override void render(World world) {
			world.scene.setShader(shdColored);

			world.scene.setModelMatrix(Matrix4.Rotate(rot) * Matrix4.CreateTranslation(position));
			world.scene.renderMesh(mshEnd, vaoEnd);

			world.scene.setModelMatrix(Matrix4.CreateRotationX(-1.57f) * Matrix4.CreateTranslation(new Vector3(0.5f, 0, 0)) * Matrix4.CreateRotationZ(angle) * Matrix4.CreateTranslation(position));
			world.scene.renderMesh(mshDrop, vaoDrop);

			world.scene.setModelMatrix(Matrix4.CreateRotationX(-1.57f) * Matrix4.CreateTranslation(new Vector3(0.5f, 0, 0)) * Matrix4.CreateRotationZ(angle + 1.5f) * Matrix4.CreateRotationY(0.785f) * Matrix4.CreateTranslation(position));
			world.scene.renderMesh(mshDrop, vaoDrop);

			world.scene.setModelMatrix(Matrix4.CreateRotationX(-1.57f) * Matrix4.CreateTranslation(new Vector3(0.5f, 0, 0)) * Matrix4.CreateRotationZ(angle - 2.5f) * Matrix4.CreateRotationY(-0.785f) * Matrix4.CreateTranslation(position));
			world.scene.renderMesh(mshDrop, vaoDrop);
		}
		
		public override void update(World world) {
            // Update rotation

            rot *= Quaternion.CreateFromAxisAngle(new Vector3(0.707f, 0.707f, 0), ((MathF.Cos(angleRot) + 1) / 4 + 1) * world.time);
            rot *= Quaternion.CreateFromAxisAngle(new Vector3(0, 0, -1), ((MathF.Sin(angleRot) + 1) / 4 + 1) * world.time);

            angle = (angle + 8 * world.time) % (2 * MathF.PI);
			angleRot = (angleRot + 1f * world.time) % (2 * MathF.PI);

			if (world.player.collisionBox.isIntersecting(collisionBox))
				world.state = World.State.Done;
		}

		public override void renderPrepare(World world) { }
		
		Shader shdColored;

		Mesh mshEnd;
		uint vaoEnd;
		Mesh mshDrop;
		uint vaoDrop;

		float angle;
		float angleRot;

		Quaternion rot = Quaternion.Identity;
	}
}
