using UnityEngine;
using WingNuts.Core;
using WingNuts.Player;

namespace WingNuts.Enemy
{
    // Spawned by GameManager (Phase 10) when all other enemies are dead.
    // Sweeps horizontally, fires a 5-bullet spread toward the player every 0.5 s.
    public class BossPlane : MonoBehaviour
    {
        [SerializeField] int   maxHp        = 20;
        [SerializeField] float fireInterval = 0.5f;
        [SerializeField] float sweepSpeed   = 1.5f;
        [SerializeField] float sweepHalfWidth = 8f;
        [SerializeField] int   scoreValue   = 5000;

        int       _hp;
        float     _fireTimer;
        float     _sweepDir    = 1f;
        float     _sweepOriginX;
        bool      _dead;
        Transform _playerTransform;

        void Awake()
        {
            _hp          = maxHp;
            _sweepOriginX = transform.position.x;
            _fireTimer   = fireInterval;

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
            _sweepOriginX      += offset.x;
        }

        void Update()
        {
            if (_dead) return;
            UpdateSweep();
            UpdateFiring();
        }

        void UpdateSweep()
        {
            transform.position += Vector3.right * _sweepDir * sweepSpeed * Time.deltaTime;
            if (Mathf.Abs(transform.position.x - _sweepOriginX) >= sweepHalfWidth)
                _sweepDir = -_sweepDir;
        }

        void UpdateFiring()
        {
            if (_playerTransform == null) return;
            _fireTimer -= Time.deltaTime;
            if (_fireTimer <= 0f)
            {
                _fireTimer = fireInterval;
                FireSpread();
            }
        }

        void FireSpread()
        {
            Vector2 toPlayer  = ((Vector2)_playerTransform.position - (Vector2)transform.position).normalized;
            float   baseAngle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;

            const int   count  = 5;
            const float spread = 90f;
            float step = spread / (count - 1);

            for (int i = 0; i < count; i++)
            {
                var bullet = BulletPool.Instance.GetBullet(BulletType.Enemy);
                if (bullet == null) continue;

                bullet.transform.position = transform.position;
                float   angle = (baseAngle - spread * 0.5f + step * i) * Mathf.Deg2Rad;
                Vector2 dir   = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

                var b = bullet.GetComponent<Bullet>();
                if (b != null) b.Launch(dir);
            }
        }

        public void TakeDamage(int amount)
        {
            if (_dead) return;
            _hp -= amount;
            if (_hp <= 0) Die();
        }

        void Die()
        {
            if (_dead) return;
            _dead = true;
            PlayerStats.Instance?.AddScore(scoreValue);
            Destroy(gameObject);
        }

        void OnCollisionEnter2D(Collision2D col)
        {
            if (_dead) return;
            if (col.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                PlayerStats.Instance?.TakeDamage(25);
                Die();
            }
        }

        // Bullet damage is applied by Bullet.cs to prevent multi-hit in one physics step.
    }
}
