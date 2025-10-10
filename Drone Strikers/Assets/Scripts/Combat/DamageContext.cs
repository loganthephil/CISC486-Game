using UnityEngine;

namespace DroneStrikers.Combat
{
    public struct DamageContext
    {
        public float DamageAmount { get; }
        public GameObject SourceObject { get; }
        public Team SourceTeam { get; }
        public IDestructionContextReceiver InstigatorContextReceiver { get; }

        /// <summary>
        ///     Context for damage being dealt to a GameObject.
        /// </summary>
        /// <param name="damageAmount"> The amount of damage being dealt. </param>
        /// <param name="sourceObject"> The GameObject that is the source of the damage. </param>
        /// <param name="sourceTeam"> The team of the source GameObject. </param>
        /// <param name="instigatorContextReceiver"> The IDestructionContextReceiver of the attack's instigator, if any. </param>
        public DamageContext(float damageAmount, GameObject sourceObject, Team sourceTeam, IDestructionContextReceiver instigatorContextReceiver)
        {
            DamageAmount = damageAmount;
            SourceObject = sourceObject;
            SourceTeam = sourceTeam;
            InstigatorContextReceiver = instigatorContextReceiver;
        }
    }
}