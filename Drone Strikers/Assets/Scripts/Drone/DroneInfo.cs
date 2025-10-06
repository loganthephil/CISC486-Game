using DroneStrikers.Combat;
using UnityEngine;

namespace DroneStrikers.Drone
{
    public class DroneInfo : MonoBehaviour
    {
        public Vector3 Position => _transform.position;
        public int Level => _droneUpgrader?.Level ?? 0;
        public Team Team => _teamMember?.Team ?? Team.Neutral;

        private Transform _transform;
        private DroneUpgrader _droneUpgrader;
        private TeamMember _teamMember;

        private void Awake()
        {
            _transform = transform;
            _droneUpgrader = GetComponent<DroneUpgrader>();
            _teamMember = GetComponent<TeamMember>();
        }
    }
}