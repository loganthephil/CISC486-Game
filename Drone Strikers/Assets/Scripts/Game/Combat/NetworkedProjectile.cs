using DroneStrikers.Core.Editor;
using DroneStrikers.Core.Types;
using DroneStrikers.Networking;
using UnityEngine;

namespace DroneStrikers.Game.Combat
{
    public class NetworkedProjectile : NetworkedEntityBase<ProjectileState>
    {
        [SerializeField] [RequiredField] private TeamMember _teamMember;

        private Vector3 _serverVelocity = Vector3.zero;

        protected override bool UsesInterpolation => true;
        protected override bool UsesExtrapolation => true;

        public void Initialize(ProjectileState projectileState)
        {
            _teamMember.Team = (Team)projectileState.team;
            InitializeFromState(projectileState);

            NetworkManager.Instance.GameStateCallbacks.OnChange(projectileState, () =>
            {
                OnNetworkStateUpdated(projectileState);
            });
        }

        protected override void ApplyAdditionalStateImmediately(ProjectileState state)
        {
            _serverVelocity = new Vector3(state.velX, 0f, state.velY);
        }

        protected override void OnStateSideEffects(ProjectileState state)
        {
            _serverVelocity = new Vector3(state.velX, 0f, state.velY);
        }

        protected override void ApplySettingOverrides()
        {
            _transformYLevel = 1.1f; // Projectiles fly above ground level

            _interpolationBackTime = 0f;
            _extrapolationLimit = 1f; // Projectiles have a consistent velocity, so we can extrapolate further
        }
    }
}