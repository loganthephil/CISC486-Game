using DroneStrikers.Combat;
using UnityEngine;

namespace DroneStrikers.Drone
{
    [RequireComponent(typeof(DroneStats))] [RequireComponent(typeof(ContactDamageResolver))]
    public class DroneBodyDamageSource : MonoBehaviour, IDamageSource
    {
        private DroneStats _droneStats;

        public int ContactDamage => _droneStats.AttackStats.AttackDamage;
        public IDestructionContextReceiver InstigatorContextReceiver { get; private set; }

        private void Awake()
        {
            _droneStats = GetComponent<DroneStats>();
            InstigatorContextReceiver = GetComponent<IDestructionContextReceiver>();
        }
    }
}