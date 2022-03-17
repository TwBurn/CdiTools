using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ldtk;
using NMotion.Nobelia.Mapping.Entities;

namespace NMotion.Nobelia.Mapping {
	public class LevelMapper {
		private readonly WorldMapper worldMapper;
		private readonly Level level;
		private readonly LayerInstance moveLayer, actionLayer, topLayer, middleLayer, bottomLayer;
		
		public byte Number { get; private set; }
		public string Name { get; private set; }
		public string Identifier { get; private set; }
		public byte Music { get; private set; }
		public byte BorderColor { get; private set; }
		public byte BackgroundColor { get; private set; }

		public Entity[] Entities { get; private set; }
		public Exit[] Exits { get; private set; }
		public Entrance[] Entrances { get; private set; }

		public LevelMapper(WorldMapper worldMapper, Level level) {
			this.worldMapper = worldMapper;
			this.level = level;
			this.Identifier = level.Identifier;
	
			foreach (var field in level.FieldInstances) {
				switch (field.Identifier) {
					case "Number":
						Number = (byte)field.Value; break;
					case "Name":
						Name = field.Value; break;
					case "Music":
						Music = (byte)field.Value; break;
					case "BorderColor":
						BorderColor = MappingHelper.ByteValue(field.Value, 1); break;
					case "BackgroundColor":
						BackgroundColor = MappingHelper.ByteValue(field.Value, 1); break;
				}
			}


			foreach (var layer in level.LayerInstances) {
				var name = worldMapper.LayerName(layer.LayerDefUid);
				switch (name) {
					case "Objects":
						Entities = MapEntities(layer).OrderBy(e => e.EntityPriority).ToArray();
						if (Entities.Length == 0 || Entities[0].EntityType != EntityType.PlayerStart) {
							Console.WriteLine($"No PlayerStart Entity in Level #{Number} ({Identifier})");
						}
						break;
					case "Exit":
						var entities = MapEntities(layer);
						Exits = entities.Where(e => e.EntityType == EntityType.Exit).Select(e => (Exit)e).ToArray();
						Entrances = entities.Where(e => e.EntityType == EntityType.Entrance).Select(e => (Entrance)e).ToArray();
						break;
					case "Move":
						moveLayer = layer; break;
					case "Action":
						actionLayer = layer; break;
					case "Top":
						topLayer = layer; break;
					case "Middle":
						middleLayer = layer; break;
					case "Bottom":
						bottomLayer = layer; break;
				}
			}

			foreach (var entrance in Entrances) {
				if (entrance.Direction == Direction.None) {
					Console.WriteLine($"Entrance[{entrance.Id}] at ({entrance.X}, {entrance.Y}) in Level #{Number} ({Identifier}) has an invalid direction");
				}
			}
		}
		private static List<Entity> MapEntities(LayerInstance layer) {
			List<Entity> entities = new();
			foreach (var entityInstance in layer.EntityInstances) {
				var entity = EntityMapper.MapEntity(entityInstance);
				if (entity != null) entities.Add(entity);
			}
			return entities;
		}
		public Entrance GetEntrance(byte id) {
			return Entrances.FirstOrDefault(e => e.Id == id);
		}

		public Data.Level GetLevel() {
			Data.Level level = new();
			level.Name = Name;
			level.MusicTrack = Music;
			level.BorderColor = BorderColor;
			level.BackgroundColor = BackgroundColor;

			MapTileLayer(topLayer, level.TopLayer);
			MapTileLayer(middleLayer, level.MiddleLayer);
			MapTileLayer(bottomLayer, level.BottomLayer);

			MapActions(level.ActionLayer);

			for (var i = 0; i < Entities.Length; i++) {
				level.LevelObjects[i] = Entities[i].GetLevelObject();
			}

			for (var i = 0; i < Exits.Length; i++) {
				level.LevelExits[i] = Exits[i].GetLevelExit();
			}

			return level;
		}
		private void MapActions(byte[] output) {
			var move   = new byte[output.Length];
			var action = new byte[output.Length];

			for (var i = 0; i < output.Length; i++) {
				move[i] = (byte)Direction.All;
			}

			MapTileLayer(moveLayer, move);
			MapTileLayer(actionLayer, action);

			for (var i = 0; i < output.Length; i++) {
				byte m = move[i];
				byte a = action[i];

				if (m > (byte)Direction.All) {
					Console.WriteLine($"Invalid Move[{m}] at position {i} in Level #{Number} ({Identifier}) - Expected 0-15");
					m = 0;
				}


				if (a == 0) {
					if (m == (byte)Direction.None) {
						a = (byte)(TileFlags.BlockFire | TileFlags.BlockProjectile | TileFlags.BlockPlayer);
					}
				}
				else if (a < 0x10 || a >= 0x20) {
					Console.WriteLine($"Invalid Action[{a}] at position {i} in Level #{Number} ({Identifier}) - Expected 16-31");
					a = 0;
				}
				else {
					a = (byte)(a << 4);
				}

				output[i] = (byte)(m | a);
			}
		}
		private static void MapTileLayer(LayerInstance input, byte[] output) {
			foreach (var tile in input.GridTiles) {
				output[MappingHelper.TileIndex(tile.Px, CoordinateType.Pixels)] = (byte)tile.T;
			}
		}
	}
}
