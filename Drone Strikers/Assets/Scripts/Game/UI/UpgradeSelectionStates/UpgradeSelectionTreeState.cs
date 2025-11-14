using System.Collections.Generic;
using DroneStrikers.Game.Drone;
using UnityEngine;

namespace DroneStrikers.Game.UI.UpgradeSelectionStates
{
    public class UpgradeSelectionTreeState : UpgradeSelectionBaseState
    {
        public UpgradeSelectionTreeState(PlayerUpgradeSelector upgradeSelector) : base(upgradeSelector) { }

        public override void OnEnter()
        {
            ClearUI();

            IReadOnlyList<UpgradeTreeSO> trees = _upgradeSelector.UpgradeTrees;

            // Create a UI element for each upgrade tree
            foreach (UpgradeTreeSO tree in trees)
            {
                // Only show trees that have available upgrades
                if (!_upgradeSelector.HasAvailableUpgradesInTree(tree)) continue;

                GameObject selectableUIObj = Object.Instantiate(_upgradeSelector.UpgradeSelectionUIPrefab, _upgradeSelector.UpgradeSelectionUIParent);
                SelectableItemUI itemUI = selectableUIObj.GetComponent<SelectableItemUI>();
                if (itemUI == null)
                {
                    Debug.LogError("SelectableItemUI component missing on UpgradeSelectionUIPrefab.");
                    continue;
                }

                itemUI.Initialize(tree.UpgradeTreeName, () =>
                {
                    _upgradeSelector.SelectedTree = tree;
                });
            }
        }
    }
}