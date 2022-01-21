using System;
using CommandLine;
using System.IO;
using NMotion.Cdi.Audio;

namespace NMotion.Cdi.Tools.AdpcmEdit {
	class Program {

		public class Options {
			[Value(0, MetaName = "Input", Required = true, HelpText = "Path of the ADPCM audio (ACM) file to be processed.")]
			public string InputPath { get; private set; }

			[Value(1, MetaName = "Output", Required = true, HelpText = "Path of the output file.")]
			public string OutputPath { get; private set; }

			[Option('h', "header", HelpText = "Write AIFF header.")]
			public bool Header { get; private set; }

			[Option('k', "kill20", HelpText = "Run Kill20 (Strip out CD block alignment bytes).")]
			public bool Kill20 { get; private set; }

			[Option('l', "left", HelpText = "Write Left channel only.")]
			public bool LeftOnly { get; private set; }

			[Option('r', "right", HelpText = "Write Right channel only.")]
			public bool RightOnly { get; private set; }
			
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

			AdpcmTrack track = AdpcmTrack.FromFile(options.InputPath);

			Console.WriteLine($"Number of blocks: {track.BlockCount}");

			if (options.LeftOnly)  track.MuteRight();
			if (options.RightOnly) track.MuteLeft();

			try {
				using var stream = File.OpenWrite(options.OutputPath);
				track.ToStream(stream, writeHeader: options.Header, writeBlockPadding: !options.Kill20);
			}
			catch (Exception e) {
				Console.WriteLine("Error: Cannot write output file: {0}", e.Message);
				return;
			}
		}
	}
}
