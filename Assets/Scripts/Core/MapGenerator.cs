using UnityEngine;
using UnityEngine.Tilemaps;

namespace WingNuts.Core
{
    [RequireComponent(typeof(Tilemap))]
    public class MapGenerator : MonoBehaviour
    {
        [SerializeField] TileBase oceanTile;
        [SerializeField] TileBase islandTile;
        [SerializeField] int mapWidthInTiles  = 80;
        [SerializeField] int mapHeightInTiles = 80;
        [SerializeField] int minIslands = 8;
        [SerializeField] int maxIslands = 12;

        Tilemap _tilemap;

        void Start()
        {
            if (oceanTile == null || islandTile == null)
            {
                Debug.LogError("[MapGenerator] Assign oceanTile and islandTile in the " +
                               "Inspector. Run WingNuts → Generate Sprites first.");
                return;
            }

            _tilemap = GetComponent<Tilemap>();

            int seed = Random.Range(0, int.MaxValue);
            Debug.Log($"[MapGenerator] seed={seed}  (hard-code in Inspector to reproduce)");
            Random.InitState(seed);

            FillOcean();
            StampIslands();
        }

        void FillOcean()
        {
            int halfW = mapWidthInTiles  / 2;
            int halfH = mapHeightInTiles / 2;
            for (int x = -halfW; x < halfW; x++)
            for (int y = -halfH; y < halfH; y++)
                _tilemap.SetTile(new Vector3Int(x, y, 0), oceanTile);
        }

        void StampIslands()
        {
            int count  = Random.Range(minIslands, maxIslands + 1);
            int halfW  = mapWidthInTiles  / 2;
            int halfH  = mapHeightInTiles / 2;
            int margin = 6;

            for (int i = 0; i < count; i++)
            {
                int cx = Random.Range(-halfW + margin, halfW - margin);
                int cy = Random.Range(-halfH + margin, halfH - margin);
                int r  = Random.Range(1, 6); // radius 1–5 → diameter 3–10 tiles

                for (int dx = -r; dx <= r; dx++)
                for (int dy = -r; dy <= r; dy++)
                {
                    if (dx * dx + dy * dy <= r * r)
                        _tilemap.SetTile(new Vector3Int(cx + dx, cy + dy, 0), islandTile);
                }
            }
        }
    }
}
