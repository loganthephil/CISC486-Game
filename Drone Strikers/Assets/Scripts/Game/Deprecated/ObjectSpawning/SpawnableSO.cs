using UnityEngine;

namespace DroneStrikers.Game.Deprecated.ObjectSpawning
{
    [CreateAssetMenu(fileName = "Spawnable_", menuName = "Spawning/Spawnable")]
    public class SpawnableSO : ScriptableObject
    {
        [SerializeField] private GameObject _prefab;
        public GameObject Prefab => _prefab;

        [SerializeField] private float _minSeparation = 1f;
        public float MinSeparation => _minSeparation;
    }
}