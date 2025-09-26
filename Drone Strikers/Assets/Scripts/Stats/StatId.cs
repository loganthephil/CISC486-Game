namespace DroneStrikers.Stats
{
    /// <summary>
    ///     Possible IDs for different stats.
    /// </summary>
    public enum StatId
    {
        // -- Drone --
        MaxHealth,

        // -- Movement --
        MoveSpeed,
        MoveAcceleration,
        MoveDeceleration,

        // -- Weapon --
        FireCooldown,
        AimSpeed,
        Recoil,

        // -- Attack --
        AttackDamage,
        AttackVelocity,
        AttackPierce
    }
}