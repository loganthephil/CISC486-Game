using DroneStrikers.Drone;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DroneStrikers.Player
{
    [RequireComponent(typeof(DroneTurret))]
    public class MouseTargetProvider : MonoBehaviour
    {
        [Tooltip("(Optional) Camera used to convert mouse position to world point. If not set, will use Camera.main.")]
        [SerializeField]
        private Camera _camera;

        [SerializeField] private InputActionReference _mousePositionReference;

        private DroneTurret _turret;

        private void Awake()
        {
            if (_camera == null) _camera = Camera.main;
            _turret = GetComponent<DroneTurret>();
        }

        private void Update()
        {
            _turret.SetTarget(TryGetTargetPoint());
        }

        private Vector3 TryGetTargetPoint()
        {
            // Cast a ray from the camera through the mouse position that intersects with a horizontal plane at y=0
            Ray ray = _camera.ScreenPointToRay(_mousePositionReference.action.ReadValue<Vector2>());
            Plane plane = new(Vector3.up, Vector3.zero); // Horizontal plane at y=0

            Vector3 point = Vector3.zero;

            // Cast a ray and get the intersection point with the plane
            return plane.Raycast(ray, out float enter)
                ? ray.GetPoint(enter) // Get the intersection point
                : point; // If no intersection, return zero vector (this shouldn't happen)
        }
    }
}