using DroneStrikers.Core.Types;
using Unity.Cinemachine;
using UnityEngine;

namespace DroneStrikers.Game.Player
{
    [RequireComponent(typeof(CinemachineCamera))]
    public class KilledByCamera : MonoBehaviour
    {
        private CinemachineCamera _cinemachineCamera;

        private void Awake()
        {
            _cinemachineCamera = GetComponent<CinemachineCamera>();
        }

        /// <summary>
        ///     Event listener. Do not call directly.
        /// </summary>
        public void OnPlayerSpawned(GameObject player)
        {
            _cinemachineCamera.Target.TrackingTarget = player.transform; // Follow the player
        }

        /// <summary>
        ///     Event listener. Do not call directly.
        /// </summary>
        public void OnPlayerDeath(DamageContext ctx)
        {
            if (ctx.Instigator == null) return; // If no instigator or it doesn't exist, do nothing
            Transform instigatorTransform = ctx.Instigator.transform;
            _cinemachineCamera.Target.TrackingTarget = instigatorTransform; // Follow the instigator
        }
    }
}