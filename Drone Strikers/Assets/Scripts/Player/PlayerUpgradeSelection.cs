using DroneStrikers.Drone;
using DroneStrikers.Events;
using DroneStrikers.FSM;
using DroneStrikers.Player.UpgradeSelectionStates;
using DroneStrikers.Upgrades;
using UnityEngine;

namespace DroneStrikers.Player
{
    [RequireComponent(typeof(DroneUpgrader))]
    public class PlayerUpgradeSelection : MonoBehaviour
    {
        // TODO: Pre instantiate UI elements and pool or disable/enable instead of instantiating/destroying
        public DroneUpgrader DroneUpgrader { get; private set; }

        [SerializeField] private Transform _upgradeSelectionUIParent;
        public Transform UpgradeSelectionUIParent => _upgradeSelectionUIParent;

        [SerializeField] private GameObject _upgradeSelectionUIPrefab;
        public GameObject UpgradeSelectionUIPrefab => _upgradeSelectionUIPrefab;

        public UpgradeTreeSO SelectedTree { get; set; }

        private LocalEvents _localEvents;
        private StateMachine _stateMachine;

        private int _remainingUpgrades;

        private void Awake()
        {
            DroneUpgrader = GetComponent<DroneUpgrader>();
            _localEvents = GetComponent<LocalEvents>();
        }

        private void Start()
        {
            _stateMachine = new StateMachine();

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

        private void OnEnable() => _localEvents.Subscribe(DroneEvents.LevelUp, OnPlayerLevelUp);
        private void OnDisable() => _localEvents.Unsubscribe(DroneEvents.LevelUp, OnPlayerLevelUp);
        private void Update() => _stateMachine.Update();

        /// <summary>
        ///     Event handler for when the player levels up.
        /// </summary>
        private void OnPlayerLevelUp(int level) => _remainingUpgrades++;

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