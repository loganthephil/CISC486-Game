using System.Collections.Generic;
using DroneStrikers.Core.Editor;
using DroneStrikers.Networking;
using TMPro;
using UnityEngine;

namespace DroneStrikers.Game.Menu
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] [RequiredField] private TMP_InputField _usernameInputField;

        public void OnJoinGameClicked()
        {
            string username = _usernameInputField.text.Trim();

            if (string.IsNullOrEmpty(username))
            {
                Debug.LogWarning("Username cannot be empty.");
                return;
            }

            // Join the game with the provided username
            Dictionary<string, object> options = new() { { "username", username } };
            Debug.Log($"Clicked Join Game with username: {username}");
            NetworkManager.Instance.JoinGame(options);
        }
    }
}