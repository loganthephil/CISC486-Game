using DroneStrikers.Core.Interfaces;
using DroneStrikers.Game.Deprecated.Stats;
using UnityEngine;

namespace DroneStrikers.Game.Deprecated.Drone
{
    [RequireComponent(typeof(DroneStats))]
    public class DroneBodyDamageSource : MonoBehaviour, IDamageSource
    {
        [SerializeField] private StatTypeSO _attackDamageStat;

        private DroneStats _droneStats;

        public float ContactDamage => _droneStats.GetStatValue(_attackDamageStat);
        public IDestructionContextReceiver InstigatorContextReceiver { get; private set; }

        private void Awake()
        {
            Debug.Assert(_attackDamageStat != null, "Missing StatType assignment on " + this);

            _droneStats = GetComponent<DroneStats>();
            InstigatorContextReceiver = GetComponent<IDestructionContextReceiver>();
        }
    }
}