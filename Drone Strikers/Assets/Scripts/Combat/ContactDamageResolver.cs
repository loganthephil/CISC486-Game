using UnityEngine;

namespace DroneStrikers.Combat
{
    /// <summary>
    ///     Required component that resolves contact damage between two colliding objects.
    ///     Must be attached to any GameObject that has an IDamageSource and/or IDamageable component.
    ///     Requires a TeamMember component to determine team affiliation.
    /// </summary>
    [RequireComponent(typeof(TeamMember))]
    public class ContactDamageResolver : MonoBehaviour, ITeamMember
    {
        private TeamMember TeamMember { get; set; }
        private IDamageSource DamageSource { get; set; }
        private IDamageable Damageable { get; set; }

        public Team Team => TeamMember.Team;

        private void Awake()
        {
            TeamMember = GetComponent<TeamMember>();
            DamageSource = GetComponent<IDamageSource>();
            Damageable = GetComponent<IDamageable>();

            if (DamageSource == null && Damageable == null) Debug.LogWarning($"ContactDamageResolver on {gameObject.name} has no IDamageSource or IDamageable component.", this);
        }

        private void HandleCollision(GameObject other)
        {
            if (other == null) return; // Safety check

            // To reduce unnecessary GetComponent calls, handle collision for both object in the same place
            if (gameObject.GetInstanceID() > other.GetInstanceID()) return; // Only run the following code for one of the two colliding objects

            ContactDamageResolver otherDamageResolver = other.GetComponent<ContactDamageResolver>();
            if (otherDamageResolver == null) return; // No resolver
            if (TeamMember.Team == otherDamageResolver.TeamMember.Team) return; // Same team

            // This object -> Other object
            if (DamageSource != null && otherDamageResolver.Damageable != null)
            {
                DamageContext damageContext = new(DamageSource.ContactDamage, gameObject, TeamMember.Team, DamageSource.InstigatorContextReceiver);
                otherDamageResolver.Damageable.TakeDamage(damageContext);
            }

            // Other object -> This object
            if (otherDamageResolver.DamageSource != null && Damageable != null)
            {
                IDamageSource otherDamageSource = otherDamageResolver.DamageSource;
                DamageContext damageContext = new(otherDamageSource.ContactDamage, other, otherDamageResolver.TeamMember.Team, otherDamageSource.InstigatorContextReceiver);
                Damageable.TakeDamage(damageContext);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            HandleCollision(collision.gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            HandleCollision(other.gameObject);
        }
    }
}