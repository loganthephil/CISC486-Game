using UnityEngine;

namespace DroneStrikers.Combat
{
    public class SimpleHealth : MonoBehaviour, IHealth
    {
        public float CurrentHealth { get; set; }

        [SerializeField] private float _maxHealth;
        public float MaxHealth => _maxHealth;

        private void Awake()
        {
            CurrentHealth = MaxHealth;
        }
    }
}