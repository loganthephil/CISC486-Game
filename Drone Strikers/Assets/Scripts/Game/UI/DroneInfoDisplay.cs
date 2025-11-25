using DroneStrikers.Core;
using DroneStrikers.Core.Editor;
using DroneStrikers.Events;
using DroneStrikers.Game.Drone;
using TMPro;
using UnityEngine;

namespace DroneStrikers.Game.UI
{
    public class DroneInfoDisplay : MonoBehaviour
    {
        [SerializeField] [RequiredField] private LocalEvents _localEvents;

        [SerializeField] [RequiredField] private TMP_Text _nameText;
        [SerializeField] [RequiredField] private TMP_Text _experienceText;

        private void OnEnable()
        {
            UpdateNameText("");
            UpdateExperienceText(0);
            _localEvents.Subscribe(DroneEvents.NameChanged, HandleDroneNameChanged);
            _localEvents.Subscribe(DroneEvents.ExperienceGained, HandleDroneExperienceChanged);
        }

        private void OnDisable()
        {
            _localEvents.Unsubscribe(DroneEvents.NameChanged, HandleDroneNameChanged);
            _localEvents.Unsubscribe(DroneEvents.ExperienceGained, HandleDroneExperienceChanged);
        }

        private void HandleDroneNameChanged(string newName) => UpdateNameText(newName);
        private void UpdateNameText(string newName) => _nameText.text = newName;

        private void HandleDroneExperienceChanged(float totalExperience) => UpdateExperienceText(totalExperience);
        private void UpdateExperienceText(float totalExperience) => _experienceText.text = totalExperience.ToAbbreviatedString();
    }
}