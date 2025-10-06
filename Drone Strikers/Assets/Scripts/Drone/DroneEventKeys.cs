using DroneStrikers.Events;

namespace DroneStrikers.Drone
{
    // Event keys for drone-related events:

    /// <summary>
    ///     Event which is triggered when the drone gains experience.
    ///     Accepts the amount of experience gained as a float parameter.
    /// </summary>
    public sealed class ExperienceGainedEvent : EventKey<float> { }

    /// <summary>
    ///     Event which is triggered when the drone levels up.
    ///     Accepts the new level as an int parameter.
    /// </summary>
    public sealed class LevelUpEvent : EventKey<int> { }

    // Static class to hold instances of combat events for convenient access
    // (eliminates the need to create new instances each time)
    public static class DroneEvents
    {
        public static readonly ExperienceGainedEvent ExperienceGained = new();
        public static readonly LevelUpEvent LevelUp = new();
    }
}