using DroneStrikers.Player;
using TMPro;
using UnityEngine;

namespace DroneStrikers.Upgrades
{
    public class UpgradeTreeUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _treeNameText;

        public UpgradeTreeSO UpgradeTree;
        public PlayerUpgradeSelection PlayerUpgradeSelection;

        private void Awake()
        {
            _treeNameText = GetComponent<TMP_Text>();
        }

        private void Start()
        {
            if (UpgradeTree == null)
            {
                Debug.LogError("UpgradeTree is not assigned in UpgradeTreeUI.");
                return;
            }

            _treeNameText.text = UpgradeTree.UpgradeTreeName;
        }

        /// <summary>
        ///     Click event handler for when the player selects this upgrade tree.
        /// </summary>
        public void OnPlayerClick()
        {
            PlayerUpgradeSelection.SelectTree(UpgradeTree);
        }
    }
}