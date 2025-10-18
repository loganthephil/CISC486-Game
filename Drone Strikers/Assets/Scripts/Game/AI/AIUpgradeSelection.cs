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
            while (_remainingUpgrades > 0)
            {
                // Wait for variable frame time to simulate thinking time
                yield return new WaitForSeconds(Random.Range(1f, 5f));

                // Select a random upgrade tree
                List<UpgradeTreeSO> availableTrees = DroneUpgrader.GetTreesWithAvailableUpgrades();
                if (availableTrees.Count == 0) yield break; // No available upgrades left
                UpgradeTreeSO selectedTree = availableTrees[Random.Range(0, availableTrees.Count)];

                // Select a random available upgrade from the selected tree
                List<UpgradeSO> availableUpgrades = DroneUpgrader.GetAvailableUpgradesInTree(selectedTree);
                UpgradeSO selectedUpgrade = availableUpgrades[Random.Range(0, availableUpgrades.Count)];

                // Apply the selected upgrade
                DroneUpgrader.ApplyUpgrade(selectedUpgrade, selectedTree);
                _remainingUpgrades--;
            }
        }
    }
}