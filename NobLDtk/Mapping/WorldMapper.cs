using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ldtk;

namespace NMotion.Nobelia.Mapping {
	public class WorldMapper {
		private readonly Dictionary<long, string> layerNames;
		private readonly Dictionary<byte, LevelMapper> levels;

		public byte LevelCount { get; private set; }

		public WorldMapper(LdtkJson world) {
			this.layerNames = new();
			this.levels = new();

			foreach (var layerDef in world.Defs.Layers) {
				layerNames[layerDef.Uid] = layerDef.Identifier;
			}

			foreach (var level in world.Levels) {
				var mapper = new LevelMapper(this, level);
				levels[mapper.Number] = mapper;
			}

			foreach (var level in levels.Values) {
				foreach (var exit in level.Exits) {
					var entrance = GetEntrance(exit.EntranceId);
					exit.SetEntrance(entrance.Item1, entrance.Item2);
					if (entrance.Item1 != 0xFF && entrance.Item2 == null) {
						Console.WriteLine($"Warning: Cannot find corresponding Entrance[{exit.EntranceId}] for Exit in Level #{level.Number} ({level.Identifier})");
					}
				}
			}

			if (levels.Count == 0) {
				LevelCount = 0;
			}
			else {
				LevelCount = (byte)(levels.Values.Max(l => l.Number) + 1);
			}
		}


		private Tuple<byte, Entities.Entrance> GetEntrance(string entranceId) {
			try {
				var entranceSpec = MappingHelper.SplitEntranceId(entranceId);
				if (levels.TryGetValue(entranceSpec.Item1, out var level)) {
					return new(entranceSpec.Item1, level.GetEntrance(entranceSpec.Item2));
				}
				else {
					return new(entranceSpec.Item1, null);
				}
			}
			catch {
				return new(0, null);
			}
		}

		public string LayerName(long uid) {
			return layerNames[uid];
		}

		public Data.Level GetLevel(byte index) {
			if (levels.TryGetValue(index, out LevelMapper level)) {
				return level.GetLevel();
			}
			else {
				return Data.Level.EmptyLevel;
			}
		}
		
		public void WriteWorld(System.IO.Stream stream) {
			for (byte i = 0; i < LevelCount; i++) {
				GetLevel(i).WriteLevel(stream);
			}
		}
	}
}
