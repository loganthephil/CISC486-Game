using UnityEngine;

namespace DroneStrikers.Game.Menu
{
    public class GameMenu : MonoBehaviour
    {
        [SerializeField] private GameObject _spawnUI;

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
        public void OnPlayerSpawn(GameObject player) => ShowMenu = false;

        /// <summary>
        ///     Event listener. Do not call directly.
        /// </summary>
        public void OnPlayerDeath() => ShowMenu = true;
    }
}