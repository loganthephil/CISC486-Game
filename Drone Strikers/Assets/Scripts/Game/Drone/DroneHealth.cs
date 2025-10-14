using DroneStrikers.Core.Interfaces;
using DroneStrikers.Core.Types;
using DroneStrikers.Events;
using DroneStrikers.Game.Combat;
using DroneStrikers.Game.Stats;
using UnityEngine;

namespace DroneStrikers.Game.Drone
{
    [RequireComponent(typeof(DroneStats))]
    public class DroneHealth : MonoBehaviour, IHealth
    {
        private const float HealthRegenDelay = 5f; // Time in seconds after taking damage before health starts regenerating

        private DroneStats _droneStats;
        private LocalEvents _localEvents;

        [SerializeField] private StatTypeSO _maxHealthStat;
        [SerializeField] private StatTypeSO _healthRegenStat;

        public float CurrentHealth { get; set; }
        public float MaxHealth => _droneStats.GetStatValue(_maxHealthStat);

        private float _timeSinceLastHit;
        private float _lastHealth;

        private void Awake()
        {
            Debug.Assert(_maxHealthStat != null && _healthRegenStat != null, "Missing StatType assignment on " + this);

            _droneStats = GetComponent<DroneStats>();
            _localEvents = GetComponent<LocalEvents>();
            if (_localEvents == null) Debug.LogWarning("No local events found. DroneHealth will not be able to respond to damage events.");
        }

        private void Start()
        {
            // Set current health to max health at the start
            CurrentHealth = MaxHealth;
        }

        private void OnEnable() => _localEvents.Subscribe(CombatEvents.DamageTaken, OnDamageTaken);
        private void OnDisable() => _localEvents.Unsubscribe(CombatEvents.DamageTaken, OnDamageTaken);

        private void Update()
        {
            if (_timeSinceLastHit < HealthRegenDelay) _timeSinceLastHit += Time.deltaTime;

            if (CurrentHealth < MaxHealth && _timeSinceLastHit >= HealthRegenDelay)
            {
                CurrentHealth += _droneStats.GetStatValue(_healthRegenStat) * Time.deltaTime; // Regenerate health over time
                CurrentHealth = Mathf.Min(CurrentHealth, MaxHealth); // Clamp to max health
            }
        }

        // Reset the timer on taking damage
        private void OnDamageTaken(DamageContext context) => _timeSinceLastHit = 0f;
    }
}