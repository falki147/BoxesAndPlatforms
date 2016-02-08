using OpenGL;
using System;

namespace BoxesAndPlatforms {
	// Handles GUI drawing and checking

	public class GUI: IDisposable {
		public GUI() {
			atlas = AssetLoader.getAtlas("menu\\menu.xml");

			b = new Batch();

			b.texture = AssetLoader.getTexture("menu\\menu.png");
			b.setProgram(AssetLoader.getShader("menu\\menu.shd"));

			uMVP = b.shader.getLocation(Shader.Input.MVPMat);
		}

		~GUI() {
			if (!disposed)
				System.Diagnostics.Debug.Fail("GUI was not disposed of properly.");

			Dispose(false);
		}

		public void begin() {
			Gl.Enable(EnableCap.Blend);
			Gl.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

			b.begin();

			Gl.UniformMatrix4fv(uMVP, Matrix4.CreateOrthographicOffCenter(0, Window.width, Window.height, 0, 0, 1));
		}

		// Check if the mouse is inside the area

		public bool checkArea(float x, float y, float w, float h) {
			return Utils.isInside(
				new Vector2(Window.mouseX, Window.mouseY) - translation, new Vector2(x, y), new Vector2(w, h)
			);
		}

		// Check if mouse is inside the area and the mouse is pressed or released

		public bool checkAreaReleased(float x, float y, float w, float h) {
			return Window.checkButtonReleased(SDL2.SDL.SDL_BUTTON_LEFT) && checkArea(x, y, w, h);
		}

		public bool checkAreaPressed(float x, float y, float w, float h) {
			return Window.checkButtonPressed(SDL2.SDL.SDL_BUTTON_LEFT) && checkArea(x, y, w, h);
		}

		// Draw simple GUI elements

		public void drawRectangle(Vector2 pos, Vector2 size, Color color) {
			b.drawImage(atlas.images["point"], pos + translation, size, color);
		}

		public void drawButton(string text, Vector2 pos, Vector2 size) {
			if (checkArea(pos.X, pos.Y, size.X, size.Y))
				b.drawImage(atlas.images["point"], pos + translation, size, new Color(0, Window.checkButton(SDL2.SDL.SDL_BUTTON_LEFT) ? (byte) 100 : (byte) 50));

			drawText(text, pos + size / 2);
		}

		public void drawText(string text, Vector2 pos) {
			pos += translation;

			b.drawText(atlas.fonts["menufont"], new Vector2(pos.X + 1, pos.Y - 3), text, Batch.HorizontalAlign.Center, Batch.VerticalAlign.Middle);
			b.drawText(atlas.fonts["menufont"], new Vector2(pos.X - 1, pos.Y - 5), text, new Color(0xFF), Batch.HorizontalAlign.Center, Batch.VerticalAlign.Middle);
		}

		public void drawLabel(string text, Vector2 pos, Vector2 size) {
			b.drawImage(atlas.images["point"], pos + translation, size, new Color(0, 50));
			drawText(text, pos + size / 2);
		}
		
		public void drawCursor() {
			b.drawImage(atlas.images["cursor"], new Vector2(Window.mouseX, Window.mouseY));
		}
		
		public void drawMenu(Vector2 pos, bool backButton, string label, params string[] buttons) {
			drawLabel(label, new Vector2(0, Window.height / 2 - 225) + pos, new Vector2(Window.width, 60));

			if (buttons.Length != 0) {
				var buttonsSize = (buttons.Length - 1) * 60 + 50;

				for (var i = 0; i < buttons.Length; ++i)
					drawButton(buttons[i], new Vector2(0, Window.height / 2 - buttonsSize / 2 + i * 60 + 35) + pos, new Vector2(Window.width, 50));
			}

			if (backButton)
				drawButton("Back", new Vector2(0, Window.height / 2 + 230) + pos, new Vector2(Window.width, 50));
        }

		public void drawPopupMenu(Vector2 pos, params string[] buttons) {
			drawRectangle(pos - new Vector2(300, 150), new Vector2(600, 300), new Color(255, 255, 191, 150));

			var buttonsSize = (buttons.Length - 1) * 60 + 50;

			for (var i = 0; i < buttons.Length; ++i)
				drawButton(buttons[i], new Vector2(pos.X - 300, pos.Y - buttonsSize / 2 + i * 60), new Vector2(600, 50));
		}

		public void drawPopupMenu(Vector2 pos, string[] buttons, bool[] label) {
			drawRectangle(pos - new Vector2(300, 150), new Vector2(600, 300), new Color(255, 255, 191, 150));

			var buttonsSize = (buttons.Length - 1) * 60 + 50;

			for (var i = 0; i < buttons.Length; ++i)
				if (label[i])
					drawText(buttons[i], new Vector2(pos.X, pos.Y - buttonsSize / 2 + i * 60 + 25));
				else
					drawButton(buttons[i], new Vector2(pos.X - 300, pos.Y - buttonsSize / 2 + i * 60), new Vector2(600, 50));
		}

		// Check if any of the buttons were triggered
		// Returns -1 on failure, Back button has the value numButtons

		public int checkMenu(Vector2 pos, bool backButton, int numButtons) {
			var buttonsSize = (numButtons - 1) * 60 + 50;

			for (var i = 0; i < numButtons; ++i)
				if (checkAreaReleased(pos.X, pos.Y + Window.height / 2 - buttonsSize / 2 + i * 60 + 35, Window.width, 50))
					return i;

			if (backButton && checkAreaReleased(pos.X, pos.Y + Window.height / 2 + 230, Window.width, 50))
				return numButtons;

			return -1;
		}

		public int checkPopupMenu(Vector2 pos, int numButtons) {
			var buttonsSize = (numButtons - 1) * 60 + 50;

			for (var i = 0; i < numButtons; ++i)
				if (checkAreaReleased(pos.X - 300, pos.Y - buttonsSize / 2 + i * 60, 600, 50))
					return i;

			return -1;
		}

		public int checkPopupMenu(Vector2 pos, bool[] labels) {
			var buttonsSize = (labels.Length - 1) * 60 + 50;

			for (var i = 0; i < labels.Length; ++i)
				if (!labels[i] && checkAreaReleased(pos.X - 300, pos.Y - buttonsSize / 2 + i * 60, 600, 50))
						return i;

			return -1;
		}

		public void end() {
			b.end();

			Gl.Disable(EnableCap.Blend);
			Gl.Enable(EnableCap.DepthTest);
		}

		void Dispose(bool disposing) {
			if (!disposed) {
				disposed = true;

				if (disposing)
					b.Dispose();
			}
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		bool disposed;

		Batch b;
		Atlas atlas;
		int   uMVP;

		public Vector2 translation;
	}
}
