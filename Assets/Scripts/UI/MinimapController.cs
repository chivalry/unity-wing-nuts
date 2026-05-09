using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using WingNuts.Core;
using WingNuts.Player;

namespace WingNuts.UI
{
    // Bakes the tilemap into a Texture2D once at startup (1 pixel per tile),
    // then pans the RawImage uvRect to follow the player. No camera needed.
    public class MinimapController : MonoBehaviour
    {
        [SerializeField] RawImage minimapDisplay;
        [SerializeField] Transform playerTransform;

        [SerializeField] Color oceanColor  = new Color(0.15f, 0.45f, 0.75f);
        [SerializeField] Color islandColor = new Color(0.80f, 0.70f, 0.40f);

        Transform _gridTransform;
        Texture2D _mapTex;
        int _mapW, _mapH;

        void Awake()
        {
            var cam = GetComponent<Camera>();
            if (cam != null) cam.enabled = false;

            if (playerTransform == null)
            {
                var pc = FindObjectOfType<PlayerController>();
                if (pc != null) playerTransform = pc.transform;
            }

            var grid = FindObjectOfType<Grid>();
            if (grid != null) _gridTransform = grid.transform;
        }

        void Start()
        {
            StartCoroutine(BakeAfterMapGen());
        }

        IEnumerator BakeAfterMapGen()
        {
            yield return null; // let MapGenerator.Start() finish

            var mapGen = FindObjectOfType<MapGenerator>();
            if (mapGen == null) yield break;

            _mapW = mapGen.MapWidth;
            _mapH = mapGen.MapHeight;
            Tilemap  tilemap    = mapGen.Tilemap;
            TileBase islandTile = mapGen.IslandTile;

            _mapTex = new Texture2D(_mapW, _mapH, TextureFormat.RGB24, false);
            _mapTex.filterMode = FilterMode.Point;
            _mapTex.wrapMode   = TextureWrapMode.Repeat;

            for (int x = 0; x < _mapW; x++)
            for (int y = 0; y < _mapH; y++)
            {
                Vector3Int cell = new Vector3Int(x - _mapW / 2, y - _mapH / 2, 0);
                bool land = tilemap.GetTile(cell) == islandTile;
                _mapTex.SetPixel(x, y, land ? islandColor : oceanColor);
            }
            _mapTex.Apply();

            if (minimapDisplay != null)
            {
                minimapDisplay.texture = _mapTex;
                AddPlayerDot();
            }
        }

        void AddPlayerDot()
        {
            var dotGO = new GameObject("PlayerDot");
            dotGO.transform.SetParent(minimapDisplay.transform, false);

            var img = dotGO.AddComponent<Image>();
            img.color = Color.white;

            var rt = dotGO.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(4f, 4f);
        }

        void LateUpdate()
        {
            if (playerTransform == null || _mapTex == null || minimapDisplay == null) return;

            float gridX = _gridTransform ? _gridTransform.position.x : 0f;
            float gridY = _gridTransform ? _gridTransform.position.y : 0f;

            // Relative position stays in (-mapW/2, mapW/2) thanks to BackgroundScroller
            float relX = playerTransform.position.x - gridX;
            float relY = playerTransform.position.y - gridY;

            // UV of the player in the texture (0-1); center the full map on that point
            float u = relX / _mapW + 0.5f;
            float v = relY / _mapH + 0.5f;

            // w=1, h=1 shows the whole map; Repeat wrap handles toroidal edges
            minimapDisplay.uvRect = new Rect(u - 0.5f, v - 0.5f, 1f, 1f);
        }
    }
}
