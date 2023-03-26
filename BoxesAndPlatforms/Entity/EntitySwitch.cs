using OpenGL;
using System;
using System.Numerics;

namespace BoxesAndPlatforms {
	public class EntitySwitch: Entity {
		public EntitySwitch(World world, WorldLayout.Entry entry): base(entry) {
			shdColored = AssetLoader.getShader("entity\\shared\\colored.shd");

			mshBase   = AssetLoader.getMesh("entity\\switch\\base.modb");
			mshSwitch = AssetLoader.getMesh("entity\\switch\\switch.modb");
			
			mshBase.prepareVAO(shdColored, vaoBase = Gl.GenVertexArray());
			mshSwitch.prepareVAO(shdColored, vaoSwitch = Gl.GenVertexArray());
			
			size     = new Vector3(0.5f, 0.25f, 0.125f);
			offset   = new Vector3(0.5f, 0.5f, 0.0625f);
			position = entry.pos + offset;
        }

		public override void free() {
			Gl.DeleteVertexArrays(1, new uint[] { vaoBase, vaoSwitch });
		}

		public override void render( World world) {
			world.scene.setShader(shdColored);

			world.scene.setModelMatrix(Matrix4.CreateTranslation(position));
			world.scene.renderMesh(mshBase, vaoBase);

			world.scene.setModelMatrix(Matrix4.CreateRotationY(angle) *  Matrix4.CreateTranslation(position + new Vector3(0, 0, -0.0625f)));
			world.scene.renderMesh(mshSwitch, vaoSwitch);
		}
		
		public override void update(World world) {
			// Handle animation and activation

			if (!triggered) {
				if (world.player.collisionBox.isIntersecting(collisionBox)) {
					triggered = true;
					
					if (entry.link != "")
						foreach (var entity in world.findEntities(entry.link))
							if ((entity.flags & Flags.Active) == 0)
								entity.flags |= Flags.Active;
				}
			}
			else
				angle = Math.Max(angle - world.time, -0.5f);
		}

		public override void renderPrepare(World world) { }
		
		bool triggered;
		float angle = 0.5f;

		Shader shdColored;

		Mesh mshBase;
		uint vaoBase;
		Mesh mshSwitch;
		uint vaoSwitch;
	}
}
