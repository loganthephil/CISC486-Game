using System.Collections.Generic;
using DroneStrikers.Core.Editor;
using DroneStrikers.Networking;
using UnityEngine;

namespace DroneStrikers.Game.UI
{
    public class LeaderboardUI : MonoBehaviour
    {
        [SerializeField] [RequiredField] private GameObject _leaderboardEntryPrefab;

        private readonly List<LeaderboardEntryUI> _leaderboardEntries = new();

        private void OnEnable()
        {
            NetworkManager.Instance.AddOnLeaderboardEntryAddedListener(OnLeaderboardEntryAdded);
            NetworkManager.Instance.AddOnLeaderboardEntryRemovedListener(OnLeaderboardEntryRemoved);
        }

        private void OnDisable()
        {
            NetworkManager.Instance.RemoveOnLeaderboardEntryAddedListener(OnLeaderboardEntryAdded);
            NetworkManager.Instance.RemoveOnLeaderboardEntryRemovedListener(OnLeaderboardEntryRemoved);
        }

        private void OnLeaderboardEntryAdded(int index, LeaderboardEntry entry)
        {
            // If the added entry index is within current list, just unhide it
            if (index < _leaderboardEntries.Count)
            {
                LeaderboardEntryUI entryUI = _leaderboardEntries[index];
                entryUI.gameObject.SetActive(true); // Show the entry
                entryUI.Initialize(entry); // Re-initialize with the entry state
                return;
            }

            // Otherwise, create a new entry object
            GameObject newObject = Instantiate(_leaderboardEntryPrefab, transform);
            LeaderboardEntryUI newEntry = newObject.GetComponent<LeaderboardEntryUI>();

            // Sanity check
            if (newEntry == null)
            {
                Debug.LogError("Leaderboard entry prefab is missing LeaderboardEntryUI component.");
                Destroy(newObject);
                return;
            }

            _leaderboardEntries.Add(newEntry); // Add to the list
            newEntry.Initialize(entry); // Initialize with the entry state
        }

        private void OnLeaderboardEntryRemoved(int index, LeaderboardEntry entry)
        {
            // Hide the leaderboard entry UI object
            if (index < 0 || index >= _leaderboardEntries.Count) return; // Sanity check

            LeaderboardEntryUI entryUI = _leaderboardEntries[index];
            entryUI.Cleanup(); // Cleanup any bindings
            entryUI.gameObject.SetActive(false); // Hide the entry
        }
    }
}