using System;
using System.Collections.Generic;

namespace BoxesAndPlatforms {
	// Keeps track of the current room and reroutes update and render functions

	static public class RoomManager {
		public abstract class Room: IDisposable {
			~Room() {
				if (!disposed)
					System.Diagnostics.Debug.Fail("RoomManager was not disposed of properly.");

				Dispose(false);
			}

			public abstract void free();
			public abstract void update();
			public abstract void render();
			public abstract void enter();
			public abstract void leave();

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

			bool disposed = false;
		}

		static public void gotoRoom(Room room) {
			if (rooms.Count > 0)
				rooms.Peek().leave();

			rooms.Push(room);
			room.enter();
		}

		static public void leaveRoom() {
			if (rooms.Count > 0) {
				rooms.Peek().leave();
				rooms.Pop().Dispose();

				if (rooms.Count > 0)
					rooms.Peek().enter();
			}
		}

		static public void render(object sender, Window.RenderEvent ev) {
			if (rooms.Count > 0)
				rooms.Peek().render();
		}

		static public void update(object sender, Window.UpdateEvent ev) {
			if (rooms.Count > 0)
				rooms.Peek().update();
		}

		static public void dispose() {
			if (!disposed) {
				disposed = true;

				while (rooms.Count > 0)
					leaveRoom();
			}
		}
		
		static bool disposed = false;
		
		static Stack<Room> rooms = new Stack<Room>();
	}
}
