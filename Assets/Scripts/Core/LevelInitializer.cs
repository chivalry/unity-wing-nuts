using UnityEngine;
using WingNuts.Enemy;

namespace WingNuts.Core
{
    // Spawns all 5 enemy formations at game start. Attach to the GameManager GameObject.
    // 3 of 5 formations include one large enemy; the rest are all small enemies.
    public class LevelInitializer : MonoBehaviour
    {
        [SerializeField] GameObject smallEnemyPrefab;
        [SerializeField] GameObject largeEnemyPrefab;

        // V-formation offsets from the anchor (up to 6 slots).
        static readonly Vector2[] FormationLayout =
        {
            new Vector2(-1.5f,  0.0f),
            new Vector2( 1.5f,  0.0f),
            new Vector2(-3.0f, -1.5f),
            new Vector2( 3.0f, -1.5f),
            new Vector2( 0.0f, -1.5f),
            new Vector2( 0.0f, -3.0f),
        };

        void Start()
        {
            SpawnFormations();
        }

        void SpawnFormations()
        {
            bool[] hasLarge = { true, true, true, false, false };
            Shuffle(hasLarge);
            for (int i = 0; i < hasLarge.Length; i++)
                SpawnFormation(hasLarge[i]);
        }

        void SpawnFormation(bool includeLarge)
        {
            // Place at least 20 units from the world origin (player start).
            Vector2 anchorPos;
            do { anchorPos = new Vector2(Random.Range(-38f, 38f), Random.Range(-38f, 38f)); }
            while (anchorPos.magnitude < 12f);

            var root   = new GameObject("Formation");
            var anchor = new GameObject("Anchor");
            anchor.transform.SetParent(root.transform);
            anchor.transform.position = anchorPos;

            var mover      = anchor.AddComponent<AircraftMover>();
            mover.Speed    = 1f;
            mover.TurnRate = 60f;

            int count = Random.Range(4, 7);
            for (int i = 0; i < count; i++)
            {
                bool isLargeSlot = includeLarge && i == 0;
                var  prefab      = isLargeSlot ? largeEnemyPrefab : smallEnemyPrefab;
                if (prefab == null) continue;

                Vector2 offset = FormationLayout[i % FormationLayout.Length];
                var plane = Instantiate(prefab, anchorPos + offset, Quaternion.identity, root.transform);

                plane.GetComponent<EnemyBase>()?.Init(anchor.transform, offset);
            }
        }

        static void Shuffle(bool[] arr)
        {
            for (int i = arr.Length - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (arr[i], arr[j]) = (arr[j], arr[i]);
            }
        }
    }
}
