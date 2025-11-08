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

        [Tooltip("The transform to use for the Y level targeted by mouse raycasts.")]
        [SerializeField] [RequiredField] private Transform _mouseRayYLevelTransform;

        private Camera _raycastCamera;
        private InputAction _movementAction;
        private InputAction _mousePositionAction;

        private float _mouseRayYLevel;

        private bool _fireHeld;
        private bool _autoFireEnabled;

        // Cache the main camera on awake as the camera used for ray casting
        private void Awake() => _raycastCamera = Camera.main;

        // Cache the Y level for mouse ray casting since it doesn't change
        private void Start() => _mouseRayYLevel = _mouseRayYLevelTransform.position.y;

        private void OnEnable()
        {
            // Get input actions from the input handler singleton
            GameInputActions inputActions = GameInputHandler.InputActions;

            inputActions.Drone.Enable(); // Enable the Drone action map

            // Cache needed input action references
            _movementAction = inputActions.Drone.Movement;
            _mousePositionAction = inputActions.Drone.MousePosition;

            // Subscribe to input events
            inputActions.Drone.Fire.started += OnFire;
            inputActions.Drone.Fire.canceled += OnFire;

            inputActions.Drone.AutoFire.started += OnAutoFirePressed;
        }

        private void OnDisable()
        {
            // Get input actions from the input handler singleton
            GameInputActions inputActions = GameInputHandler.InputActions;

            inputActions.Drone.Disable(); // Disable the Drone action map

            // Unsubscribe from input events
            inputActions.Drone.Fire.started -= OnFire;
            inputActions.Drone.Fire.canceled -= OnFire;

            inputActions.Drone.AutoFire.started -= OnAutoFirePressed;
        }

        private void Update()
        {
            _turret.SetTarget(TryGetTargetPoint());
            UpdateMovement(); // Update movement based on input
        }

        private void FixedUpdate()
        {
            if (_autoFireEnabled || _fireHeld) _turret.RequestFire();
        }

        private void UpdateMovement()
        {
            Vector3 rawMovement = _movementAction.ReadValue<Vector2>();
            Vector3 direction = rawMovement.x * Vector3.right + rawMovement.y * Vector3.forward;
            _droneMovement.SetMovementDirection(direction);
        }

        // Triggered on fire input action started/canceled
        private void OnFire(InputAction.CallbackContext context) => _fireHeld = context.started; // true if started, false if canceled

        // Triggered on auto-fire input action started
        private void OnAutoFirePressed(InputAction.CallbackContext context) => _autoFireEnabled = !_autoFireEnabled; // Toggle auto-fire state

        private Vector3 TryGetTargetPoint()
        {
            // Cast a ray from the camera through the mouse position that intersects with a horizontal plane at y=0
            Ray ray = _raycastCamera.ScreenPointToRay(_mousePositionAction.ReadValue<Vector2>());

            // Horizontal plane at y = _mouseRayYLevel
            Vector3 targetPoint = new(0f, _mouseRayYLevel, 0f);
            Plane plane = new(Vector3.up, targetPoint);

            // Cast a ray and get the intersection point with the plane
            return plane.Raycast(ray, out float enter)
                ? ray.GetPoint(enter) // Get the intersection point
                : Vector3.zero; // If no intersection, return zero vector (this shouldn't happen)
        }
    }
}