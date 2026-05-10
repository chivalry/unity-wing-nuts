using UnityEngine;

namespace WingNuts.Core
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField] float speed    = 15f;
        [SerializeField] float maxRange = 20f;

        Vector2 _direction;
        Vector3 _spawnPosition;

        public void Launch(Vector2 direction)
        {
            _direction     = direction.normalized;
            _spawnPosition = transform.position;
        }

        void Update()
        {
            transform.position += (Vector3)(_direction * speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, _spawnPosition) > maxRange)
                BulletPool.Instance.ReturnBullet(gameObject);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (gameObject.layer == LayerMask.NameToLayer("EnemyBullet") &&
                other.gameObject.layer == LayerMask.NameToLayer("Player"))
                WingNuts.Player.PlayerStats.Instance.TakeDamage(10);

            // Player-bullet damage is handled by the enemy in Phase 7.
            BulletPool.Instance.ReturnBullet(gameObject);
        }
    }
}
