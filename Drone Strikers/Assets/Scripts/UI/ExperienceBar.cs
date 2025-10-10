using DroneStrikers.Drone;
using UnityEngine;

namespace DroneStrikers.UI
{
    public class ExperienceBar : ProgressBar
    {
        [SerializeField] private DroneUpgrader _droneUpgrader;

        private void Update()
        {
            // TODO: Optimize to only update on experience change event
            UpdateValue(_droneUpgrader.ProgressToNextLevel, 1f);
        }
    }
}