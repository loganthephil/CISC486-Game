using DroneStrikers.Core;
using DroneStrikers.Core.Interfaces;
using DroneStrikers.Core.Types;
using DroneStrikers.Game.Combat;
using DroneStrikers.Game.Deprecated.Drone;
using UnityEngine;

namespace DroneStrikers.Game.Deprecated.Projectile
{
    [RequireComponent(typeof(TeamMember))]
    [RequireComponent(typeof(Rigidbody))]
    public class FiredProjectile : MonoBehaviour, IAttack, IDamageSource, IDamageable
    {
        private TeamMember _teamMember;
        private Rigidbody _rigidbody;

        // -- Drone Specific --
        public IDestructionContextReceiver InstigatorContextReceiver { get; private set; }
        public float ContactDamage { get; private set; } = 1;
        private float _speed; // Speed in units per second
        private float _pierce; // Number of enemies the projectile can pierce through

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

        public void InitializeAttack(in AttackInitData initData)
        {
            ContactDamage = initData.Damage;
            _speed = initData.Velocity;
            _pierce = initData.Pierce;

            _teamMember.Team = initData.Team;
            InstigatorContextReceiver = initData.InstigatorContextReceiver;
        }

        public void TakeDamage(in DamageContext context)
        {
            // Return to pool if no pierce left
            if (_pierce == 0) ObjectPoolManager.ReturnObject(gameObject);
            _pierce--;
        }
    }
}