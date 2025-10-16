using DroneStrikers.Core.Interfaces;
using DroneStrikers.Core.Types;
using DroneStrikers.Game.Combat;
using UnityEngine;

namespace DroneStrikers.Game.Drone
{
    public class DroneInfo : MonoBehaviour
    {
        public Vector3 Position => _transform.position;
        public int Level => _droneUpgrader?.Level ?? 0;
        public Team Team => _teamMember?.Team ?? Team.Neutral;

        public float MaxHealth => _health.MaxHealth;
        public float CurrentHealth => _health.CurrentHealth;
        public float HealthPercent => _health.HealthPercent;

        private Transform _transform;
        private DroneUpgrader _droneUpgrader;
        private TeamMember _teamMember;
        private IHealth _health;

        private void Awake()
        {
            _transform = transform;
            _droneUpgrader = GetComponent<DroneUpgrader>();
            _teamMember = GetComponent<TeamMember>();
            _health = GetComponent<IHealth>();
        }
    }
}