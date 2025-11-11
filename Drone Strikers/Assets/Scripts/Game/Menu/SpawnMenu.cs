using DroneStrikers.Core.Types;
using DroneStrikers.Networking;
using UnityEngine;

namespace DroneStrikers.Game.Menu
{
    public class SpawnMenu : MonoBehaviour
    {
        public void OnRedTeamButtonPressed() => RequestSpawnPlayerDrone((int)Team.Red);
        public void OnBlueTeamButtonPressed() => RequestSpawnPlayerDrone((int)Team.Blue);

        private static void RequestSpawnPlayerDrone(int team)
        {
            NetworkManager.Send(GameMessages.PlayerJoinTeam, new PlayerJoinTeamMessage { team = team });
        }
    }
}