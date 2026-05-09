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

        public Tilemap   Tilemap    => _tilemap;
        public TileBase  IslandTile => islandTile;
        public int       MapWidth   => mapWidthInTiles;
        public int       MapHeight  => mapHeightInTiles;

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
            // Fill 3x the logical size so the camera never sees an edge regardless
            // of how far the grid shifts during wrapping.
            int halfW = mapWidthInTiles  / 2;
            int halfH = mapHeightInTiles / 2;
            int extW  = halfW * 3;
            int extH  = halfH * 3;

            var bounds = new BoundsInt(-extW, -extH, 0, extW * 2, extH * 2, 1);
            var tiles  = new TileBase[extW * 2 * extH * 2];
            for (int i = 0; i < tiles.Length; i++) tiles[i] = oceanTile;
            _tilemap.SetTilesBlock(bounds, tiles);
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
                int r  = Random.Range(1, 6);

                // Stamp at all 9 tiled positions so the map wraps seamlessly.
                for (int tx = -1; tx <= 1; tx++)
                for (int ty = -1; ty <= 1; ty++)
                    StampCircle(cx + tx * mapWidthInTiles,
                                cy + ty * mapHeightInTiles, r);
            }
        }

        void StampCircle(int cx, int cy, int r)
        {
            for (int dx = -r; dx <= r; dx++)
            for (int dy = -r; dy <= r; dy++)
            {
                if (dx * dx + dy * dy <= r * r)
                    _tilemap.SetTile(new Vector3Int(cx + dx, cy + dy, 0), islandTile);
            }
        }
    }
}
