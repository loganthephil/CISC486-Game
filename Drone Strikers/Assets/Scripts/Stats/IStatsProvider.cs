namespace DroneStrikers.Stats
{
    public interface IStatsProvider
    {
        /// <summary>
        ///     Gets the current value of the specified stat.
        /// </summary>
        /// <param name="stat"> The StatTypeSO of the stat to retrieve. </param>
        /// <returns> The current value of the stat. </returns>
        float GetStatValue(StatTypeSO stat);
    }
}