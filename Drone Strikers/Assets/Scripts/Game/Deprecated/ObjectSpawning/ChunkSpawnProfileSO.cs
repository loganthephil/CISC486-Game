using System.Collections.Generic;
using UnityEngine;

namespace DroneStrikers.Game.ObjectSpawning
{
    [CreateAssetMenu(fileName = "SpawnProfile_", menuName = "Spawning/Chunk Spawn Profile")]
    public class ChunkSpawnProfileSO : ScriptableObject
    {
        [Tooltip("How many credits should the spawn director get per spawn cycle?")]
        [SerializeField]
        [Min(1)]
        private int _budgetPerCycle = 10;
        /// <summary>
        ///     The number of credits the ChunkSpawnDirector gets per spawn cycle.
        /// </summary>
        public int BudgetPerCycle => _budgetPerCycle;

        [Tooltip("How often (in seconds) should the spawn director issue a new spawn cycle?")]
        [SerializeField]
        [Min(0)]
        private float _cycleFrequency = 5f;
        /// <summary>
        ///     How many seconds between spawn cycles.
        /// </summary>
        public float CycleFrequency => _cycleFrequency;

        [Tooltip("What is the amount of random variance (positive and negative) that can be applied to the cycle frequency?")]
        [SerializeField]
        [Min(0)]
        private float _frequencyRandomVariance = 0.5f;
        /// <summary>
        ///     The maximum variance (positive and negative) that can be applied to the cycle frequency.
        ///     For example, a variance of 0.5 on a frequency of 5 means the actual frequency will be between 4.5 and 5.5 seconds.
        /// </summary>
        public float FrequencyRandomVariance => _frequencyRandomVariance;

        [Tooltip("What is the maximum number of objects that can be spawned in this chunk?")]
        [SerializeField]
        private int _totalMaxPerChunk = 10;
        /// <summary>
        ///     The maximum number of objects that can be spawned in the chunk.
        /// </summary>
        public int TotalMaxPerChunk => _totalMaxPerChunk;

        [SerializeField] private List<SpawnableRuleSO> _spawnableRules = new();
        /// <summary>
        ///     List of rules defining what can be spawned in a chunk and how.
        ///     Only items in this list can be spawned by the ChunkSpawnDirector that uses this profile.
        /// </summary>
        public List<SpawnableRuleSO> SpawnableRules => _spawnableRules;
    }
}