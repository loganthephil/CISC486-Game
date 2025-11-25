using System.Globalization;
using DroneStrikers.Core.Editor;
using DroneStrikers.Events;
using DroneStrikers.Game.Drone;
using TMPro;
using UnityEngine;

namespace DroneStrikers.Game.UI
{
    public class PlayerUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _experienceText;
        [SerializeField] private TMP_Text _levelText;

        [SerializeField] [RequiredField] private LocalEvents _localEvents;

        private void Start()
        {
            OnPlayerExperienceGained(0);
            OnPlayerLevelUp(1);
        }

        private void OnEnable()
        {
            _localEvents.Subscribe(DroneEvents.ExperienceGained, OnPlayerExperienceGained);
            _localEvents.Subscribe(DroneEvents.LevelUp, OnPlayerLevelUp);
        }

        private void OnDisable()
        {
            _localEvents.Unsubscribe(DroneEvents.ExperienceGained, OnPlayerExperienceGained);
            _localEvents.Unsubscribe(DroneEvents.LevelUp, OnPlayerLevelUp);
        }

        private void OnPlayerExperienceGained(float totalExperience) => _experienceText.text = ((int)totalExperience).ToString("N0", CultureInfo.InvariantCulture);
        private void OnPlayerLevelUp(int newLevel) => _levelText.text = "Level: " + newLevel;
    }
}