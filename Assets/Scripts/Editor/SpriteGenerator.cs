using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using System.IO;

namespace WingNuts.Editor
{
    public static class SpriteGenerator
    {
        const string OutputPath = "Assets/Sprites";
        const int PPU = 32;

        [MenuItem("WingNuts/Generate Sprites")]
        public static void GenerateAll()
        {
            Directory.CreateDirectory(Application.dataPath + "/Sprites");

            GeneratePlane("PlayerPlane",     64, 64, PlayerPlane);
            GeneratePlane("SmallEnemy",      48, 48, SmallEnemy);
            GeneratePlane("LargeEnemy",      80, 80, LargeEnemy);
            GeneratePlane("BossPlane",      128,128, BossPlane);
            GeneratePlane("TankerPlane",     96, 64, TankerPlane);
            GenerateSimple("BulletPlayer",    8, 16, BulletPlayer);
            GenerateSimple("BulletEnemy",     8, 16, BulletEnemy);
            GenerateParachuteShields("PickupShields");
            GenerateParachuteFuel("PickupFuel");
            GenerateParachuteColleague("PickupColleague");
            GenerateOcean("OceanTile",       32, 32);
            GenerateIsland("IslandTile",     32, 32);

            AssetDatabase.Refresh();
            Debug.Log("[SpriteGenerator] All sprites generated in Assets/Sprites/");
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        static void GeneratePlane(string name, int w, int h,
            System.Action<Color32[], int, int> painter)
        {
            var tex = NewTex(w, h);
            painter(tex.GetPixels32(), w, h);
            // painter writes into a local array; re-fetch after paint
            var px = new Color32[w * h];
            painter(px, w, h);
            tex.SetPixels32(px);
            tex.Apply();
            Save(tex, name);
        }

        static void GenerateSimple(string name, int w, int h,
            System.Action<Color32[], int, int> painter)
        {
            var tex = NewTex(w, h);
            var px  = new Color32[w * h];
            painter(px, w, h);
            tex.SetPixels32(px);
            tex.Apply();
            Save(tex, name);
        }

        static Texture2D NewTex(int w, int h)
        {
            var t = new Texture2D(w, h, TextureFormat.RGBA32, false);
            t.filterMode = FilterMode.Point;
            return t;
        }

        static void Save(Texture2D tex, string name)
        {
            string path = $"{OutputPath}/{name}.png";
            File.WriteAllBytes(Application.dataPath + $"/Sprites/{name}.png",
                               tex.EncodeToPNG());
            Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer != null)
            {
                importer.textureType         = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = PPU;
                importer.filterMode          = FilterMode.Point;
                importer.textureCompression  = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }

        static void Set(Color32[] px, int w, int x, int y, Color32 c)
        {
            if (x < 0 || x >= w || y < 0 || y >= px.Length / w) return;
            px[y * w + x] = c;
        }

        static void FillRect(Color32[] px, int w, int x0, int y0,
                              int rw, int rh, Color32 c)
        {
            for (int x = x0; x < x0 + rw; x++)
            for (int y = y0; y < y0 + rh; y++)
                Set(px, w, x, y, c);
        }

        static void Ellipse(Color32[] px, int tw, int cx, int cy,
                            int rx, int ry, Color32 c, bool fill = true)
        {
            for (int x = cx - rx; x <= cx + rx; x++)
            for (int y = cy - ry; y <= cy + ry; y++)
            {
                float dx = (float)(x - cx) / rx;
                float dy = (float)(y - cy) / ry;
                if (fill ? dx*dx + dy*dy <= 1f : Mathf.Abs(dx*dx + dy*dy - 1f) < 0.3f)
                    Set(px, tw, x, y, c);
            }
        }

        // ── Plane painters ───────────────────────────────────────────────────

        // All planes are painted top-down. +Y = nose (forward), origin = bottom-left.

        static void PlayerPlane(Color32[] px, int w, int h)
        {
            // Clear to transparent
            var clear = new Color32(0,0,0,0);
            for (int i = 0; i < px.Length; i++) px[i] = clear;

            var yellow  = new Color32(255, 220,  40, 255);
            var dark    = new Color32(180, 140,  20, 255);
            var cockpit = new Color32( 80, 180, 255, 200);

            int cx = w / 2, cy = h / 2;

            // Fuselage
            FillRect(px, w, cx-3, cy-10, 6, 20, yellow);
            // Nose taper
            FillRect(px, w, cx-2, cy+10, 4,  3, yellow);
            FillRect(px, w, cx-1, cy+13, 2,  2, yellow);
            // Tail taper
            FillRect(px, w, cx-2, cy-12, 4,  2, dark);
            // Wings
            FillRect(px, w, cx-14, cy-2, 28,  5, yellow);
            // Wing shading
            FillRect(px, w, cx-14, cy-2,  5,  2, dark);
            FillRect(px, w, cx+ 9, cy-2,  5,  2, dark);
            // Tail fins
            FillRect(px, w, cx- 8, cy-12,  5, 4, dark);
            FillRect(px, w, cx+ 3, cy-12,  5, 4, dark);
            // Cockpit
            Ellipse(px, w, cx, cy+4, 3, 4, cockpit);
            // Outline accents
            FillRect(px, w, cx-3, cy-1, 1, 14, dark);
            FillRect(px, w, cx+2, cy-1, 1, 14, dark);
        }

        static void SmallEnemy(Color32[] px, int w, int h)
        {
            var clear  = new Color32(0,0,0,0);
            for (int i = 0; i < px.Length; i++) px[i] = clear;

            var red   = new Color32(220,  50,  50, 255);
            var dark  = new Color32(140,  20,  20, 255);
            var glass = new Color32(255, 100, 100, 180);

            int cx = w/2, cy = h/2;

            FillRect(px, w, cx-2, cy-8,  4, 16, red);
            FillRect(px, w, cx-2, cy+8,  4,  2, red);
            FillRect(px, w, cx-1, cy+10, 2,  2, red);
            FillRect(px, w, cx-10, cy-1, 20,  4, red);
            FillRect(px, w, cx-10, cy-1,  4,  2, dark);
            FillRect(px, w, cx+ 6, cy-1,  4,  2, dark);
            FillRect(px, w, cx-5, cy-9,  4,  3, dark);
            FillRect(px, w, cx+1, cy-9,  4,  3, dark);
            Ellipse(px, w, cx, cy+3, 2, 3, glass);
        }

        static void LargeEnemy(Color32[] px, int w, int h)
        {
            var clear  = new Color32(0,0,0,0);
            for (int i = 0; i < px.Length; i++) px[i] = clear;

            var orange = new Color32(230, 120,  30, 255);
            var dark   = new Color32(150,  70,  10, 255);
            var glass  = new Color32(255, 160,  80, 180);

            int cx = w/2, cy = h/2;

            FillRect(px, w, cx-5, cy-14,  10, 28, orange);
            FillRect(px, w, cx-3, cy+14,   6,  4, orange);
            FillRect(px, w, cx-2, cy+18,   4,  2, orange);
            FillRect(px, w, cx-18, cy-3,  36,  6, orange);
            FillRect(px, w, cx-18, cy-3,   6,  3, dark);
            FillRect(px, w, cx+12, cy-3,   6,  3, dark);
            FillRect(px, w, cx- 8, cy-15,  6,  5, dark);
            FillRect(px, w, cx+ 2, cy-15,  6,  5, dark);
            Ellipse(px, w, cx, cy+4, 4, 6, glass);
            FillRect(px, w, cx-5, cy-2, 1, 18, dark);
            FillRect(px, w, cx+4, cy-2, 1, 18, dark);
        }

        static void BossPlane(Color32[] px, int w, int h)
        {
            var clear  = new Color32(0,0,0,0);
            for (int i = 0; i < px.Length; i++) px[i] = clear;

            var orange = new Color32(220, 100,  20, 255);
            var dark   = new Color32(140,  55,   5, 255);
            var red    = new Color32(200,  30,  30, 255);
            var glass  = new Color32(255, 150,  60, 200);

            int cx = w/2, cy = h/2;

            FillRect(px, w, cx-9, cy-24, 18, 48, orange);
            FillRect(px, w, cx-5, cy+24, 10,  6, orange);
            FillRect(px, w, cx-3, cy+30,  6,  4, orange);
            FillRect(px, w, cx-32, cy-6, 64, 10, orange);
            FillRect(px, w, cx-32, cy-6, 10,  5, dark);
            FillRect(px, w, cx+22, cy-6, 10,  5, dark);
            FillRect(px, w, cx-14, cy-26, 10,  8, dark);
            FillRect(px, w, cx+ 4, cy-26, 10,  8, dark);
            // Engine pods under wings
            FillRect(px, w, cx-26, cy-4,  6, 8, dark);
            FillRect(px, w, cx+20, cy-4,  6, 8, dark);
            // Cockpit
            Ellipse(px, w, cx, cy+6, 6, 9, glass);
            // Hull detail lines
            FillRect(px, w, cx-9, cy-4, 1, 28, dark);
            FillRect(px, w, cx+8, cy-4, 1, 28, dark);
            FillRect(px, w, cx-3, cy-4, 1, 28, dark);
            FillRect(px, w, cx+2, cy-4, 1, 28, dark);
            // Nose warning stripe
            FillRect(px, w, cx-6, cy+28, 12, 3, red);
        }

        static void TankerPlane(Color32[] px, int w, int h)
        {
            var clear  = new Color32(0,0,0,0);
            for (int i = 0; i < px.Length; i++) px[i] = clear;

            var green  = new Color32( 80, 200, 100, 255);
            var dark   = new Color32( 40, 120,  55, 255);
            var white  = new Color32(240, 240, 240, 255);
            var hose   = new Color32(200, 180,  60, 255);

            int cx = w/2, cy = h/2;

            // Wide body
            FillRect(px, w, cx-6, cy-20, 12, 40, green);
            FillRect(px, w, cx-4, cy+20,  8,  4, green);
            FillRect(px, w, cx-2, cy+24,  4,  3, green);
            // Large wings
            FillRect(px, w, cx-22, cy-4, 44,  8, green);
            FillRect(px, w, cx-22, cy-4,  8,  4, dark);
            FillRect(px, w, cx+14, cy-4,  8,  4, dark);
            // Tail fins
            FillRect(px, w, cx-10, cy-22,  8, 6, dark);
            FillRect(px, w, cx+ 2, cy-22,  8, 6, dark);
            // Friendly markings (white cross on fuselage)
            FillRect(px, w, cx-1, cy-8,  2, 10, white);
            FillRect(px, w, cx-4, cy-4,  8,  2, white);
            // Fuel hose/drogue at tail (bottom)
            FillRect(px, w, cx-1, cy-24,  2, 6, hose);
            Ellipse(px, w, cx, cy-28, 3, 2, hose);
        }

        // ── Bullet painters ──────────────────────────────────────────────────

        static void BulletPlayer(Color32[] px, int w, int h)
        {
            var clear  = new Color32(0,0,0,0);
            var yellow = new Color32(255, 240, 80, 255);
            var bright = new Color32(255, 255,180, 255);
            for (int i = 0; i < px.Length; i++) px[i] = clear;
            Ellipse(px, w, w/2, h/2, w/2-1, h/2-1, yellow);
            Set(px, w, w/2, h/2, bright);
        }

        static void BulletEnemy(Color32[] px, int w, int h)
        {
            var clear = new Color32(0,0,0,0);
            var red   = new Color32(255,  60, 60, 255);
            var bright= new Color32(255, 180,180, 255);
            for (int i = 0; i < px.Length; i++) px[i] = clear;
            Ellipse(px, w, w/2, h/2, w/2-1, h/2-1, red);
            Set(px, w, w/2, h/2, bright);
        }

        // ── Parachute painters ───────────────────────────────────────────────

        // Shared: draw canopy + ropes into px, return the y of the payload top.
        static int DrawCanopy(Color32[] px, int w, int h)
        {
            var white = new Color32(240, 240, 240, 255);
            var rope  = new Color32(180, 160, 120, 255);
            int cx = w / 2;
            int canopyTop = h - 22; // bottom of texture = y=0; canopy sits near top

            // Filled dome (half-ellipse, opening downward)
            for (int x = 0; x < w; x++)
            for (int y = canopyTop; y < h; y++)
            {
                float dx = (float)(x - cx) / (w / 2 - 2);
                float dy = (float)(y - canopyTop) / 18f;
                if (dx * dx + dy * dy <= 1f)
                    Set(px, w, x, y, white);
            }
            // Canopy panel lines
            for (int s = 0; s < 4; s++)
            {
                int sx = cx - 15 + s * 10;
                for (int y = canopyTop; y < canopyTop + 18; y++)
                    Set(px, w, sx, y, rope);
            }
            // Ropes converging from canopy bottom to payload
            int payloadTop = canopyTop - 14;
            for (int r = 0; r < 4; r++)
            {
                int rx = cx - 9 + r * 6;
                for (int y = payloadTop + 10; y < canopyTop; y++)
                    Set(px, w, rx, y, rope);
            }
            return payloadTop;
        }

        // Shields pickup: white box with red cross (first aid kit)
        static void GenerateParachuteShields(string name)
        {
            int w = 48, h = 64;
            var tex = NewTex(w, h);
            var px  = new Color32[w * h];
            var clear = new Color32(0, 0, 0, 0);
            for (int i = 0; i < px.Length; i++) px[i] = clear;

            int pt = DrawCanopy(px, w, h);
            int cx = w / 2;

            // White kit box
            var boxWhite  = new Color32(240, 240, 240, 255);
            var boxOutline = new Color32(180, 180, 180, 255);
            var crossRed  = new Color32(210,  30,  30, 255);

            FillRect(px, w, cx - 7, pt, 14, 10, boxWhite);
            // Outline
            for (int x = cx-7; x < cx+7; x++) { Set(px, w, x, pt,    boxOutline);
                                                  Set(px, w, x, pt+9,  boxOutline); }
            for (int y = pt; y < pt+10; y++)   { Set(px, w, cx-7, y,  boxOutline);
                                                  Set(px, w, cx+6, y,  boxOutline); }
            // Red cross
            FillRect(px, w, cx - 1, pt + 2, 2, 6, crossRed); // vertical bar
            FillRect(px, w, cx - 3, pt + 4, 6, 2, crossRed); // horizontal bar

            tex.SetPixels32(px);
            tex.Apply();
            Save(tex, name);
        }

        // Fuel pickup: cylindrical barrel (dark grey/green with bands)
        static void GenerateParachuteFuel(string name)
        {
            int w = 48, h = 64;
            var tex = NewTex(w, h);
            var px  = new Color32[w * h];
            var clear = new Color32(0, 0, 0, 0);
            for (int i = 0; i < px.Length; i++) px[i] = clear;

            int pt = DrawCanopy(px, w, h);
            int cx = w / 2;

            var barrelBody  = new Color32( 60,  80,  60, 255);
            var barrelLight = new Color32( 90, 115,  90, 255);
            var barrelBand  = new Color32(180, 150,  60, 255);
            var barrelDark  = new Color32( 35,  50,  35, 255);
            var capColor    = new Color32(140, 110,  40, 255);

            // Barrel body (rounded rectangle — cylinder side view)
            FillRect(px, w, cx - 5, pt,     10, 11, barrelBody);
            FillRect(px, w, cx - 4, pt - 1,  8,  1, barrelBody); // top cap curve
            FillRect(px, w, cx - 4, pt + 11, 8,  1, barrelBody); // bottom cap curve
            // Light stripe (highlight)
            FillRect(px, w, cx - 4, pt + 1,  2,  9, barrelLight);
            // Dark edge
            FillRect(px, w, cx + 3, pt + 1,  2,  9, barrelDark);
            // Metal bands
            FillRect(px, w, cx - 5, pt + 3, 10,  1, barrelBand);
            FillRect(px, w, cx - 5, pt + 7, 10,  1, barrelBand);
            // Cap top
            FillRect(px, w, cx - 3, pt + 11, 6,  2, capColor);
            // Spout
            Set(px, w, cx,     pt + 12, capColor);
            Set(px, w, cx + 1, pt + 12, capColor);

            tex.SetPixels32(px);
            tex.Apply();
            Save(tex, name);
        }

        // Colleague pickup: soldier silhouette (helmet, torso, arms raised)
        static void GenerateParachuteColleague(string name)
        {
            int w = 48, h = 64;
            var tex = NewTex(w, h);
            var px  = new Color32[w * h];
            var clear = new Color32(0, 0, 0, 0);
            for (int i = 0; i < px.Length; i++) px[i] = clear;

            int pt = DrawCanopy(px, w, h);
            int cx = w / 2;

            var skin    = new Color32(210, 170, 120, 255);
            var uniform = new Color32( 80, 100,  60, 255);
            var dark    = new Color32( 50,  65,  35, 255);
            var helmet  = new Color32( 55,  75,  40, 255);
            var boot    = new Color32( 50,  40,  30, 255);

            // Legs
            FillRect(px, w, cx - 3, pt,      2, 5, uniform);
            FillRect(px, w, cx + 1, pt,      2, 5, uniform);
            // Boots
            FillRect(px, w, cx - 4, pt,      2, 2, boot);
            FillRect(px, w, cx + 2, pt,      2, 2, boot);
            // Torso
            FillRect(px, w, cx - 3, pt + 5,  6, 5, uniform);
            FillRect(px, w, cx - 2, pt + 5,  4, 5, dark); // shading
            // Belt
            FillRect(px, w, cx - 3, pt + 5,  6, 1, boot);
            // Arms raised (V shape)
            // Left arm
            FillRect(px, w, cx - 6, pt + 7,  2, 2, uniform);
            FillRect(px, w, cx - 7, pt + 9,  2, 2, uniform);
            Set(px, w, cx - 7, pt + 10, skin); Set(px, w, cx - 6, pt + 10, skin); // left hand
            // Right arm
            FillRect(px, w, cx + 4, pt + 7,  2, 2, uniform);
            FillRect(px, w, cx + 5, pt + 9,  2, 2, uniform);
            Set(px, w, cx + 5, pt + 10, skin); Set(px, w, cx + 6, pt + 10, skin); // right hand
            // Head
            FillRect(px, w, cx - 2, pt + 10, 4, 3, skin);
            // Helmet
            FillRect(px, w, cx - 3, pt + 12, 6, 3, helmet);
            FillRect(px, w, cx - 2, pt + 14, 4, 1, dark); // helmet brim

            tex.SetPixels32(px);
            tex.Apply();
            Save(tex, name);
        }

        // ── Tile painters ────────────────────────────────────────────────────

        static void GenerateOcean(string name, int w, int h)
        {
            var tex = NewTex(w, h);
            var px  = new Color32[w * h];

            var deep     = new Color32( 15,  55, 120, 255);
            var mid      = new Color32( 20,  70, 150, 255);
            var surface  = new Color32( 30,  90, 170, 255);
            var crest    = new Color32( 60, 130, 200, 255);

            for (int i = 0; i < px.Length; i++) px[i] = mid;

            // Subtle wave lines
            for (int x = 0; x < w; x++)
            {
                Set(px, w, x, 4,  surface);
                Set(px, w, x, 5,  surface);
                Set(px, w, x, 12, deep);
                Set(px, w, x, 20, surface);
                Set(px, w, x, 21, surface);
                Set(px, w, x, 28, deep);
            }
            // Wave crests (small white tips)
            Set(px, w,  5,  5, crest); Set(px, w,  6,  5, crest);
            Set(px, w, 20,  5, crest); Set(px, w, 21,  5, crest);
            Set(px, w, 10, 21, crest); Set(px, w, 11, 21, crest);
            Set(px, w, 25, 21, crest); Set(px, w, 26, 21, crest);

            tex.SetPixels32(px);
            tex.Apply();
            Save(tex, name);
        }

        static void GenerateIsland(string name, int w, int h)
        {
            var tex = NewTex(w, h);
            var px  = new Color32[w * h];

            var sand   = new Color32(210, 185, 110, 255);
            var grass  = new Color32( 70, 140,  50, 255);
            var dark   = new Color32( 45,  95,  30, 255);
            var rock   = new Color32(120, 110,  95, 255);
            var clear  = new Color32(  0,   0,   0,   0);

            for (int i = 0; i < px.Length; i++) px[i] = clear;

            // Sand base
            FillRect(px, w, 2, 2, 28, 28, sand);
            // Grass patches
            FillRect(px, w,  5,  6, 10, 10, grass);
            FillRect(px, w, 15, 14, 12,  8, grass);
            FillRect(px, w,  8, 18,  7,  7, dark);
            // Rock
            FillRect(px, w, 20,  6,  5,  5, rock);
            // Shoreline (lighter edge)
            for (int x = 2; x < 30; x++) { Set(px, w, x, 2, sand); Set(px, w, x, 29, sand); }
            for (int y = 2; y < 30; y++) { Set(px, w, 2, y, sand); Set(px, w, 29, y,  sand); }

            tex.SetPixels32(px);
            tex.Apply();
            Save(tex, name);
        }
    }
}
