namespace DroneStrikers.Game.UI.UpgradeSelectionStates
{
    public class UpgradeSelectionNoneState : UpgradeSelectionBaseState
    {
        public UpgradeSelectionNoneState(PlayerUpgradeSelector upgradeSelector) : base(upgradeSelector) { }

        public override void OnEnter() => ClearUI();
    }
}