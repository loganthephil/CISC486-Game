using DroneStrikers.Core.Interfaces;
using DroneStrikers.Core.Types;
using DroneStrikers.Events;
using DroneStrikers.Events.EventSO;
using UnityEngine;

namespace DroneStrikers.Game.Combat
{
    /// <summary>
    ///     Basic implementation of IDamageable that reduces health on taking damage.
    ///     Requires an IHealth component and a ContactDamageResolver component on the same GameObject.
    /// </summary>
    [RequireComponent(typeof(IHealth))]
    public class DamageReceiver : MonoBehaviour, IDamageable
    {
        [Tooltip("Optional global event to raise when this object is destroyed.")]
        [SerializeField] private DamageContextEventSO _onDestroyedEvent;

        private IHealth _health;
        private IExperienceProvider _experienceProvider;
        private LocalEvents _localEvents;

        private void Awake()
        {
            _health = GetComponent<IHealth>();
            _experienceProvider = GetComponent<IExperienceProvider>();
            _localEvents = GetComponent<LocalEvents>();
        }

        // TODO: Explore ways to make more robust
        public void TakeDamage(in DamageContext context)
        {
            // Invoke local damaged event
            _localEvents?.Invoke(CombatEvents.DamageTaken, context);

            _health.CurrentHealth -= context.DamageAmount;

            // Check for destruction
            if (_health.CurrentHealth <= 0)
            {
                // Determine experience to award to destroyer, if any
                float experienceToAward = _experienceProvider?.ExperienceOnDestroy ?? 0f;

                // Create destruction context
                ObjectDestructionContext destructionContext = new(experienceToAward);

                // Invoke local destroyed event
                _localEvents?.Invoke(CombatEvents.Destroyed, destructionContext);

                // TODO: Consider a cleaner method
                // Notify the instigator's context receiver, if any
                context.InstigatorContextReceiver?.HandleDestructionContext(destructionContext);

                _onDestroyedEvent?.Raise(context); // Raise global event if assigned

                // TODO: Only destroy after animation or effects
                Destroy(gameObject);
            }
        }
    }
}