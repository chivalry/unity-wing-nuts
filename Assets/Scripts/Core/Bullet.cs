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
            // Damage is applied by the target (enemy or player) via their own
            // OnTriggerEnter2D, so we just return the bullet here.
            BulletPool.Instance.ReturnBullet(gameObject);
        }
    }
}
