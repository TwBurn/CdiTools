using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NMotion.Nobelia.Mapping.Entities;

namespace NMotion.Nobelia.Mapping {
	public static class MappingHelper {
		public const int LevelWidth = 23;
		public const int LevelHeight = 15;

		public static ushort TileIndex(long[] grid, CoordinateType coordinateType = CoordinateType.Tiles) {
			int x = (int)grid[0] / (int)coordinateType;
			int y = (int)grid[1] / (int)coordinateType;
			return TileIndex(x, y);
		}
		public static object TileIndex(Newtonsoft.Json.Linq.JObject value) {
			return TileIndex((int)value["cx"], (int)value["cy"]);
		}
		public static ushort TileIndex(int x, int y) {
			return Convert.ToUInt16(LevelWidth * y + x);
		}

		public static byte? ByteValue(string value, int start) {
			if (value == null) {
				return null;
			}
			else {
				return Convert.ToByte(value[start..(start + 2)], 16);
			}
		}
		public static Tuple<byte, byte> SplitEntranceId(string entranceId) {
			if (entranceId == "EXIT") {
				return new(0xFF, 0);
			}
			else {
				string[] parts = entranceId.Split('.');
				return new(byte.Parse(parts[0]), byte.Parse(parts[1]));
			}
		}

		public static ushort MakeAction(ActionType action = ActionType.Hidden, Direction direction = Direction.None, byte count = 0) {
			return Convert.ToUInt16((ushort)action + (ushort)direction * 256 + count);
		}

		public const byte SpritePlayer = 0x00;
		public const byte SpriteTorch  = 0x8B;
		public const byte SpriteFire   = 0xE8;
		public const byte SpriteScript = 0x95;
	}
}

