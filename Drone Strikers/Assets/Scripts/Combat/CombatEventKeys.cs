using DroneStrikers.Events;

namespace DroneStrikers.Combat
{
    // Event keys for combat-related events:

    /// <summary>
    ///     Event which is triggered when an object takes damage.
    /// </summary>
    public sealed class DamageTakenEvent : EventKey<DamageContext> { }

    /// <summary>
    ///     Event which is triggered when an object is destroyed.
    /// </summary>
    public sealed class DestroyedEvent : EventKey<ObjectDestructionContext> { }

    /// <summary>
    ///     Event which is triggered when an object's team changes.
    /// </summary>
    public sealed class TeamChangedEvent : EventKey<Team> { }

    // Static class to hold instances of combat events for convenient access
    // (eliminates the need to create new instances each time)
    public static class CombatEvents
    {
        public static readonly DamageTakenEvent DamageTaken = new();
        public static readonly DestroyedEvent Destroyed = new();
        public static readonly TeamChangedEvent TeamChanged = new();
    }
}