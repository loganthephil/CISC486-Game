using DroneStrikers.Drone;
using TMPro;
using UnityEngine;

namespace DroneStrikers.UI
{
    public class PlayerUI : MonoBehaviour
    {
        [SerializeField] private DroneUpgrader _droneUpgrader;

        [SerializeField] private TMP_Text _experienceText;
        [SerializeField] private TMP_Text _levelText;

        private void Update()
        {
            // TODO: Tie to events instead of updating every frame
            _experienceText.text = ((int)_droneUpgrader.Experience).ToString();
            _levelText.text = "Level: " + _droneUpgrader.Level;
        }
    }
}