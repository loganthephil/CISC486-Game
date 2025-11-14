using DroneStrikers.Core;
using DroneStrikers.Core.Editor;
using DroneStrikers.Networking;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DroneStrikers.Game.Player
{
    public class PlayerInputController : MonoBehaviour
    {
        [Tooltip("The transform to use for the Y level targeted by mouse raycasts.")]
        [SerializeField] [RequiredField] private Transform _mouseRayYLevelTransform;

        private Camera _raycastCamera;
        private InputAction _movementAction;
        private InputAction _mousePositionAction;

        private float _mouseRayYLevel;

        private Vector2 _movementInput = Vector2.zero;
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
            UpdateMovement(); // Update movement based on input
        }

        private void FixedUpdate()
        {
            // if (_autoFireEnabled || _fireHeld) _turret.RequestFire();

            // Send movement input to the server
            if (_movementInput != Vector2.zero) NetworkManager.Send(ClientMessages.PlayerMove, new PlayerMoveMessage(_movementInput));

            // Send aiming direction to the server
            if (TryGetTargetPoint(out Vector3 targetPoint))
            {
                Vector3 direction = (targetPoint - transform.position).normalized;
                NetworkManager.Send(ClientMessages.PlayerAim, new PlayerAimMessage(direction.ToVector2()));
            }

            if (_autoFireEnabled || _fireHeld) NetworkManager.Send(ClientMessages.PlayerShoot);
        }

        private void UpdateMovement()
        {
            _movementInput = _movementAction.ReadValue<Vector2>();
            // Debug.Log("Movement Input: " + _movementInput);
        }

        // Triggered on fire input action started/canceled
        private void OnFire(InputAction.CallbackContext context) => _fireHeld = context.started; // true if started, false if canceled

        // Triggered on auto-fire input action started
        private void OnAutoFirePressed(InputAction.CallbackContext context) => _autoFireEnabled = !_autoFireEnabled; // Toggle auto-fire state

        // Attempts to get the world point where the mouse is pointing at on a horizontal plane at _mouseRayYLevel
        // If there is no mouse input or the raycast fails, returns false
        private bool TryGetTargetPoint(out Vector3 targetPoint)
        {
            Vector2 mousePosition = _mousePositionAction.ReadValue<Vector2>();
            if (_raycastCamera == null)
            {
                targetPoint = Vector3.zero;
                return false;
            }

            // Cast a ray from the camera through the mouse position that intersects with a horizontal plane at y=0
            Ray ray = _raycastCamera.ScreenPointToRay(mousePosition);

            // Horizontal plane at y = _mouseRayYLevel
            targetPoint = new Vector3(0f, _mouseRayYLevel, 0f);
            Plane plane = new(Vector3.up, targetPoint);

            if (plane.Raycast(ray, out float enter))
            {
                targetPoint = ray.GetPoint(enter); // Get the intersection point
                return true;
            }

            // If no intersection, return zero vector (this shouldn't happen)
            targetPoint = Vector3.zero;
            return false;
        }
    }
}