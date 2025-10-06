using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DroneStrikers.ObjectSpawning
{
    /// <summary>
    ///     Directs the spawning of objects within a chunk.
    /// </summary>
    public class ChunkSpawnDirector : MonoBehaviour
    {
        [SerializeField] private ChunkSpawnProfileSO _chunkSpawnProfile;

        [Header("Spawning")]
        [Tooltip("How many attempts should the spawn director make to place an object before giving up?")]
        [SerializeField]
        private int _maxPlacementAttempts = 5;

        [Tooltip("The Y position to spawn objects at within the chunk.")]
        [SerializeField]
        private float _yPosition = 0.5f;

        private MeshCollider _spawnAreaCollider;
        private Transform _spawnAreaTransform;

        private int _totalBudget;

        private int _lowestCost;
        private int _highestCost;

        private void Awake()
        {
            _spawnAreaCollider = GetComponent<MeshCollider>();
            _spawnAreaTransform = transform;
        }

        private void OnEnable()
        {
            InitializeDirector();
            StartCoroutine(SpawnCycle());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private void InitializeDirector()
        {
            // Store the lowest and highest costs from the spawnable rules
            List<int> costs = _chunkSpawnProfile.SpawnableRules.Select(ruleSO => ruleSO.Cost).ToList();
            _lowestCost = costs.Min();
            _highestCost = costs.Max();

            // Give initial budget to boost early spawning
            _totalBudget = _highestCost;
        }

        private IEnumerator SpawnCycle()
        {
            while (enabled)
            {
                SpawnObjects();

                // Apply random variance to the cycle frequency
                float variance = Random.Range(-_chunkSpawnProfile.FrequencyRandomVariance, _chunkSpawnProfile.FrequencyRandomVariance);
                yield return new WaitForSeconds(_chunkSpawnProfile.CycleFrequency + variance);
            }
        }

        private void SpawnObjects()
        {
            _totalBudget += _chunkSpawnProfile.BudgetPerCycle; // Add budget for this cycle
            _totalBudget = Mathf.Min(_totalBudget, _highestCost); // Cap budget to the highest cost

            // TODO: Also need to check individual max per chunk for each spawnable rule. Do that around the affordable rules selection.
            // Do nothing if there are the maximum number of objects already spawned
            if (transform.childCount >= _chunkSpawnProfile.TotalMaxPerChunk) return;

            while (_totalBudget >= _lowestCost)
            {
                // Determine if we should spawn this cycle or save budget for next cycle
                float chanceToSpawn = (float)_totalBudget / _highestCost;
                if (Random.value > chanceToSpawn) return; // Save budget for next cycle

                List<SpawnableRuleSO> affordableRules = GetAffordableRules();
                SpawnableRuleSO selectedRule = SelectSpawnableRule(affordableRules);

                if (TrySpawnObject(selectedRule)) _totalBudget -= selectedRule.Cost;
            }
        }

        /// <summary>
        ///     Attempts to spawn an object based on the provided spawnable rule.
        /// </summary>
        /// <param name="rule"> The rule to use for spawning. </param>
        /// <returns> Whether the spawn was successful. </returns>
        private bool TrySpawnObject(SpawnableRuleSO rule)
        {
            if (rule?.Spawnable?.Prefab == null)
            {
                Debug.LogError("SpawnableRuleSO or its Prefab is null.");
                return false;
            }

            Bounds spawnAreaBounds = _spawnAreaCollider.bounds;

            for (int attempt = 0; attempt < _maxPlacementAttempts; attempt++)
            {
                // Generate a random position within the spawn area
                float x = Random.Range(spawnAreaBounds.min.x, spawnAreaBounds.max.x);
                float z = Random.Range(spawnAreaBounds.min.z, spawnAreaBounds.max.z);
                Vector3 spawnPosition = new(x, _yPosition, z);

                // Check if the position would collide with existing objects
                Collider[] results = { };
                int size = Physics.OverlapSphereNonAlloc(spawnPosition, rule.Spawnable.MinSeparation, results);
                if (size > 0) continue; // Collision detected, try again

                // Instantiate the object
                GameObject spawnedObject = Instantiate(rule.Spawnable.Prefab, spawnPosition, Quaternion.identity, _spawnAreaTransform);
                return true; // Successfully spawned
            }

            Debug.Log("Did not find a valid position to spawn after max attempts on Chunk: " + gameObject.name);
            return false; // Failed to find a valid position after max attempts
        }

        private List<SpawnableRuleSO> GetAffordableRules() => _chunkSpawnProfile.SpawnableRules.Where(rule => rule.Cost <= _totalBudget).ToList();

        private static SpawnableRuleSO SelectSpawnableRule(List<SpawnableRuleSO> rules)
        {
            float totalWeight = rules.Sum(rule => rule.Weight);
            float randomValue = Random.Range(0, totalWeight);

            float cumulativeWeight = 0f;
            foreach (SpawnableRuleSO rule in rules)
            {
                cumulativeWeight += rule.Weight;
                if (cumulativeWeight >= randomValue) return rule;
            }

            return null; // Should never reach here if rules are not empty
        }
    }
}