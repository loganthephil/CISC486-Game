using DroneStrikers.Core.Interfaces;
using DroneStrikers.Game.Drone;
using UnityEngine;

namespace DroneStrikers.Game.Combat
{
    [RequireComponent(typeof(DroneUpgrader))]
    public class DroneValueDangerProvider : MonoBehaviour, IValueDangerProvider
    {
        public float BaseValue => _droneUpgrader.ExperienceOnDestroy;
        public float DangerLevel => _droneUpgrader.Level; // TODO: More complex formula. Have it based on actual stats.

        private DroneUpgrader _droneUpgrader;

        private void Awake()
        {
            _droneUpgrader = GetComponent<DroneUpgrader>();
        }
    }
}