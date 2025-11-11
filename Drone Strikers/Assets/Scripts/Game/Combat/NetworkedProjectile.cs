using DroneStrikers.Core.Editor;
using DroneStrikers.Core.Types;
using DroneStrikers.Networking;
using UnityEngine;

namespace DroneStrikers.Game.Combat
{
    public class NetworkedProjectile : MonoBehaviour
    {
        [SerializeField] [RequiredField] private TeamMember _teamMember;

        [SerializeField] private float _spawnYLevel = 1.1f;

        private Vector3 _velocity = Vector3.zero;

        public void Initialize(ProjectileState projectileState)
        {
            // Initialize projectile based on its state
            transform.position = new Vector3(projectileState.posX, _spawnYLevel, projectileState.posY);
            _velocity = new Vector3(projectileState.velX, 0, projectileState.velY);
            _teamMember.Team = (Team)projectileState.team;
        }

        // TODO: Take into account network updates for more accurate movement
        // We are already getting the position any way so we might as well use it, plus it'll fix de-sync issues
        
        private void FixedUpdate()
        {
            transform.position += _velocity * Time.fixedDeltaTime;
        }
    }
}