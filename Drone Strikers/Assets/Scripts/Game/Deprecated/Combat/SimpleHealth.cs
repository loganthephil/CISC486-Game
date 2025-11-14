using DroneStrikers.Core.Interfaces;
using UnityEngine;

namespace DroneStrikers.Game.Deprecated.Combat
{
    // TODO: Consider making SimpleHealth the base class for DroneHealth to reduce code duplication
    public class SimpleHealth : MonoBehaviour, IHealth
    {
        public float CurrentHealth { get; set; }

        [SerializeField] private float _maxHealth;
        public float MaxHealth => _maxHealth;
        public float HealthPercent => MaxHealth > 0 ? CurrentHealth / MaxHealth : 0;

        private void Awake()
        {
            CurrentHealth = MaxHealth;
        }
    }
}