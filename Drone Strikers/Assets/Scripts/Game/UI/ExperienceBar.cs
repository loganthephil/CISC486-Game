using DroneStrikers.Core.Editor;
using DroneStrikers.Events;
using DroneStrikers.Game.Drone;
using UnityEngine;

namespace DroneStrikers.Game.UI
{
    public class ExperienceBar : ProgressBar
    {
        [SerializeField] [RequiredField] private LocalEvents _localEvents;
        [SerializeField] [RequiredField] private NetworkedDrone _drone;

        private void Start()
        {
            UpdateUI();
        }

        private void OnEnable() => _localEvents.Subscribe(DroneEvents.ExperienceGained, OnPlayerExperienceGained);
        private void OnDisable() => _localEvents.Unsubscribe(DroneEvents.ExperienceGained, OnPlayerExperienceGained);

        private void OnPlayerExperienceGained(float totalExperience)
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            UpdateValue(_drone.CurrentState.progressToNextLevel, 1f);
        }
    }
}