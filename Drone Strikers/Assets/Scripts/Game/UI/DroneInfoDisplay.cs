using DroneStrikers.Core.Editor;
using DroneStrikers.Events;
using DroneStrikers.Game.Drone;
using TMPro;
using UnityEngine;

namespace DroneStrikers.Game.UI
{
    public class DroneInfoDisplay : MonoBehaviour
    {
        [SerializeField] [RequiredField] private DroneInfoProvider _droneInfoProvider;
        [SerializeField] [RequiredField] private LocalEvents _localEvents;

        [SerializeField] [RequiredField] private TMP_Text _experienceText;

        private void OnEnable()
        {
            _localEvents.Subscribe(DroneEvents.ExperienceGained, HandleDroneExperienceChanged);
            UpdateExperienceText();
        }

        private void OnDisable() => _localEvents.Unsubscribe(DroneEvents.ExperienceGained, HandleDroneExperienceChanged);

        private void HandleDroneExperienceChanged(float experienceGained) => UpdateExperienceText();
        private void UpdateExperienceText() => _experienceText.text = ((int)_droneInfoProvider.Experience).ToString();
    }
}