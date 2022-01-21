using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace NMotion.Cdi.Audio {
	public class AdpcmTrack {
		public const int HEADER_SIZE  = 54;
		public const int BLOCK_GROUPS = 18;
		public const int PADDING_SIZE = 20;

		public byte[] Header { get; private set; }

		public SoundGroup[] SoundGroups { get; private set; }

		public int FileSize { get; private set; }

		public int BlockSize { get; private set; }
		public int BlockCount { get; private set; }

		public int Channels { get; private set; }
		public int SampleBits { get; private set; }
		public int SampleRate { get; private set; }
		
		public int DataSize { get; private set; }

		public static AdpcmTrack FromFile(string filename) {
			using var stream = File.OpenRead(filename);
			return FromBinaryStream(stream);
		}
		public static AdpcmTrack FromBinaryStream(Stream stream) {
			var track = new AdpcmTrack {
				Header = new byte[HEADER_SIZE]
			};

			/* Read Header */
			stream.Read(track.Header, 0, HEADER_SIZE);

			track.ParseHeader();


			track.SoundGroups = new SoundGroup[track.BlockCount * BLOCK_GROUPS];
			/* Read Blocks */
			byte[] buffer = new byte[track.BlockSize];
			for (int b = 0; b < track.BlockCount; b++) {
				stream.Read(buffer, 0, track.BlockSize);
				for (int s = 0; s < BLOCK_GROUPS; s++) {
					track.SoundGroups[b * BLOCK_GROUPS + s] = SoundGroup.FromByteArray(buffer, s * SoundGroup.SOUND_GROUP_SIZE);
				}
			}

			return track;
		}
		private void ParseHeader() {
			FileSize = ConvertHeaderBytes(4, 4) + 8;
			Channels = ConvertHeaderBytes(20, 2);
			SampleBits = ConvertHeaderBytes(26, 2);
			SampleRate = ConvertHeaderBytes(30, 2);
			DataSize = ConvertHeaderBytes(42, 4) - 8;
			BlockSize = ConvertHeaderBytes(50, 4);

			BlockCount = DataSize / BlockSize;
		}

		private int ConvertHeaderBytes(int startIndex, int count) {
			int value = 0;
			for (int i = 0; i < count; i++) {
				value = (value << 8) + Header[startIndex + i];
			}
			return value;
		}

		public void MuteLeft() {
			foreach (var sg in SoundGroups) sg.MuteLeft();
		}

		public void MuteRight() {
			foreach (var sg in SoundGroups) sg.MuteRight();
		}

		public void ToStream(Stream stream, bool writeHeader = true, bool writeBlockPadding = true) {
			if (writeHeader) {
				stream.Write(Header, 0, HEADER_SIZE);
			}

			for (int b = 0; b < BlockCount; b++) {
				for (int s = 0; s < BLOCK_GROUPS; s++) {
					SoundGroups[b * BLOCK_GROUPS + s].ToStream(stream);
				}

				if (writeBlockPadding) {
					for (int i = 0; i < PADDING_SIZE; i++) stream.WriteByte(0x00);
				}
			}

			
		}
	}
}
