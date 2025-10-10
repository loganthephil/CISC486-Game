using DroneStrikers.Core;
using DroneStrikers.Events;
using UnityEngine;

namespace DroneStrikers.Combat
{
    [RequireComponent(typeof(TeamMember))]
    public class TeamColorizer : MonoBehaviour
    {
        private TeamMember _teamMember;
        private LocalEvents _localEvents;
        private Renderer[] _renderers;
        private static readonly int ColorID = Shader.PropertyToID("_Color");

        private void Awake()
        {
            _teamMember = GetComponent<TeamMember>();
            _localEvents = GetComponent<LocalEvents>();
            _renderers = GetComponentsInChildren<Renderer>(true);
        }

        private void Start()
        {
            _localEvents.Subscribe(CombatEvents.TeamChanged, UpdateTeamColor);
            UpdateTeamColor(_teamMember.Team); // Ensure color is set at start
        }

        private void UpdateTeamColor(Team newTeam)
        {
            // Set each renderer that has the _Color property to the new team's color
            foreach (Renderer r in _renderers)
                if (r.material.HasProperty(ColorID))
                    r.material.SetColor(ColorID, TeamColorAssignment.LDRColors[newTeam]);
        }
    }
}