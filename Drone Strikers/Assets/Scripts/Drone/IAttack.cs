using DroneStrikers.Combat;

namespace DroneStrikers.Drone
{
    public interface IAttack
    {
        /// <summary>
        ///     Sets up the attack with the given attack stats.
        ///     Takes in a destruction context receiver to receive context if this attack destroys something.
        /// </summary>
        /// <param name="stats"> The attack stats to apply to the attack. </param>
        /// <param name="team"> The team this attack belongs to. </param>
        /// <param name="instigatorContextReceiver"> The destruction context receiver of the instigator of this attack, if any. </param>
        void InitializeAttack(AttackStats stats, Team team, IDestructionContextReceiver instigatorContextReceiver);
    }
}