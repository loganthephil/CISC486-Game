using DroneStrikers.Core.Editor;
using DroneStrikers.Core.Interfaces;
using DroneStrikers.Game.Drone;
using UnityEngine;

namespace DroneStrikers.Game.AI
{
    public class DroneValueDangerProvider : MonoBehaviour, IValueDangerProvider
    {
        [SerializeField] [RequiredField] private DroneUpgrader _droneUpgrader;

        public float BaseValue => _droneUpgrader.ExperienceOnDestroy;
        public float DangerLevel => _droneUpgrader.Level; // TODO: More complex formula. Have it based on actual stats.
    }
}