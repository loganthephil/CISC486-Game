using System.Collections.Generic;
using DroneStrikers.Core.Editor;
using DroneStrikers.Core.Types;
using DroneStrikers.Game.Combat;
using DroneStrikers.Networking;
using UnityEngine;

namespace DroneStrikers.Game.Drone
{
    public class NetworkedDrone : MonoBehaviour
    {
        private struct DroneSnapshot
        {
            public float Time;
            public float PosX;
            public float PosY;
            public float UpperRotation;
        }

        [Header("References")]
        [SerializeField] [RequiredField] private Transform _bodyTransform;
        [SerializeField] [RequiredField] private Transform _movementTransform;

        [SerializeField] [RequiredField] private GameObject _playerModule;

        [SerializeField] [RequiredField] private TeamMember _teamMember;

        [Header("Interpolation Settings")]
        [Tooltip("How far (seconds) to rewind when choosing interpolation window.")]
        [SerializeField] private float _interpolationBackTime = 0.10f; // 100 ms
        [Tooltip("Max extrapolation time (seconds) if we are ahead of newest snapshot.")]
        [SerializeField] private float _extrapolationLimit = 0.25f;
        [Tooltip("Smoothing factor for applying interpolated position.")]
        [SerializeField] private float _positionLerpSpeed = 15f;
        [Tooltip("Smoothing factor for applying interpolated rotation.")]
        [SerializeField] private float _rotationLerpSpeed = 15f;

        private Transform _transform;

        private bool _isLocalPlayer;

        private const int MaxBufferSize = 20;
        private readonly List<DroneSnapshot> _snapshotBuffer = new(MaxBufferSize + 1);

        private void Awake()
        {
            _transform = transform;
        }

        public void Initialize(DroneState droneState, bool isLocalPlayer)
        {
            _isLocalPlayer = isLocalPlayer;
            _playerModule.SetActive(isLocalPlayer); // Enable player module only for local player

            PushSnapshot(droneState); // Initial snapshot

            _teamMember.Team = (Team)droneState.team; // Set team based on drone state

            NetworkManager.Instance.GameStateCallbacks.OnChange(droneState, () =>
            {
                PushSnapshot(droneState);
            });
        }

        private void PushSnapshot(DroneState state)
        {
            _snapshotBuffer.Add(new DroneSnapshot
            {
                Time = Time.time,
                PosX = state.posX,
                PosY = state.posY,
                UpperRotation = state.upperRotation
            });

            // Trim old snapshots.
            if (_snapshotBuffer.Count > MaxBufferSize)
                _snapshotBuffer.RemoveRange(0, _snapshotBuffer.Count - MaxBufferSize);
        }

        private void Update()
        {
            SynchronizeFromServer();
        }

        
        // TODO: Handle local player prediction and reconciliation locally
        private void SynchronizeFromServer()
        {
            if (_snapshotBuffer.Count == 0) return;

            float renderTime = Time.time - (_isLocalPlayer ? 0f : _interpolationBackTime);

            // Find interpolation pair.
            DroneSnapshot newer = default;
            DroneSnapshot older = default;
            bool foundPair = false;

            for (int i = _snapshotBuffer.Count - 1; i >= 0; i--)
                if (_snapshotBuffer[i].Time <= renderTime)
                {
                    older = _snapshotBuffer[i];
                    // If there is a newer one, use it.
                    if (i + 1 < _snapshotBuffer.Count)
                    {
                        newer = _snapshotBuffer[i + 1];
                        foundPair = true;
                    }
                    else
                    {
                        // We are exactly on the newest available snapshot.
                        newer = older;
                        foundPair = false;
                    }

                    break;
                }

            Vector3 targetPos;
            float targetRot;

            if (foundPair)
            {
                float tRange = newer.Time - older.Time;
                float t = tRange > 0.0001f ? (renderTime - older.Time) / tRange : 0f;
                t = Mathf.Clamp01(t);

                float posX = Mathf.Lerp(older.PosX, newer.PosX, t);
                float posY = Mathf.Lerp(older.PosY, newer.PosY, t);
                float rot = Mathf.LerpAngle(older.UpperRotation * Mathf.Rad2Deg, newer.UpperRotation * Mathf.Rad2Deg, t);

                targetPos = new Vector3(posX, 0f, posY);
                targetRot = rot;
            }
            else
            {
                // Either before oldest or after newest snapshot.
                DroneSnapshot latest = _snapshotBuffer[_snapshotBuffer.Count - 1];
                if (renderTime > latest.Time)
                {
                    // Extrapolate forward (clamped).
                    float dt = Mathf.Min(renderTime - latest.Time, _extrapolationLimit);
                    // Velocity estimate (simple): use last two snapshots if possible.
                    Vector2 velocity = Vector2.zero;
                    if (_snapshotBuffer.Count >= 2)
                    {
                        DroneSnapshot prev = _snapshotBuffer[_snapshotBuffer.Count - 2];
                        float dvTime = Mathf.Max(latest.Time - prev.Time, 0.0001f);
                        velocity = new Vector2(
                            (latest.PosX - prev.PosX) / dvTime,
                            (latest.PosY - prev.PosY) / dvTime
                        );
                    }

                    float extrapolatedPosX = latest.PosX + velocity.x * dt;
                    float extrapolatedPosY = latest.PosY + velocity.y * dt;

                    targetPos = new Vector3(extrapolatedPosX, 0f, extrapolatedPosY);
                    targetRot = latest.UpperRotation * Mathf.Rad2Deg;
                }
                else
                {
                    // Before oldest: just use oldest.
                    DroneSnapshot oldest = _snapshotBuffer[0];
                    targetPos = new Vector3(oldest.PosX, 0f, oldest.PosY);
                    targetRot = oldest.UpperRotation * Mathf.Rad2Deg;
                }
            }

            // Smooth application.
            _transform.position = Vector3.Lerp(_transform.position, targetPos, Time.deltaTime * _positionLerpSpeed);
            Quaternion desiredRot = Quaternion.Euler(0f, targetRot, 0f);
            _bodyTransform.rotation = Quaternion.Slerp(_bodyTransform.rotation, desiredRot, Time.deltaTime * _rotationLerpSpeed);
        }
    }
}