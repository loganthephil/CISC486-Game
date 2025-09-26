using UnityEngine;

namespace DroneStrikers.Combat
{
    public struct DamageContext
    {
        public int DamageAmount;
        public GameObject Source;
        public Team SourceTeam;

        /// <summary>
        ///     Context for damage being dealt to a GameObject.
        /// </summary>
        /// <param name="damageAmount"> The amount of damage being dealt. </param>
        /// <param name="source"> The GameObject that is the source of the damage. </param>
        /// <param name="sourceTeam"> The team of the source GameObject. </param>
        public DamageContext(int damageAmount, GameObject source, Team sourceTeam)
        {
            DamageAmount = damageAmount;
            Source = source;
            SourceTeam = sourceTeam;
        }
    }
}