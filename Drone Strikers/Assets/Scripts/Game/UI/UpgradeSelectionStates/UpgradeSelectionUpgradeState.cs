using System.Collections.Generic;
using DroneStrikers.Game.Drone;
using UnityEngine;

namespace DroneStrikers.Game.UI.UpgradeSelectionStates
{
    public class UpgradeSelectionUpgradeState : UpgradeSelectionBaseState
    {
        public UpgradeSelectionUpgradeState(PlayerUpgradeSelector upgradeSelector) : base(upgradeSelector) { }

        public override void OnEnter()
        {
            ClearUI();

            IReadOnlyList<UpgradeSO> upgrades = _upgradeSelector.GetAvailableUpgradesInTree(_upgradeSelector.SelectedTree);

            // Create a UI element for each available upgrade
            foreach (UpgradeSO upgrade in upgrades)
            {
                GameObject selectableUIObj = Object.Instantiate(_upgradeSelector.UpgradeSelectionUIPrefab, _upgradeSelector.UpgradeSelectionUIParent);
                SelectableItemUI itemUI = selectableUIObj.GetComponent<SelectableItemUI>();
                if (itemUI == null) Debug.LogError("SelectableItemUI component missing on UpgradeSelectionUIPrefab.");
                itemUI.Initialize(upgrade.UpgradeName, () =>
                {
                    _upgradeSelector.SelectedTree = null;
                    _upgradeSelector.SelectUpgrade(upgrade);
                });
            }
        }
    }
}