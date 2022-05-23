using System;
using System.IO;
using CommandLine;
using System.Collections.Generic;
using NMotion.Cdi.Graphics;

namespace NMotion.Cdi.Tools {
	class ImageToClut {
		public class Options {
			[Value(0, MetaName = "Input", Required = true, HelpText = "Path of the image file to be processed.")]
			public string InputPath { get; private set; }

			[Value(1, MetaName = "Output", Required = true, HelpText = "Path of the output file.")]
			public string OutputPath { get; private set; }

			[Option('p', "palette", Required = true, HelpText = "Path of palette file to use.")]
			public string PalettePath { get; private set; }

			[Option('a', "align", HelpText = "Fill output file to multiples of 2048 bytes.")]
			public bool Align { get; private set; }


		
			[Option('f', "format", Default = ClutFormat.Clut7, HelpText = "Output Format: Clut4, Clut7, Clut8, Rle4, Rle7")]
			public ClutFormat Format { get; private set; }


		}
		static void Main(string[] args) {
			Parser.Default.ParseArguments<Options>(args)
				.WithParsed(Execute)
				.WithNotParsed(HandleParseError);
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

			var rawImage = RawImage.FromImage(System.Drawing.Image.FromFile(options.InputPath));

			using var paletteStream = File.OpenRead(options.PalettePath);
			var palette = Palette.FromStream(paletteStream);	
			var clutImage = ClutImage.FromRawImage(rawImage, palette);
			clutImage.Validate(options.Format);

			try {
				using var stream = File.Create(options.OutputPath);
				clutImage.ToStream(stream, options.Format);
				if (options.Align) {
					var lastBlockSize = (2048 - (stream.Length % 2048)) % 2048;
					for (var i = 0; i < lastBlockSize; i++) {
						stream.WriteByte(0);
					}
				}
			}
			catch (Exception e) {
				Console.WriteLine("Error: Cannot write output file: {0}", e.Message);
				return;
			}
		}

		static void HandleParseError(IEnumerable<Error> errs) {

		}
	}
}
