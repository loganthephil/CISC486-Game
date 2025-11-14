using System;
using System.Collections.Generic;
using UnityEngine;

namespace DroneStrikers.Game.Deprecated.Stats
{
    [CreateAssetMenu(fileName = "StatTemplate_", menuName = "Stats/Stat Template")]
    public class StatTemplateSO : ScriptableObject
    {
        [Header("Provide a list of stats that should have their default values overridden.")]
        [SerializeField]
        private List<StatOverride> _statOverrides = new();
        public List<StatOverride> StatOverrides => _statOverrides;

        [Serializable]
        public class StatOverride
        {
            [SerializeField] private StatTypeSO _statType;
            public StatTypeSO StatType => _statType;

            [SerializeField] private float _value;
            public float Value => _value;
        }
    }
}