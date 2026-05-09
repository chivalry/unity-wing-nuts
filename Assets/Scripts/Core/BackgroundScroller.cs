// Script Execution Order: BackgroundScroller must run AFTER CameraFollow.
// Set this in Project Settings → Script Execution Order (e.g. +100).
using System;
using UnityEngine;

namespace WingNuts.Core
{
    public class BackgroundScroller : MonoBehaviour
    {
        public static event Action<Vector2> OnMapWrapped;

        [SerializeField] Transform gridTransform;
        [SerializeField] Camera mainCamera;
        [SerializeField] float mapWidth  = 80f;
        [SerializeField] float mapHeight = 80f;

        void Start()
        {
            if (mainCamera == null) mainCamera = Camera.main;
        }

        void LateUpdate()
        {
            CheckWrap(Vector2.right, mapWidth);
            CheckWrap(Vector2.up,    mapHeight);
        }

        void CheckWrap(Vector2 axis, float mapSize)
        {
            // Measure how far the camera has drifted from the grid origin along
            // this axis. When it exceeds one full map length, shift the grid back
            // by that length and notify world objects to shift with it.
            float camPos  = axis == Vector2.right
                ? mainCamera.transform.position.x
                : mainCamera.transform.position.y;
            float gridPos = axis == Vector2.right
                ? gridTransform.position.x
                : gridTransform.position.y;

            float relative = camPos - gridPos;
            if (Mathf.Abs(relative) < mapSize) return;

            float sign    = Mathf.Sign(relative);
            Vector3 shift = (Vector3)(axis * sign * mapSize);
            gridTransform.position += shift;
            OnMapWrapped?.Invoke(axis * sign * mapSize);
        }
    }
}
