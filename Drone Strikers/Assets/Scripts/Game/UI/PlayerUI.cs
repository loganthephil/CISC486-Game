using DroneStrikers.Core.Editor;
using DroneStrikers.Events;
using DroneStrikers.Game.Player;
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
            _localEvents.Subscribe(PlayerEvents.ExperienceGained, OnPlayerExperienceGained);
            _localEvents.Subscribe(PlayerEvents.LevelUp, OnPlayerLevelUp);
        }

        private void OnDisable()
        {
            _localEvents.Unsubscribe(PlayerEvents.ExperienceGained, OnPlayerExperienceGained);
            _localEvents.Unsubscribe(PlayerEvents.LevelUp, OnPlayerLevelUp);
        }

        private void OnPlayerExperienceGained(float totalExperience) => _experienceText.text = ((int)totalExperience).ToString();
        private void OnPlayerLevelUp(int newLevel) => _levelText.text = "Level: " + newLevel;
    }
}