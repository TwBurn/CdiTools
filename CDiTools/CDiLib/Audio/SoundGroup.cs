using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace NMotion.Cdi.Audio {
	public class SoundGroup {

		public const int PARAMETER_COUNT = 16;
		public const int SAMPLE_COUNT = 112;
		public const int SOUND_GROUP_SIZE = PARAMETER_COUNT + SAMPLE_COUNT;

		public byte[] Parameters { get; private set; }
		public byte[] Samples { get; private set; }

		public static SoundGroup FromBinaryStream(Stream stream) {
			SoundGroup soundGroup = new() {
				Parameters = new byte[PARAMETER_COUNT],
				Samples = new byte[SAMPLE_COUNT]
			};

			stream.Read(soundGroup.Parameters, 0, PARAMETER_COUNT);
			stream.Read(soundGroup.Samples, 0, SAMPLE_COUNT);

			return soundGroup;
		}

		public static SoundGroup FromByteArray(byte[] data, int startByte) {
			SoundGroup soundGroup = new() {
				Parameters = new byte[PARAMETER_COUNT],
				Samples = new byte[SAMPLE_COUNT]
			};

			for (int i = 0; i < PARAMETER_COUNT; i++) {
				soundGroup.Parameters[i] = data[startByte + i];
			}

			for (int i = 0; i < SAMPLE_COUNT; i++) {
				soundGroup.Samples[i] = data[startByte + PARAMETER_COUNT + i];
			}

			return soundGroup;
		}

		public void ToStream(Stream stream) {
			stream.Write(Parameters, 0, PARAMETER_COUNT);
			stream.Write(Samples, 0, SAMPLE_COUNT);
		}

		public void MuteLeft() {
			for (int i = 0; i < PARAMETER_COUNT; i += 2) {
				Parameters[i + 0] = 0x00; /* Clear even bytes */
			}
			for (int i = 0; i < SAMPLE_COUNT; i++) {
				Samples[i] &= 0xF0; /* Clear lower 4 bits */
			}
		}

		public void MuteRight() {
			for (int i = 0; i < PARAMETER_COUNT; i += 2) {
				Parameters[i + 1] = 0x00; /* Clear odd bytes */
			}
			for (int i = 0; i < SAMPLE_COUNT; i++) {
				Samples[i] &= 0x0F; /* Clear higher 4 bits */
			}
		}
	}
}
