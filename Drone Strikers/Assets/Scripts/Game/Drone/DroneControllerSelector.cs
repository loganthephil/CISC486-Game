using DroneStrikers.Core.Types;
using UnityEngine;

namespace DroneStrikers.Game.Drone
{
    public class DroneControllerSelector : MonoBehaviour
    {
        private const DroneControllerType DefaultControllerType = DroneControllerType.AI;

        public DroneControllerType CurrentControllerType { get; private set; } = DefaultControllerType;

        [Header("Player Components")]
        [Tooltip("Components that should only be enabled for player-controlled drones.")]
        [SerializeField] private Behaviour[] _playerComponents;
        [Tooltip("GameObjects that should only be active for player-controlled drones.")]
        [SerializeField] private GameObject[] _playerGameObjects;

        [Header("AI Components")]
        [Tooltip("Components that should only be enabled for AI-controlled drones.")]
        [SerializeField] private Behaviour[] _aiComponents;
        [Tooltip("GameObjects that should only be active for AI-controlled drones.")]
        [SerializeField] private GameObject[] _aiGameObjects;

        private void Awake()
        {
            SetControllerType(DefaultControllerType);
        }

        /// <summary>
        ///     Set the controller type of the drone.
        ///     Player - Enables the drone to be controlled by the player, disabling AI components.
        ///     AI - Enables an AI to control the drone, disabling player components.
        /// </summary>
        /// <param name="controllerType"> The controller type to set. </param>
        public void SetControllerType(DroneControllerType controllerType)
        {
            bool isPlayer = controllerType == DroneControllerType.Player;

            SetComponentsActiveState(_playerComponents, isPlayer);
            SetGameObjectsActiveState(_playerGameObjects, isPlayer);

            SetComponentsActiveState(_aiComponents, !isPlayer);
            SetGameObjectsActiveState(_aiGameObjects, !isPlayer);
        }

        private static void SetComponentsActiveState(in Behaviour[] components, bool isActive)
        {
            foreach (Behaviour component in components)
                if (component != null)
                    component.enabled = isActive;
        }

        private static void SetGameObjectsActiveState(in GameObject[] gameObjects, bool isActive)
        {
            foreach (GameObject obj in gameObjects)
                if (obj != null)
                    obj.SetActive(isActive);
        }
    }
}