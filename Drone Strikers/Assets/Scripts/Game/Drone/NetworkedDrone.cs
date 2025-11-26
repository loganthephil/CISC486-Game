using System;
using DroneStrikers.Core.Editor;
using DroneStrikers.Core.Types;
using DroneStrikers.Events;
using DroneStrikers.Events.EventSO;
using DroneStrikers.Game.Combat;
using DroneStrikers.Game.UI;
using DroneStrikers.Networking;
using UnityEngine;

namespace DroneStrikers.Game.Drone
{
    public class NetworkedDrone : NetworkedEntityBase<DroneState>
    {
        [Header("References")]
        [SerializeField] [RequiredField] private Transform _bodyTransform;
        [SerializeField] [RequiredField] private Transform _movementTransform;
        [SerializeField] [RequiredField] private WorldHealthBar _healthBar;

        [SerializeField] [RequiredField] private GameObject _playerModule; // Module for the local player
        [SerializeField] [RequiredField] private GameObject _serverModule; // Module for server-side drones

        [SerializeField] [RequiredField] private TeamMember _teamMember;

        [Header("Upgrade Visuals")]
        [SerializeField] [RequiredField] private UpgradeTreeCollectionSO _upgradeCollection;
        [SerializeField] [RequiredField] private MeshFilter _turretMeshFilter;
        [SerializeField] [RequiredField] private MeshFilter _bodyMeshFilter;
        [SerializeField] [RequiredField] private MeshFilter _movementMeshFilter;

        [Header("Events")]
        [SerializeField] [RequiredField] private LocalEvents _localEvents;
        [SerializeField] [RequiredField] private VoidEventSO _onPlayerDeath;
        [SerializeField] [RequiredField] private IntEventSO _onPlayerUpgradePointGained;
        [SerializeField] [RequiredField] private StringEventSO _onPlayerUpgradeApplied;

        public string DroneId { get; private set; }
        public DroneState CurrentState { get; private set; }

        private bool _isLocalPlayer;

        public void Initialize(DroneState droneState, string droneId, bool isLocalPlayer)
        {
            DroneId = droneId; // Set the DroneId
            _isLocalPlayer = isLocalPlayer;

            _playerModule.SetActive(isLocalPlayer); // Enable player module only for local player
            _serverModule.SetActive(!isLocalPlayer); // Enable server module for non-local players

            _teamMember.Team = (Team)droneState.team; // Set team based on drone state

            InitializeFromState(droneState);
            CurrentState = droneState;

            NetworkManager instance = NetworkManager.Instance;
            instance.GameStateCallbacks.OnChange(droneState, () =>
            {
                CurrentState = droneState;
                OnNetworkStateUpdated(droneState);
            });

            // TODO: Consider stopping capture of _localEvents
            instance.GameStateCallbacks.Listen(droneState, state => state.name, (currentValue, _) =>
            {
                _localEvents.Invoke(DroneEvents.NameChanged, currentValue);
            });

            instance.GameStateCallbacks.Listen(droneState, state => state.experience, (currentValue, _) =>
            {
                _localEvents.Invoke(DroneEvents.ExperienceGained, currentValue);
            });

            instance.GameStateCallbacks.Listen(droneState, state => state.level, (currentValue, _) =>
            {
                _localEvents.Invoke(DroneEvents.LevelUp, currentValue);
            });

            instance.GameStateCallbacks.Listen(droneState, state => state.upgradePoints, (currentValue, _) =>
            {
                // Notify only if this is the local player
                // TODO: Consider using a LocalEvent for this and player upgrade applied.
                if (_isLocalPlayer) _onPlayerUpgradePointGained.Raise(currentValue);
            });

            instance.GameStateCallbacks.Listen(droneState, state => state.lastTurretUpgradeId, (currentValue, _) =>
            {
                OnUpgradeApplied(currentValue);
            });

            instance.GameStateCallbacks.Listen(droneState, state => state.lastBodyUpgradeId, (currentValue, _) =>
            {
                OnUpgradeApplied(currentValue);
            });

            instance.GameStateCallbacks.Listen(droneState, state => state.lastMovementUpgradeId, (currentValue, _) =>
            {
                OnUpgradeApplied(currentValue);
            });
        }

        protected override float ExtractYawDeg(DroneState state) => state.upperRotation * Mathf.Rad2Deg;

        protected override void ApplyTransform(Vector3 targetPos, float targetYawDeg)
        {
            _transform.position = targetPos;

            // Rotation only on body transform
            _bodyTransform.rotation = Quaternion.Euler(0f, targetYawDeg, 0f);
        }

        protected override void OnStateSideEffects(DroneState state)
        {
            _healthBar.UpdatePercentage(state.health, state.maxHealth);
        }

        protected override void ApplySettingOverrides()
        {
            // Temporary local player settings (replace with proper client-side prediction later)
            // TODO: Implement client-side prediction for local player drone
            if (_isLocalPlayer)
            {
                _interpolationBackTime = 0.07f;
                _usesExtrapolation = false;
            }
        }

        private void OnUpgradeApplied(string upgradeId)
        {
            if (_upgradeCollection.TryGetUpgrade(upgradeId, out UpgradeSO upgrade)) ApplyUpgradeVisuals(upgrade);
            if (_isLocalPlayer) _onPlayerUpgradeApplied.Raise(upgradeId); // If this is the local player, raise the event
        }

        private void ApplyUpgradeVisuals(UpgradeSO upgrade)
        {
            if (upgrade == null) throw new ArgumentNullException(nameof(upgrade));

            // Update visuals of applicable mesh filter
            switch (upgrade.UpgradeType)
            {
                case UpgradeType.Turret:
                    if (upgrade.Mesh is not null) _turretMeshFilter.mesh = upgrade.Mesh;
                    break;
                case UpgradeType.Body:
                    if (upgrade.Mesh is not null) _bodyMeshFilter.mesh = upgrade.Mesh;
                    break;
                case UpgradeType.Movement:
                    if (upgrade.Mesh is not null) _movementMeshFilter.mesh = upgrade.Mesh;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnDestroy()
        {
            if (_isLocalPlayer) _onPlayerDeath.Raise();
        }
    }
}