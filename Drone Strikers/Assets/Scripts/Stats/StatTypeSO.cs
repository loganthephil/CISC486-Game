using UnityEngine;

namespace DroneStrikers.Stats
{
    [CreateAssetMenu(fileName = "StatType_", menuName = "Stats/Stat Type")]
    public class StatTypeSO : ScriptableObject
    {
        [SerializeField] private string _name = "New Stat Name";
        public string Name => _name;

        [SerializeField] private float _defaultValue;
        public float DefaultValue => _defaultValue;
    }
}