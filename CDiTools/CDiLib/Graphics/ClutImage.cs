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
