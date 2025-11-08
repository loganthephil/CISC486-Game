using UnityEngine;

namespace DroneStrikers.Core.Types
{
    public struct DroneInfoSnapshot
    {
        public GameObject GameObject { get; private set; }
        public DroneControllerType ControllerType { get; private set; }

        public Vector3 Position { get; private set; }
        public int Level { get; private set; }
        public Team Team { get; private set; }

        public float MaxHealth { get; private set; }
        public float CurrentHealth { get; private set; }
        public float HealthPercent { get; private set; }

        public DroneInfoSnapshot(
            GameObject gameObject,
            DroneControllerType controllerType,
            Vector3 position,
            int level,
            Team team,
            float maxHealth,
            float currentHealth,
            float healthPercent)
        {
            GameObject = gameObject;
            ControllerType = controllerType;
            Position = position;
            Level = level;
            Team = team;
            MaxHealth = maxHealth;
            CurrentHealth = currentHealth;
            HealthPercent = healthPercent;
        }
    }
}