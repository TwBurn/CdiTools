using System;
using NMotion.Cdi.Graphics;
using CommandLine;
using System.IO;

namespace NMotion.Cdi.Tools {
	class PaletteConvert {

		public enum OutputMode {
			p,
			a,
			b,
			c,
			j
		}

		public class Options {
			[Value(0, MetaName = "InputPath", Required = true, HelpText = "Path of the input palette file to be processed.")]
			public string InputPath { get; private set; }

			[Value(1, MetaName = "OutputPath", HelpText = "Path of the output file.")]
			public string OutputPath { get; private set; }

			[Option('f', "fill", Default = false, HelpText = "Fill palette to 128 colors.")]
			public bool FillPalette { get; private set; }

			[Option('m', "mode", Default = OutputMode.p, HelpText = "Output Mode:\n\tp = Binary Palette\n\ta = Plane A\n\tb = Plane B\n\tc = C Code\n\tj = JSON")]
			public OutputMode Mode { get; private set; }

			[Option('s', "scan", HelpText = "Scan InputPath as binary file for palettes")]
			public bool Scan { get; private set; }

			[Option('e', "extract", HelpText = "Extract Palette # from binary file InputPath")]
			public int? Extract { get; private set; }

		}
		static void Main(string[] args) {
			Parser.Default.ParseArguments<Options>(args)
					.WithParsed(Execute);
		}

		static void Execute(Options options) {
			if (!File.Exists(options.InputPath)) {
				Console.WriteLine($"Error: Input file '{options.InputPath}' does not exist.");
				return;
			}

			Palette palette;
			if (options.Scan) { 
				int count = ExtractPalette(options.InputPath, -1, out _);
				if (count > 0) {
					Console.WriteLine($"{Path.GetFileName(options.InputPath)}: {count} palettes found. Use option '-e <id>' for extraction.");
				}
				else {
					Console.WriteLine($"{Path.GetFileName(options.InputPath)}: No palettes found.");
				}
				return;
			}
			else if (options.Extract.HasValue) {
				int count = ExtractPalette(options.InputPath, options.Extract.Value, out palette);

				if (palette == null) {
					throw new Exception($"Invalid Palette Number - valid values are 0-{count}.");
				}
			}
			else {
				palette = Palette.FromFile(options.InputPath);
			}

			var mode = MapMode(options.Mode);
			

			if (palette.Colors.Length < 128 && options.FillPalette) {
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

		static int ExtractPalette(string filename, int extractId, out Palette palette) {
			var data = File.ReadAllBytes(filename);
			int count = 0;

			int position = 0;
			palette = null;

			while (position < (data.Length - 520)) {
				if (data[position] == 0xC3 && data[position + 1] == 0x00 && data[position + 2] == 0x00 && data[position + 3] < 4 &&
					data[position + 260] == 0xC3 && data[position + 261] == 0x00 && data[position + 262] == 0x00 && data[position + 263] < 4) {
					count++;

					if (count == extractId) {
						MemoryStream buffer = new MemoryStream();
						buffer.Write(data, position, 520);
						buffer.Seek(0, SeekOrigin.Begin);
						palette = Palette.FromStream(buffer);
						return -1;
					}
					position += 520;
				}
				else {
					position++;
				}
			}

			return count;
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
	}
}
