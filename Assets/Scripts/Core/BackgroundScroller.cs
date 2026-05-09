using System;
using UnityEngine;

namespace WingNuts.Core
{
    // Script Execution Order: must run AFTER CameraFollow (+100).
    // Shifts the Grid by one map length when the player drifts past the
    // half-map boundary. The tilemap is generated 3x the logical size and
    // tiled seamlessly, so the shift is invisible to the camera.
    // Neither the player nor the camera ever move as a result of wrapping.
    public class BackgroundScroller : MonoBehaviour
    {
        public static event Action<Vector2> OnMapWrapped;

        [SerializeField] Transform playerTransform;
        [SerializeField] Transform gridTransform;
        [SerializeField] float mapWidth  = 80f;
        [SerializeField] float mapHeight = 80f;

        void LateUpdate()
        {
            if (playerTransform == null || gridTransform == null) return;

            Vector2 offset = Vector2.zero;
            offset += CheckAxis(Vector2.right, mapWidth);
            offset += CheckAxis(Vector2.up,    mapHeight);

            if (offset != Vector2.zero)
                OnMapWrapped?.Invoke(offset);
        }

        Vector2 CheckAxis(Vector2 axis, float mapSize)
        {
            float halfSize = mapSize * 0.5f;

            float playerPos = axis == Vector2.right
                ? playerTransform.position.x
                : playerTransform.position.y;
            float gridPos = axis == Vector2.right
                ? gridTransform.position.x
                : gridTransform.position.y;

            float relative = playerPos - gridPos;
            if (Mathf.Abs(relative) < halfSize) return Vector2.zero;

            float sign = Mathf.Sign(relative);
            gridTransform.position += (Vector3)(axis * sign * mapSize);
            return axis * sign * mapSize;
        }
    }
}
