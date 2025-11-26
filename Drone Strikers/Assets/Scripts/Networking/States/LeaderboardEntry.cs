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
	public partial class LeaderboardEntry : Schema {
#if UNITY_5_3_OR_NEWER
[Preserve]
#endif
public LeaderboardEntry() { }
		[Type(0, "string")]
		public string name = default(string);

		[Type(1, "number")]
		public float experience = default(float);

		[Type(2, "uint8")]
		public byte team = default(byte);
	}
}
