using UnityEngine;

namespace DroneStrikers.Combat
{
    [RequireComponent(typeof(IHealth))] [RequireComponent(typeof(ContactDamageResolver))]
    public class DamageReceiver : MonoBehaviour, IDamageable
    {
        private IHealth _health;

        private void Awake()
        {
            _health = GetComponent<IHealth>();
        }

        public void TakeDamage(in DamageContext context)
        {
            _health.CurrentHealth -= context.DamageAmount;
        }
    }
}