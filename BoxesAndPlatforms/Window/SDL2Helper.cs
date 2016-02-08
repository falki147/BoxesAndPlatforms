using SDL2;

namespace BoxesAndPlatforms {
	// Handles SDL2 initialization and freeing

	static class SDL2Helper {
		static public void init(uint flags) {
			SDL.SDL_Init(flags);
			++refCount;
		}

		static public void quit() {
			if (--refCount == 0)
				SDL.SDL_Quit();
		}

		static int refCount = 0;
	}
}
