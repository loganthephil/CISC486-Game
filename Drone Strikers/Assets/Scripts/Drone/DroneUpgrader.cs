using System;
using System.Collections.Generic;
using System.Linq;
using DroneStrikers.Combat;
using DroneStrikers.Events;
using DroneStrikers.Upgrades;
using UnityEngine;

namespace DroneStrikers.Drone
{
    [RequireComponent(typeof(DroneStats))]
    public class DroneUpgrader : MonoBehaviour, IExperienceProvider, IDestructionContextReceiver
    {
        private static float ExperienceToLevel(float level) => 10f * Mathf.Pow(level, 1.5f); // Temporary formula for required experience

        private DroneStats _droneStats;
        private LocalEvents _localEvents;

        // TODO: Revisit leveling system and experience requirements later. This is just a placeholder.
        public int Level { get; private set; } = 1;
        public float Experience { get; private set; }

        public int AvailableUpgradePoints { get; private set; }

        public float ExperienceOnDestroy => Experience * 0.75f; // Provide experience based on level (temporary formula)

        [SerializeField] private UpgradeTreeCollectionSO _upgradeTreeCollectionSO;

        private List<UpgradeTreeSO> _upgradeTrees = new();
        public IReadOnlyList<UpgradeTreeSO> UpgradeTrees => _upgradeTrees;

        private readonly Dictionary<UpgradeTreeSO, UpgradeSO> _lastUpgradeInTrees = new();

        /// <summary>
        ///     The amount of experience required to reach the next level.
        /// </summary>
        public float RequiredExperienceToNextLevel => ExperienceToLevel(Level);

        /// <summary>
        ///     A percentage (0 to 1) representing the progress towards the next level, where 0 means no progress has been made since the last level up.
        /// </summary>
        public float ProgressToNextLevel => (Experience - ExperienceToLevel(Level - 1)) / ExperienceToLevel(Level);

        private void Awake()
        {
            _droneStats = GetComponent<DroneStats>();
            _localEvents = GetComponent<LocalEvents>();
        }

        private void Start()
        {
            // Initialize upgrade trees
            _upgradeTrees = _upgradeTreeCollectionSO.GetUpgradeTrees();
        }

        public void HandleDestructionContext(in ObjectDestructionContext context)
        {
            AddExperience(context.ExperienceToAward);
        }

        public bool IsUpgradeAvailable() => AvailableUpgradePoints > 0 && _upgradeTrees.Any(tree => GetAvailableUpgradesInTree(tree).Count > 0);

        /// <summary>
        ///     Apply a given upgrade to the drone.
        ///     Requires to specify the upgrade tree the upgrade belongs to.
        ///     Verifies if the upgrade is available in the specified tree and if there are enough upgrade points.
        /// </summary>
        /// <param name="upgrade"> The upgrade to apply. </param>
        /// <param name="upgradeTree"> The upgrade tree the upgrade belongs to. </param>
        /// <returns> True if the upgrade was applied, false otherwise. </returns>
        public bool ApplyUpgrade(UpgradeSO upgrade, UpgradeTreeSO upgradeTree)
        {
            if (upgrade == null) throw new ArgumentNullException(nameof(upgrade));
            if (upgradeTree == null) throw new ArgumentNullException(nameof(upgradeTree));

            if (!IsUpgradeAvailable()) return false; // No upgrade points available
            if (!GetAvailableUpgradesInTree(upgradeTree).Contains(upgrade)) return false; // Upgrade not available in the specified tree

            _lastUpgradeInTrees[upgradeTree] = upgrade; // Set the last upgrade in this tree to the applied upgrade

            // Apply each modifier to the drone stats with the upgrade as the source
            foreach (StatUpgradeModifier modifier in upgrade.Modifiers)
                _droneStats.AddModifier(modifier.Stat, modifier.ModType, modifier.Value, upgrade);

            // Consume an upgrade point and return true
            AvailableUpgradePoints--;
            return true;
        }

        /// <summary>
        ///     Returns a list of available upgrades in the given upgrade tree.
        /// </summary>
        /// <param name="tree"> The upgrade tree to get available upgrades from. </param>
        /// <returns> A list of available upgrades in the given upgrade tree. </returns>
        public List<UpgradeSO> GetAvailableUpgradesInTree(UpgradeTreeSO tree)
        {
            // If there has been an upgrade in this tree, return its next upgrades
            if (_lastUpgradeInTrees.TryGetValue(tree, out UpgradeSO lastUpgradeInTree))
            {
                if (lastUpgradeInTree == null) throw new Exception("Last upgrade in tree is null, this should never happen.");
                return new List<UpgradeSO>(lastUpgradeInTree.NextUpgrades);
            }

            // Otherwise, return the starting upgrades
            return new List<UpgradeSO>(tree.StartingUpgrades);
        }

        /// <summary>
        ///     Returns true if there are available upgrades in the given upgrade tree.
        /// </summary>
        /// <param name="tree"> The upgrade tree to check. </param>
        /// <returns> True if there are available upgrades in the given upgrade tree, false otherwise. </returns>
        public bool HasAvailableUpgradesInTree(UpgradeTreeSO tree) => GetAvailableUpgradesInTree(tree).Count > 0;

        /// <summary>
        ///     Returns a list of upgrade trees that have available upgrades.
        /// </summary>
        /// <returns> A list of upgrade trees that have available upgrades. </returns>
        public List<UpgradeTreeSO> GetTreesWithAvailableUpgrades() => _upgradeTrees.Where(HasAvailableUpgradesInTree).ToList();

        /// <summary>
        ///     Adds experience to the drone. Levels up if enough experience is gained.
        /// </summary>
        /// <param name="amount"> The amount of experience to add. </param>
        private void AddExperience(float amount)
        {
            _localEvents.Invoke(DroneEvents.ExperienceGained, amount);
            Experience += amount;
            while (Experience >= RequiredExperienceToNextLevel)
            {
                Level++;
                AvailableUpgradePoints++; // Refine later. May want to limit upgrade points at later levels.
                _localEvents.Invoke(DroneEvents.LevelUp, Level);
            }
        }
    }
}