using DroneStrikers.FSM;
using UnityEngine;

namespace DroneStrikers.Game.UI.UpgradeSelectionStates
{
    public abstract class UpgradeSelectionBaseState : BaseState
    {
        protected PlayerUpgradeSelector _upgradeSelector;

        protected UpgradeSelectionBaseState(PlayerUpgradeSelector upgradeSelector) => _upgradeSelector = upgradeSelector;

        protected void ClearUI()
        {
            foreach (Transform child in _upgradeSelector.UpgradeSelectionUIParent)
            {
                Debug.Log($"Destroying child UI element: {child.gameObject.name}");
                Object.Destroy(child.gameObject);
            }
        }
    }
}