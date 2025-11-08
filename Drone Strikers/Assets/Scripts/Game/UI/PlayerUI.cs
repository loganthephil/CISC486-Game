using DroneStrikers.Events;
using DroneStrikers.Game.Drone;
using TMPro;
using UnityEngine;

namespace DroneStrikers.Game.UI
{
    public class PlayerUI : MonoBehaviour
    {
        [SerializeField] private DroneUpgrader _droneUpgrader;
        [SerializeField] private LocalEvents _localEvents;

        [SerializeField] private TMP_Text _experienceText;
        [SerializeField] private TMP_Text _levelText;

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
            _experienceText.text = ((int)_droneUpgrader.Experience).ToString();
            // _experienceText.text = (int)_droneUpgrader.Experience + " / " + Mathf.Ceil(_droneUpgrader.RequiredExperienceToNextLevel);
            _levelText.text = "Level: " + _droneUpgrader.Level;
        }
    }
}