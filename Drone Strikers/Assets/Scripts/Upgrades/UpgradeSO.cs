using System.Collections.Generic;
using UnityEngine;

namespace DroneStrikers.Upgrades
{
    [CreateAssetMenu(fileName = "UpgradeSO", menuName = "Scriptable Objects/UpgradeSO")]
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