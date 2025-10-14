using System.Collections.Generic;
using DroneStrikers.Game.UI;
using DroneStrikers.Game.Upgrades;
using UnityEngine;

namespace DroneStrikers.Game.Player.UpgradeSelectionStates
{
    public class UpgradeSelectionUpgradeState : UpgradeSelectionBaseState
    {
        public UpgradeSelectionUpgradeState(PlayerUpgradeSelection upgradeSelection) : base(upgradeSelection) { }

        public override void OnEnter()
        {
            ClearUI();

            IReadOnlyList<UpgradeSO> upgrades = _upgradeSelection.DroneUpgrader.GetAvailableUpgradesInTree(_upgradeSelection.SelectedTree);

            // Create a UI element for each available upgrade
            foreach (UpgradeSO upgrade in upgrades)
            {
                GameObject selectableUIObj = Object.Instantiate(_upgradeSelection.UpgradeSelectionUIPrefab, _upgradeSelection.UpgradeSelectionUIParent);
                SelectableItemUI itemUI = selectableUIObj.GetComponent<SelectableItemUI>();
                if (itemUI == null) Debug.LogError("SelectableItemUI component missing on UpgradeSelectionUIPrefab.");
                itemUI.Initialize(upgrade.UpgradeName, () => { _upgradeSelection.SelectUpgrade(upgrade); });
            }
        }
    }
}