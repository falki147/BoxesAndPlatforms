using System;
using System.Collections.Generic;
using OpenGL;
using System.Diagnostics;

namespace BoxesAndPlatforms {
	public class World: IDisposable {
		public enum State {
			Normal,
			Paused,  // World is paused
			Restart, // World needs a restart
			Done     // Player has reached end
		}

		public World(WorldLayout layout) {
			scene   = new Scene(Window.width, Window.height);
			skybox  = new SkyBox(AssetLoader.getCubeMap("sky\\sky.dds"), AssetLoader.getShader("sky\\sky.shd"));
			random  = new Random();
			physics = new Physics();

			physics.gravity = gravity;
			
			// Create an entity for each entry in the WorldLayout

			foreach (var entry in layout.entries) {
				switch (entry.id) {
				case "big":
					entities.Add(new EntityBig(this, entry));
					break;
				case "box":
					entities.Add(new EntityBox(this, entry));
					break;
				case "cactus":
					entities.Add(new EntityCactus(this, entry));
					break;
				case "diamond":
					entities.Add(new EntityDiamond(this, entry));
					break;
				case "end":
					entities.Add(new EntityEnd(this, entry));
					break;
				case "falling":
					entities.Add(new EntityFalling(this, entry));
					break;
				case "fallingplatform":
					entities.Add(new EntityFallingPlatform(this, entry));
					break;
				case "movingspikes":
					entities.Add(new EntityMovingSpikes(this, entry));
					break;
				case "player":
					entities.Add(player = new EntityPlayer(this, entry));
					break;
				case "small":
					entities.Add(new EntitySmall(this, entry));
					break;
				case "spikes":
					entities.Add(new EntitySpikes(this, entry));
					break;
				case "switch":
					entities.Add(new EntitySwitch(this, entry));
					break;
				case "tree":
					entities.Add(new EntityTree(this, entry));
					break;
				}
			}
        }

		~World() {
			if (!disposed)
				Debug.Fail("World was not disposed of properly.");

			Dispose(false);
		}

		public void update() {
			if (state == State.Normal) {
				time = (float) sw.Elapsed.TotalSeconds;
				sw.Restart();

				// Go through every entity and store the entity it stands on

				foreach (var other in entities)
					if ((other.flags & Entity.Flags.Collidable) != 0 && (other.flags & Entity.Flags.Movable) != 0)
						other.standingOn = getEntityBelowOther(other, minDist, 0.05f);
				
				// Update entities

				foreach (var entity in entities) {
					entity.update(this);

					if ((entity.flags & Entity.Flags.Animation) != 0)
						updateAnimation(entity);
				}
			
				physics.update(time);

				// Update positions (they are chnaged by the physics engine)

				foreach (var entity in entities)
					if (entity.physics != null)
						entity.position = entity.physics.translation;

				// Check if any entity requests a delete

				for (var i = 0; i < entities.Count; ++i) {
					if (entities[i].remove) {
						if (entities[i].physics != null)
							physics.remove(entities[i].physics);

						entities[i].Dispose();
						entities.RemoveAt(i--);
					}
				}
				
				score = Math.Max(0, score - 2f * time);
			}
			else
				sw.Restart();
		}
		
		public void render() {
			foreach (var entity in entities)
				entity.renderPrepare(this);
			
			foreach (var entity in entities)
				entity.render(this);

			// Render sky

			Gl.Disable(EnableCap.DepthTest);
			Gl.Enable(EnableCap.Blend);
			Gl.BlendFunc(BlendingFactorSrc.OneMinusDstAlpha, BlendingFactorDest.DstAlpha);

            skybox.render(scene.matrixP, scene.matrixV, 1);

			Gl.Enable(EnableCap.DepthTest);
			Gl.Disable(EnableCap.Blend);

			scene.end();
		}

		// Returns the entity which is below another one
		// If two are beneath it takes the one with the largest area
		
		public Entity getEntityBelowOther(Entity other, float minDist, float minArea) {
			var box = other.collisionBox;

			var maxArea = minArea;

			Entity found = null;

			foreach (var entity in entities) {
				if (!ReferenceEquals(entity, other) && (entity.flags & Entity.Flags.Collidable) != 0 && Math.Abs(entity.collisionBox.distanceZ(box)) < minDist) {
					var dx = entity.collisionBox.distanceX(box);
					var dy = entity.collisionBox.distanceY(box);

					if (dx < 0 && dy < 0 && dx * dy > maxArea) {
						maxArea = dx * dy;
						found = entity;
					}
				}
            }

			return found;
		}
		
		public IEnumerable<Entity> findEntities(string name) {
			foreach (var entity in entities)
				if (entity.entry.name == name)
					yield return entity;
		}
		
		// Update animations of entitiys

		void updateAnimation(Entity entity) {
			if ((entity.flags & Entity.Flags.Active) != 0) {
				switch (entity.animationState) {
				case Entity.AnimationState.None:
					updateActivation(entity, Entity.AnimationState.Normal);
					break;
				case Entity.AnimationState.Normal:
					if (entity.animationTime < entity.entry.duration) {
						var trans = entity.entry.getPosition(entity.animationTime += time) + entity.offset - entity.physics.translation;

						entity.translate(trans);

						foreach (var standing in entity.standingEntities)
							standing.translate(trans);
					}
					else {
						switch (entity.entry.onEnd) {
						case WorldLayout.Entry.Action.Repeat:
							updateActivation(entity, Entity.AnimationState.Normal);
							break;
						case WorldLayout.Entry.Action.Reverse:
							updateActivation(entity, Entity.AnimationState.Reverse);
							break;
						}
					}
					
					break;
				case Entity.AnimationState.Reverse:
					if (entity.animationTime < entity.entry.duration) {
						var trans = entity.entry.getPosition(entity.entry.duration - (entity.animationTime += time)) + entity.offset - entity.physics.translation;

						entity.translate(trans);

						foreach (var standing in entity.standingEntities)
							standing.translate(trans);
					}
					else
						updateActivation(entity, Entity.AnimationState.Normal);

					break;
				}
			}
		}

		bool updateActivation(Entity entity, Entity.AnimationState state) {
			switch (entity.entry.activation) {
			case WorldLayout.Entry.Activation.Always:
				entity.animationState = state;
				entity.animationTime = 0;
				entity.flags &= ~Entity.Flags.Waiting;
				return true;
			case WorldLayout.Entry.Activation.Player:
				if (ReferenceEquals(player.standingOn, entity)) {
					if ((entity.flags & Entity.Flags.Waiting) != 0) {
						entity.animationState = state;
						entity.animationTime = 0;
						entity.flags &= ~Entity.Flags.Waiting;
						return true;
					}
				}
				else
					entity.flags |= Entity.Flags.Waiting;

				break;
			}

			return false;
		}
		
		void Dispose(bool disposing) {
			if (!disposed) {
				disposed = true;

				if (disposing) {
					foreach (var entry in entities)
						entry.Dispose();

					skybox.Dispose();
					physics.Dispose();
				}
			}
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		const float gravity = 10f;
		public const float minDist = 0.1f;

		bool disposed;

		Stopwatch sw = new Stopwatch();

		List<Entity> entities = new List<Entity>();

		public float score = 0;

		public State state;

		public Random random   { get; private set; }
		public SkyBox skybox   { get; private set; }
		public Scene scene     { get; private set; }
		public float time      { get; private set; }
		public Entity player   { get; private set; }
		public Physics physics { get; private set; }
	}
}
