using System.Collections.Generic;
using DroneStrikers.Core.Types;
using UnityEngine;

namespace DroneStrikers.Core
{
    public static class TeamColorAssignment
    {
        public static readonly Dictionary<Team, Color> LDRColors = new()
        {
            { Team.Neutral, new Color(0.8f, 0.8f, 0.8f, 1f) },
            { Team.Red, new Color(0.85f, 0.3f, 0.3f, 1f) },
            { Team.Blue, new Color(0.1f, 0.4f, 0.8f, 1f) }
        };

        public static readonly Dictionary<Team, Color> HDRColors = new()
        {
            { Team.Neutral, new Color(1f, 1f, 1f, 7f) },
            { Team.Red, new Color(1f, 0f, 0f, 7f) },
            { Team.Blue, new Color(0f, 0f, 1f, 7f) }
        };
    }
}