using System.Collections.Generic;
using UnityEngine;

namespace WingNuts.Core
{
    public class BulletPool : MonoBehaviour
    {
        public static BulletPool Instance { get; private set; }

        [SerializeField] GameObject playerBulletPrefab;
        [SerializeField] GameObject enemyBulletPrefab;
        [SerializeField] int playerPoolSize = 20;
        [SerializeField] int enemyPoolSize  = 50;

        readonly Queue<GameObject> _playerPool = new Queue<GameObject>();
        readonly Queue<GameObject> _enemyPool  = new Queue<GameObject>();

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            Fill(_playerPool, playerBulletPrefab, playerPoolSize);
            Fill(_enemyPool,  enemyBulletPrefab,  enemyPoolSize);
        }

        void Fill(Queue<GameObject> pool, GameObject prefab, int count)
        {
            if (prefab == null) return;
            for (int i = 0; i < count; i++)
            {
                var obj = Instantiate(prefab, transform);
                obj.SetActive(false);
                pool.Enqueue(obj);
            }
        }

        public GameObject GetBullet(BulletType type)
        {
            var pool = type == BulletType.Player ? _playerPool : _enemyPool;
            if (pool.Count == 0) return null;
            var bullet = pool.Dequeue();
            bullet.SetActive(true);
            return bullet;
        }

        public void ReturnBullet(GameObject bullet)
        {
            bullet.SetActive(false);
            // Determine pool by layer
            bool isPlayer = bullet.layer == LayerMask.NameToLayer("PlayerBullet");
            if (isPlayer) _playerPool.Enqueue(bullet);
            else          _enemyPool.Enqueue(bullet);
        }
    }
}
