namespace DroneStrikers.Game.Player.UpgradeSelectionStates
{
    public class UpgradeSelectionNoneState : UpgradeSelectionBaseState
    {
        public UpgradeSelectionNoneState(PlayerUpgradeSelection upgradeSelection) : base(upgradeSelection) { }

        public override void OnEnter() => ClearUI();
    }
}