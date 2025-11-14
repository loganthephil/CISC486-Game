using System;
using System.Collections.Generic;
using System.Linq;
using DroneStrikers.Core.Editor;
using DroneStrikers.Core.Interfaces;
using DroneStrikers.Core.Types;
using DroneStrikers.Events;
using DroneStrikers.Game.Drone;
using DroneStrikers.Game.Player;
using UnityEngine;

namespace DroneStrikers.Game.Deprecated.Drone
{
    [RequireComponent(typeof(DroneStats))]
    public class DroneUpgrader : MonoBehaviour, IExperienceProvider, IDestructionContextReceiver
    {
        private static readonly Dictionary<int, float> ExperienceToNextLevelMap = new(); // Cache required experience since we call it a lot

        // Levels at which the player gains an upgrade point
        private static readonly HashSet<int> UpgradePointLevels = new()
        {
            5, 10, 15, 20, 25, 30, 40, 50, 75
        };

        public int Level { get; private set; } = 1;
        public float Experience { get; private set; }

        public int AvailableUpgradePoints { get; private set; }

        public float ExperienceOnDestroy => Experience * 0.75f; // Provide experience based on level (temporary formula)

        private List<UpgradeTreeSO> _upgradeTrees = new();
        public IReadOnlyList<UpgradeTreeSO> UpgradeTrees => _upgradeTrees;

        [SerializeField] [RequiredField] private UpgradeTreeCollectionSO _upgradeTreeCollectionSO;
        [SerializeField] [RequiredField] private MeshFilter _turretMeshFilter;
        [SerializeField] [RequiredField] private MeshFilter _bodyMeshFilter;
        [SerializeField] [RequiredField] private MeshFilter _movementMeshFilter;

        private DroneStats _droneStats;
        private LocalEvents _localEvents;

        private readonly Dictionary<UpgradeTreeSO, UpgradeSO> _lastUpgradeInTrees = new();

        /// <summary>
        ///     The amount of experience required to reach the next level.
        /// </summary>
        public float RequiredExperienceToNextLevel => ExperienceToLevel(Level + 1);

        /// <summary>
        ///     A percentage (0 to 1) representing the progress towards the next level, where 0 means no progress has been made since the last level up.
        /// </summary>
        public float ProgressToNextLevel => (Experience - ExperienceToLevel(Level)) / (ExperienceToLevel(Level + 1) - ExperienceToLevel(Level));

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
            // foreach (StatUpgradeModifier modifier in upgrade.Modifiers)
            //     _droneStats.AddModifier(modifier.Stat, modifier.ModType, modifier.Value, upgrade);

            // Update visuals of applicable mesh filter
            switch (upgrade.UpgradeType)
            {
                case UpgradeType.Turret:
                    if (upgrade.Mesh is not null) _turretMeshFilter.mesh = upgrade.Mesh;
                    break;
                case UpgradeType.Body:
                    if (upgrade.Mesh is not null) _bodyMeshFilter.mesh = upgrade.Mesh;
                    break;
                case UpgradeType.Movement:
                    if (upgrade.Mesh is not null) _movementMeshFilter.mesh = upgrade.Mesh;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

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

        private static float ExperienceToLevel(int level)
        {
            if (!ExperienceToNextLevelMap.ContainsKey(level))
                ExperienceToNextLevelMap[level] = 10f * Mathf.Pow(level - 1, 1.5f); // Tentative formula for required experience
            return ExperienceToNextLevelMap[level];
        }

        /// <summary>
        ///     Adds experience to the drone. Levels up if enough experience is gained.
        /// </summary>
        /// <param name="amount"> The amount of experience to add. </param>
        private void AddExperience(float amount)
        {
            Experience += amount;
            while (Experience >= RequiredExperienceToNextLevel)
            {
                Level++;
                if (UpgradePointLevels.Contains(Level))
                {
                    // Get an upgrade point if this level grants one
                    AvailableUpgradePoints++;
                    _localEvents.Invoke(PlayerEvents.UpgradePointGained, AvailableUpgradePoints);
                }

                _localEvents.Invoke(PlayerEvents.LevelUp, Level);
            }

            // Notify experience after leveling up due to subscribers possibly needing updated level info
            _localEvents.Invoke(PlayerEvents.ExperienceGained, amount);
        }
    }
}