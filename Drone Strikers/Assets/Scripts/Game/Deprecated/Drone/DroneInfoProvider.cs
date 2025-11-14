using DroneStrikers.Core.Editor;
using DroneStrikers.Core.Interfaces;
using DroneStrikers.Core.Types;
using DroneStrikers.Game.Combat;
using UnityEngine;

namespace DroneStrikers.Game.Deprecated.Drone
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
        public float Experience => _droneUpgrader.Experience;
        public Team Team => _teamMember.Team;

        public float MaxHealth => _health.MaxHealth;
        public float CurrentHealth => _health.CurrentHealth;
        public float HealthPercent => _health.HealthPercent;

        private Transform _transform;

        [SerializeField] [RequiredField] private DroneControllerSelector _controllerSelector;
        [SerializeField] [RequiredField] private DroneUpgrader _droneUpgrader;
        [SerializeField] [RequiredField] private TeamMember _teamMember;
        private IHealth _health;

        private void Awake()
        {
            _transform = transform;
            _health = GetComponent<IHealth>();
        }
    }
}