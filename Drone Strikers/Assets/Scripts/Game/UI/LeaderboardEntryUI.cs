using System;
using DroneStrikers.Core;
using DroneStrikers.Core.Editor;
using DroneStrikers.Core.Types;
using DroneStrikers.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DroneStrikers.Game.UI
{
    public class LeaderboardEntryUI : MonoBehaviour
    {
        [SerializeField] [RequiredField] private TMP_Text _playerNameText;
        [SerializeField] [RequiredField] private TMP_Text _experienceText;
        [SerializeField] [RequiredField] private Image _backgroundImage;

        [SerializeField] [RequiredField] private Sprite _redTeamSprite;
        [SerializeField] [RequiredField] private Sprite _blueTeamSprite;

        private Action _unbindCallbacks;

        public void Initialize(LeaderboardEntry entryState)
        {
            Cleanup(); // Clean up any previous bindings

            NetworkManager networkManager = NetworkManager.Instance;
            _unbindCallbacks += networkManager.GameStateCallbacks.Listen(entryState, state => state.name, (currentValue, _) => _playerNameText.text = currentValue);
            _unbindCallbacks += networkManager.GameStateCallbacks.Listen(entryState, state => state.experience, (currentValue, _) => _experienceText.text = currentValue.ToAbbreviatedString());
            _unbindCallbacks += networkManager.GameStateCallbacks.Listen(entryState, state => state.team, (currentValue, _) => SetTeam((Team)currentValue));
        }

        public void Cleanup()
        {
            _unbindCallbacks?.Invoke();
            _unbindCallbacks = null;
        }

        private void SetTeam(Team team)
        {
            switch (team)
            {
                case Team.Red:
                    _backgroundImage.sprite = _redTeamSprite;
                    break;
                case Team.Blue:
                    _backgroundImage.sprite = _blueTeamSprite;
                    break;
                case Team.Neutral:
                default:
                    Debug.LogWarning("Unknown team for leaderboard entry: " + team);
                    break;
            }
        }
    }
}