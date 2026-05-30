using UnityEngine;
using WingNuts.Player;

namespace WingNuts.Core
{
    // Smooth random wander using Perlin noise. Used by formation anchors and the tanker.
    public class AircraftMover : MonoBehaviour
    {
        public float Speed        = 1f;
        public float TurnRate     = 60f;
        public float WanderRadius = 20f;

        [SerializeField] float mapWidth  = 80f;
        [SerializeField] float mapHeight = 80f;

        Vector2   _home;
        float     _noiseOffset;
        Transform _playerTransform;

        void Awake()
        {
            _home        = transform.position;
            _noiseOffset = Random.Range(0f, 100f);
            var pc = FindAnyObjectByType<PlayerController>();
            if (pc != null) _playerTransform = pc.transform;
            BackgroundScroller.OnMapWrapped += OnMapWrapped;
        }

        void OnDestroy()
        {
            BackgroundScroller.OnMapWrapped -= OnMapWrapped;
        }

        void OnMapWrapped(Vector2 offset)
        {
            transform.position += (Vector3)offset;
            _home              += offset;
            CorrectToNearest();
        }

        // After a wrap shift, put the anchor at whichever toroidal copy is
        // closest to the player so world-space navigation matches the minimap.
        void CorrectToNearest()
        {
            if (_playerTransform == null) return;

            Vector3 pos = transform.position;
            Vector3 p   = _playerTransform.position;

            float dx = pos.x - p.x;
            if      (dx >  mapWidth  * 0.5f) { pos.x -= mapWidth;  _home.x -= mapWidth; }
            else if (dx < -mapWidth  * 0.5f) { pos.x += mapWidth;  _home.x += mapWidth; }

            float dy = pos.y - p.y;
            if      (dy >  mapHeight * 0.5f) { pos.y -= mapHeight; _home.y -= mapHeight; }
            else if (dy < -mapHeight * 0.5f) { pos.y += mapHeight; _home.y += mapHeight; }

            transform.position = pos;
        }

        void Update()
        {
            Vector2 toHome = _home - (Vector2)transform.position;
            float   dist   = toHome.magnitude;
            float   steer;

            if (dist > WanderRadius && dist > 0.01f)
            {
                float desired = Mathf.Atan2(-toHome.x, toHome.y) * Mathf.Rad2Deg;
                float delta   = Mathf.DeltaAngle(transform.eulerAngles.z, desired);
                steer = Mathf.Clamp(delta / (TurnRate * Time.deltaTime), -1f, 1f);
            }
            else
            {
                float noise = Mathf.PerlinNoise(Time.time * 0.3f + _noiseOffset, 0f);
                steer = (noise - 0.5f) * 2f;
            }

            transform.Rotate(0f, 0f, steer * TurnRate * Time.deltaTime);
            transform.position += transform.up * Speed * Time.deltaTime;
        }
    }
}
