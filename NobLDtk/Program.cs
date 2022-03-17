using System;
using ldtk;
using System.IO;
using CommandLine;

using NMotion.Nobelia.Mapping;
namespace NMotion.Nobelia.NobLDtk {
	class Program {
		public class Options {
			[Value(0, MetaName = "InputPath", Required = true, HelpText = "Path of the World File (*.ldtk) to be processed.")]
			public string InputPath { get; private set; }

			[Value(1, MetaName = "OutputPath", Required = true, HelpText = "Path of the output file.")]
			public string OutputPath { get; private set; }

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

			var worldText = File.ReadAllText(options.InputPath);
			var worldData = LdtkJson.FromJson(worldText);

			var worldMapper = new WorldMapper(worldData);
			using var outputFile = File.Create(options.OutputPath);
			worldMapper.WriteWorld(outputFile);

			Console.WriteLine($"Conversion complete\r\n  Number of levels: {worldMapper.LevelCount}\r\n  Number of chests: {Mapping.Entities.Chest.ChestCount}");
		}
	}
}
