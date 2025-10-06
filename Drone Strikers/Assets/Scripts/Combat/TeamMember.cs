using DroneStrikers.Events;
using UnityEngine;

namespace DroneStrikers.Combat
{
    public class TeamMember : MonoBehaviour
    {
        private LocalEvents _localEvents;

        [SerializeField] private Team _team = Team.Neutral;
        public Team Team
        {
            get => _team;
            set
            {
                // Only change team and invoke event if the team is actually changing
                if (_team != value)
                {
                    _team = value;
                    _localEvents?.Invoke(CombatEvents.TeamChanged, _team);
                }
            }
        }

        private void Awake()
        {
            _localEvents = GetComponent<LocalEvents>();
        }
    }
}