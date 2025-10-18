using DroneStrikers.Core.Interfaces;
using DroneStrikers.Core.Types;
using UnityEngine;

namespace DroneStrikers.Game.Combat
{
    /// <summary>
    ///     Required component that resolves contact damage between two colliding objects.
    ///     Must be placed on the same GameObject as the rigidbody.
    ///     Requires a TeamMember component, and at least one of IDamageSource or IDamageable on the same GameObject.
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

            if (DamageSource is null && Damageable is null) Debug.LogWarning($"ContactDamageResolver on {gameObject.name} has no IDamageSource or IDamageable component.", this);
        }

        private void HandleCollision(GameObject otherObject)
        {
            if (otherObject == null) return; // Safety check

            // To reduce unnecessary GetComponent calls, handle collision for both object in the same place
            if (gameObject.GetInstanceID() > otherObject.GetInstanceID()) return; // Only run the following code for one of the two colliding objects

            ContactDamageResolver otherDamageResolver = otherObject.GetComponent<ContactDamageResolver>();
            if (otherDamageResolver == null) return; // No resolver
            if (TeamMember.Team == otherDamageResolver.TeamMember.Team) return; // Same team

            // Need to cast interfaces to Object since an interface won't perform Unity null checks correctly
            // Don't need to cast the interfaces on 'this' since they can only be null if they were missing on Awake,
            // and never if this object was destroyed (this script wouldn't be running then)

            GameObject thisObject = gameObject;

            // This object -> Other object
            if (DamageSource is not null && (Object)otherDamageResolver.Damageable != null)
            {
                GameObject instigator = DamageSource.InstigatorContextReceiver?.gameObject ?? gameObject;
                DamageContext damageContext = new(thisObject, instigator, otherObject, DamageSource.ContactDamage, TeamMember.Team, DamageSource.InstigatorContextReceiver);
                otherDamageResolver.Damageable.TakeDamage(damageContext);
            }

            // Other object -> This object
            if ((Object)otherDamageResolver.DamageSource != null && Damageable is not null)
            {
                IDamageSource otherDamageSource = otherDamageResolver.DamageSource;
                GameObject instigator = (Object)otherDamageSource.InstigatorContextReceiver != null ? otherDamageSource.InstigatorContextReceiver.gameObject : otherObject; // TODO: Scuffed, remove gameObject property from IDestructionContextReceiver and find a better way to get instigator
                Team otherTeam = otherDamageResolver.TeamMember.Team;
                DamageContext damageContext = new(otherObject, instigator, thisObject, otherDamageSource.ContactDamage, otherTeam, otherDamageSource.InstigatorContextReceiver);
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