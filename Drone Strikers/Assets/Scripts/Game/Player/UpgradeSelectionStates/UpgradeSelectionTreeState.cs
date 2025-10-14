using System.Collections.Generic;
using DroneStrikers.Game.UI;
using DroneStrikers.Game.Upgrades;
using UnityEngine;

namespace DroneStrikers.Game.Player.UpgradeSelectionStates
{
    public class UpgradeSelectionTreeState : UpgradeSelectionBaseState
    {
        public UpgradeSelectionTreeState(PlayerUpgradeSelection upgradeSelection) : base(upgradeSelection) { }

        public override void OnEnter()
        {
            ClearUI();

            IReadOnlyList<UpgradeTreeSO> trees = _upgradeSelection.DroneUpgrader.UpgradeTrees;

            // Create a UI element for each upgrade tree
            foreach (UpgradeTreeSO tree in _upgradeSelection.DroneUpgrader.UpgradeTrees)
            {
                // Only show trees that have available upgrades
                if (!_upgradeSelection.DroneUpgrader.HasAvailableUpgradesInTree(tree)) continue;

                GameObject selectableUIObj = Object.Instantiate(_upgradeSelection.UpgradeSelectionUIPrefab, _upgradeSelection.UpgradeSelectionUIParent);
                SelectableItemUI itemUI = selectableUIObj.GetComponent<SelectableItemUI>();
                if (itemUI == null)
                {
                    Debug.LogError("SelectableItemUI component missing on UpgradeSelectionUIPrefab.");
                    continue;
                }

                itemUI.Initialize(tree.UpgradeTreeName, () => { _upgradeSelection.SelectedTree = tree; });
            }
        }
    }
}