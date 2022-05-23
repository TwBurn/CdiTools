using System;
using System.IO;
using CommandLine;
using System.Collections.Generic;
using NMotion.Cdi.Graphics;

namespace NMotion.Cdi.Tools {
	class ClutToImage {
		public class Options {
			[Value(0, MetaName = "Input", Required = true, HelpText = "Path of the CLUT file to be processed.")]
			public string InputPath { get; private set; }

			[Value(1, MetaName = "Output", Required = true, HelpText = "Path of the output file (PNG).")]
			public string OutputPath { get; private set; }

			[Option('p', "palette", Required = true, HelpText = "Path of palette file to use.")]
			public string PalettePath { get; private set; }

			[Option('w', "width", Required = true, HelpText ="Image Width")]
			public int Width { get; private set; }

			[Option('h', "height", Required = false, HelpText = "Image Max. Height")]
			public int Height { get; private set; } = -1;

			[Option('s', "skip", Default = 0 , HelpText = "Number of bytes to skip.")]
			public int SkipBytes { get; private set; }

			[Option('f', "format", Default = ClutFormat.Clut7, HelpText = "Input Format: Clut4, Clut7, Clut8, Rle4, Rle7")]
			public ClutFormat Format { get; private set; }


		}
		static void Main(string[] args) {
			Parser.Default.ParseArguments<Options>(args)
				.WithParsed(Execute);
		}

		static void Execute(Options options) {
			if (!File.Exists(options.InputPath)) {
				Console.WriteLine("Error: Input file '{0}' does not exist.", options.InputPath);
				return;
			}

			if (!File.Exists(options.PalettePath)) {
				Console.WriteLine("Error: Palette file '{0}' does not exist.", options.PalettePath);
				return;
			}

			var palette = Palette.FromFile(options.PalettePath);
			var clutImage = ClutImage.FromFile(options.InputPath, options.Width, palette, options.Format, options.SkipBytes, options.Height);
			var rawImage = RawImage.FromClutImage(clutImage);
			var bitmap = rawImage.ToBitmap();
			bitmap.Save(options.OutputPath, System.Drawing.Imaging.ImageFormat.Png);
		}
	}
}
