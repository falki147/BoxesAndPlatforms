using OpenGL;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace BoxesAndPlatforms {
	public abstract class Entity: IDisposable {
		public enum AnimationState {
			None,   // Stay still
			Normal, // Move
			Reverse // Move but in reverse
		}
		
		[Flags]
		public enum Flags {
			Collidable = 1, // Can collide with other objects (physics has to be not null)
			Animation  = 2, // Has an animation
			Active     = 4, // Is activated
			Movable    = 8, // Can be moved
			Waiting    = 16 // Waits for player, ...
		}

		public Entity(WorldLayout.Entry entry) {
			this.entry = entry;
			
			standingEntities = new List<Entity>();

			flags |= Flags.Waiting;

			if (!entry.inactive)
				flags |= Flags.Active;

			if (entry.frames != null)
				flags |= Flags.Animation;
        }

		~Entity() {
			if (!disposed)
				System.Diagnostics.Debug.Fail("Entity was not disposed of properly.");

			Dispose(false);
		}

		// Overridable function for updating, drawing and freeing the entity

		public abstract void free();
		public abstract void update(World world);
		public abstract void renderPrepare(World world);
		public abstract void render(World world);
		
		// Translate object

		public void translate(Vector3 trans) {
			if ((flags & Flags.Collidable) != 0)
				physics.translate(trans);
			
			position += trans;
		}

		void Dispose(bool disposing) {
			if (!disposed) {
				disposed = true;

				if (disposing)
					free();
			}
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		
		public AABB collisionBox {
			get {
				return AABB.fromCenter(position, size);
			}
		}

		// Sets or gets the entity where it stands on
		
		public Entity standingOn {
			set {
				if (ReferenceEquals(standingOnCurrent, value))
					return;

				if (standingOnCurrent != null)
					standingOnCurrent.standingEntities.Remove(this);

				standingOnCurrent = value;

				if (value != null)
					value.standingEntities.Add(this);
			}
			get {
				return standingOnCurrent;
			}
		}

		public bool hasStandingEntities {
			get {
				return standingEntities.Count > 0;
			}
		}

		// List of entities which are standing on this entity

		public List<Entity> standingEntities { get; private set; }

		bool disposed;

		Entity standingOnCurrent;

		public Flags flags;

		public bool remove;
		
		public AnimationState animationState;
		
		public Vector3 size;
		public Vector3 offset;

		public Physics.Object physics;
		
		public float animationTime;
		
		public Vector3 position;
		
		public WorldLayout.Entry entry;
	}
}
