using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DroneStrikers.Game.Deprecated.ObjectSpawning
{
    /// <summary>
    ///     Directs the spawning of objects within a chunk.
    /// </summary>
    [RequireComponent(typeof(SpawnWithinCollider))]
    public class ChunkSpawnDirector : MonoBehaviour
    {
        [SerializeField] private ChunkSpawnProfileSO _chunkSpawnProfile;

        [Header("Spawning")]
        [Tooltip("How many attempts should the spawn director make to place an object before giving up?")]
        [SerializeField] private int _maxPlacementAttempts = 5;

        [Tooltip("The layers to check for collisions against when placing objects.")]
        [SerializeField] private LayerMask _collisionLayers = -1;

        private SpawnWithinCollider _spawner;

        private int _totalBudget;

        private int _lowestCost;
        private int _highestCost;

        private void Awake()
        {
            _spawner = GetComponent<SpawnWithinCollider>();
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

            int strikes = 0;
            while (_totalBudget >= _lowestCost)
            {
                // Determine if we should spawn this cycle or save budget for next cycle
                float chanceToSpawn = (float)_totalBudget / _highestCost;
                if (Random.value > chanceToSpawn) return; // Save budget for next cycle

                List<SpawnableRuleSO> affordableRules = GetAffordableRules();
                SpawnableRuleSO selectedRule = SelectSpawnableRule(affordableRules);

                if (TrySpawnObject(selectedRule)) _totalBudget -= selectedRule.Cost;
                else strikes++;

                // If we have too many failed attempts, break to avoid infinite loop
                if (strikes >= 3)
                {
                    Debug.LogWarning("Failed to spawn object after multiple attempts, breaking out of spawn loop.");
                    break;
                }
            }
        }

        // Actually attempt to spawn the object
        private bool TrySpawnObject(SpawnableRuleSO rule)
        {
            SpawnableSO spawnable = rule.Spawnable;
            return _spawner.TrySpawnObject(spawnable.Prefab, spawnable.MinSeparation, _collisionLayers, _maxPlacementAttempts);
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