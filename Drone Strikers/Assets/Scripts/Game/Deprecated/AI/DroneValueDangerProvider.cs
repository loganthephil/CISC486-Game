using DroneStrikers.Core.Editor;
using DroneStrikers.Core.Interfaces;
using DroneStrikers.Game.Deprecated.Drone;
using UnityEngine;

namespace DroneStrikers.Game.Deprecated.AI
{
    public class DroneValueDangerProvider : MonoBehaviour, IValueDangerProvider
    {
        [SerializeField] [RequiredField] private DroneUpgrader _droneUpgrader;

        public float BaseValue => _droneUpgrader.ExperienceOnDestroy;
        public float DangerLevel => _droneUpgrader.Level; // TODO: More complex formula. Have it based on actual stats.
    }
}