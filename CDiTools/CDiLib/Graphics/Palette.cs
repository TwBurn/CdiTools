using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NMotion.Cdi.Graphics {

	public enum PaletteFormat {
		Binary,
		PlaneA,
		PlaneB,
		Code,
		Json
	}
	public class Palette {
		public Color[] Colors { get; private set; }

		public Palette(Color[] colors) {
			Colors = colors;
		}
		public static Palette FromRawImage(RawImage image) {
			var colorList = new List<Color>();
			var colorSet = new HashSet<Color>();

			colorList.Add(Color.Transparent);
			colorSet.Add(Color.Transparent);

			foreach (var color in image.Pixels()) {
				if (colorSet.Add(color)) {
					colorList.Add(color);
				}
			}

			return new Palette(colorList.ToArray());
		}


		public static Palette FromGrid(RawImage image, int gridSize) {
			var colorList = new List<Color>();
			for (var y = 0; y < image.Height; y += gridSize) {
				for (var x = 0; x < image.Width; x += gridSize) {
					colorList.Add(image.PixelData[x, y]);
				}
			}
			return new Palette(colorList.ToArray());
		}

		public static Palette FromFile(string inputPath) {
			using var paletteStream = File.OpenRead(inputPath);
			return FromStream(paletteStream);
		}

		public static Palette FromStream(Stream stream) {
			var firstByte = stream.ReadByte();
			stream.Seek(0, SeekOrigin.Begin);
			return firstByte switch {
				0x00 or 0xC3 => FromBinaryStream(stream),
				'[' or ' ' or '\t' => FromJsonStream(stream),
				_ => throw new Exception("Invalid format for palette"),
			};
		}

		private static Palette FromJsonStream(Stream stream) {
			using StreamReader reader = new(stream);
			var text = reader.ReadToEnd();
			var colors = JsonConvert.DeserializeObject<Color[]>(text);
			return new Palette(colors);
		}

		private static Palette FromBinaryStream(Stream stream) {
			var buffer = new byte[4];		
			var colors = new List<Color>();

			while(stream.Read(buffer, 0, 4) == 4) {
				if (buffer[0] == 0xC3) continue; //Skip Plane header
				colors.Add(new Color(buffer[1], buffer[2], buffer[3]));
			}

			return new Palette(colors.ToArray());
		}

		

		public int MatchColor(Color color) {
			if (color.IsTransparent) return 0;

			int minDelta = int.MaxValue;
			int minIndex = 0;

			for (var i = 0; i < Colors.Length; i++) {
				var delta = color.ColorDistance(Colors[i]);
				if (delta < minDelta) {
					minDelta = delta;
					minIndex = i;
				}
			}

			return minIndex;
		}

		public void ToStream(Stream stream, PaletteFormat format) {
			switch(format) {
				case PaletteFormat.Code:
					ToCodeStream(stream);
					break;
				case PaletteFormat.Json:
					ToJsonStream(stream);
					break;
				default:
					ToBinaryStream(stream, format);
					break;
			}
		}

		private void ToCodeStream(Stream stream) {
			throw new NotImplementedException();
		}

		private void ToJsonStream(Stream stream) {
			using var output = new StreamWriter(stream); output.WriteLine("[");
			for (var i = 0; i < Colors.Length; i++) {
				output.WriteLine(
					"\t{0}{1}",
					Colors[i].ToJson(),
					i == Colors.Length - 1 ? "" : ","
				);
			}
			output.WriteLine("]");
		}

		private void ToBinaryStream(Stream stream, PaletteFormat format) {
			for (var i = 0; i < Colors.Length; i++) {

				//Write header
				if (i % 64 == 0 && format != PaletteFormat.Binary) {
					stream.WriteByte(0xC3);
					stream.WriteByte(0x00);
					stream.WriteByte(0x00);
					stream.WriteByte((byte)(i / 64 + (format == PaletteFormat.PlaneB ? 2 : 0)));
				}

				if (format == PaletteFormat.Binary) {
					stream.WriteByte(0x00);
				}
				else {
					stream.WriteByte((byte)(0x80 + i % 64));
				}
				stream.WriteByte(Colors[i].R);
				stream.WriteByte(Colors[i].G);
				stream.WriteByte(Colors[i].B);
			}
		}
	}
}

