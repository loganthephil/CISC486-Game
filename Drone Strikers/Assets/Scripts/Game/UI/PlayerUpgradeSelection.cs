using DroneStrikers.Core.Editor;
using DroneStrikers.Events;
using DroneStrikers.FSM;
using DroneStrikers.Game.Drone;
using DroneStrikers.Game.UI.UpgradeSelectionStates;
using DroneStrikers.Game.Upgrades;
using UnityEngine;

namespace DroneStrikers.Game.UI
{
    public class PlayerUpgradeSelection : MonoBehaviour
    {
        [field: SerializeField] [field: RequiredField] public DroneUpgrader DroneUpgrader { get; private set; }
        [SerializeField] [RequiredField] private LocalEvents _localEvents;

        [SerializeField] private Transform _upgradeSelectionUIParent;
        public Transform UpgradeSelectionUIParent => _upgradeSelectionUIParent;

        [SerializeField] private GameObject _upgradeSelectionUIPrefab;
        public GameObject UpgradeSelectionUIPrefab => _upgradeSelectionUIPrefab;

        public UpgradeTreeSO SelectedTree { get; set; }
        private FiniteStateMachine _stateMachine;
        private int _remainingUpgrades;

        private void Start()
        {
            _stateMachine = new FiniteStateMachine();

            // Initialize states
            UpgradeSelectionNoneState noneState = new(this);
            UpgradeSelectionTreeState treeState = new(this);
            UpgradeSelectionUpgradeState upgradeState = new(this);

            // Define transitions
            _stateMachine.AddTransition(noneState, treeState, new FuncPredicate(() => _remainingUpgrades > 0));

            _stateMachine.AddTransition(treeState, upgradeState, new FuncPredicate(() => SelectedTree != null));

            _stateMachine.AddTransition(upgradeState, noneState, new FuncPredicate(() => _remainingUpgrades == 0));
            _stateMachine.AddTransition(upgradeState, treeState, new FuncPredicate(() => SelectedTree == null && _remainingUpgrades > 0));

            // Set initial state
            _stateMachine.SetState(noneState);
        }

        private void OnEnable() => _localEvents.Subscribe(DroneEvents.UpgradePointGained, OnPlayerUpgradePointGained);
        private void OnDisable() => _localEvents.Unsubscribe(DroneEvents.UpgradePointGained, OnPlayerUpgradePointGained);
        private void Update() => _stateMachine.Update();

        /// <summary>
        ///     Event handler for when the player levels up.
        /// </summary>
        private void OnPlayerUpgradePointGained(int upgradePoints) => _remainingUpgrades = upgradePoints;

        /// <summary>
        ///     Called when the player selects an upgrade to apply.
        /// </summary>
        /// <param name="upgrade"> The selected upgrade. </param>
        public void SelectUpgrade(UpgradeSO upgrade)
        {
            DroneUpgrader.ApplyUpgrade(upgrade, SelectedTree);
            _remainingUpgrades--;
            SelectedTree = null;
        }
    }
}