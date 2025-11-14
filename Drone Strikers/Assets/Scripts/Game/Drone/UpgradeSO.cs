using System.Collections.Generic;
using UnityEngine;

namespace DroneStrikers.Game.Drone
{
    [CreateAssetMenu(fileName = "Upgrade_", menuName = "Upgrades/Upgrade")]
    public class UpgradeSO : ScriptableObject
    {
        [SerializeField] private string _upgradeId;
        public string UpgradeId => _upgradeId;

        [SerializeField] private string _upgradeName;
        public string UpgradeName => _upgradeName;

        [SerializeField] private UpgradeType _upgradeType;
        public UpgradeType UpgradeType => _upgradeType;

        [Header("Next Upgrades")]
        [SerializeField] private List<UpgradeSO> _nextUpgrades = new();
        public IReadOnlyList<UpgradeSO> NextUpgrades => _nextUpgrades;

        [Header("Visuals")]
        [SerializeField] private Mesh _mesh;
        public Mesh Mesh => _mesh;
    }
}