using DroneStrikers.Core;
using DroneStrikers.Core.Editor;
using DroneStrikers.Game.UI;
using DroneStrikers.Networking;
using UnityEngine;

namespace DroneStrikers.Game.Combat
{
    public class NetworkedArenaObject : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] [RequiredField] private WorldHealthBar _healthBar;
        [SerializeField] [RequiredField] private MeshFilter _meshFilter;
        [SerializeField] [RequiredField] private Colorizer _colorizer;

        [SerializeField] [RequiredField] private NetworkedObjectSO _smallObjectSO;
        [SerializeField] [RequiredField] private NetworkedObjectSO _mediumObjectSO;
        [SerializeField] [RequiredField] private NetworkedObjectSO _largeObjectSO;

        private Transform _transform;

        private void Awake()
        {
            _transform = transform;
        }

        public void Initialize(ArenaObjectState objectState)
        {
            // Immediately set initial position
            _transform.position = new Vector3(objectState.posX, 0f, objectState.posY);

            // Set object appearance based on type
            switch (objectState.objectType)
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

            NetworkManager.Instance.GameStateCallbacks.OnChange(objectState, () =>
            {
                _transform.position = new Vector3(objectState.posX, 0f, objectState.posY);
                _healthBar.UpdatePercentage(objectState.health, objectState.maxHealth);
            });
        }
    }
}