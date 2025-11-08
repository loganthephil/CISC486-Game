using DroneStrikers.Core.Types;
using UnityEngine;

namespace DroneStrikers.Game.Menu
{
    public class GameMenu : MonoBehaviour
    {
        [SerializeField] private GameObject _spawnUI;
        [SerializeField] private GameObject _playerObject;

        private bool _showMenu;
        public bool ShowMenu
        {
            get => _showMenu;
            set
            {
                _showMenu = value;
                _spawnUI.SetActive(_showMenu);
            }
        }

        /// <summary>
        ///     Event listener. Do not call directly.
        /// </summary>
        public void OnPlayerSpawn(GameObject player)
        {
            ShowMenu = false;
            _playerObject = player;
        }

        /// <summary>
        ///     Event listener. Do not call directly.
        /// </summary>
        public void OnDroneDeath(DamageContext ctx)
        {
            if (ctx.Receiver != _playerObject) return; // Only respond if the drone that died is the player drone
            ShowMenu = true;
        }
    }
}