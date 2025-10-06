using DroneStrikers.Combat;
using DroneStrikers.Core;
using DroneStrikers.Drone;
using UnityEngine;

namespace DroneStrikers.Projectile
{
    [RequireComponent(typeof(TeamMember))]
    [RequireComponent(typeof(Rigidbody))]
    public class FiredProjectile : MonoBehaviour, IAttack, IDamageSource, IDamageable
    {
        private TeamMember _teamMember;
        private Rigidbody _rigidbody;

        // -- Drone Specific --
        public IDestructionContextReceiver InstigatorContextReceiver { get; private set; }
        public int ContactDamage { get; private set; } = 1;
        private float _speed; // Speed in units per second
        private int _pierce; // Number of enemies the projectile can pierce through

        // -- Shared --
        private readonly float _lifetime = 2f; // Lifetime in seconds
        private float _age; // Age in seconds

        private void Awake()
        {
            _teamMember = GetComponent<TeamMember>();
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            // Update age and check lifetime
            _age += Time.deltaTime;

            // Return to pool when lifetime is exceeded
            if (_age >= _lifetime) ObjectPoolManager.ReturnObject(gameObject);
        }

        private void FixedUpdate()
        {
            // Move the projectile forward
            _rigidbody.linearVelocity = transform.forward * _speed;
        }

        private void OnEnable()
        {
            _age = 0f; // Reset age when enabled
        }

        public void InitializeAttack(AttackStats stats, Team team, IDestructionContextReceiver instigatorContextReceiver)
        {
            _teamMember.Team = team;

            InstigatorContextReceiver = instigatorContextReceiver;
            ContactDamage = stats.AttackDamage;
            _speed = stats.AttackVelocity;
            _pierce = stats.AttackPierce;
        }

        public void TakeDamage(in DamageContext context)
        {
            if (_pierce <= 0) ObjectPoolManager.ReturnObject(gameObject); // Return to pool if no pierce left
            _pierce--;
        }
    }
}