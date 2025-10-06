using System.Collections.Generic;
using DroneStrikers.Drone;
using DroneStrikers.Events;
using DroneStrikers.Upgrades;
using UnityEngine;

namespace DroneStrikers.Player
{
    [RequireComponent(typeof(DroneUpgrader))]
    public class PlayerUpgradeSelection : MonoBehaviour
    {
        // TODO: Pre instantiate UI elements and pool or disable/enable instead of instantiating/destroying
        private DroneUpgrader _droneUpgrader;
        private LocalEvents _localEvents;

        private bool _showingTreeSelection;
        private bool _showingUpgradeSelection;

        [SerializeField] private Transform _upgradeSelectionUIParent;

        [SerializeField] private GameObject _treeSelectionUIPrefab;
        [SerializeField] private GameObject _upgradeSelectionUIPrefab;

        private UpgradeTreeSO _selectedTree;

        private void Awake()
        {
            _droneUpgrader = GetComponent<DroneUpgrader>();
            _localEvents = GetComponent<LocalEvents>();
        }

        private void OnEnable() => _localEvents.Subscribe(DroneEvents.LevelUp, OnPlayerLevelUp);
        private void OnDisable() => _localEvents.Unsubscribe(DroneEvents.LevelUp, OnPlayerLevelUp);

        /// <summary>
        ///     Event handler for when the player levels up.
        /// </summary>
        private void OnPlayerLevelUp(int level)
        {
            ShowUpgradeTrees();
        }

        /// <summary>
        ///     Called when the player selects an upgrade tree to view available upgrades.
        /// </summary>
        /// <param name="tree"> The selected upgrade tree. </param>
        public void SelectTree(UpgradeTreeSO tree)
        {
            _selectedTree = tree;
            ShowUpgradeSelectionsInTree(tree);
        }

        /// <summary>
        ///     Called when the player selects an upgrade to apply.
        /// </summary>
        /// <param name="upgrade"> The selected upgrade. </param>
        public void SelectUpgrade(UpgradeSO upgrade)
        {
            _droneUpgrader.ApplyUpgrade(upgrade, _selectedTree);

            // After applying the upgrade, check if more upgrades are available
            if (_droneUpgrader.IsUpgradeAvailable())
                ShowUpgradeTrees(); // If so, show the upgrade trees again
            else
                ClearUpgradeSelectionUI(); // Otherwise, clear the UI
        }

        private void ShowUpgradeTrees()
        {
            if (_showingTreeSelection) return; // Already showing trees

            ClearUpgradeSelectionUI();
            IReadOnlyList<UpgradeTreeSO> trees = _droneUpgrader.UpgradeTrees;

            // Create a UI element for each upgrade tree
            foreach (UpgradeTreeSO tree in trees)
            {
                GameObject treeUIObj = Instantiate(_treeSelectionUIPrefab, _upgradeSelectionUIParent);
                UpgradeTreeUI treeUI = treeUIObj.GetComponent<UpgradeTreeUI>();
                treeUI.UpgradeTree = tree;
                treeUI.PlayerUpgradeSelection = this;
            }

            _showingTreeSelection = true;
        }

        private void ShowUpgradeSelectionsInTree(UpgradeTreeSO tree)
        {
            if (_showingUpgradeSelection) return; // Already showing upgrades

            ClearUpgradeSelectionUI();
            IReadOnlyList<UpgradeSO> upgrades = _droneUpgrader.GetAvailableUpgradesInTree(tree);

            // Create a UI element for each available upgrade
            foreach (UpgradeSO upgrade in upgrades)
            {
                GameObject upgradeUIObj = Instantiate(_upgradeSelectionUIPrefab, _upgradeSelectionUIParent);
                UpgradeSelectionUI upgradeUI = upgradeUIObj.GetComponent<UpgradeSelectionUI>();
                upgradeUI.Upgrade = upgrade;
                upgradeUI.PlayerUpgradeSelection = this;
            }

            _showingUpgradeSelection = true;
        }

        private void ClearUpgradeSelectionUI()
        {
            _showingTreeSelection = false;
            _showingUpgradeSelection = false;
            foreach (Transform child in _upgradeSelectionUIParent) Destroy(child.gameObject);
        }
    }
}