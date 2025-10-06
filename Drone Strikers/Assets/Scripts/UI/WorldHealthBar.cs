using DroneStrikers.Combat;
using UnityEngine;
using UnityEngine.UI;

namespace DroneStrikers.UI
{
    [RequireComponent(typeof(Slider))] [RequireComponent(typeof(Canvas))]
    public class WorldHealthBar : MonoBehaviour
    {
        private const float MinSliderValue = 0.157f;
        private static readonly Color FullHealthColor = new(0.46f, 0.92f, 0.38f, 1f);
        private static readonly Color MidHealthColor = new(0.92f, 0.58f, 0.38f, 1f);
        private static readonly Color LowHealthColor = new(0.92f, 0.38f, 0.38f, 1f);

        private IHealth _health;
        private Slider _slider;
        private Canvas _canvas;
        [SerializeField] private Image _fillImage;

        private float _lastCurrentHealth = -1f;

        private void Awake()
        {
            _health = GetComponentInParent<IHealth>();
            if (_health == null)
            {
                Debug.LogError("WorldHealthBar could not find IHealth component in parent.");
                Destroy(this);
            }

            _slider = GetComponent<Slider>();
            _canvas = GetComponent<Canvas>();
        }

        private void Update()
        {
            // Only update if health has changed
            if (Mathf.Approximately(_lastCurrentHealth, _health.CurrentHealth)) return;
            _lastCurrentHealth = _health.CurrentHealth;

            // Update health bar visibility and value
            float healthPercent = _health.CurrentHealth / (float)_health.MaxHealth;
            _canvas.enabled = healthPercent < 1; // Only show if not full health
            _slider.value = Mathf.Max(healthPercent, MinSliderValue);

            // Change color based on health percentage
            _fillImage.color = healthPercent switch
            {
                > 0.75f => FullHealthColor,
                > 0.35f => MidHealthColor,
                _ => LowHealthColor
            };
        }
    }
}