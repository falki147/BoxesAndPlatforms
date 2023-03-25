using OpenGL;
using System;

namespace BoxesAndPlatforms {
	public class EntityFalling: Entity {
		public EntityFalling(World world, WorldLayout.Entry entry): base(entry) {
			shd = AssetLoader.getShader("entity\\shared\\textured.shd");
			msh = AssetLoader.getMesh("entity\\falling\\falling.modb");
			tex = AssetLoader.getTexture("entity\\falling\\falling.dds");

			msh.prepareVAO(shd, vao = Gl.GenVertexArray());

			flags |= Flags.Collidable | Flags.Movable;

			size     = new Vector3(1, 1, 1);
			offset   = new Vector3(0.5f, 0.5f, 0.5f);
			position = entry.pos + offset;
			physics  = world.physics.addBox(position, size, 0, 1, Physics.Filter.Static, Physics.Filter.All & ~Physics.Filter.Static);

			// Set texture filters

			Gl.BindTexture(tex);
			Gl.TexParameteri(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, TextureParameter.Linear);
			Gl.TexParameteri(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, TextureParameter.LinearMipMapLinear);

			if (Graphics.hasAnisotropicFiltering)
				Graphics.setAnisotropicFiltering(TextureTarget.Texture2D, Math.Min(8f, Graphics.maxAnisotropicFiltering));
		}

		public override void free() {
			Gl.DeleteVertexArrays(1, new uint[] { vao });
		}

		public override void render(World world) {
			world.scene.setShader(shd);
			world.scene.setModelMatrix(Matrix4.CreateRotationZ(direction) * Matrix4.CreateTranslation(position));
			world.scene.setTexture(tex);
			world.scene.renderMesh(msh, vao);
		}
		
		public override void update(World world) {
			// Set angle of entity

			var len = new Vector2(world.player.position.X - position.X, world.player.position.Y - position.Y);

			if (len.LengthSquared() > deactivationRadius * deactivationRadius) {
				var angle = (MathF.Atan2(world.player.position.Y - position.Y, world.player.position.X - position.X) / (2 * MathF.PI) + 1) % 1;

				if (angle < 0.125f || angle > 0.875f)
					destDirection = 0;
				else if (angle < 0.375f)
					destDirection = MathF.PI / 2;
				else if (angle < 0.625f)
					destDirection = MathF.PI;
				else
					destDirection = MathF.PI * 3 / 2;
			}
			
			direction = Ease.expAngle(direction, destDirection, 1e-8f, world.time);

			// Check if player is beneath and if he is fall down
			// When done go up again

			var colPlayer = world.player.collisionBox;
			
			if (!movingUp) {
				if (physics.mass == 0) {
					if (collisionBox.min.Z > colPlayer.max.Z && collisionBox.distanceX(colPlayer) < 0 && collisionBox.distanceY(colPlayer) < 0) {
						physics.mass = 10;
						physics.setFilter(Physics.Filter.Default, Physics.Filter.All);
					}
				}
				else if (standingOn != null) {
					if (ReferenceEquals(standingOn, world.player))
						world.state = World.State.Restart;
					else {
						physics.mass = 0;
						physics.setFilter(Physics.Filter.Static, Physics.Filter.All & ~Physics.Filter.Static);
						movingUp = true;
					}
				}
			}
			else {
				var dif  = (entry.pos.Z + offset.Z) - physics.translation.Z;
				var dist = 2 * world.time;

				if (dif > dist)
					physics.translate(new Vector3(0, 0, dist));
				else {
					physics.translate(new Vector3(0, 0, dif));
					movingUp = false;
				}
			}
		}

		public override void renderPrepare(World world) { }
		
		const float deactivationRadius = 1.5f;

		bool movingUp = false;

		float direction;
		float destDirection;

		Shader  shd;
		Mesh    msh;
		Texture tex;
		uint    vao;
	}
}
