using System;
using System.IO;
using CommandLine;
using System.Collections.Generic;
using NMotion.Cdi.Graphics;

namespace NMotion.Cdi.Tools.ImageToClut {
	class Program {
		public class Options {
			[Value(0, MetaName = "Input", Required = true, HelpText = "Path of the image file to be processed.")]
			public string InputPath { get; private set; }

			[Value(1, MetaName = "Output", Required = true, HelpText = "Path of the output file.")]
			public string OutputPath { get; private set; }

			[Option('p', "palette", Required = true, HelpText = "Path of palette file to use.")]
			public string PalettePath { get; private set; }
			/*
			[Option('m', "mode", Default = OutputMode.p, HelpText = "Output Mode:\n\tp = Binary Palette\n\ta = Plane A\n\tb = Plane B\n\tc = C Code\n\tj = JSON")]
			public OutputMode Mode { get; private set; }
			*/

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

			if (palette.Colors.Length > 128) {
				Console.WriteLine("Error: Input palette has too many colors");
			}

			var clutImage = ClutImage.FromRawImage(rawImage, palette);

			try {
				using var stream = File.OpenWrite(options.OutputPath);
				clutImage.ToStream(stream, ClutFormat.Clut7);
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
