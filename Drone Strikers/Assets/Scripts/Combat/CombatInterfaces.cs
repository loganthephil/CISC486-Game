namespace DroneStrikers.Combat
{
    public interface IDamageSource
    {
        /// <summary>
        ///     The amount of damage this source deals on contact.
        /// </summary>
        int ContactDamage { get; }
    }

    public interface IDamageable
    {
        /// <summary>
        ///     Applies damage to the implementing object.
        /// </summary>
        /// <param name="context"> The context of the damage being applied. Readonly. </param>
        void TakeDamage(in DamageContext context);
    }

    public interface IHealth
    {
        /// <summary>
        ///     The current health of the implementing object.
        /// </summary>
        int CurrentHealth { get; set; }

        /// <summary>
        ///     The maximum health of the implementing object.
        /// </summary>
        int MaxHealth { get; }
    }

    public interface ITeamMember
    {
        /// <summary>
        ///     The team this object belongs to.
        /// </summary>
        Team Team { get; }
    }
}