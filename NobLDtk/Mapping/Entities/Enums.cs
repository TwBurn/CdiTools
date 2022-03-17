using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMotion.Nobelia.Mapping.Entities {
	[Flags] public enum Direction : byte {
		None  = 0x00,
		Up    = 0x04,
		Down  = 0x08,
		Left  = 0x01,
		Right = 0x02,
		All   = 0x0F
	}
	[Flags] public enum TileFlags : byte {
		Destroyable     = 0x10,
		BlockProjectile = 0x20,
		BlockFire       = 0x40,
		BlockPlayer     = 0x80
	}

	public enum ActionType : ushort {
		Hidden = 0x0000,
		Idle   = 0x1000,
		Active = 0x2000,
		Burn   = 0xB000,
		Dead   = 0xD000
	}
	public enum ExitType : byte {
		Slow = 0x00,
		Fast = 0x01,
		Spin = 0x0F,
		ReverseSlow = 0x10,
		ReverseFast = 0x11
	}
	public enum ChestContent : byte {
		ST_BOMB  = 0x02,
		ST_FIRE  = 0x03,
		ST_MAGIC = 0x04,
		ST_CLOAK = 0x05,
		ST_COIN  = 0x0C
	}
	public enum AnimationType : byte {
		Leaves   = 0xC0,
		Smoke    = 0xC4,
		Wood     = 0xC8,
		RedPlant   = 0xB0,
		SpikePlant = 0xB4
	}
	public enum EnemyType : byte {
		Skelet = 0x20,
		Ghost  = 0x30,
		Blob   = 0x40,
		Spider = 0x50,
		Bat    = 0x60
	}
	public enum EntityType : byte {
		Exit        = 0xFF,
		Entrance    = 0xFE,
		PlayerStart = 0x01,
		Bomb        = 0x02,
		Switch      = 0x10,
		Trigger     = 0x11,
		Chest       = 0x20,
		Trap        = 0x30,
		Burner      = 0x31,
		Cannon      = 0x40,
		Torch       = 0x41,
		Pedestal    = 0x42,
		Bloom       = 0x50,
		Enemy       = 0x80,
		Animation   = 0xA0,
		Script      = 0xC0,
		Unknown     = 0x00
	}

	public enum CoordinateType : int {
		Tiles = 1,
		Pixels = 16
	}
	public enum AttackType: byte {
		None = 0,
		Projectile = 1,
		Bomb = 2
	}
}