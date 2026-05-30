using UnityEngine;

namespace WingNuts.Core
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField] float speed    = 15f;
        [SerializeField] float maxRange = 20f;

        Vector2 _direction;
        Vector3 _spawnPosition;
        bool    _spent;

        public void Launch(Vector2 direction)
        {
            _direction     = direction.normalized;
            _spawnPosition = transform.position;
            _spent         = false;
        }

        void Update()
        {
            transform.position += (Vector3)(_direction * speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, _spawnPosition) > maxRange)
                Return();
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (_spent) return;

            if (gameObject.layer == LayerMask.NameToLayer("EnemyBullet") &&
                other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                WingNuts.Player.PlayerStats.Instance.TakeDamage(10);
            }
            else if (gameObject.layer == LayerMask.NameToLayer("PlayerBullet"))
            {
                var enemy = other.GetComponent<WingNuts.Enemy.EnemyBase>();
                var boss  = other.GetComponent<WingNuts.Enemy.BossPlane>();
                if (enemy != null) enemy.TakeDamage(10);
                if (boss  != null) boss.TakeDamage(10);
            }

            Return();
        }

        void Return()
        {
            if (_spent) return;
            _spent = true;
            BulletPool.Instance.ReturnBullet(gameObject);
        }
    }
}
