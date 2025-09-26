using DroneStrikers.Drone;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DroneStrikers.Player
{
    [RequireComponent(typeof(DroneMovement))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private DroneTurret _turret;

        private DroneMovement _droneMovement;

        private bool _fireHeld;

        private void Start()
        {
            _droneMovement = GetComponent<DroneMovement>();
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
    }
}