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
	public partial class ArenaObjectState : TransformState {
#if UNITY_5_3_OR_NEWER
[Preserve]
#endif
public ArenaObjectState() { }
		[Type(4, "string")]
		public string arenaObjectType = default(string);

		[Type(5, "number")]
		public float maxHealth = default(float);

		[Type(6, "number")]
		public float health = default(float);
	}
}
