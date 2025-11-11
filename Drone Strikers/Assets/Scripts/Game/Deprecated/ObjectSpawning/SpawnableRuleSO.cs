using UnityEngine;

namespace DroneStrikers.Game.ObjectSpawning
{
    [CreateAssetMenu(fileName = "SpawnableRule_", menuName = "Spawning/Spawnable Rule")]
    public class SpawnableRuleSO : ScriptableObject
    {
        [SerializeField] private SpawnableSO _spawnable;
        public SpawnableSO Spawnable => _spawnable;

        [SerializeField] private int _cost = 1;
        /// <summary>
        ///     Cost to spawn this spawnable. A higher cost is more expensive when being chosen by the chunk spawn director.
        /// </summary>
        public int Cost => _cost;

        /// <summary>
        ///     Weight is the inverse of cost. Used for weighted random selection.
        /// </summary>
        public float Weight => 1f / _cost;

        [SerializeField] private int _maxPerChunk = 10;
        /// <summary>
        ///     The maximum number of this spawnable that can be spawned in a single chunk.
        /// </summary>
        public int MaxPerChunk => _maxPerChunk;
    }
}