using System;
using System.Collections.Generic;
using System.IO;

namespace NMotion.Cdi.Graphics {

	public enum ClutFormat {
		Clut8,
		Clut7,
		Clut4,
		Rle7,
		Rle4
	}
	public class ClutImage {
		public byte[,] PixelData { get; private set; }
		public Palette Palette { get; private set; }

		public int Width { get; private set; }
		public int Height { get; private set; }

		public static ClutImage FromRawImage(RawImage image, Palette palette) {
			Dictionary<Color, byte> colorMap = new();

			var pixels = new byte[image.Width, image.Height];

			for (var y = 0; y < image.Height; y++) {
				for (var x = 0; x < image.Width; x++) {
					var color = image.PixelData[x, y];
					if (!colorMap.TryGetValue(color, out byte index)) {
						index = (byte)palette.MatchColor(color);
						colorMap[color] = index;
					}
					pixels[x, y] = index;
				}
			}

			return new ClutImage() {
				PixelData = pixels,
				Palette = palette,
				Width = image.Width,
				Height = image.Height
			};
		}

		private RleEntry[] GetRleLine(int line) {
			List<RleEntry> rleLine = new();

			RleEntry lastEntry = new(PixelData[0, line]);
			rleLine.Add(lastEntry);
			for (var x = 1; x < Width - 1; x++) {
				var color = PixelData[x, line];
				if (color == lastEntry.Color && lastEntry.Count < 255) {
					lastEntry.Count++;
				}
				else {
					lastEntry = new(color);
					rleLine.Add(lastEntry);
				}
			}
			
			lastEntry.Count = 0;
			while (rleLine.Count > 1 && rleLine[^2].Color == lastEntry.Color) {
				rleLine.RemoveAt(rleLine.Count - 2);
			}

			return rleLine.ToArray();
		}

		public static ClutImage FromFile(string inputPath, int width, Palette palette, ClutFormat format, int skipBytes = 0, int maxHeight = 0) {
			using var fs = File.OpenRead(inputPath);
			fs.Seek(skipBytes, SeekOrigin.Begin);

			int length = (int)fs.Length - skipBytes;
			byte[] data = new byte[length];

			fs.Read(data, 0, length);

			byte[,] pixels = format switch {
				ClutFormat.Clut7 => GetClut7PixelData(data, width, maxHeight),
				ClutFormat.Rle7  => GetRle7PixelData(data, width, maxHeight),
				_ => throw new NotImplementedException(),
			};

			return new ClutImage() {
				PixelData = pixels,
				Palette = palette,
				Width = width,
				Height = pixels.GetLength(1)
			};
		}

		private static byte[,] GetClut7PixelData(byte[] data, int width, int maxHeight) {
			int height = data.Length / width;
			if (maxHeight > 0 && maxHeight < height) height = maxHeight;

			byte[,] pixels = new byte[width, height];
			for (var y = 0; y < height; y++) {
				for (var x = 0; x <width; x++) {
					pixels[x, y] = (byte)(data[width * y + x] & 0x7f);
				}
			}
			return pixels;
		}

		private static byte[,] GetRle7PixelData(byte[] data, int width, int maxHeight) {
			List<byte[]> lines = GetRle7PixelLines(data, width, maxHeight);

			int height = lines.Count;
			byte[,] pixels = new byte[width, height];
			for (var y = 0; y < height; y++) {
				for (var x = 0; x < width; x++) {
					pixels[x, y] = lines[y][x];
				}
			}
			return pixels;
		}

		private static List<byte[]> GetRle7PixelLines(byte[] data, int width, int maxHeight) {
			List<byte[]> lines = new();

			int size = data.Length;
			int index = 0; /* Data Index */
			int lineNumber = 0; /* Lines Read */

			while (index < data.Length) {
				var x = 0;
				var line = new byte[width];
				while (x < width) {
					if (size == index) return lines;
					var color = data[index]; index++;

					if (color >= 0x80) {
						/* RLE - color in lower 7 bits */
						if (size == index) return lines;
						int count = data[index]; index++;

						if (count == 0) count = width - x;

						while (count > 0) {
							line[x] = (byte)(color & 0x7f); x++;
							count--;
						}
					}
					else {
						line[x] = color; x++;
					}
				}

				lines.Add(line);
				lineNumber++;

				if (lineNumber == maxHeight) return lines;
			}
			return lines;
		}


		public void ToStream(Stream stream, ClutFormat format) {
			switch (format) {
				case ClutFormat.Clut7:
					ToClut7Stream(stream);
					break;
				case ClutFormat.Rle7:
					ToRle7Stream(stream);
					break;
				default:
					throw new NotImplementedException();
			}
		}

		public void Validate(ClutFormat format) {
			switch(format) {
				case ClutFormat.Clut7:
				case ClutFormat.Rle7:
					if (Palette.Colors.Length > 128) {
						throw new ArithmeticException("Clut7 images cannot exceed 128 colors");
					}
					break;
				default:
					throw new NotImplementedException();
			}
		}

		public void ToClut7Stream(Stream stream) {
			for (var y = 0; y < Height; y++) {
				for (var x = 0; x < Width; x++) {
					stream.WriteByte(PixelData[x, y]);
				}
			}
		}

		public void ToRle7Stream(Stream stream) {
			for (var y = 0; y < Height; y++) {
				var line = GetRleLine(y);
				foreach (var entry in line) {
					if (entry.Count == 1) {
						stream.WriteByte(entry.Color);
					}
					else {
						stream.WriteByte((byte)(entry.Color | 0x80));
						stream.WriteByte((byte)entry.Count);
					}
				}
			}
		}
	}
}
