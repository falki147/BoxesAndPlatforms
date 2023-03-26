using OpenGL;
using System.Numerics;

namespace BoxesAndPlatforms {
	public class RoomGame: RoomManager.Room {
		// Renders GUI and handles World object

		public RoomGame(GameManager mgr) {
			this.mgr = mgr;

			world = new World(mgr.world);
			
			gui = new GUI();
		}

		public override void free() {
			world.Dispose();
			gui.Dispose();
		}

		public override void render() {
			Gl.ClearColor(0, 0, 0, 0);
			Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			
			world.render();

			Gl.Disable(EnableCap.DepthTest);

			gui.begin();
			
			gui.drawLabel($"Score: {world.score:0}", new Vector2(0, Window.height - 50), new Vector2(300, 50));
			
			var posPauseY = Utils.lerp(-300, Window.height / 2, world.state == World.State.Paused ? Ease.bounceOut(timePause) : 1 - Ease.cubicIn(timePause));
			var posDoneY = Utils.lerp(-300, Window.height / 2, Ease.bounceOut(timeDone));

			// Draw pause menu

			gui.drawPopupMenu(new Vector2(Window.width / 2, posPauseY), "Resume", "Restart", "Go to Menu", "Quit Game");

			// Draw end menu

			if (mgr.hasNext)
				gui.drawPopupMenu(new Vector2(Window.width / 2, posDoneY), new string[] { $"Score: {world.score:0}", "Play the next level?", "Yes", "No" }, new bool[] { true, true, false, false });
			else
				gui.drawPopupMenu(new Vector2(Window.width / 2, posDoneY), new string[] { $"Score: {world.score:0}", "Ok" }, new bool[] { true, false });

			if (world.state == World.State.Paused || world.state == World.State.Done)
				gui.drawCursor();

			gui.end();
			
			Gl.Enable(EnableCap.DepthTest);
		}

		public override void update() {
			world.update();

			// Enable or disable pause menu

			if (Window.checkKeyPressed(SDL2.SDL.SDL_Keycode.SDLK_ESCAPE)) {
				if (world.state == World.State.Paused) {
					world.state = World.State.Normal;
					Window.mouseLocked = true;
					timePause = 0;
				}
				else if (world.state == World.State.Normal) {
					world.state = World.State.Paused;
					Window.mouseLocked = false;
					timePause = 0;
				}
			}
			
			if (timePause < 1)
				timePause += world.time;
			
			// If paused check menu buttons

			if (world.state == World.State.Paused) {
				var posY = Utils.lerp(-300, Window.height / 2, world.state == World.State.Paused ? Ease.bounceOut(timePause) : 1 - Ease.cubicIn(timePause));

				switch (gui.checkPopupMenu(new Vector2(Window.width / 2, posY), 4)) {
				case 0:
					world.state = World.State.Normal;
					Window.mouseLocked = true;
					break;
				case 1:
					Window.mouseLocked = true;
                    world.Dispose();
					world = new World(mgr.world);
					break;
				case 2:
					RoomManager.leaveRoom();
					break;
				case 3:
					Window.close();
					break;
				}
			}

			// If finished check menu buttons

			if (world.state == World.State.Done) {
				if (Window.mouseLocked)
					Window.mouseLocked = false;

				if (timeDone < 1)
					timeDone += world.time;
				
				var posY = Utils.lerp(-300, Window.height / 2, Ease.bounceOut(timeDone));

				if (mgr.hasNext)
					switch (gui.checkPopupMenu(new Vector2(Window.width / 2, posY), new bool[] { true, true, false, false })) {
					case 2:
						Window.mouseLocked = true;

						timePause = float.MaxValue;
						timeDone  = 0;

						mgr.next();

						world.Dispose();
						world = new World(mgr.world);
						break;
					case 3:
						RoomManager.leaveRoom();
						break;
					}
				else
					if (gui.checkPopupMenu(new Vector2(Window.width / 2, posY), new bool[] { true, false }) == 1)
						RoomManager.leaveRoom();
			}

			// Restart the world when necessary
			
			if ((world.state == World.State.Normal && Window.checkKeyPressed(SDL2.SDL.SDL_Keycode.SDLK_r)) || world.state == World.State.Restart) {
				world.Dispose();
				world = new World(mgr.world);
			}
		}

		public override void enter() {
			Gl.Enable(EnableCap.DepthTest);
			Gl.Enable(EnableCap.CullFace);
			Gl.CullFace(CullFaceMode.Front);

			Window.mouseLocked = true;
		}

		public override void leave() {
			Gl.Disable(EnableCap.DepthTest);
			Gl.Disable(EnableCap.CullFace);

			Window.mouseLocked = false;
		}
		
		GUI gui;
		
		float timePause = float.MaxValue;
		float timeDone  = 0;

		GameManager mgr;

		World world;
	}
}
