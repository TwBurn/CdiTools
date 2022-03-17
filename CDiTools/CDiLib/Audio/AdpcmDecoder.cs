using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMotion.Cdi.Audio {
	public static class AdpcmDecoder {
		/*short[] DecodeSoundGroup(SoundGroup group, bool stereo) {

		}
		*/

		private static ReadOnlySpan<sbyte> PositiveXaAdpcmTable => new sbyte[] { 0, 60, 115, 98, 122 };
		private static ReadOnlySpan<sbyte> NegativeXaAdpcmTable => new sbyte[] { 0, 0, -52, -55, -60 };
		private static short oldL = 0, olderL = 0, oldR = 0, olderR = 0;
		public static short[] Decode(SoundGroup group, bool stereo) {
			List<short> samples = new(224);

			

			for (int blk = 0; blk < 4; blk++) {
				if (stereo) {
					var l = DecodeNibbles(group, blk, 0, ref oldL, ref olderL);
					var r = DecodeNibbles(group, blk, 1, ref oldR, ref olderR);
					for(var i = 0; i < 28; i++) {
						samples.Add(l[i]);
						samples.Add(r[i]);
					}
				}
				else {
					samples.AddRange(DecodeNibbles(group, blk, 0, ref oldL, ref olderL));
					samples.AddRange(DecodeNibbles(group, blk, 1, ref oldL, ref olderL));
				}
			}

			return samples.ToArray();
		}

		private static short[] DecodeNibbles(SoundGroup group, int blk, int nibble, ref short old, ref short older) {
			short[] nibbleBuffer = new short[28];

			int shift = 12 - (group[4 + blk * 2 + nibble] & 0x0F);
			int filter = (group[4 + blk * 2 + nibble] & 0x30) >> 4;

			int f0 = PositiveXaAdpcmTable[filter];
			int f1 = NegativeXaAdpcmTable[filter];

			for (int i = 0; i < 28; i++) {
				int t = SignedNibble((byte)((group[16 + blk + i * 4] >> (nibble * 4)) & 0x0F));
				int s = (t << shift) + ((old * f0 + older * f1 + 32) / 64);
				short sample = (short)Math.Clamp(s, -0x8000, 0x7FFF);

				nibbleBuffer[i] = sample;
				older = old;
				old = sample;
			}

			return nibbleBuffer;
		}

		private static int SignedNibble(byte value) {
			return (value << 28) >> 28;
		}
	}
}
