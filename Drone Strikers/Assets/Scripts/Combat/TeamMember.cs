using DroneStrikers.Core;
using UnityEngine;

namespace DroneStrikers.Combat
{
    public class TeamMember : MonoBehaviour
    {
        [SerializeField] private Team _team = Team.Neutral;
        public Team Team
        {
            get => _team;
            set
            {
                _team = value;
                UpdateTeamColor();
            }
        }

        private Renderer[] _renderers;
        private static readonly int TeamColorID = Shader.PropertyToID("_TeamColor");

        private void Awake()
        {
            _renderers = GetComponentsInChildren<Renderer>(true);
        }

        private void Start()
        {
            UpdateTeamColor(); // Ensure color is set at start
        }

        private void UpdateTeamColor()
        {
            foreach (Renderer r in _renderers)
                if (r.material.HasProperty(TeamColorID))
                    r.material.SetColor(TeamColorID, TeamColorAssignment.LDRColors[Team]);
        }
    }
}