using DroneStrikers.Core.Editor;
using DroneStrikers.Game.Drone;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DroneStrikers.Game.Player
{
    public class PlayerInputController : MonoBehaviour
    {
        [SerializeField] [RequiredField] private DroneTurret _turret;
        [SerializeField] [RequiredField] private DroneMovement _droneMovement;
        [SerializeField] [RequiredField] private InputActionReference _mousePositionInputReference;

        [Header("Optional")]
        [Tooltip("Camera used to convert mouse position to world point. If not set, will use Camera.main.")]
        [SerializeField]
        private Camera _camera;

        private bool _fireHeld;

        private void Awake()
        {
            if (_camera == null) _camera = Camera.main;
        }

        private void Update()
        {
            _turret.SetTarget(TryGetTargetPoint());
        }

        private void FixedUpdate()
        {
            if (_fireHeld) _turret.RequestFire();
        }

        /// <summary>
        ///     EVENT HANDLER. DO NOT CALL DIRECTLY.
        /// </summary>
        public void OnMovement(InputAction.CallbackContext context)
        {
            Vector3 rawMovement = context.ReadValue<Vector2>();
            Vector3 direction = rawMovement.x * transform.right + rawMovement.y * transform.forward;
            _droneMovement.SetMovementDirection(direction);
        }

        public void OnFire(InputAction.CallbackContext context)
        {
            if (context.started) _fireHeld = true;
            else if (context.canceled) _fireHeld = false;
        }

        private Vector3 TryGetTargetPoint()
        {
            // Cast a ray from the camera through the mouse position that intersects with a horizontal plane at y=0
            Ray ray = _camera.ScreenPointToRay(_mousePositionInputReference.action.ReadValue<Vector2>());
            Plane plane = new(Vector3.up, Vector3.zero); // Horizontal plane at y=0

            Vector3 point = Vector3.zero;

            // Cast a ray and get the intersection point with the plane
            return plane.Raycast(ray, out float enter)
                ? ray.GetPoint(enter) // Get the intersection point
                : point; // If no intersection, return zero vector (this shouldn't happen)
        }
    }
}