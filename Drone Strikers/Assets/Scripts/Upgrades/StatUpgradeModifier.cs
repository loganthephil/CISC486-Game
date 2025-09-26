using System;
using DroneStrikers.Stats;

namespace DroneStrikers.Upgrades
{
    [Serializable]
    public struct StatUpgradeModifier
    {
        public StatId Stat;
        public StatModType ModType;
        public float Value;
    }
}