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

        protected override void Awake()
        {
            _canvas = GetComponent<Canvas>();
            _canvas.enabled = false; // Initially hidden
            base.Awake();
        }

        public void UpdatePercentage(float currentHealth, float maxHealth)
        {
            float percentage = UpdateValue(currentHealth, maxHealth);

            _canvas.enabled = percentage is < 1 and > 0; // Only show if not full health or empty

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