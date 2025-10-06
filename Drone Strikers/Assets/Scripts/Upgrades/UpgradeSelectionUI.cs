using DroneStrikers.Player;
using TMPro;
using UnityEngine;

namespace DroneStrikers.Upgrades
{
    public class UpgradeSelectionUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _upgradeNameText;

        public UpgradeSO Upgrade;
        public PlayerUpgradeSelection PlayerUpgradeSelection;

        private void Start()
        {
            if (Upgrade == null)
            {
                Debug.LogError("UpgradeTis not assigned in UpgradeSelectionUI.");
                return;
            }

            _upgradeNameText.text = Upgrade.UpgradeName;
        }

        /// <summary>
        ///     Click event handler for when the player selects this upgrade.
        /// </summary>
        public void OnPlayerClick()
        {
            PlayerUpgradeSelection.SelectUpgrade(Upgrade);
        }
    }
}