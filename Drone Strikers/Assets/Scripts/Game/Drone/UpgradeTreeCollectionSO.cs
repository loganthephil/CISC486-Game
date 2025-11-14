using System.Collections.Generic;
using UnityEngine;

namespace DroneStrikers.Game.Drone
{
    [CreateAssetMenu(fileName = "TreeCollection_", menuName = "Upgrades/Upgrade Tree Collection")]
    public class UpgradeTreeCollectionSO : ScriptableObject
    {
        [SerializeField] private List<UpgradeTreeSO> _upgradeTrees = new();

        private Dictionary<UpgradeSO, UpgradeTreeSO> _upgradeToTree;
        private Dictionary<string, UpgradeSO> _idToUpgrade;
        private Dictionary<UpgradeSO, string> _upgradeToId;

        /// <summary>
        ///     Returns a new list containing all upgrade trees in the collection.
        /// </summary>
        /// <returns> A new list of UpgradeTreeSO objects. </returns>
        public List<UpgradeTreeSO> GetUpgradeTrees() => new(_upgradeTrees);

        public void OnEnable()
        {
            // Recursively map all upgrades to their IDs from all trees
            _upgradeToTree = new Dictionary<UpgradeSO, UpgradeTreeSO>();
            _idToUpgrade = new Dictionary<string, UpgradeSO>();
            _upgradeToId = new Dictionary<UpgradeSO, string>();
            foreach (UpgradeTreeSO tree in _upgradeTrees)
            {
                List<UpgradeSO> upgrades = new();
                foreach (UpgradeSO startingUpgrade in tree.StartingUpgrades) upgrades.AddRange(GetAllUpgradesRecursively(startingUpgrade));

                foreach (UpgradeSO upgrade in upgrades)
                {
                    _upgradeToTree[upgrade] = tree;
                    _idToUpgrade[upgrade.UpgradeId] = upgrade;
                    _upgradeToId[upgrade] = upgrade.UpgradeId;
                }
            }
        }

        public bool TryGetUpgrade(string id, out UpgradeSO upgrade) => _idToUpgrade.TryGetValue(id, out upgrade);
        public string UpgradeToId(UpgradeSO upgrade) => _upgradeToId.GetValueOrDefault(upgrade);

        public UpgradeTreeSO GetTreeOfUpgrade(UpgradeSO upgrade) => _upgradeToTree.GetValueOrDefault(upgrade);

        private static List<UpgradeSO> GetAllUpgradesRecursively(UpgradeSO upgrade)
        {
            List<UpgradeSO> upgrades = new() { upgrade };
            foreach (UpgradeSO nextUpgrade in upgrade.NextUpgrades) upgrades.AddRange(GetAllUpgradesRecursively(nextUpgrade));
            return upgrades;
        }
    }
}