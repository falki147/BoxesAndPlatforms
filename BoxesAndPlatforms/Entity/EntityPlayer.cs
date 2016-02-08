using OpenGL;
using SDL2;
using System;

namespace BoxesAndPlatforms {
	public class EntityPlayer: Entity {
		public EntityPlayer(World world, WorldLayout.Entry entry) : base(entry) {
			shdTextured = AssetLoader.getShader("entity\\shared\\textured.shd");
			shdColored = AssetLoader.getShader("entity\\shared\\colored.shd");
			
			mshPlayer = AssetLoader.getMesh("entity\\player\\player.modb");
			texPlayer = AssetLoader.getTexture("entity\\player\\player.dds");
			mshArm    = AssetLoader.getMesh("entity\\player\\arm.modb");
			mshLeg    = AssetLoader.getMesh("entity\\player\\leg.modb");

			mshPlayer.prepareVAO(shdTextured, vaoPlayer = Gl.GenVertexArray());
			mshArm.prepareVAO(shdColored, vaoArm = Gl.GenVertexArray());
			mshLeg.prepareVAO(shdColored, vaoLeg = Gl.GenVertexArray());

			flags |= Flags.Collidable | Flags.Movable;

			size     = new Vector3(0.6f, 0.6f, 0.72f);
			offset   = new Vector3(0.5f, 0.5f, 0.36f);
			position = entry.pos + offset;
			physics  = world.physics.addBox(position, size, 0.1f, 0.5f, Physics.Filter.Character, Physics.Filter.All);
			
			cameraShape = world.physics.addSphereShape(0.35f);

			// Set texture filters

			Gl.BindTexture(texPlayer);
			Gl.TexParameteri(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, TextureParameter.Linear);
			Gl.TexParameteri(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, TextureParameter.LinearMipMapLinear);

			if (Graphics.hasAnisotropicFiltering)
				Graphics.setAnisotropicFiltering(TextureTarget.Texture2D, Math.Min(8f, Graphics.maxAnisotropicFiltering));
		}

		public override void free() {
			Gl.DeleteVertexArrays(1, new uint[] { vaoPlayer, vaoArm, vaoLeg });
		}

		public override void render(World world) {
			if (renderPlayer) {
				// Render body

				world.scene.setShader(shdTextured);

				world.scene.setModelMatrix(Matrix4.CreateRotationZ(playerYaw) * Matrix4.CreateTranslation(position + new Vector3(0, 0, 0.11f)));
				world.scene.setTexture(texPlayer);
				world.scene.renderMesh(mshPlayer, vaoPlayer);

				// Render arms

				world.scene.setShader(shdColored);

				world.scene.setModelMatrix(Matrix4.CreateRotationX(0.025f * MathF.Sin(armTime)) * Matrix4.CreateTranslation(new Vector3(0, 0.31f, 0)) * Matrix4.CreateRotationX(0.1f) * Matrix4.CreateRotationY(armDist * MathF.Cos(armTime)) * Matrix4.CreateRotationZ(playerYaw) * Matrix4.CreateTranslation(position + new Vector3(0, 0, 0.14f)));
				world.scene.renderMesh(mshArm, vaoArm);

				world.scene.setModelMatrix(Matrix4.CreateRotationX(0.025f * MathF.Sin(armTime)) * Matrix4.CreateTranslation(new Vector3(0, -0.31f, 0)) * Matrix4.CreateRotationX(-0.1f) * Matrix4.CreateRotationY(-armDist * MathF.Cos(armTime)) * Matrix4.CreateRotationZ(playerYaw) * Matrix4.CreateTranslation(position + new Vector3(0, 0, 0.14f)));
				world.scene.renderMesh(mshArm, vaoArm);

				// Render legs

				world.scene.setModelMatrix(Matrix4.CreateTranslation(new Vector3(0, 0, -0.08f)) * Matrix4.CreateRotationY(legDist * MathF.Sin(legTime)) * Matrix4.CreateTranslation(new Vector3(0, 0.125f, -0.08f)) * Matrix4.CreateRotationZ(playerYaw) * Matrix4.CreateTranslation(position));
				world.scene.renderMesh(mshLeg, vaoLeg);

				world.scene.setModelMatrix(Matrix4.CreateTranslation(new Vector3(0, 0, -0.08f)) * Matrix4.CreateRotationY(-legDist * MathF.Sin(legTime)) * Matrix4.CreateTranslation(new Vector3(0, -0.125f, -0.08f)) * Matrix4.CreateRotationZ(playerYaw) * Matrix4.CreateTranslation(position));
				world.scene.renderMesh(mshLeg, vaoLeg);
			}
		}

		float aniso = 1;

		public override void update(World world) {
			if (Window.checkKeyPressed(SDL.SDL_Keycode.SDLK_c)) {
				aniso = aniso == 1 ? 8 : 1;

				Gl.BindTexture(texPlayer);
				
				if (Graphics.hasAnisotropicFiltering)
					Graphics.setAnisotropicFiltering(TextureTarget.Texture2D, Math.Min(aniso, Graphics.maxAnisotropicFiltering));
			}

			handleInput(world);
			limitVelocity();
			handleAnimations(world);

			if (position.Z < -2)
				world.state = World.State.Restart;
		}

		void limitVelocity() {
			var vel = new Vector2(physics.velocity.X, physics.velocity.Y);
			var len = vel.Length;

			if (len > maxVelocity) {
				vel *= maxVelocity / len;
				physics.velocity = new Vector3(vel.X, vel.Y, physics.velocity.Z);
			}
		}

		void handleInput(World world) {
			cameraYawDest -= Window.relMouseX * 0.01f;
			cameraPitchDest = Utils.clamp(cameraPitchDest + Window.relMouseY * 0.005f, -1, 1);

			cameraYaw = Ease.exp(cameraYaw, cameraYawDest, 1e-7f, world.time);
			cameraPitch = Ease.exp(cameraPitch, cameraPitchDest, 1e-7f, world.time);

			playerYaw = Ease.expAngle(playerYaw, playerYawDest, 1e-12f, world.time);

			// Combine each direction into a single vector

			var dir = new Vector2();

			if (Window.checkKey(SDL.SDL_Keycode.SDLK_w))
				dir += new Vector2(-MathF.Cos(cameraYaw), MathF.Sin(cameraYaw));

			if (Window.checkKey(SDL.SDL_Keycode.SDLK_a))
				dir += new Vector2(MathF.Sin(cameraYaw), MathF.Cos(cameraYaw));

			if (Window.checkKey(SDL.SDL_Keycode.SDLK_s))
				dir += new Vector2(MathF.Cos(cameraYaw), -MathF.Sin(cameraYaw));

			if (Window.checkKey(SDL.SDL_Keycode.SDLK_d))
				dir += new Vector2(-MathF.Sin(cameraYaw), -MathF.Cos(cameraYaw));

			// Normalize vector

			if (dir != new Vector2()) {
				dir = dir.Normalize();
				playerYawDest = MathF.Atan2(dir.Y, dir.X);
			}

			physics.applyImpulse(new Vector3(dir.X * velocity * world.time, dir.Y * velocity * world.time, 0));

			jumpCooldown += world.time;

			if (standingOn != null && Window.checkKey(SDL.SDL_Keycode.SDLK_SPACE) && jumpCooldown > 0.06f) {
				jumpCooldown = 0;
				physics.applyImpulse(new Vector3(0, 0, 0.6f));
			}
		}

		void handleAnimations(World world) {
			var len = new Vector2(physics.velocity.X, physics.velocity.Y).Length;

			if (standingOn != null) {
				legTime = len < 0.05 ? 0 : (legTime + 12 * world.time) % (2 * MathF.PI);
				legDist = len / maxVelocity * 0.3f;
			}
			else
				legTime = ((legTime + MathF.PI) % (2 * MathF.PI) - MathF.PI) * MathF.Pow(2e-7f, world.time);

			armDist = Math.Max(len / maxVelocity * 0.25f, 0.025f);
			armTime = (armTime + 3 * world.time * Math.Max(armDist, 0.15f) / 0.25f) % (2 * MathF.PI);
		}

		public override void renderPrepare(World world) {
			// Calculate position of camera

			var dir = new Vector3(
				MathF.Cos(cameraYaw),
				-MathF.Sin(cameraYaw),
				MathF.Sin(cameraPitch)
			);

			var from = position + dir * cameraDist;
			
			// Test for collisions with other entities when moving the camera from the player to it's location
			// If there's a hit take the nearest distance

			// Disabled for now
			// TODO: Check for collisions with objects first. If it hits something, do the sweep test

			/*Vector3 colPos;

			renderPlayer = true;

			if (world.physics.convexSweepTest(cameraShape, position, from, out colPos, Physics.Filter.Default, Physics.Filter.All & ~Physics.Filter.Character)) {
				from = colPos;

				if ((from - position).SquaredLength < minCamDist * minCamDist)
					renderPlayer = false;
			}*/
			
			world.scene.updateCamera(from, from == position ? position - dir : position, new Vector3(0, 0, 1));
		}
		
		const float velocity    = 1.8f;
		const float maxVelocity = 3;
		const float minCamDist  = 0.9f; 

		bool renderPlayer = true;

		float legTime = 0;
		float legDist = 0;

		float armTime = 0;
		float armDist = 0;

		float jumpCooldown = float.MaxValue;

		Physics.Shape cameraShape;

		float cameraDist = 2;

		float cameraYaw   = MathF.PI;
		float cameraPitch = 0.45f;

		float cameraYawDest   = MathF.PI;
		float cameraPitchDest = 0.45f;

		float playerYaw     = 0;
		float playerYawDest = 0;

		Shader shdTextured;
		Shader shdColored;

		Mesh    mshPlayer;
		Texture texPlayer;
		Mesh    mshArm;
		Mesh    mshLeg;

		uint vaoPlayer;
		uint vaoArm;
		uint vaoLeg;
	}
}
