namespace DroneStrikers.Stats
{
    public enum StatModType
    {
        Flat, // Added before any multipliers
        Additive, // Summed, then used as (1 + sumAdditive)
        Multiplicative // Multiplied together as factors
    }
}