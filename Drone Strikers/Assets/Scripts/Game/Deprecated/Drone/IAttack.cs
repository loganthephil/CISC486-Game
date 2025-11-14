namespace DroneStrikers.Game.Deprecated.Drone
{
    public interface IAttack
    {
        /// <summary>
        ///     Initializes the attack using the provided data.
        /// </summary>
        /// <param name="initData"> The initialization data for this attack instance. </param>
        void InitializeAttack(in AttackInitData initData);
    }
}