using System.Collections.Generic;
using UnityEngine;

namespace DroneStrikers.Upgrades
{
    [CreateAssetMenu(fileName = "TreeCollection_", menuName = "Upgrades/Upgrade Tree Collection")]
    public class UpgradeTreeCollectionSO : ScriptableObject
    {
        [SerializeField] private List<UpgradeTreeSO> _upgradeTrees = new();

        /// <summary>
        ///     Returns a new list containing all upgrade trees in the collection.
        /// </summary>
        /// <returns> A new list of UpgradeTreeSO objects. </returns>
        public List<UpgradeTreeSO> GetUpgradeTrees() => new(_upgradeTrees);
    }
}