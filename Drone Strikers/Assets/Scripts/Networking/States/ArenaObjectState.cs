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
		[Type(5, "string")]
		public string objectType = default(string);

		[Type(6, "uint8")]
		public byte team = default(byte);

		[Type(7, "number")]
		public float maxHealth = default(float);

		[Type(8, "number")]
		public float health = default(float);
	}
}
