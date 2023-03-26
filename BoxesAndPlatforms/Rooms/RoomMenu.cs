using OpenGL;
using SDL2;
using System;
using System.Diagnostics;
using System.Numerics;

namespace BoxesAndPlatforms {
	public class RoomMenu: RoomManager.Room {
		// Render background, GUI and handle input

		public RoomMenu() {
			gameMgr = new GameManager("levels\\levels.xml");
			
			shd = AssetLoader.getShader("entity\\diamond\\diamond.shd");
			msh = AssetLoader.getMesh("entity\\diamond\\diamond.modb");
			tex = AssetLoader.getCubeMap("sky\\sky.dds");

			msh.prepareVAO(shd, vao = Gl.GenVertexArray());

			proj = Matrix.createPerspectiveLH(1.3f, (float) Window.width / Window.height, 1, 400);

			width      = Settings.width;
			height     = Settings.height;
			fullscreen = Settings.fullscreen;
			maxfps     = Settings.maxfps;

			gui = new GUI();
		}

		public override void free() {
			gui.Dispose();
		}
		
		public override void render() {
			Gl.ClearColor(1, 1, 0.75f, 1);
			Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			
			Gl.Enable(EnableCap.DepthTest);

			renderDiamond();

			Gl.Disable(EnableCap.DepthTest);

			gui.begin();

			gui.translation = trans;

			// Render menu

			gui.drawMenu(new Vector2(), false, "Boxes 'n' Platforms", "Play", "Options", "Quit");

			// Render levels

			gui.drawMenu(new Vector2(Window.width, 0), true, "Levels");

			if (gameMgr.levels.Count > 6)
				gui.drawRectangle(new Vector2(2 * Window.width - 64, Utils.lerp(Window.height / 2 - 150, Window.height / 2 + 160, scrollBarPerc)), new Vector2(20, 60), new Color(0, 50));

			for (var i = 0; i < gameMgr.levels.Count; ++i) {
				var ymin = Window.height / 2 - 150 + i * 60 - scrollBarPerc * Math.Max(0, gameMgr.levels.Count - 6) * 60;
				var ymax = ymin + 50;

				if (ymin > Window.height / 2 - 151 && ymax < Window.height / 2 + 210)
					gui.drawButton(gameMgr.levels[i].name, new Vector2(Window.width + 100, ymin), new Vector2(Window.width - 200, 50));
			}

			// Render options

			gui.drawMenu(new Vector2(-Window.width, 0), true, "Options", $"Resolution: {width}x{height}", $"Fullscreen: {fullscreen}", $"Max FPS: {(maxfps < 0 ? "Unlimited" : maxfps.ToString())}");

			gui.drawCursor();

			gui.end();
		}

		public override void update() {
			deltaTime = (float) sw.Elapsed.TotalSeconds;
			sw.Restart();

			// Handle input for menu

			switch (gui.checkMenu(new Vector2(), false, 3)) {
			case 0:
				gotoPosition(new Vector2(-Window.width, 0));
				break;
			case 1:
				gotoPosition(new Vector2(Window.width, 0));
				break;
			case 2:
				Window.close();
				break;
			}

			// Handle input for level selection

			if (gui.checkMenu(new Vector2(Window.width, 0), true, 0) == 0)
				gotoPosition(new Vector2(0, 0));

			for (var i = 0; i < gameMgr.levels.Count; ++i) {
				var ymin = Window.height / 2 - 150 + i * 60 - scrollBarPerc * Math.Max(0, gameMgr.levels.Count - 6) * 60;
				var ymax = ymin + 50;

				if (ymin > Window.height / 2 - 151 && ymax < Window.height / 2 + 210 && gui.checkAreaReleased(Window.width + 100, ymin, Window.width - 200, 50)) {
					gameMgr.set(i);
					RoomManager.gotoRoom(new RoomGame(gameMgr));
				}
			}

			if (gameMgr.levels.Count > 6) {
				if (gui.checkArea(Window.width, 0, Window.width, Window.height))
					scrollBarPerc = Utils.clamp(scrollBarPerc - Window.wheelY * 0.2f, 0, 1);

				if (gui.checkAreaPressed(2 * Window.width - 64, Utils.lerp(Window.height / 2 - 150, Window.height / 2 + 160, scrollBarPerc), 20, 60))
					relY = Window.mouseY + trans.Y - Utils.lerp(Window.height / 2 - 150, Window.height / 2 + 160, scrollBarPerc);

				if (relY >= 0) {
					scrollBarPerc = Utils.clamp((Window.mouseY - relY + trans.Y - Window.height / 2 + 150) / 310, 0, 1);

					if (Window.checkButtonReleased(SDL.SDL_BUTTON_LEFT))
						relY = -1;
				}
			}

			// Handle input for settings

			switch (gui.checkMenu(new Vector2(-Window.width, 0), true, 3)) {
			case 0:
				// Find best display mode

				var first = new Window.DisplayMode();
				var newMode = new Window.DisplayMode();

				foreach (var mode in Window.displayModes(0)) {
					if (mode.width < 800 || mode.height < 600)
						continue;

					if (first.width == 0 || first.height == 0)
						first = mode;

					if (width <= mode.width && height < mode.height) {
						newMode = mode;
						break;
					}
				}

				if (first.width != 0 && first.height != 0) {
					if (newMode.width != 0 && newMode.height != 0) {
						width = newMode.width;
						height = newMode.height;
					}
					else {
						width = first.width;
						height = first.height;
					}
				}
				break;
			case 1:
				fullscreen = !fullscreen;
				break;
			case 2:
				maxfps = maxfps == 30 ? 60 : (maxfps == 60 ? -1 : 30);
				break;
			case 3:
				// Update settings if changed

				if (Settings.width != width || Settings.height != height) {
					Settings.width = width;
					Settings.height = height;

					Window.setSize(width, height);
					Gl.Viewport(0, 0, width, height);
					proj = Matrix.createPerspectiveLH(1.3f, (float) Window.width / Window.height, 1, 400);
				}

				if (Settings.fullscreen != fullscreen) {
					Settings.fullscreen = fullscreen;
					Window.fullscreen = fullscreen;
				}

				if (Settings.maxfps != maxfps) {
					Settings.maxfps = maxfps;

					if (maxfps < 0)
						Window.limitFPS = false;
					else {
						Window.limitFPS = true;
						Window.frequency = maxfps;
					}
				}

				gotoPosition(new Vector2(0, 0));
				break;
			}
			
			// Update time for diamond and animation

			angle = (angle + 0.3f * deltaTime) % (2 * MathF.PI);
			
			if (time >= 0) {
				time += deltaTime;

				trans.X = Utils.lerp(from.X, to.X, Ease.quintInOut(time));
				trans.Y = Utils.lerp(from.Y, to.Y, Ease.quintInOut(time));
			}
		}

		void gotoPosition(Vector2 pos) {
			from = trans;
			to   = pos;
			time = 0;
		}
		
		// Render diamond

		void renderDiamond() {
			var mod = Matrix4.CreateRotationZ(angle) * Matrix4.CreateTranslation(new Vector3(0, 0, 0.05f * MathF.Sin(2 * angle)));
			var mvp = Matrix.createLookAtLH(new Vector3(0.75f, 0.75f, 0.75f), new Vector3(), new Vector3(0, 0, 1)) * proj;

			shd.use();

			shd.setMatrix3(Shader.Input.NormMat, Matrix.normalMatrix(mod));
			shd.setMatrix4(Shader.Input.MVPMat, mod * mvp);
			shd.setMatrix4(Shader.Input.ModelMat, Matrix4.Identity);
			shd.setVector3(Shader.Input.CameraPos, new Vector3(0.5f, 0.5f, 0.5f));
			shd.setTexture(Shader.Input.CubeMap, tex);

			Gl.BindVertexArray(vao);
			Gl.DrawArrays(BeginMode.Triangles, 0, msh.vertices);
			Gl.BindVertexArray(0);
		}
		
		public override void enter() { }

		public override void leave() { }

		GUI gui;

		GameManager gameMgr;

		Vector2 from, to;

		Stopwatch sw = new Stopwatch();

		float deltaTime = 0;
		float time = -1;

		float scrollBarPerc = 0;
		float relY = -1;
		
		Matrix4 proj;

		Vector2 trans;

		float angle;

		Shader  shd;
		Mesh    msh;
		CubeMap tex;
		uint    vao;

		int width, height;
		bool fullscreen;
		int maxfps;
	}
}
