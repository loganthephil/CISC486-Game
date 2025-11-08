using DroneStrikers.Core.Types;
using UnityEngine;

namespace DroneStrikers.Core.Interfaces
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

        /// <summary>
        ///     The current health of the implementing object as a percentage of its maximum health (0 to 1).
        /// </summary>
        float HealthPercent { get; }
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
        public GameObject gameObject { get; } // TODO: Remove and figure out a better way to get instigator inside ContactDamageResolver

        /// <summary>
        ///     Called when the implementing object is destroyed, providing context about the destruction.
        /// </summary>
        /// <param name="context"> The context of the destruction. Readonly. </param>
        void HandleDestructionContext(in ObjectDestructionContext context);
    }

    public interface IValueDangerProvider
    {
        /// <summary>
        ///     Arbitrary score that represents how "valuable" destroying this object is (e.g. experience gained on destroy).
        ///     Values should be normalized across all objects, but for reference, 5 is a little, 50 is medium, 200 is a lot.
        /// </summary>
        float BaseValue { get; }

        /// <summary>
        ///     How dangerous this object is (0+).
        ///     Values should be normalized across all objects, but for reference, 0 is harmless, 5 is a little, 20 is medium, 100 is very dangerous.
        /// </summary>
        float DangerLevel { get; }
    }
}