using DroneStrikers.Core.Types;
using Unity.Cinemachine;
using UnityEngine;

namespace DroneStrikers.Game.Player
{
    [RequireComponent(typeof(CinemachineCamera))]
    public class KilledByCamera : MonoBehaviour
    {
        private CinemachineCamera _cinemachineCamera;
        private GameObject _followingPlayerObject;

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
            _followingPlayerObject = player;
        }

        /// <summary>
        ///     Event listener. Do not call directly.
        /// </summary>
        public void OnPlayerDeath(DamageContext ctx)
        {
            if (ctx.Receiver != _followingPlayerObject) return; // Only respond if the drone that died is the player drone being followed
            if (ctx.Instigator == null) return; // If no instigator or it doesn't exist, do nothing

            Transform instigatorTransform = ctx.Instigator.transform;
            _cinemachineCamera.Target.TrackingTarget = instigatorTransform; // Follow the instigator
        }
    }
}