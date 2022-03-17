using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ldtk;
using NMotion.Nobelia.Mapping.Entities;
using System.Reflection;

namespace NMotion.Nobelia.Mapping {
	public static class EntityMapper {

		public static EntityType GetEntityType(EntityInstance entityInstance) {
			if (Enum.TryParse(entityInstance.Identifier, out EntityType type)) {
				return type;
			}
			else {
				Console.WriteLine($"Unknown EntityType: {entityInstance.Identifier}");
				return EntityType.Unknown;
			}
		}

		private static readonly Dictionary<string, Type> TypeMapping = new();
		public static Entity MapEntity(EntityInstance entityInstance) {
			if (!TypeMapping.TryGetValue(entityInstance.Identifier, out Type entityType)) {
				entityType = Type.GetType($"NMotion.Nobelia.Mapping.Entities.{entityInstance.Identifier}", true, false);
				TypeMapping[entityInstance.Identifier] = entityType;
			}

			if (entityType == null) { return null; }
			return (Entity)Activator.CreateInstance(entityType, entityInstance);
		}
	}
}
