using System;
using System.Collections.Generic;
using NMotion.Cdi.Graphics;
using System.IO;
using CommandLine;

namespace NMotion.Cdi.Tools.ImgToPalette {
	class Program {

		public enum OutputMode {
			p,
			a,
			b,
			c,
			j
		}

		public class Options {
			[Value(0, MetaName = "InputPath", Required = true, HelpText = "Path of the image file to be processed.")]
			public string InputPath { get; private set; }

			[Value(1, MetaName = "OutputPath", Required = true, HelpText = "Path of the output file.")]
			public string OutputPath { get; private set; }

			[Option('f', "fill", Default = false, HelpText = "Fill palette to 128 colors.")]
			public bool FillPalette { get; private set; }

			[Option('m', "mode", Default = OutputMode.p, HelpText = "Output Mode:\n\tp = Binary Palette\n\ta = Plane A\n\tb = Plane B\n\tc = C Code\n\tj = JSON")]
			public OutputMode Mode { get; private set; }

		}

		static void Main(string[] args) {
			//var ri = RawImage.FromImage(System.Drawing.Image.FromFile(@"C:\Temp\graphics\CDiGame\Colors.png"));
			//var p = Palette.FromRawImage(ri);
			Parser.Default.ParseArguments<Options>(args)
				.WithParsed(Execute)
				.WithNotParsed(HandleParseError);


		}

		static void Execute(Options options) {
			if (!File.Exists(options.InputPath)) {
				Console.WriteLine("Error: Input file '{0}' does not exist.", options.InputPath);
				return;
			}

			var mode = MapMode(options.Mode);
			var image = RawImage.FromImage(System.Drawing.Image.FromFile(options.InputPath));
			var palette = Palette.FromRawImage(image);
			

			if (palette.Colors.Length > 128) {
				Console.WriteLine("Error: Input image has too many colors");
			}
			else if (palette.Colors.Length < 128 && options.FillPalette) {
				var colors = new Color[128];
				for (var i = 0; i < palette.Colors.Length; i++) {
					colors[i] = palette.Colors[i];
				}
				for (var i = palette.Colors.Length; i < 128; i++) {
					colors[i] = Color.Transparent;
				}
				palette = new Palette(colors);
			}

			try {
				using var stream = File.OpenWrite(options.OutputPath);
				palette.ToStream(stream, mode);
			}
			catch (Exception e) {
				Console.WriteLine("Error: Cannot write output file: {0}", e.Message);
				return;
			}
		}

		static PaletteFormat MapMode(OutputMode mode) {
			return mode switch {
				OutputMode.p => PaletteFormat.Binary,
				OutputMode.a => PaletteFormat.PlaneA,
				OutputMode.b => PaletteFormat.PlaneB,
				OutputMode.c => PaletteFormat.Code,
				OutputMode.j => PaletteFormat.Json,
				_ => throw new Exception(string.Format("Invalid Mode: {0}", mode)),
			};
		}

		static void HandleParseError(IEnumerable<Error> errs) {

		}
	}
}
