using System.IO;

namespace NMotion.Nobelia.Data {
	public class Level {
		public const int Width = 23;
		public const int Height = 15;
		public const int ObjectCount = 36;
		public const int ExitCount = 8;

		public static readonly Level EmptyLevel = new() { Name = "XXEMPTY" };

		public string Name { get; set; }
		public byte MusicTrack { get; set; }
		public byte BorderColor { get; set; }
		public byte BackgroundColor { get; set; }
		public byte[] ActionLayer { get; private set; }
		public byte[] TopLayer { get; private set; }
		public byte[] MiddleLayer { get; private set; }
		public byte[] BottomLayer { get; private set; }

		public LevelObject[] LevelObjects { get; private set; }
		public LevelExit[] LevelExits { get; private set; }

		public Level() {
			ActionLayer = new byte[Width * Height];
			TopLayer = new byte[Width * Height];
			MiddleLayer = new byte[Width * Height];
			BottomLayer = new byte[Width * Height];

			LevelObjects = new LevelObject[ObjectCount];
			LevelExits = new LevelExit[ExitCount];
		}

		public void WriteLevel(Stream stream) {
			WriteHeader(stream);
			WriteLayers(stream);
			WriteObjects(stream);
		}

		private byte[] LevelName() {
			var bytes = new byte[7];

			var length = Name.Length;
			if (length > 7) length = 7;
			for (var i = 0; i < length; i++) {
				bytes[i] = (byte)Name[i];
			}

			return bytes;
		}
		private void WriteHeader(Stream stream) {
			stream.Write(LevelName(), 0, 7);
			stream.WriteByte(MusicTrack);

			stream.WriteByte(BorderColor);
			stream.WriteByte(BorderColor);
			stream.WriteByte(BorderColor);
			stream.WriteByte(BorderColor);

			stream.WriteByte(BackgroundColor);
			stream.WriteByte(BackgroundColor);
			stream.WriteByte(BackgroundColor);
			stream.WriteByte(BackgroundColor);

			for (var i = 0; i < 12; i++) stream.WriteByte(0);

		}
		private void WriteLayers(Stream stream) {
			for (var i = 0; i < Width * Height; i++) {
				stream.WriteByte(ActionLayer[i]);
				stream.WriteByte(TopLayer[i]);
				stream.WriteByte(MiddleLayer[i]);
				stream.WriteByte(BottomLayer[i]);
			}
		}
		private void WriteObjects(Stream stream) {
			foreach (var obj in LevelObjects) {
				obj.WriteObject(stream);
			}
			foreach (var obj in LevelExits) {
				obj.WriteObject(stream);
			}
		}
	}

}
