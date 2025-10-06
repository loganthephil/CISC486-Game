using UnityEngine;

namespace DroneStrikers.ObjectSpawning
{
    [CreateAssetMenu(fileName = "SpawnableSO", menuName = "Scriptable Objects/SpawnableSO")]
    public class SpawnableSO : ScriptableObject
    {
        [SerializeField] private GameObject _prefab;
        public GameObject Prefab => _prefab;

        [SerializeField] private float _minSeparation = 1f;
        public float MinSeparation => _minSeparation;
    }
}