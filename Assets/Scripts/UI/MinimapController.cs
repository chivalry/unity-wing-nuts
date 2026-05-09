using UnityEngine;
using UnityEngine.UI;

namespace WingNuts.UI
{
    // Keeps the minimap camera aligned with the Tilemap as it wraps.
    // Attach to the MinimapCamera GameObject.
    public class MinimapController : MonoBehaviour
    {
        [SerializeField] RawImage minimapDisplay;
        [SerializeField] RenderTexture minimapRT;

        Camera _cam;

        void Awake()
        {
            _cam = GetComponent<Camera>();
            if (_cam != null && minimapRT != null)
                _cam.targetTexture = minimapRT;
            if (minimapDisplay != null && minimapRT != null)
                minimapDisplay.texture = minimapRT;
        }

        void OnEnable()
        {
            Core.BackgroundScroller.OnMapWrapped += HandleWrap;
        }

        void OnDisable()
        {
            Core.BackgroundScroller.OnMapWrapped -= HandleWrap;
        }

        void HandleWrap(Vector2 offset)
        {
            // Shift the minimap camera by the same offset as the Grid so it
            // stays framing the full Tilemap after each wrap.
            transform.position += new Vector3(offset.x, offset.y, 0f);
        }
    }
}
