using System;
using System.Collections.Generic;
using System.IO;

namespace NMotion.Cdi.Graphics {

	public enum ClutFormat {
		Clut8,
		Clut7,
		Clut4,
		RLE7,
		RLE4
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


		public void ToStream(Stream stream, ClutFormat format) {
			switch (format) {
				case ClutFormat.Clut7:
					ToClut7Stream(stream);
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
	}
}
