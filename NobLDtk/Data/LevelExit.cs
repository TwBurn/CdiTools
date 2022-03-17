using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace NMotion.Nobelia.Data {
	public struct LevelExit {

		public ushort Tile;
		public byte Next;
		public byte Direction;
		public byte Type;
		public byte SpawnDirection;
		public ushort SpawnTile;

		public void WriteObject(Stream stream) {
			stream.WriteByte((byte)(Tile >> 8 & 0xff));
			stream.WriteByte((byte)(Tile >> 0 & 0xff));

			stream.WriteByte(Next);
			stream.WriteByte(Direction);
			stream.WriteByte(Type);
			stream.WriteByte(SpawnDirection);

			stream.WriteByte((byte)(SpawnTile >> 8 & 0xff));
			stream.WriteByte((byte)(SpawnTile >> 0 & 0xff));
		}
	}
}
