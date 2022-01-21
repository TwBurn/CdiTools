using System;
using System.Collections.Generic;

namespace NMotion.Cdi.Graphics {
	public struct Color {
		public readonly bool IsTransparent;
		public readonly byte R, G, B;

		public static readonly Color Transparent = new(true, 0, 0, 0);

		private Color(bool isTransparent, byte r, byte g, byte b) {
			IsTransparent = isTransparent;
			R = r;
			G = g;
			B = b;
		}
		public Color(byte r, byte g, byte b) : this(false, r, g, b) {}

		public static Color FromColor(System.Drawing.Color color) {
			if (color.A < 128) {
				return Transparent;
			}
			else {
				return new Color(color.R, color.G, color.B);
			}
		}

		public override bool Equals(object obj) {
			if (obj is Color c) {
				if (c.IsTransparent && this.IsTransparent) {
					return true;
				}
				else {
					return c.R == this.R && c.G == this.G && c.B == this.B;
				}
			}
			else {
				return false;
			}
		}

		public override int GetHashCode() {
			if (IsTransparent) {
				return 1 << 24;
			}
			else {
				return R << 16 + G << 8 + B;
			}
		}

		public int ColorDistance(Color toColor) {
			int dR = this.R - toColor.R;
			int dG = this.G - toColor.G;
			int dB = this.B - toColor.B;

			return dR * dR + dG * dG + dB * dB;
		}

		public static bool operator ==(Color left, Color right) {
			return left.Equals(right);
		}

		public static bool operator !=(Color left, Color right) {
			return !(left == right);
		}

		public string ToJson() {
			return string.Format("{{r:{0}, g:{1}, b:{2}}}", R, G, B);
		}

		private static readonly System.Text.RegularExpressions.Regex jsonRegex = new(@"\{\s*r\s*:\s*(\d+)\s*,\s*g\s*:\s*(\d+)\s*,\s*b\s*:\s*(\d+)\s*\}", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
		public static Color FromJson(string json) {
			if (jsonRegex.IsMatch(json)) {
				byte r = Byte.Parse(jsonRegex.Replace(json, "$1"));
				byte g = Byte.Parse(jsonRegex.Replace(json, "$2"));
				byte b = Byte.Parse(jsonRegex.Replace(json, "$3"));
				return new Color(r, g, b);
			}
			else {
				throw new Exception(string.Format("Cannot convert string '{0}' to a color.", json));
			}
		}
	}
}
