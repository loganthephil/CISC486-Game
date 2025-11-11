// 
// THIS FILE HAS BEEN GENERATED AUTOMATICALLY
// DO NOT CHANGE IT MANUALLY UNLESS YOU KNOW WHAT YOU'RE DOING
// 
// GENERATED USING @colyseus/schema 3.0.67
// 

using Colyseus.Schema;
#if UNITY_5_3_OR_NEWER
using UnityEngine.Scripting;
#endif

namespace DroneStrikers.Networking {
	public partial class GameState : Schema {
#if UNITY_5_3_OR_NEWER
[Preserve]
#endif
public GameState() { }
		[Type(0, "number")]
		public float gameTimeSeconds = default(float);

		[Type(1, "map", typeof(MapSchema<DroneState>))]
		public MapSchema<DroneState> drones = null;

		[Type(2, "map", typeof(MapSchema<ArenaObjectState>))]
		public MapSchema<ArenaObjectState> arenaObjects = null;

		[Type(3, "map", typeof(MapSchema<ProjectileState>))]
		public MapSchema<ProjectileState> projectiles = null;
	}
}
