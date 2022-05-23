using System;
using CommandLine;
using System.IO;
using NMotion.Cdi.Audio;

namespace NMotion.Cdi.Tools {
	class AdpcmEdit {

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
			/*
			using var os = File.OpenWrite(@"c:\temp\audio\test.pcm");
			byte[] header = { 0x46, 0x4F, 0x52, 0x4D, 0x00, 0x98, 0x11, 0xC2, 0x41, 0x49, 0x46, 0x46, 0x43, 0x4F, 0x4D, 0x4D, 0x00, 0x00, 0x00, 0x12, 0x00, 0x02, 0x00, 0x26, 0x04, 0x65, 0x00, 0x10, 0x40, 0x0D, 0x93, 0xA8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x53, 0x53, 0x4E, 0x44, 0x00, 0x98, 0x11, 0x9C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x09, 0x14 };
			os.Write(header, 0, header.Length);
			foreach (var sg in track.SoundGroups) {
				var dec = AdpcmDecoder.Decode(sg, track.Channels == 2);
				foreach (var sample in dec) {
					//var bytes = BitConverter.GetBytes(sample);
					os.WriteByte((byte)(sample >> 8));
					os.WriteByte((byte)(sample & 0xFF));
				}
			}
			os.Close();
			*/
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
