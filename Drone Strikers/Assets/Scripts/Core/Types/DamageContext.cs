using DroneStrikers.Core.Interfaces;
using UnityEngine;

namespace DroneStrikers.Core.Types
{
    public struct DamageContext
    {
        public GameObject DamageSourceObject { get; }
        public GameObject Instigator { get; }
        public GameObject Receiver { get; }
        public float DamageAmount { get; }
        public Team InstigatorTeam { get; }
        public IDestructionContextReceiver InstigatorContextReceiver { get; }

        /// <summary>
        ///     Context for damage being dealt to a GameObject.
        /// </summary>
        /// <param name="damageDamageSourceObject"></param>
        /// <param name="instigator"> The GameObject that instigated the attack. </param>
        /// <param name="receiver"> The GameObject receiving the damage. </param>
        /// <param name="damageAmount"> The amount of damage being dealt. </param>
        /// <param name="instigatorTeam"> The team of the instigator/source GameObject. </param>
        /// <param name="instigatorContextReceiver"> The IDestructionContextReceiver of the attack's instigator, if any. </param>
        public DamageContext(GameObject damageDamageSourceObject, GameObject instigator, GameObject receiver, float damageAmount, Team instigatorTeam, IDestructionContextReceiver instigatorContextReceiver)
        {
            DamageSourceObject = damageDamageSourceObject;
            Instigator = instigator;
            Receiver = receiver;
            DamageAmount = damageAmount;
            InstigatorTeam = instigatorTeam;
            InstigatorContextReceiver = instigatorContextReceiver;
        }
    }
}