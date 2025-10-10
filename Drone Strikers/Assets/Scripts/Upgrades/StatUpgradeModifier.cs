using System;
using DroneStrikers.Stats;
using UnityEngine;

namespace DroneStrikers.Upgrades
{
    [Serializable]
    public struct StatUpgradeModifier
    {
        public StatTypeSO Stat;
        [Tooltip("Positive will increase the stat, negative will decrease it. For example, if using a percentage modifier, -0.1 will reduce the stat by 10%.")]
        public float Value;
        [Tooltip("Flat is added first. PercentAdditive are all summed before being multiplied onto the stat. PercentMultiplicative are all multiplied individually last.")]
        public StatModType ModType;
    }
}