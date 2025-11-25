using DroneStrikers.Core;
using DroneStrikers.Core.Editor;
using DroneStrikers.Game.UI;
using DroneStrikers.Networking;
using UnityEngine;

namespace DroneStrikers.Game.Combat
{
    public class NetworkedArenaObject : NetworkedEntityBase<ArenaObjectState>
    {
        [Header("References")]
        [SerializeField] [RequiredField] private WorldHealthBar _healthBar;
        [SerializeField] [RequiredField] private MeshFilter _meshFilter;
        [SerializeField] [RequiredField] private Colorizer _colorizer;

        [SerializeField] [RequiredField] private NetworkedObjectSO _smallObjectSO;
        [SerializeField] [RequiredField] private NetworkedObjectSO _mediumObjectSO;
        [SerializeField] [RequiredField] private NetworkedObjectSO _largeObjectSO;

        protected override bool UsesInterpolation => false;
        protected override bool UsesExtrapolation => false;

        public void Initialize(ArenaObjectState objectState)
        {
            // Apply initial state and push first snapshot
            InitializeFromState(objectState);

            // Start listening for state changes
            NetworkManager.Instance.GameStateCallbacks.OnChange(objectState, () =>
            {
                OnNetworkStateUpdated(objectState);
            });
        }

        protected override void ApplyAdditionalStateImmediately(ArenaObjectState state)
        {
            // Set visual appearance based on type once at spawn
            switch (state.arenaObjectType)
            {
                case "large":
                    _meshFilter.mesh = _largeObjectSO.Mesh;
                    _colorizer.SetColor(_largeObjectSO.Color);
                    break;

                case "medium":
                    _meshFilter.mesh = _mediumObjectSO.Mesh;
                    _colorizer.SetColor(_mediumObjectSO.Color);
                    break;

                default:
                    _meshFilter.mesh = _smallObjectSO.Mesh;
                    _colorizer.SetColor(_smallObjectSO.Color);
                    break;
            }

            // Initialize health bar
            _healthBar.UpdatePercentage(state.health, state.maxHealth);
        }

        protected override void OnStateSideEffects(ArenaObjectState state)
        {
            // Just keep health in sync; position is handled by the base interpolation
            _healthBar.UpdatePercentage(state.health, state.maxHealth);
        }

        protected override void ApplySettingOverrides()
        {
            _maxRelevanceDistance = 500f;
        }
    }
}