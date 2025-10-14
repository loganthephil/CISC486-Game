using System.Collections.Generic;
using UnityEngine;

namespace DroneStrikers.Game.Upgrades
{
    [CreateAssetMenu(fileName = "Upgrade_", menuName = "Upgrades/Upgrade")]
    public class UpgradeSO : ScriptableObject
    {
        [SerializeField] private string _upgradeName;
        public string UpgradeName => _upgradeName;

        [Header("Stat Modifications")]
        [SerializeField] private List<StatUpgradeModifier> _modifiers = new();
        public IReadOnlyList<StatUpgradeModifier> Modifiers => _modifiers;

        [Header("Next Upgrades")]
        [SerializeField] private List<UpgradeSO> _nextUpgrades = new();
        public IReadOnlyList<UpgradeSO> NextUpgrades => _nextUpgrades;
    }
}