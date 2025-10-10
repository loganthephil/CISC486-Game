using UnityEngine;
using UnityEngine.UI;

namespace DroneStrikers.UI
{
    [RequireComponent(typeof(Slider))]
    public abstract class ProgressBar : MonoBehaviour
    {
        private Slider _slider;

        [SerializeField] private float _minSliderValue = 0.157f;
        [SerializeField] protected Image _fillImage;

        private float _lastValue = -1f;

        protected virtual void Awake()
        {
            _slider = GetComponent<Slider>();
        }

        protected float UpdateValue(float value, float maxValue)
        {
            // Only update if health has changed
            if (Mathf.Approximately(_lastValue, value)) return value / maxValue;
            _lastValue = value;

            // Update health bar visibility and value
            float barPercentage = value / maxValue;

            // Scale the percentage to fit within the slider's min value and 1
            _slider.value = Mathf.Lerp(_minSliderValue, 1f, barPercentage);

            return barPercentage; // Return the raw percentage for further use
        }
    }
}