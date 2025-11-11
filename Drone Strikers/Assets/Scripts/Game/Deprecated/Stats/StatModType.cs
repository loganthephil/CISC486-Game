namespace DroneStrikers.Game.Stats
{
    public enum StatModType
    {
        Flat, // Flat addition before anything else
        PercentAdditive, // All percent additive mods are summed before being multiplied
        PercentMultiplicative // Multiplied on top of everything else
    }
}