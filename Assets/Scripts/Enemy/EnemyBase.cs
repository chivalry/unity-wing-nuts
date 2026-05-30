using UnityEngine;
using WingNuts.Core;
using WingNuts.Player;

namespace WingNuts.Enemy
{
    public abstract class EnemyBase : MonoBehaviour
    {
        protected enum EnemyState { Patrol, Active, Hunt, Dead }

        [SerializeField] protected int   maxHp        = 1;
        [SerializeField] protected float fireInterval = 2f;
        [SerializeField] protected int   scoreValue   = 100;

        protected EnemyState    State = EnemyState.Patrol;
        protected int           Hp;
        protected Transform     PlayerTransform;

        Transform _anchor;
        Vector2   _offset;
        Camera    _mainCam;
        float     _fireTimer;

        public void Init(Transform anchor, Vector2 offset)
        {
            _anchor = anchor;
            _offset = offset;
        }

        protected virtual void Awake()
        {
            Hp       = maxHp;
            _mainCam = Camera.main;

            var pc = FindAnyObjectByType<PlayerController>();
            if (pc != null) PlayerTransform = pc.transform;

            BackgroundScroller.OnMapWrapped += OnMapWrapped;
        }

        void OnDestroy()
        {
            BackgroundScroller.OnMapWrapped -= OnMapWrapped;
        }

        void OnMapWrapped(Vector2 offset)
        {
            // Only shift enemies that aren't following an anchor each frame.
            if (State == EnemyState.Hunt || _anchor == null)
                transform.position += (Vector3)offset;
        }

        protected virtual void Update()
        {
            switch (State)
            {
                case EnemyState.Patrol: UpdatePatrol(); break;
                case EnemyState.Active: UpdateActive(); break;
                case EnemyState.Hunt:   UpdateHunt();   break;
            }
        }

        void UpdatePatrol()
        {
            if (_anchor == null || PlayerTransform == null || _mainCam == null) return;
            FollowAnchor();

            float threshold = Mathf.Max(_mainCam.orthographicSize * 1.5f, 15f);
            if (Vector2.Distance(_anchor.position, PlayerTransform.position) < threshold)
                EnterActive();
        }

        protected virtual void EnterActive()
        {
            State      = EnemyState.Active;
            _fireTimer = fireInterval;
        }

        void UpdateActive()
        {
            FollowAnchor();
            TickFire();
        }

        protected virtual void UpdateHunt() { }

        void FollowAnchor()
        {
            if (_anchor != null)
                transform.position = (Vector2)_anchor.position + _offset;
        }

        protected void TickFire()
        {
            if (PlayerTransform == null) return;
            _fireTimer -= Time.deltaTime;
            if (_fireTimer <= 0f)
            {
                _fireTimer = fireInterval;
                Fire();
            }
        }

        protected virtual void Fire()
        {
            if (PlayerTransform == null) return;

            var bullet = BulletPool.Instance.GetBullet(BulletType.Enemy);
            if (bullet == null) return;

            bullet.transform.position = transform.position;

            Vector2 dir = ((Vector2)(PlayerTransform.position - transform.position)).normalized;
            dir = Quaternion.Euler(0f, 0f, Random.Range(-15f, 15f)) * dir;

            var b = bullet.GetComponent<Bullet>();
            if (b != null) b.Launch(dir);
        }

        public virtual void TakeDamage(int amount)
        {
            if (State == EnemyState.Dead) return;
            Hp -= amount;
            Debug.Log($"[HIT] {gameObject.name} -{amount} hp={Hp}");
            if (Hp <= 0) Die();
        }

        protected virtual void Die()
        {
            if (State == EnemyState.Dead) return;
            State = EnemyState.Dead;
            Debug.Log($"[DIE] {gameObject.name} pos={transform.position} hp={Hp}");
            PlayerStats.Instance?.AddScore(scoreValue);
            Destroy(gameObject);
        }

        void OnCollisionEnter2D(Collision2D col)
        {
            if (State == EnemyState.Dead) return;
            if (col.gameObject.layer == LayerMask.NameToLayer("Player"))
                PlayerStats.Instance?.TakeDamage(25);
        }

        // Bullet damage is applied by Bullet.cs to prevent multi-hit in one physics step.
    }
}
