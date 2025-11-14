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
	public partial class DroneState : TransformState {
#if UNITY_5_3_OR_NEWER
[Preserve]
#endif
public DroneState() { }
		[Type(5, "string")]
		public string name = default(string);

		[Type(6, "uint8")]
		public byte team = default(byte);

		[Type(7, "number")]
		public float experience = default(float);

		[Type(8, "uint8")]
		public byte level = default(byte);

		[Type(9, "uint8")]
		public byte upgradePoints = default(byte);

		[Type(10, "number")]
		public float progressToNextLevel = default(float);

		[Type(11, "string")]
		public string lastTurretUpgradeId = default(string);

		[Type(12, "string")]
		public string lastBodyUpgradeId = default(string);

		[Type(13, "string")]
		public string lastMovementUpgradeId = default(string);

		[Type(14, "number")]
		public float maxHealth = default(float);

		[Type(15, "number")]
		public float health = default(float);

		[Type(16, "number")]
		public float lowerRotation = default(float);

		[Type(17, "number")]
		public float upperRotation = default(float);
	}
}
