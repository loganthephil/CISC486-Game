using System;
using DroneStrikers.Core.Types;
using DroneStrikers.Events;
using UnityEngine;

namespace DroneStrikers.Game.Combat
{
    [RequireComponent(typeof(TeamMember))]
    public class TeamColorizer : MonoBehaviour
    {
        private TeamMember _teamMember;
        private LocalEvents _localEvents;
        private Renderer[] _renderers;
        private static readonly int TeamID = Shader.PropertyToID("_TEAM");

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
            if (newTeam == Team.Neutral) return; // Neutral team has no color
            foreach (Renderer r in _renderers)
                if (r.material.HasProperty(TeamID))
                {
                    foreach (Team team in Enum.GetValues(typeof(Team)))
                    {
                        if (team == Team.Neutral || team == newTeam) continue;
                        r.material.DisableKeyword("_TEAM_" + team.ToString().ToUpper());
                    }

                    r.material.EnableKeyword("_TEAM_" + newTeam.ToString().ToUpper());
                }
        }
    }
}