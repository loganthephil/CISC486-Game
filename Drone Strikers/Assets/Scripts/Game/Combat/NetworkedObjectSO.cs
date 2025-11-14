using UnityEngine;

namespace DroneStrikers.Game.Combat
{
    [CreateAssetMenu(fileName = "NetworkedObjectSO", menuName = "Scriptable Objects/NetworkedObjectSO")]
    public class NetworkedObjectSO : ScriptableObject
    {
        [SerializeField] private Mesh _mesh;
        public Mesh Mesh => _mesh;

        [SerializeField] private Color _color;
        public Color Color => _color;
    }
}