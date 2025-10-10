using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DroneStrikers.UI
{
    public class SelectableItemUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _label;
        [SerializeField] private Button _button;

        public void Initialize(string label, UnityAction onClick)
        {
            if (_label != null) _label.text = label;

            if (_button != null)
            {
                _button.onClick.RemoveAllListeners();
                _button.onClick.AddListener(() => onClick?.Invoke());
            }
        }
    }
}