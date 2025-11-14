using System.Collections.Generic;
using UnityEngine;

namespace DroneStrikers.Game.Drone
{
    [CreateAssetMenu(fileName = "UpgradeTree_", menuName = "Upgrades/Upgrade Tree")]
    public class UpgradeTreeSO : ScriptableObject
    {
        [SerializeField] private string _upgradeTreeName;
        public string UpgradeTreeName => _upgradeTreeName;

        [SerializeField] private List<UpgradeSO> _startingUpgrades = new();
        public IReadOnlyList<UpgradeSO> StartingUpgrades => _startingUpgrades;
    }
}