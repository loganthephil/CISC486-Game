using DroneStrikers.Core.Editor;
using DroneStrikers.Core.Types;
using DroneStrikers.Networking;
using UnityEngine;

namespace DroneStrikers.Game.Combat
{
    public class NetworkedProjectile : NetworkedEntityBase<ProjectileState>
    {
        [SerializeField] [RequiredField] private TeamMember _teamMember;

        // protected override void Update()
        // {
        //     base.Update();
        //
        //     Debug.Log("Position: " + transform.position + " | ~Server Time: " + NetworkManager.EstimatedServerTime);
        // }

        public void Initialize(ProjectileState projectileState)
        {
            _teamMember.Team = (Team)projectileState.team;
            InitializeFromState(projectileState);

            NetworkManager.Instance.GameStateCallbacks.OnChange(projectileState, () =>
            {
                OnNetworkStateUpdated(projectileState);
            });
        }

        protected override void ApplySettingOverrides()
        {
            _transformYLevel = 1.1f; // Projectiles fly above ground level

            _usesInterpolation = false;
            _extrapolationLimit = 1f; // Projectiles have a consistent velocity, so we can extrapolate further
        }
    }
}