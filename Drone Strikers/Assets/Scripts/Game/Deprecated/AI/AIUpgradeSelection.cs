using System.Collections;
using System.Collections.Generic;
using DroneStrikers.Core.Editor;
using DroneStrikers.Events;
using DroneStrikers.Game.Drone;
using DroneStrikers.Game.Upgrades;
using UnityEngine;

namespace DroneStrikers.Game.AI
{
    public class AIUpgradeSelection : MonoBehaviour
    {
        [field: SerializeField] [field: RequiredField]
        public DroneUpgrader DroneUpgrader { get; private set; }
        [SerializeField] [RequiredField] private LocalEvents _localEvents;

        private int _remainingUpgrades;

        private Coroutine _currentUpgradeSelectionCoroutine;

        private void OnEnable() => _localEvents.Subscribe(DroneEvents.UpgradePointGained, OnDroneUpgradePointGained);
        private void OnDisable() => _localEvents.Unsubscribe(DroneEvents.UpgradePointGained, OnDroneUpgradePointGained);

        /// <summary>
        ///     Event handler for when the drone gains an upgrade point.
        /// </summary>
        private void OnDroneUpgradePointGained(int upgradePoints)
        {
            _remainingUpgrades = upgradePoints;

            // Only start the coroutine if it's not already running
            _currentUpgradeSelectionCoroutine ??= StartCoroutine(SelectUpgrades());
        }

        private IEnumerator SelectUpgrades()
        {
            float maxDelay = 15f; // Max delay for first upgrade to simulate noticing there are available upgrades

            // For each remaining upgrade point, wait a bit and then select an upgrade
            while (_remainingUpgrades > 0)
            {
                // Wait for variable time depending on whether it's the first upgrade since calling the coroutine or a subsequent one
                yield return new WaitForSeconds(Random.Range(2f, maxDelay));

                // Apply a random upgrade from the available trees
                if (!SelectAndApplyUpgrade()) break; // If no upgrade was applied, exit loop

                // Decrease max delay to simulate decision-making instead of noticing that there are upgrades
                maxDelay = 8f;
            }

            _currentUpgradeSelectionCoroutine = null; // Reset coroutine reference when done
        }

        private bool SelectAndApplyUpgrade()
        {
            List<UpgradeTreeSO> availableTrees = DroneUpgrader.GetTreesWithAvailableUpgrades();
            if (availableTrees.Count == 0) return false; // No available upgrade trees

            // Select a random upgrade tree
            UpgradeTreeSO selectedTree = availableTrees[Random.Range(0, availableTrees.Count)];

            // Select a random available upgrade from the selected tree
            List<UpgradeSO> availableUpgrades = DroneUpgrader.GetAvailableUpgradesInTree(selectedTree);
            UpgradeSO selectedUpgrade = availableUpgrades[Random.Range(0, availableUpgrades.Count)];

            // Apply the selected upgrade
            if (!DroneUpgrader.ApplyUpgrade(selectedUpgrade, selectedTree)) return false; // If for some reason it fails, return false
            _remainingUpgrades--;

            return true;
        }
    }
}