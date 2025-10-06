namespace DroneStrikers.Stats
{
    public enum StatModType
    {
        Flat, // Added before any multipliers
        AdditiveMult, // Summed, then used as (1 + sumAdditive)
        MultiplicativeMult // Multiplied together as factors
    }
}