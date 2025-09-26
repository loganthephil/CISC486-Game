using DroneStrikers.Combat;
using DroneStrikers.Stats;
using UnityEngine;

namespace DroneStrikers.Drone
{
    [RequireComponent(typeof(DroneStats))] [RequireComponent(typeof(ContactDamageResolver))]
    public class DroneBodyDamageSource : MonoBehaviour, IDamageSource
    {
        private DroneStats _droneStats;
        public int ContactDamage => _droneStats.AttackStats.AttackDamage;

        private void Awake()
        {
            _droneStats = GetComponent<DroneStats>();
        }
    }
}