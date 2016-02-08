using OpenGL;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace BoxesAndPlatforms {
	// Stores a CubeMap

	public class CubeMap: IDisposable {
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct PixelFormat {
			public int size;
			public int flags;
			public string fourCC { get { return new string(new char[] { (char) fourCC0, (char) fourCC1, (char) fourCC2, (char) fourCC3 }); } }
			public byte fourCC0;
			public byte fourCC1;
			public byte fourCC2;
			public byte fourCC3;
			public int RGBBitCount;
			public uint RBitMask;
			public uint GBitMask;
			public uint BBitMask;
			public uint ABitMask;

			public const int structSize = 32;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct Header {
			[Flags]
			public enum Flags: int {
				Caps        = 0x1,
				Height      = 0x2,
				Width       = 0x4,
				Pitch       = 0x8,
				PixelFormat = 0x1000,
				MipMapCount = 0x20000,
				LinearSize  = 0x80000,
				Depth       = 0x800000
			}

			[Flags]
			public enum SurfaceFlags: int {
				Complex = 0x8,
				MipMap  = 0x400000,
				Texture = 0x1000
			}

			[Flags]
			public enum ComplexFlags: int {
				CubeMap          = 0x200,
				CubeMapPositiveX = 0x400,
				CubeMapNegativeX = 0x800,
				CubeMapPositiveY = 0x1000,
				CubeMapNegativeY = 0x2000,
				CubeMapPositiveZ = 0x4000,
				CubeMapNegativeZ = 0x8000,
				Volume           = 0x200000
			}

			public int size;
			public Flags flags;
			public int height;
			public int width;
			public int linearSize;
			public int depth;
			public int mipmapCount;
			private int reserved0;
			private int reserved1;
			private int reserved2;
			private int reserved3;
			private int reserved4;
			private int reserved5;
			private int reserved6;
			private int reserved7;
			private int reserved8;
			private int reserved9;
			private int reserved10;
			public PixelFormat pixelFormat;
			public SurfaceFlags surfaceFlags;
			public ComplexFlags complexFlags;
			private int reserved11;
			private int reserved12;
			private int reserved13;

			public const int structSize = 124;

			public static Header fromBytes(byte[] data) {
				var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
				var desc = Marshal.PtrToStructure<Header>(handle.AddrOfPinnedObject());
				handle.Free();
				return desc;
			}
		}

		public CubeMap() { }

		public CubeMap(string file) {
			try {
				load(file);
			}
			catch (Exception) {
				throw;
			}
			finally {
				cleanup();
			}
		}

		~CubeMap() {
			if (!disposed)
				System.Diagnostics.Debug.Fail("CubeMap was not disposed of properly.");

			Dispose(false);
		}

		public void load(string file) {
			load(new FileStream(file, FileMode.Open));
		}

		public void load(byte[] data) {
			load(new MemoryStream(data));
		}

		// Parse a CubeMap stored as an DDS file from a stream

		public void load(Stream stream) {
			cleanup();

			var reader = new BinaryReader(stream);

			if (new string(reader.ReadChars(4)) != "DDS ")
				throw new Exception("Invalid DDS file (Invalid Magic Number)");

			var desc = Header.fromBytes(reader.ReadBytes(Header.structSize));
			
			const Header.ComplexFlags reqFlags = Header.ComplexFlags.CubeMap |
				Header.ComplexFlags.CubeMapNegativeX | Header.ComplexFlags.CubeMapNegativeY |
				Header.ComplexFlags.CubeMapNegativeZ | Header.ComplexFlags.CubeMapPositiveX |
				Header.ComplexFlags.CubeMapPositiveY | Header.ComplexFlags.CubeMapPositiveZ;

			if ((desc.complexFlags & reqFlags) != reqFlags)
				throw new Exception("Invalid CubeMap");
			
			PixelInternalFormat fmt;

			int factor, size;

			switch (desc.pixelFormat.fourCC) {
			case "DXT1":
				fmt    = PixelInternalFormat.CompressedRgbaS3tcDxt1Ext;
				factor = 2;
				size   = 8;
				break;
			case "DXT3":
				fmt    = PixelInternalFormat.CompressedRgbaS3tcDxt3Ext;
				factor = 4;
				size   = 16;
				break;
			case "DXT5":
				fmt    = PixelInternalFormat.CompressedRgbaS3tcDxt5Ext;
				factor = 4;
				size   = 16;
				break;
			default:
				throw new Exception("File compression is not supported");
			}

			var data = reader.ReadBytes(desc.linearSize != 0 ? (desc.mipmapCount > 1 ? desc.linearSize * factor : desc.linearSize) :
				(int) (reader.BaseStream.Length - reader.BaseStream.Position));

			var handle = GCHandle.Alloc(data, GCHandleType.Pinned);

			try {
				texture = Gl.GenTexture();
				bind();

				// Set texture filtering

				Gl.TexParameteri(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, TextureParameter.Linear);
				Gl.TexParameteri(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, TextureParameter.Linear);
				Gl.TexParameteri(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, TextureParameter.ClampToEdge);
				Gl.TexParameteri(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, TextureParameter.ClampToEdge);
				Gl.TexParameteri(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, TextureParameter.ClampToEdge);

				// Go through every texture and it's mipmaps and upload them to the GPU

				var offset = 0;

				for (var i = 0; i < 6; ++i) {
					var width  = desc.width;
					var height = desc.height;

					for (var j = 0; j < Math.Max(1, desc.mipmapCount); ++j) {
						width  = Math.Max(1, width);
						height = Math.Max(1, height);
						
						var texSize = ((width + 3) / 4) * ((height + 3) / 4) * size;
						
						Gl.CompressedTexImage2D(TextureTarget.TextureCubeMapPositiveX + i, j, fmt, width, height, 0, texSize, (IntPtr) (handle.AddrOfPinnedObject().ToInt64() + offset));
						
						offset += texSize;

						width  /= 2;
						height /= 2;
					}
				}
			}
			catch (Exception) {
				throw;
			}
			finally {
				handle.Free();
			}
		}

		public void bind() {
			Gl.BindTexture(TextureTarget.TextureCubeMap, texture);
		}

		void cleanup() {
			if (texture == 0) {
				Gl.DeleteTextures(1, new uint[] { texture });
				texture = 0;
			}
		}

		void Dispose(bool disposing) {
			if (!disposed) {
				disposed = true;

				if (disposing)
					cleanup();
			}
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		
		bool disposed;

		public uint texture { get; private set; }
	}
}
