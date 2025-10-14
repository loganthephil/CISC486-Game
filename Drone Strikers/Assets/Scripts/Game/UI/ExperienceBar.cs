using DroneStrikers.Events;
using DroneStrikers.Game.Drone;
using UnityEngine;

namespace DroneStrikers.Game.UI
{
    public class ExperienceBar : ProgressBar
    {
        [SerializeField] private DroneUpgrader _droneUpgrader;
        [SerializeField] private LocalEvents _localEvents;

        private void Start()
        {
            UpdateUI();
        }

        private void OnEnable() => _localEvents.Subscribe(DroneEvents.ExperienceGained, OnPlayerExperienceGained);
        private void OnDisable() => _localEvents.Unsubscribe(DroneEvents.ExperienceGained, OnPlayerExperienceGained);

        private void OnPlayerExperienceGained(float experienceGained)
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            UpdateValue(_droneUpgrader.ProgressToNextLevel, 1f);
        }
    }
}