using System;
using System.Collections.Generic;
using DroneStrikers.Core.Editor;
using DroneStrikers.FSM;
using DroneStrikers.Game.Drone;
using DroneStrikers.Game.UI.UpgradeSelectionStates;
using DroneStrikers.Networking;
using UnityEngine;

namespace DroneStrikers.Game.UI
{
    public class PlayerUpgradeSelector : MonoBehaviour
    {
        [SerializeField] [RequiredField] private UpgradeTreeCollectionSO _upgradeCollection;

        [SerializeField] private Transform _upgradeSelectionUIParent;
        public Transform UpgradeSelectionUIParent => _upgradeSelectionUIParent;

        [SerializeField] private GameObject _upgradeSelectionUIPrefab;
        public GameObject UpgradeSelectionUIPrefab => _upgradeSelectionUIPrefab;

        private List<UpgradeTreeSO> _upgradeTrees = new();
        public IReadOnlyList<UpgradeTreeSO> UpgradeTrees => _upgradeTrees;

        public UpgradeTreeSO SelectedTree { get; set; }
        private FiniteStateMachine _stateMachine;
        private bool _showUpgradeSelection;

        private readonly Dictionary<UpgradeTreeSO, UpgradeSO> _lastUpgradeInTrees = new();

        private void Start()
        {
            // Initialize upgrade trees
            _upgradeTrees = _upgradeCollection.GetUpgradeTrees();

            _stateMachine = new FiniteStateMachine();

            // Initialize states
            UpgradeSelectionNoneState noneState = new(this);
            UpgradeSelectionTreeState treeState = new(this);
            UpgradeSelectionUpgradeState upgradeState = new(this);

            // Define transitions
            _stateMachine.AddTransition(noneState, treeState, new FuncPredicate(() => _showUpgradeSelection));

            _stateMachine.AddTransition(treeState, upgradeState, new FuncPredicate(() => SelectedTree != null));

            _stateMachine.AddTransition(upgradeState, noneState, new FuncPredicate(() => !_showUpgradeSelection));
            _stateMachine.AddTransition(upgradeState, treeState, new FuncPredicate(() => SelectedTree == null && _showUpgradeSelection));

            // Set initial state
            _stateMachine.SetState(noneState);
        }

        private void Update() => _stateMachine.Update();

        /// <summary>
        ///     Event listener. Do not call directly.
        /// </summary>
        public void OnUpgradePointsChanged(int upgradePoints)
        {
            if (upgradePoints <= 0) return; // No upgrade points, do nothing

            _showUpgradeSelection = true;
        }

        /// <summary>
        ///     Event listener. Do not call directly.
        /// </summary>
        public void OnUpgradeApplied(string upgradeId)
        {
            if (!_upgradeCollection.TryGetUpgrade(upgradeId, out UpgradeSO appliedUpgrade)) return;

            UpgradeTreeSO tree = _upgradeCollection.GetTreeOfUpgrade(appliedUpgrade);
            _lastUpgradeInTrees[tree] = appliedUpgrade;
        }

        public List<UpgradeSO> GetAvailableUpgradesInTree(UpgradeTreeSO tree)
        {
            // If there has been an upgrade in this tree, return its next upgrades
            if (_lastUpgradeInTrees.TryGetValue(tree, out UpgradeSO lastUpgradeInTree))
            {
                if (lastUpgradeInTree == null) throw new Exception("Last upgrade in tree is null, this should never happen.");
                return new List<UpgradeSO>(lastUpgradeInTree.NextUpgrades);
            }

            // Otherwise, return the starting upgrades
            return new List<UpgradeSO>(tree.StartingUpgrades);
        }

        public bool HasAvailableUpgradesInTree(UpgradeTreeSO tree) => GetAvailableUpgradesInTree(tree).Count > 0;

        /// <summary>
        ///     Called when the player selects an upgrade to apply.
        /// </summary>
        /// <param name="upgrade"> The selected upgrade. </param>
        public void SelectUpgrade(UpgradeSO upgrade)
        {
            _showUpgradeSelection = false; // Hide upgrade selection until server updates the upgrade points

            string upgradeId = _upgradeCollection.UpgradeToId(upgrade);
            NetworkManager.Send(ClientMessages.PlayerSelectUpgrade, new PlayerSelectUpgradeMessage(upgradeId));
        }
    }
}