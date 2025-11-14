using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

namespace DroneStrikers.Game.Player
{
    [RequireComponent(typeof(CinemachineCamera))]
    public class PlayerDroneCamera : MonoBehaviour
    {
        private CinemachineCamera _cinemachineCamera;
        private GameObject _followingPlayerObject;

        private int _cameraPriority;

        private void Awake()
        {
            _cinemachineCamera = GetComponent<CinemachineCamera>();
            _cameraPriority = _cinemachineCamera.Priority; // Store initial priority
        }

        /// <summary>
        ///     Event listener. Do not call directly.
        /// </summary>
        /// <param name="player"></param>
        public void OnPlayerSpawned(GameObject player)
        {
            _cinemachineCamera.Target.TrackingTarget = player.transform;
            _cinemachineCamera.Priority = _cameraPriority; // Restore priority to enable camera
            _followingPlayerObject = player;
        }

        /// <summary>
        ///     Event listener. Do not call directly.
        /// </summary>
        public void OnPlayerDeath()
        {
            // if (ctx.Receiver != _followingPlayerObject) return; // Only respond if the drone that died is the player drone being followed

            _cinemachineCamera.Target.TrackingTarget = null;
            StartCoroutine(DisablePlayerCameraNextFrame());
        }

        private IEnumerator DisablePlayerCameraNextFrame()
        {
            yield return null; // Wait for the end of the frame
            _cinemachineCamera.Priority = -1; // Lower priority to enable other cameras
        }
    }
}