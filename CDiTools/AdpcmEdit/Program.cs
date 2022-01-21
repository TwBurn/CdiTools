using System;
using CommandLine;
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
			public bool MuteLeft { get; private set; }

			[Option('r', "right", HelpText = "Write Right channel only.")]
			public bool MuteRight { get; private set; }
			/*
			[Option('m', "mode", Default = OutputMode.p, HelpText = "Output Mode:\n\tp = Binary Palette\n\ta = Plane A\n\tb = Plane B\n\tc = C Code\n\tj = JSON")]
			public OutputMode Mode { get; private set; }
			*/
			
		}
		static void Main(string[] args) {
			AdpcmTrack track = AdpcmTrack.FromFile(@"\\SESHADRI\c\Temp\GWC.ACM");
		}
	}
}
