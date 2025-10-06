using UnityEngine;

namespace DroneStrikers.Stats
{
    [CreateAssetMenu(fileName = "DroneStatsSO", menuName = "Scriptable Objects/DroneStatsSO")]
    public class DroneStatsSO : ScriptableObject
    {
        [Header("Drone")]
        public float MaxHealth = 10; // Maximum health of the drone
        public float HealthRegen = 0.5f; // Health regeneration rate per second

        [Header("Movement")]
        public float MoveSpeed = 10f; // Movement speed of the drone
        public float MoveAcceleration = 2f; // Acceleration of the drone
        public float MoveDeceleration = 4f; // Deceleration of the drone

        [Header("Weapon")]
        public float FireCooldown = 0.5f; // Time between attacks
        public float AimSpeed = 5f; // Speed of aiming the drone's weapon
        public float Recoil = 1f; // Recoil force on the drone when attacking

        [Header("Attack")]
        public float AttackVelocity = 10f; // Speed of the attack itself
        public float AttackDamage = 1; // Damage dealt by the attack
        public float AttackPierce; // Number of enemies the attack can hit
    }
}