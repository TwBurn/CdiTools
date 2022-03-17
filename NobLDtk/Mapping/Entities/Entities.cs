using ldtk;
using NMotion.Nobelia.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMotion.Nobelia.Mapping.Entities {
	public abstract class Entity {
		public ushort TileIndex { get; private set; }
		public short X { get; private set; }
		public short Y { get; private set; }

		protected EntityInstance entityInstance;
		public Entity(EntityInstance entityInstance) {
			this.entityInstance = entityInstance;
			TileIndex = MappingHelper.TileIndex(entityInstance.Grid);
			X = (short)entityInstance.Px[0];
			Y = (short)entityInstance.Px[1];
		}

		protected dynamic GetField(string identifier) {
			var fi = entityInstance.FieldInstances.First(f => f.Identifier == identifier);
			return fi.Type switch {
				"Array<LocalEnum.Direction>" => ((Newtonsoft.Json.Linq.JArray)fi.Value).Aggregate(Direction.None, (result, item) => result | Enum.Parse<Direction>((string)item)),
				"LocalEnum.Direction" => Enum.Parse<Direction>(fi.Value),
				"LocalEnum.ExitType" => Enum.Parse<ExitType>(fi.Value),
				"LocalEnum.ChestContent" => Enum.Parse<ChestContent>(fi.Value),
				"LocalEnum.Animation" => Enum.Parse<AnimationType>(fi.Value),
				"LocalEnum.ActionType" => Enum.Parse<ActionType>(fi.Value),
				"LocalEnum.EnemyType" => Enum.Parse<EnemyType>(fi.Value),
				"LocalEnum.AttackType" => Enum.Parse<AttackType>(fi.Value),
				"LocalEnum.Sprite" => MappingHelper.ByteValue(fi.Value, 1),
				"LocalEnum.Tile" => MappingHelper.ByteValue(fi.Value, 4),
				"LocalEnum.ColorIndex" => MappingHelper.ByteValue(fi.Value, 4),
				"Point" => MappingHelper.TileIndex(fi.Value),
				_ => fi.Value
			};
		}

		protected T GetField<T>(string identifier) {
			return (T)GetField(identifier);
		}


		public abstract EntityType EntityType { get; }
		public abstract LevelObject GetLevelObject();

		public virtual int EntityPriority => (int)EntityType * 2 + 1000;

		protected LevelObject MakeLevelObject(byte sprite = 0, byte frame = 0, byte description = 0, ushort action = 0, ushort data = 0, uint word = 0) {
			return new() {
				Type = (byte)EntityType,
				Sprite = sprite,
				Frame = frame,
				Description = description,
				Action = action,
				Data = data,
				Word = word,
				X = X,
				Y = Y
			};
		}
		
		protected LevelObject MakeLevelObject(byte sprite = 0, byte frame = 0, byte description = 0, ActionType action = ActionType.Hidden, Direction direction = Direction.None, byte actionSize = 0, ushort data = 0, uint word = 0) {
			return new() {
				Type = (byte)EntityType,
				Sprite = sprite,
				Frame = frame,
				Description = description,
				Action = MappingHelper.MakeAction(action, direction, actionSize),
				Data = data,
				Word = word,
				X = X,
				Y = Y
			};
		}
	}

	public class Exit : Entity {
		public override EntityType EntityType => EntityType.Exit;

		public string EntranceId { get; private set; }
		public Direction Direction { get; private set; }
		public ExitType ExitType { get; private set; }

		public bool Enabled { get; private set; }
		public Exit(EntityInstance entityInstance) : base(entityInstance) {
			Enabled = GetField("Enabled");
			EntranceId = GetField("EntranceId");
			Direction = GetField("Direction");
			ExitType = GetField("ExitType");
		}

		public void SetEntrance(byte nextLevel, Entrance entrance) {
			NextLevel = nextLevel;
			if (entrance == null) {
				SpawnDirection = Direction.None;
				SpawnTileIndex = 0;
			}
			else {
				SpawnDirection = entrance.Direction;
				SpawnTileIndex = entrance.TileIndex;
			}
		}

		public override LevelObject GetLevelObject() {
			throw new NotImplementedException();
		}

		public LevelExit GetLevelExit() {
			return new() {
				Tile = TileIndex,
				Next = NextLevel,
				Direction = (byte)Direction,
				Type = Convert.ToByte((byte)ExitType | (Enabled ? 0 : 0x80)),
				SpawnDirection = (byte)SpawnDirection,
				SpawnTile = SpawnTileIndex
			};
		}

		public byte NextLevel { get; private set; }
		public Direction SpawnDirection { get; private set; }
		public ushort SpawnTileIndex { get; private set; }
	}

	public class Entrance: Entity {
		public override EntityType EntityType => EntityType.Entrance;

		public byte Id { get; private set; }
		public Direction Direction { get; private set; }

		public Entrance(EntityInstance entityInstance) : base(entityInstance) {
			Id = GetField<byte>("Id");
			Direction = GetField("Direction");
		}
		public override LevelObject GetLevelObject() {
			throw new NotImplementedException();
		}
	}

	public class PlayerStart : Entity {
		public override EntityType EntityType => EntityType.PlayerStart;
		public override int EntityPriority => 0;

		public Direction Direction { get; private set; }

		public PlayerStart(EntityInstance entityInstance) : base(entityInstance) {
			Direction = GetField("Direction");
		}
		public override LevelObject GetLevelObject() {
			return MakeLevelObject(
				sprite:    MappingHelper.SpritePlayer,
				action:    ActionType.Idle,
				direction: Direction
			);;
		}
	}

	public class Bomb : Entity {
		public override EntityType EntityType => EntityType.Bomb;

		public Direction ExplodeDirections { get; private set; }
		public byte ExplodeSize { get; private set; }

		public Bomb(EntityInstance entityInstance) : base(entityInstance) {
			ExplodeDirections = GetField("ExplodeDirections");
			ExplodeSize = GetField<byte>("ExplodeSize");
		}
		public override LevelObject GetLevelObject() {
			return MakeLevelObject(
				direction: ExplodeDirections,
				description: ExplodeSize
			);
		}
	}

	public class Cannon : Entity {
		public override EntityType EntityType => EntityType.Cannon;

		public Direction FireDirection { get; private set; }
		public byte FireTime { get; private set; }
		public byte FireReload { get; private set; }

		public Cannon(EntityInstance entityInstance) : base(entityInstance) {
			FireDirection = GetField("FireDirection");
			FireTime = GetField<byte>("FireTime");
			FireReload = GetField<byte>("FireReload");
		}
		public override LevelObject GetLevelObject() {
			return MakeLevelObject(
				direction: FireDirection,
				actionSize: FireTime,
				data: FireReload
			);
		}
	}

	public class Chest : Entity {
		public override EntityType EntityType => EntityType.Chest;
		public override int EntityPriority => 10;
		public static int ChestCount { get; private set; } = 0;

		public int ChestId { get; private set; }
		public byte? ChestSprite { get; private set; }
		public byte ChestOpenTile { get; private set; }
		public ChestContent ChestContent { get; private set; }
		public ushort CoinId { get; private set; }

		public Chest(EntityInstance entityInstance) : base(entityInstance) {
			ChestSprite = GetField<byte?>("ChestSprite");
			ChestOpenTile = GetField<byte>("ChestOpenTile");
			ChestContent = GetField("ChestContent");
			CoinId = GetField<ushort>("CoinId");
			
			ChestId = ChestCount++; /* Set ChestId, increment ChestCount */
		}

		private byte ChestAnimation {
			get {
				return ChestContent switch {
					ChestContent.ST_BOMB => 0x90,
					ChestContent.ST_FIRE => 0x91,
					ChestContent.ST_MAGIC => 0x92,
					ChestContent.ST_CLOAK => 0x93,
					ChestContent.ST_COIN => 0x97,
					_ => throw new NotImplementedException()
				};
			}
		}
		public override LevelObject GetLevelObject() {
			return MakeLevelObject(
				sprite: ChestSprite.GetValueOrDefault(0),
				action: ChestSprite.HasValue ? ActionType.Idle : ActionType.Hidden,
				data: Convert.ToUInt16(ChestOpenTile * 0x100 + ChestAnimation),
				description: (byte)ChestContent,
				word: (uint)((ChestId << 16) + CoinId)
			);
		}
	}

	public class Enemy : Entity {
		public override EntityType EntityType => EntityType.Enemy;
		public override int EntityPriority => 20;
		public ActionType Action { get; private set; }
		public Direction Direction { get; private set; }
		public EnemyType EnemyType { get; private set; }
		public byte Description { get; private set; }
		public ushort Data { get; private set; }
		public uint Word { get; private set; }

		public Enemy(EntityInstance entityInstance) : base(entityInstance) {
			Action = GetField("Action");
			Direction = GetField("Direction");
			EnemyType = GetField("EnemyType");
			Description = GetField<byte>("Description");
			Data = GetField<ushort>("Data");
			Word = GetField<uint>("Word");
		}
		public override LevelObject GetLevelObject() {
			return MakeLevelObject(
				sprite: (byte)EnemyType,
				direction: Direction,
				action: Action,
				description: Description,
				data: Data,
				word: Word
			);
		}
	}

	public class Switch : Entity {
		public override EntityType EntityType => EntityType.Switch;

		public byte SwitchedTile { get; private set; }
		public ushort ReplaceStart { get; private set; }
		public Direction ReplaceDirection { get; private set; }
		public byte ReplaceCount { get; private set; }
		public byte ReplaceTile { get; private set; }
		public AnimationType ReplaceAnimation { get; private set; }
		public uint ScriptId { get; private set; }

		public Switch(EntityInstance entityInstance) : base(entityInstance) {
			SwitchedTile = GetField<byte>("SwitchedTile");
			ReplaceStart = GetField<ushort>("ReplaceStart");
			ReplaceDirection = GetField("ReplaceDirection");
			ReplaceCount = GetField<byte>("ReplaceCount");
			ReplaceTile = GetField<byte>("ReplaceTile");
			ReplaceAnimation = GetField("ReplaceAnimation");
			ScriptId = GetField<uint>("ScriptId");
		}
		public override LevelObject GetLevelObject() {
			return MakeLevelObject(
				sprite: SwitchedTile,
				frame: (byte)ReplaceAnimation,
				description: ReplaceTile,
				direction: ReplaceDirection,
				actionSize: ReplaceCount,
				data: ReplaceStart,
				word: ScriptId
			);
		}
	}

	public class Trigger : Entity {
		public override EntityType EntityType => EntityType.Trigger;

		public byte SwitchedTile { get; private set; }
		public uint ScriptId { get; private set; }

		public ushort Data { get; private set; }

		public Trigger(EntityInstance entityInstance) : base(entityInstance) {
			SwitchedTile = GetField<byte>("SwitchedTile");
			ScriptId = GetField<uint>("ScriptId");
			Data = GetField<ushort>("Data");
		}
		public override LevelObject GetLevelObject() {
			return MakeLevelObject(
				sprite: SwitchedTile,
				action: ActionType.Hidden,
				data: Data,
				word: ScriptId
			);
		}
	}

	public class Torch : Entity {
		public override EntityType EntityType => EntityType.Torch;

		public Direction FireDirection { get; private set; }
		public byte FireCount { get; private set; }

		public Torch(EntityInstance entityInstance) : base(entityInstance) {
			FireDirection = GetField("FireDirection");
			FireCount = GetField<byte>("FireCount");
		}
		public override LevelObject GetLevelObject() {
			return MakeLevelObject(
				sprite: MappingHelper.SpriteTorch,
				action: ActionType.Active,
				direction: FireDirection,
				actionSize: FireCount
			);
		}
	}

	public class Burner : Entity {
		public override EntityType EntityType => EntityType.Burner;

		public bool Active { get; private set; }

		public Burner(EntityInstance entityInstance) : base(entityInstance) {
			Active = GetField("Active");
		}

		public override LevelObject GetLevelObject() {
			return MakeLevelObject(
				sprite: MappingHelper.SpriteFire,
				frame: 0,
				action: Active ? ActionType.Burn : ActionType.Hidden
			);
		}
	}

	public class Pedestal : Entity {
		public override EntityType EntityType => EntityType.Pedestal;

		public bool Active { get; private set; }

		public Pedestal(EntityInstance entityInstance) : base(entityInstance) {
			Active = GetField("Active");
		}

		public override LevelObject GetLevelObject() {
			return MakeLevelObject(
				sprite: MappingHelper.SpriteTorch,
				frame: Convert.ToByte(Active ? 0 : 3),
				action: Active ? ActionType.Active : ActionType.Idle
			);
		}
	}

	public class Bloom : Entity {
		public override EntityType EntityType => EntityType.Bloom;

		public AnimationType Animation { get; private set; }

		public Direction Directions { get; private set; }
		public byte Delay { get; private set; }
		public byte Speed { get; private set; }
		public AttackType AttackType { get; private set; }

		public Bloom(EntityInstance entityInstance) : base(entityInstance) {
			Animation = GetField("Animation");
			Delay = GetField<byte>("Delay");
			Speed = GetField<byte>("Speed");
			Directions = GetField("Directions");
			AttackType = GetField("AttackType");
		}

		public override LevelObject GetLevelObject() {
			return MakeLevelObject(
				sprite: (byte)Animation,
				frame: 0,
				action: ActionType.Hidden,
				direction: Directions,
				word: (uint)AttackType,
				data: (ushort)(Speed + (Delay << 8))
			);
		}
	}

	public class Script : Entity {
		public override EntityType EntityType => EntityType.Script;

		public ushort ScriptId { get; private set; }
		public byte Description { get; private set; }
		public override int EntityPriority => 5;
		public ushort Data { get; private set; }

		public Script(EntityInstance entityInstance) : base(entityInstance) {
			ScriptId = GetField<ushort>("ScriptId");
			Description = GetField<byte>("Description");
			Data = GetField<ushort>("Data");
		}

		public override LevelObject GetLevelObject() {
			return MakeLevelObject(
				sprite: MappingHelper.SpriteScript,
				action: ScriptId,
				description: Description,
				data: Data
			);
		}
	}
}
