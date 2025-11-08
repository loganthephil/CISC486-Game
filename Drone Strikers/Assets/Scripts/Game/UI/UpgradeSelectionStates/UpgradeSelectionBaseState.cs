using DroneStrikers.FSM;
using UnityEngine;

namespace DroneStrikers.Game.UI.UpgradeSelectionStates
{
    public abstract class UpgradeSelectionBaseState : BaseState
    {
        protected PlayerUpgradeSelection _upgradeSelection;

        protected UpgradeSelectionBaseState(PlayerUpgradeSelection upgradeSelection) => _upgradeSelection = upgradeSelection;

        protected void ClearUI()
        {
            foreach (Transform child in _upgradeSelection.UpgradeSelectionUIParent) Object.Destroy(child.gameObject);
        }
    }
}