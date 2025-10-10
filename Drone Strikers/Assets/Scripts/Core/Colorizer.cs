using UnityEngine;

namespace DroneStrikers.Core
{
    public class Colorizer : MonoBehaviour
    {
        private static readonly int ColorID = Shader.PropertyToID("_Color");

        [SerializeField] private Color _color;

        private void Start()
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);

            // Set each renderer that has the _Color property to the new team's color
            foreach (Renderer r in renderers)
                if (r.material.HasProperty(ColorID))
                    r.material.SetColor(ColorID, _color);

            Destroy(this); // No longer needed after setting color
        }
    }
}