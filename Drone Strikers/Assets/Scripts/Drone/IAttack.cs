namespace DroneStrikers.Drone
{
    public interface IAttack
    {
        /// <summary>
        ///     Applies the given attack stats to the implementing class.
        /// </summary>
        /// <param name="stats"> The attack stats to apply. </param>
        void ApplyAttackStats(AttackStats stats);
    }
}