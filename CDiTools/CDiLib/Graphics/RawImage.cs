using System.Collections.Generic;
using System.Drawing;

namespace NMotion.Cdi.Graphics {
	public class RawImage {
		public Color[,] PixelData { get; private set; }
		public int Width { get; private set; }
		public int Height { get; private set; }

		public static RawImage FromImage(Image image) {
			var bitmap = new Bitmap(image);
			var pixels = new Color[bitmap.Width, bitmap.Height];

			for (int y = 0; y < bitmap.Height; y++) {
				for (int x = 0; x < bitmap.Width; x++) {
					pixels[x, y] = Color.FromColor(bitmap.GetPixel(x, y));
				}
			}

			return new RawImage() {
				PixelData = pixels,
				Width = bitmap.Width,
				Height = bitmap.Height
			};
		}

		public IEnumerable<Color> Pixels() {
			for (int y = 0; y < Height; y++) {
				for (int x = 0; x < Width; x++) {
					yield return PixelData[x, y];
				}
			}
		}

		public static RawImage FromClutImage(ClutImage image) {
			var pixels = new Color[image.Width, image.Height];

			for (int y = 0; y < image.Height; y++) {
				for (int x = 0; x < image.Width; x++) {
					pixels[x, y] = image.Palette.Colors[image.PixelData[x, y]];
				}
			}

			return new RawImage() {
				PixelData = pixels,
				Width = image.Width,
				Height = image.Height
			};
		}

		public Bitmap ToBitmap() {
			var bitmap = new Bitmap(Width, Height);
			for (int y = 0; y < bitmap.Height; y++) {
				for (int x = 0; x < bitmap.Width; x++) {
					bitmap.SetPixel(x, y, PixelData[x, y].ToColor());
				}
			}

			return bitmap;
		}
	}
}
