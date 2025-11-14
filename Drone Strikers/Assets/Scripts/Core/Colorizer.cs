using UnityEngine;

namespace DroneStrikers.Core
{
    public class Colorizer : MonoBehaviour
    {
        private static readonly int ColorID = Shader.PropertyToID("_Color");

        [SerializeField] private bool _setOnStart = true;
        [SerializeField] private Color _color;

        private Renderer[] _renderers;

        private void Awake() => _renderers = GetComponentsInChildren<Renderer>(true);

        private void Start()
        {
            if (_setOnStart) SetColor(_color);
        }

        public void SetColor(Color color)
        {
            // Set each renderer that has the _Color property to the new team's color
            foreach (Renderer r in _renderers)
                if (r.material.HasProperty(ColorID))
                    r.material.SetColor(ColorID, color);
        }
    }
}