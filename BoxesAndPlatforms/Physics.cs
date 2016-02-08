using BulletSharp;
using System;
using System.Collections.Generic;

namespace BoxesAndPlatforms {
	public class Physics: IDisposable {
		// Filter for collisions
		// Only objects wich share it's filters can collide (e.g. Default and Default)

		[Flags]
		public enum Filter: short {
			Default    = 1,
			Static     = 2,
			Kinematic  = 4,
			Debris     = 8,
			SensorTrig = 16,
			Character  = 32,
			All        = -1
		}

		// Holds the BulletSharp RigidBody

		public class Object {
			public Object(RigidBody body, DynamicsWorld world) {
				this.body  = body;
				this.world = world;
			}

			public void applyForce(OpenGL.Vector3 force) {
				if (!body.IsActive)
					body.Activate();
				
				body.ApplyCentralForce(new Vector3(force.X, force.Y, force.Z));
			}

			public void applyImpulse(OpenGL.Vector3 impulse) {
				if (!body.IsActive)
					body.Activate();

				body.ApplyCentralImpulse(new Vector3(impulse.X, impulse.Y, impulse.Z));
			}

			public void translate(OpenGL.Vector3 trans) {
				if (body.MotionState != null)
					body.MotionState.WorldTransform = BulletSharp.Matrix.Translation(trans.X, trans.Y, trans.Z) * body.MotionState.WorldTransform;

				body.WorldTransform = BulletSharp.Matrix.Translation(trans.X, trans.Y, trans.Z) * body.WorldTransform;
			}

			public void setFilter(Filter group, Filter mask) {
				world.RemoveRigidBody(body);
				world.AddRigidBody(body, (short) group, (short) mask);
			}
			
			public OpenGL.Vector3 velocity {
				get {
					var vel = body.LinearVelocity;

					return new OpenGL.Vector3(vel.X, vel.Y, vel.Z);
				}
				set {
					if (!body.IsActive)
						body.Activate();
					
					body.LinearVelocity = new Vector3(value.X, value.Y, value.Z);
				}
			}

			public OpenGL.Vector3 translation {
				get {
					BulletSharp.Matrix matrix;

					if (body.MotionState != null)
						matrix = body.MotionState.WorldTransform;
					else
						matrix = body.WorldTransform;

					return new OpenGL.Vector3(matrix.M41, matrix.M42, matrix.M43);
				}
			}

			// If changed from static to default or default to static setFilter should be called
			
			public float mass {
				get {
					return body.InvMass == 0 ? 0 : 1 / body.InvMass;
				}
				set {
					body.SetMassProps(value, value != 0 ? body.CollisionShape.CalculateLocalInertia(value) : Vector3.Zero);
					
					if (!body.IsStaticObject && !body.IsActive)
						body.Activate();
				}
			}
			
			DynamicsWorld world;

			public RigidBody body { get; private set; }
		}
		
		public struct Shape {
			public ConvexShape shape;
		}
		
		public Physics() {
			config     = new DefaultCollisionConfiguration();
			dispatcher = new CollisionDispatcher(config);
			broadphase = new DbvtBroadphase();
			solver     = new SequentialImpulseConstraintSolver();
			world      = new DiscreteDynamicsWorld(dispatcher, broadphase, solver, config);
		}

		~Physics() {
			if (!disposed)
				System.Diagnostics.Debug.Fail("Physics were not disposed of properly.");

			Dispose(false);
		}

		public Object addBox(OpenGL.Vector3 origin, OpenGL.Vector3 size, float mass, float friction) {
			var box = addBoxShape(size);

			var localInertia = mass == 0 ? Vector3.Zero : box.shape.CalculateLocalInertia(mass);

			var motionState = new DefaultMotionState(
				BulletSharp.Matrix.Translation(origin.X, origin.Y, origin.Z)
			);
			
			var constrInfo = new RigidBodyConstructionInfo(mass, motionState, box.shape, localInertia);

			constrInfo.Friction = friction;

			var body = new RigidBody(constrInfo);

			constrInfo.Dispose();

			body.InvInertiaDiagLocal = Vector3.Zero;
			body.UpdateInertiaTensor();
			
			world.AddRigidBody(body);
			
			return new Object(body, world);
		}

		public Object addBox(OpenGL.Vector3 origin, OpenGL.Vector3 size, float mass, float friction, Filter group, Filter mask) {
			var box = addBoxShape(size);

			var localInertia = mass == 0 ? Vector3.Zero : box.shape.CalculateLocalInertia(mass);

			var motionState = new DefaultMotionState(
				BulletSharp.Matrix.Translation(origin.X, origin.Y, origin.Z)
			);

			var constrInfo = new RigidBodyConstructionInfo(mass, motionState, box.shape, localInertia);

			constrInfo.Friction = friction;

			var body = new RigidBody(constrInfo);

			constrInfo.Dispose();

			body.InvInertiaDiagLocal = Vector3.Zero;
			body.UpdateInertiaTensor();
			
			world.AddRigidBody(body, (short) group, (short) mask);

			return new Object(body, world);
		}

		public Shape addSphereShape(float radius) {
			var sphere = new SphereShape(radius);

			shapes.Add(sphere);

			return new Shape() { shape = sphere };
		}

		public Shape addBoxShape(OpenGL.Vector3 size) {
			var box = new BoxShape(
				size.X / 2,
				size.Y / 2,
				size.Z / 2
			);

			shapes.Add(box);

			return new Shape() { shape = box };
		}

		// Test for collisions when moved from "from" to "to" and return position where it first collides
		
		public bool convexSweepTest(Shape shape, OpenGL.Vector3 from, OpenGL.Vector3 to, out OpenGL.Vector3 pos, Filter group, Filter mask) {
			var callback = new ClosestConvexResultCallback();

			callback.ConvexFromWorld      = new Vector3(from.X, from.Y, from.Z);
			callback.ConvexToWorld        = new Vector3(to.X, to.Y, to.Z);
			callback.ClosestHitFraction   = 1.0f;
			callback.CollisionFilterGroup = (CollisionFilterGroups) group;
			callback.CollisionFilterMask  = (CollisionFilterGroups) mask;

			world.ConvexSweepTest(shape.shape, BulletSharp.Matrix.Translation(from.X, from.Y, from.Z), BulletSharp.Matrix.Translation(to.X, to.Y, to.Z), callback);

			if (callback.HasHit) {
				var vec = Vector3.Lerp(new Vector3(from.X, from.Y, from.Z), new Vector3(to.X, to.Y, to.Z), callback.ClosestHitFraction);
				pos = new OpenGL.Vector3(vec.X, vec.Y, vec.Z);
				return true;
			}

			pos = new OpenGL.Vector3();
			return false;
		}
		
		public void remove(Object obj) {
			world.RemoveRigidBody(obj.body);
			remove(obj.body.CollisionShape);
			obj.body.Dispose();
		}

		public void remove(CollisionShape shape) {
			shapes.Remove(shape);
			shape.Dispose();
		}

		public void update(float time) {
			world.StepSimulation(time, 10);
		}
		
		void Dispose(bool disposing) {
			if (!disposed) {
				disposed = true;

				while (world.NumCollisionObjects > 0) {
					var obj = world.CollisionObjectArray[0];
					var body = obj as RigidBody;

					if (body != null) {
						if (body.MotionState != null)
							body.MotionState.Dispose();

						world.RemoveRigidBody(body);
					}

					obj.Dispose();
				}

				foreach (var shape in shapes)
					shape.Dispose();

				shapes.Clear();

				world.Dispose();
				solver.Dispose();
				broadphase.Dispose();
				dispatcher.Dispose();
				config.Dispose();
				
				world = null;
				solver = null;
				broadphase = null;
				dispatcher = null;
				config = null;
			}
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public float gravity {
			set {
				world.Gravity = new Vector3(0, 0, -value);
			}
			get {
				return -world.Gravity.Z;
			}
		}

		bool disposed;

		List<CollisionShape> shapes = new List<CollisionShape>();

		DefaultCollisionConfiguration config;
		CollisionDispatcher dispatcher;
		DbvtBroadphase broadphase;
		SequentialImpulseConstraintSolver solver;
		DiscreteDynamicsWorld world;
	}
}
