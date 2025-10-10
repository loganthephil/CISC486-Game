namespace DroneStrikers.Combat
{
    public interface IDamageSource
    {
        /// <summary>
        ///     The amount of damage this source deals on contact.
        /// </summary>
        float ContactDamage { get; }

        /// <summary>
        ///     The destruction context receiver of the instigator of this damage source, if any.
        /// </summary>
        IDestructionContextReceiver InstigatorContextReceiver { get; }
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
        float CurrentHealth { get; set; }

        /// <summary>
        ///     The maximum health of the implementing object.
        /// </summary>
        float MaxHealth { get; }
    }

    public interface ITeamMember
    {
        /// <summary>
        ///     The team this object belongs to.
        /// </summary>
        Team Team { get; }
    }

    public interface IExperienceProvider
    {
        /// <summary>
        ///     The amount of experience this object provides when destroyed.
        /// </summary>
        float ExperienceOnDestroy { get; }
    }

    public interface IDestructionContextReceiver
    {
        /// <summary>
        ///     Called when the implementing object is destroyed, providing context about the destruction.
        /// </summary>
        /// <param name="context"> The context of the destruction. Readonly. </param>
        void HandleDestructionContext(in ObjectDestructionContext context);
    }
}