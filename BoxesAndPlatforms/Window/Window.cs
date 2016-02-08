using SDL2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace BoxesAndPlatforms {
	// Handle window creation and mouse/keyboard input

	static public class Window {
		public struct DisplayMode {
			public int width;
			public int height;
		}

		public class UpdateEvent: EventArgs { }
		public class RenderEvent: EventArgs { }
		
		static public void init(string title, int width, int height) {
			SDL2Helper.init(SDL.SDL_INIT_VIDEO);

			// Enable Anti-Aliasing
			SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_MULTISAMPLEBUFFERS, 1);
			SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_MULTISAMPLESAMPLES, 4);

			// Force 8-Bits per component
			SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_RED_SIZE, 8);
			SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_GREEN_SIZE, 8);
			SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_BLUE_SIZE, 8);
			SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_ALPHA_SIZE, 8);

			// Enable double buffering
			SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_DOUBLEBUFFER, 1);

			wnd = SDL.SDL_CreateWindow(
				title,
				SDL.SDL_WINDOWPOS_UNDEFINED,
				SDL.SDL_WINDOWPOS_UNDEFINED,
				width, height,
				SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL | SDL.SDL_WindowFlags.SDL_WINDOW_HIDDEN
			);
			
			if (wnd == IntPtr.Zero)
				throw new Exception("Failed to initialize window");

			Window.width  = width;
			Window.height = height;

			con = SDL.SDL_GL_CreateContext(wnd);

			if (con == IntPtr.Zero)
				throw new Exception("Failed to initialize OpenGL context");

			SDL.SDL_GL_MakeCurrent(wnd, con);
		}

		static public void show() {
			SDL.SDL_ShowWindow(wnd);
		}
		
		static public void loop() {
			SDL.SDL_Event ev;

			var sw = new Stopwatch();
			
			for (;;) {
				if (limitFPS)
					sw.Restart();

				wheelX = wheelY = 0;
				relMouseX = relMouseY = 0;

				// Handle SDL Events

				while (SDL.SDL_PollEvent(out ev) != 0) {
					switch (ev.type) {
					case SDL.SDL_EventType.SDL_KEYDOWN:
						keysHaveChanged = true;
						keysCur[mapKeycode(ev.key.keysym.sym)] = true;
						break;
					case SDL.SDL_EventType.SDL_KEYUP:
						keysHaveChanged = true;
						keysCur[mapKeycode(ev.key.keysym.sym)] = false;
						break;
					case SDL.SDL_EventType.SDL_MOUSEMOTION:
						if (mouseLocked) {
							relMouseX += ev.motion.xrel;
							relMouseY += ev.motion.yrel;
						}
						else {
							mouseX = ev.motion.x;
							mouseY = ev.motion.y;
						}
						break;
					case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
						if (ev.button.button >= 1 && ev.button.button <= 3) {
							buttonsHaveChanged = true;
							buttonsCur[ev.button.button] = true;
						}
						break;
					case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
						if (ev.button.button >= 1 && ev.button.button <= 3) {
							buttonsHaveChanged = true;
							buttonsCur[ev.button.button] = false;
						}
						break;
					case SDL.SDL_EventType.SDL_MOUSEWHEEL:
						wheelX += ev.wheel.x;
						wheelY += ev.wheel.y;
						break;
					case SDL.SDL_EventType.SDL_QUIT:
						return;
					}
				}
				
				// Update room

				if (eventUpdate != null)
					eventUpdate.Invoke(null, new UpdateEvent());

				// Swap key or button BitArray if needed
				// Needs to be done in order to determine if the button was pressed or released

				if (keysHaveChanged) {
					keysHaveChanged = false;
					keysLast = (BitArray) keysCur.Clone();
				}

				if (buttonsHaveChanged) {
					buttonsHaveChanged = false;
					buttonsLast = (BitArray) buttonsCur.Clone();
				}

				// Render room

				if (eventRender != null)
					eventRender.Invoke(null, new RenderEvent());

				SDL.SDL_GL_SwapWindow(wnd);
				
				if (limitFPS) {
					sw.Stop();
				
					if (sw.Elapsed < periodeTime)
						Thread.Sleep(periodeTime - sw.Elapsed); // TODO: Use multimedia timer
				}
			}
		}

		static public bool checkKey(SDL.SDL_Keycode code) {
			return keysCur[mapKeycode(code)];
		}

		// Check key if it was pressed or not (rising edge)

		static public bool checkKeyPressed(SDL.SDL_Keycode code) {
			return keysCur[mapKeycode(code)] && !keysLast[mapKeycode(code)];
		}

		// Check key if it was released or not (falling edge)

		static public bool checkKeyReleased(SDL.SDL_Keycode code) {
			return !keysCur[mapKeycode(code)] && keysLast[mapKeycode(code)];
		}

		static private int mapKeycode(SDL.SDL_Keycode code) {
			// Map keycodes to the indecies.
			// Some keycodes are bigger than 2 ^ 30 for some reason...

			return (int) code >= 1 << 30 ? (int) code - 0x3FFFFF81 : (int) code;
		}

		static public bool checkButton(uint button) {
			return buttonsCur[(int) button];
		}

		// Check button if it was pressed or not (rising edge)

		static public bool checkButtonPressed(uint button) {
			return buttonsCur[(int) button] && !buttonsLast[(int) button];
		}

		// Check button if it was released or not (falling edge)

		static public bool checkButtonReleased(uint button) {
			return !buttonsCur[(int) button] && buttonsLast[(int) button];
		}

		static public void setMousePos(int x, int y) {
			SDL.SDL_WarpMouseInWindow(wnd, x, y);
		}
		
		static public void dispose() {
			if (!disposed) {
				SDL.SDL_GL_DeleteContext(con);
				con = IntPtr.Zero;

				SDL.SDL_DestroyWindow(wnd);
				wnd = IntPtr.Zero;

				SDL2Helper.quit();

				disposed = true;
			}
		}
		
		static public void errorMessageBox(string msg, string title) {
			SDL.SDL_ShowSimpleMessageBox(SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR, title, msg, IntPtr.Zero);
		}

		static public void close() {
			var ev = new SDL.SDL_Event() { type = SDL.SDL_EventType.SDL_QUIT };
			SDL.SDL_PushEvent(ref ev);
		}

		static public void setSize(int width, int height) {
			SDL.SDL_SetWindowSize(wnd, Window.width = width, Window.height = height);
		}

		// Iterate over all display modes of a display

		static public IEnumerable<DisplayMode> displayModes(int display) {
			var num  = SDL.SDL_GetNumDisplayModes(display);
			var list = new List<DisplayMode>();

			for (var i = 0; i < num; ++i) {
				SDL.SDL_DisplayMode mode;

				SDL.SDL_GetDisplayMode(display, i, out mode);
				
				var dispMode = new DisplayMode() { width = mode.w, height = mode.h };

				if (list.FindIndex(d => d.width == dispMode.width && d.height == dispMode.height) < 0)
					list.Add(dispMode);
			}

			list.Sort((x, y) => x.width == y.width ? x.height - y.height : x.width - y.width);

			return list;
        }

		// Set frequency of main loop (how often per second update and render functions are called)

		static public double frequency {
			get {
				return 1 / periodeTime.TotalSeconds;
			}	

			set {
				periodeTime = TimeSpan.FromSeconds(1 / value);
			}
		}

		static public bool fullscreen {
			get {
				return (SDL.SDL_GetWindowFlags(wnd) & (uint) SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN) != 0;
            }

			set {
				if (value)
					SDL.SDL_SetWindowFullscreen(wnd, (uint) SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN);
				else
					SDL.SDL_SetWindowFullscreen(wnd, 0);
			}
		}

		static public bool cursorVisible {
			get {
				return SDL.SDL_ShowCursor(-1) != 0 ? true : false;
			}

			set {
				SDL.SDL_ShowCursor(value ? 1 : 0);
			}
		}

		// Enable or disable mouse locking
		// Only realtive coordinates are stored and the cursor is bound to the window

		static public bool mouseLocked {
			get {
				return mouseLockedHidden;
			}

			set {
				SDL.SDL_SetRelativeMouseMode((mouseLockedHidden = value) ? SDL.SDL_bool.SDL_TRUE : SDL.SDL_bool.SDL_FALSE);

				if (!value)
					setMousePos(mouseX, mouseY);
			}
		}

		static public int width { get; private set; }
		static public int height { get; private set; }

		static public int mouseX { get; private set; }
		static public int mouseY { get; private set; }

		static public int wheelX { get; private set; }
		static public int wheelY { get; private set; }

		static public int relMouseX { get; private set; }
		static public int relMouseY { get; private set; }

		static bool mouseLockedHidden;

		static bool disposed = false;
		
		static public bool limitFPS = true;

		static public event EventHandler<UpdateEvent> eventUpdate;
		static public event EventHandler<RenderEvent> eventRender;

		static IntPtr wnd;
		static IntPtr con;

		static TimeSpan periodeTime = TimeSpan.FromSeconds(1.0 / 60.0);

		static bool keysHaveChanged;

		static BitArray keysCur  = new BitArray(0x200);
		static BitArray keysLast = new BitArray(0x200);

		static bool buttonsHaveChanged;

		static BitArray buttonsCur  = new BitArray(0x04);
		static BitArray buttonsLast = new BitArray(0x04);
	}
}
