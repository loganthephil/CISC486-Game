using DroneStrikers.Events;

namespace DroneStrikers.Game.Player
{
    // Event keys for drone-related events:

    /// <summary>
    ///     Event which is triggered when the player gains experience.
    ///     Accepts the new total experience as a float parameter.
    /// </summary>
    public sealed class ExperienceGainedEvent : EventKey<float> { }

    /// <summary>
    ///     Event which is triggered when the player levels up.
    ///     Accepts the new level as an int parameter.
    /// </summary>
    public sealed class LevelUpEvent : EventKey<int> { }

    /// <summary>
    ///     Event which is triggered when the player gains an upgrade point.
    ///     Accepts the new total remaining upgrade points as an int parameter.
    /// </summary>
    public sealed class UpgradePointGainedEvent : EventKey<int> { }

    // Static class to hold instances of combat events for convenient access
    // (eliminates the need to create new instances each time)
    public static class PlayerEvents
    {
        public static readonly ExperienceGainedEvent ExperienceGained = new();
        public static readonly LevelUpEvent LevelUp = new();
        public static readonly UpgradePointGainedEvent UpgradePointGained = new();
    }
}