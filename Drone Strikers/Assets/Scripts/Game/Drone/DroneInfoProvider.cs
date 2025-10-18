using DroneStrikers.Core.Interfaces;
using DroneStrikers.Core.Types;
using DroneStrikers.Game.Combat;
using UnityEngine;

namespace DroneStrikers.Game.Drone
{
    public class DroneInfoProvider : MonoBehaviour
    {
        /// <summary>
        ///     Return a new DroneInfoSnapshot struct containing relevant information about this drone.
        /// </summary>
        public DroneInfoSnapshot GetSnapshot() => new(
            gameObject,
            _controllerSelector.CurrentControllerType,
            _transform.position,
            _droneUpgrader.Level,
            _teamMember.Team,
            _health.MaxHealth,
            _health.CurrentHealth,
            _health.HealthPercent);

        public DroneControllerType ControllerType => _controllerSelector.CurrentControllerType;
        public Vector3 Position => _transform.position;
        public int Level => _droneUpgrader.Level;
        public Team Team => _teamMember.Team;

        public float MaxHealth => _health.MaxHealth;
        public float CurrentHealth => _health.CurrentHealth;
        public float HealthPercent => _health.HealthPercent;

        private Transform _transform;

        private DroneControllerSelector _controllerSelector;
        private DroneUpgrader _droneUpgrader;
        private TeamMember _teamMember;
        private IHealth _health;

        private void Awake()
        {
            _transform = transform;
            _controllerSelector = GetComponent<DroneControllerSelector>();
            _droneUpgrader = GetComponent<DroneUpgrader>();
            _teamMember = GetComponent<TeamMember>();
            _health = GetComponent<IHealth>();
        }
    }
}