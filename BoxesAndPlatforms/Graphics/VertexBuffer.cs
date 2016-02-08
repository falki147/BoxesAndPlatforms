using OpenGL;
using System;
using System.Runtime.InteropServices;

namespace BoxesAndPlatforms {
	public class VertexBuffer<T>: IDisposable where T : struct {
		#region Constructors and Destructors

		public VertexBuffer() {
			buffer = Gl.GenBuffer();
		}
		
		public VertexBuffer(T[] data, BufferUsageHint usage = BufferUsageHint.StaticDraw): this() {
			bind();
			create(data, usage);
		}

		public VertexBuffer(ArraySegment<T> data, BufferUsageHint usage = BufferUsageHint.StaticDraw): this() {
			bind();
			create(data, usage);
		}

		public VertexBuffer(int size, BufferUsageHint usage = BufferUsageHint.StaticDraw): this() {
			bind();
			resize(size, usage);
		}

		~VertexBuffer() {
			if (!disposed)
				System.Diagnostics.Debug.Fail("VertexBuffer was not disposed of properly.");

			Dispose(false);
		}

		#endregion

		#region Functions

		public void bind(BufferTarget target = BufferTarget.ArrayBuffer) {
			Gl.BindBuffer(target, buffer);
		}
		
		public void create(T[] data, BufferUsageHint usage = BufferUsageHint.StaticDraw) {
			Gl.BufferData(BufferTarget.ArrayBuffer, data.Length * Marshal.SizeOf<T>(), data, usage);
			size = data.Length;
		}

		public void create(ArraySegment<T> data, BufferUsageHint usage = BufferUsageHint.StaticDraw) {
			var handle = GCHandle.Alloc(data.Array, GCHandleType.Pinned);

			try {
				Gl.BufferData(
					BufferTarget.ArrayBuffer,
					(IntPtr) (data.Count * Marshal.SizeOf<T>()),
					handle.AddrOfPinnedObject() + data.Offset * Marshal.SizeOf<T>(),
					usage
				);
			}
			finally {
				handle.Free();
			}

			size = data.Count;
		}

		public void copy(VertexBuffer<T> vb) {
			copy(vb, 0, 0, Math.Min(size, vb.size));
		}

		public void copy(VertexBuffer<T> vb, int size) {
			copy(vb, 0, 0, size);
		}

		public void copy(VertexBuffer<T> vb, int offs, int size) {
			copy(vb, offs, offs, size);
		}

		public void copy(VertexBuffer<T> vb, int destOffs, int srcOffs, int size) {
			bind(BufferTarget.CopyWriteBuffer);
			vb.bind(BufferTarget.CopyReadBuffer);

			Gl.CopyBufferSubData(
				BufferTarget.CopyReadBuffer,
				BufferTarget.CopyWriteBuffer,
				(IntPtr) (srcOffs * Marshal.SizeOf<T>()),
				(IntPtr) (destOffs * Marshal.SizeOf<T>()),
				(IntPtr) (size * Marshal.SizeOf<T>())
			);
		}
		
		// Map buffer so it can be accessed by this[] operator

		public void map(BufferAccess access = BufferAccess.WriteOnly) {
			map(BufferTarget.ArrayBuffer, access);
		}

		public void map(BufferTarget target, BufferAccess access) {
			mapped = Gl.MapBuffer(target, access);
		}
		
		public void mapRange(int offset, int length, BufferAccessMask access = BufferAccessMask.MapWriteBit) {
			mapRange(offset, length, BufferTarget.ArrayBuffer, access);
		}

		public void mapRange(int offset, int length, BufferTarget target, BufferAccessMask access) {
			mapped = Gl.MapBufferRange(
				target,
				(IntPtr) (offset * Marshal.SizeOf<T>()),
				(IntPtr) (length * Marshal.SizeOf<T>()),
				access
			);
		}

		// Resize buffer and loose data

		public void resize(int size, BufferUsageHint usage = BufferUsageHint.StaticDraw) {
			Gl.BufferData(BufferTarget.ArrayBuffer, (this.size = size) * Marshal.SizeOf<T>(), (T[]) null, usage);
		}

		// Resize buffer and copy data

		public void resizeCopy(int size, BufferUsageHint usage = BufferUsageHint.StaticDraw) {
			VertexBuffer<T> vb = new VertexBuffer<T>(size, usage);

			swap(vb);
			copy(vb);

			vb.Dispose();
		}

		public void swap(VertexBuffer<T> vb) {
			var tBuffer = vb.buffer;
			vb.buffer = buffer;
			buffer = tBuffer;

			var tSize = vb.size;
			vb.size = size;
			size = tSize;
		}

		public void unmap(BufferTarget target = BufferTarget.ArrayBuffer) {
			mapped = IntPtr.Zero;
			Gl.UnmapBuffer(target);
		}

		// Write to buffer without the use of map

		public void write(T[] data, int offset) {
			var handle = GCHandle.Alloc(data, GCHandleType.Pinned);

			try {
				Gl.BufferSubData(BufferTarget.ArrayBuffer, 
					(IntPtr) offset,
					(IntPtr) (data.Length * Marshal.SizeOf<T>()),
					handle.AddrOfPinnedObject()
				);
			}
			finally {
				handle.Free();
			}
		}

		public void write(ArraySegment<T> data, int offset) {
			var handle = GCHandle.Alloc(data.Array, GCHandleType.Pinned);

			try {
				Gl.BufferSubData(BufferTarget.ArrayBuffer,
					(IntPtr) offset,
					(IntPtr) (data.Count * Marshal.SizeOf<T>()),
					handle.AddrOfPinnedObject() + data.Offset * Marshal.SizeOf<T>()
				);
			}
			finally {
				handle.Free();
			}
		}

		void Dispose(bool disposing) {
			if (!disposed) {
				disposed = true;

				if (disposing) {
					if (buffer != 0) {
						Gl.DeleteBuffer(buffer);
						buffer = 0;
					}
				}
			}
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion

		#region Operators

		public T this[int index] {
			get {
				return Marshal.PtrToStructure<T>(mapped + index * Marshal.SizeOf<T>());
			}

			set {
				Marshal.StructureToPtr(value, mapped + index * Marshal.SizeOf<T>(), false);
			}
		}

		#endregion

		#region Variables

		bool disposed;

		public uint buffer { get; private set; }
		public int size { get; private set; }

		IntPtr mapped;

		#endregion
	}
}
