using DroneStrikers.Combat;
using DroneStrikers.Stats;
using UnityEngine;

namespace DroneStrikers.Drone
{
    [CreateAssetMenu(fileName = "AttackDefinition", menuName = "Scriptable Objects/AttackDefinitionSO")]
    public class AttackDefinitionSO : ScriptableObject
    {
        [Header("Attack Prefab")]
        [SerializeField] private GameObject _prefab;
        public GameObject Prefab => _prefab;

        [Header("Stat Types")]
        [SerializeField] private StatTypeSO _damageStat;
        [SerializeField] private StatTypeSO _velocityStat;
        [SerializeField] private StatTypeSO _pierceStat;

        private void Awake()
        {
            Debug.Assert(_damageStat != null && _velocityStat != null && _pierceStat != null, "Missing StatType assignment on " + this);
        }

        /// <summary>
        ///     Creates an AttackInitData struct from the given parameters.
        /// </summary>
        /// <param name="statsProvider"> The stats provider to get the stat values from. </param>
        /// <param name="team"> The team this attack belongs to. </param>
        /// <param name="instigatorContextReceiver"> The destruction context receiver of the instigator of this attack, if any. </param>
        /// <returns></returns>
        public AttackInitData CreateInitData(IStatsProvider statsProvider, Team team, IDestructionContextReceiver instigatorContextReceiver) =>
            new()
            {
                Damage = statsProvider.GetStatValue(_damageStat),
                Velocity = statsProvider.GetStatValue(_velocityStat),
                Pierce = statsProvider.GetStatValue(_pierceStat),
                Team = team,
                InstigatorContextReceiver = instigatorContextReceiver
            };
    }
}