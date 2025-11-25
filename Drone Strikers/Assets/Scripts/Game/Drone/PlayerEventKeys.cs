using DroneStrikers.Events;

namespace DroneStrikers.Game.Drone
{
    // Event keys for drone-related events:

    public sealed class NameChangedEvent : EventKey<string> { }

    public sealed class ExperienceGainedEvent : EventKey<float> { }

    public sealed class LevelUpEvent : EventKey<int> { }

    public sealed class UpgradePointGainedEvent : EventKey<int> { }

    // Static class to hold instances of combat events for convenient access
    // (eliminates the need to create new instances each time)
    public static class DroneEvents
    {
        /// <summary>
        ///     Event which is triggered when the drone's name changes.
        ///     Accepts the new name as a string parameter.
        /// </summary>
        public static readonly NameChangedEvent NameChanged = new();
        /// <summary>
        ///     Event which is triggered when the drone gains experience.
        ///     Accepts the new total experience as a float parameter.
        /// </summary>
        public static readonly ExperienceGainedEvent ExperienceGained = new();
        /// <summary>
        ///     Event which is triggered when the drone levels up.
        ///     Accepts the new level as an int parameter.
        /// </summary>
        public static readonly LevelUpEvent LevelUp = new();
        /// <summary>
        ///     Event which is triggered when the drone gains an upgrade point.
        ///     Accepts the new total remaining upgrade points as an int parameter.
        /// </summary>
        public static readonly UpgradePointGainedEvent UpgradePointGained = new();
    }
}