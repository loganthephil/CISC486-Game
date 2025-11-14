using System;

// ReSharper disable InconsistentNaming

namespace DroneStrikers.Networking
{
    [Serializable]
    public class UpgradeAppliedMessage
    {
        public string droneId;
        public string upgradeId;
    }
}