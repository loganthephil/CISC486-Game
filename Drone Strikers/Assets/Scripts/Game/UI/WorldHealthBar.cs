using DroneStrikers.Core.Interfaces;
using UnityEngine;

namespace DroneStrikers.Game.UI
{
    [RequireComponent(typeof(Canvas))]
    public class WorldHealthBar : ProgressBar
    {
        private static readonly Color FullHealthColor = new(0.46f, 0.92f, 0.38f, 1f);
        private static readonly Color MidHealthColor = new(0.92f, 0.58f, 0.38f, 1f);
        private static readonly Color LowHealthColor = new(0.92f, 0.38f, 0.38f, 1f);

        private Canvas _canvas;
        private IHealth _health;

        protected override void Awake()
        {
            _canvas = GetComponent<Canvas>();
            _health = GetComponentInParent<IHealth>();
            if (_health == null)
            {
                Debug.LogError("WorldHealthBar could not find IHealth component in parent.");
                Destroy(this);
            }

            base.Awake();
        }

        private void Update()
        {
            float percentage = UpdateValue(_health.CurrentHealth, _health.MaxHealth);

            _canvas.enabled = percentage < 1; // Only show if not full health

            // Change color based on health percentage
            _fillImage.color = percentage switch
            {
                > 0.75f => FullHealthColor,
                > 0.35f => MidHealthColor,
                _ => LowHealthColor
            };
        }
    }
}