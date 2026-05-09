using System;
using UnityEngine;

namespace WingNuts.Player
{
    public class PlayerStats : MonoBehaviour
    {
        public static PlayerStats Instance { get; private set; }

        [Header("Starting Values")]
        [SerializeField] int startShields = 100;
        [SerializeField] int startFuel    = 100;
        [SerializeField] int startLives   = 3;

        [Header("Drain")]
        [SerializeField] float fuelDrainRate = 2f; // fuel points/s at multiplier 1

        public int  Shields { get; private set; }
        public int  Fuel    { get; private set; }
        public int  Lives   { get; private set; }
        public int  Score   { get; private set; }

        // Set each frame by PlayerController before Update() reads it.
        public float DrainMultiplier { get; set; } = 1f;

        public event Action<int> OnShieldsChanged;
        public event Action<int> OnFuelChanged;
        public event Action<int> OnLivesChanged;
        public event Action<int> OnScoreChanged;
        public event Action      OnDeath;
        public event Action      OnGameOver;

        float _fuelAccum;
        bool  _dead;
        bool  _invincible;
        float _invincibleTimer;

        const float InvincibleDuration = 5f;
        const float FlashHz = 10f;

        SpriteRenderer _sprite;

        void Awake()
        {
            Instance = this;
            _sprite  = GetComponent<SpriteRenderer>();
            Shields  = startShields;
            Fuel     = startFuel;
            Lives    = startLives;
            Score    = 0;
        }

        void Update()
        {
            if (_dead) return;

            HandleInvincibility();
            DrainFuel();
        }

        void DrainFuel()
        {
            _fuelAccum += fuelDrainRate * DrainMultiplier * Time.deltaTime;
            int drain   = Mathf.FloorToInt(_fuelAccum);
            if (drain <= 0) return;
            _fuelAccum -= drain;
            SetFuel(Fuel - drain);
        }

        void HandleInvincibility()
        {
            if (!_invincible) return;
            _invincibleTimer -= Time.deltaTime;
            if (_invincibleTimer <= 0f)
            {
                _invincible = false;
                if (_sprite != null) _sprite.enabled = true;
                SetLayerCollisions(true);
                return;
            }
            // Flash sprite on/off at FlashHz
            if (_sprite != null)
                _sprite.enabled = Mathf.FloorToInt(_invincibleTimer * FlashHz) % 2 == 0;
        }

        public void TakeDamage(int amount)
        {
            if (_invincible || _dead) return;
            SetShields(Shields - amount);
            if (Shields <= 0) Die();
        }

        public void AddScore(int amount)
        {
            Score += amount;
            OnScoreChanged?.Invoke(Score);
        }

        public void SetFuel(int value)
        {
            Fuel = Mathf.Clamp(value, 0, 100);
            OnFuelChanged?.Invoke(Fuel);
            if (Fuel <= 0 && !_dead) Die();
        }

        void SetShields(int value)
        {
            Shields = Mathf.Clamp(value, 0, 100);
            OnShieldsChanged?.Invoke(Shields);
        }

        void Die()
        {
            if (_dead) return;
            _dead = true;
            OnDeath?.Invoke();

            Lives--;
            OnLivesChanged?.Invoke(Lives);

            if (Lives <= 0)
            {
                OnGameOver?.Invoke();
                return;
            }

            // Respawn handled by GameManager; reset state here.
            Invoke(nameof(Respawn), 1.5f);
        }

        void Respawn()
        {
            _dead    = false;
            Shields  = startShields;
            Fuel     = startFuel;
            OnShieldsChanged?.Invoke(Shields);
            OnFuelChanged?.Invoke(Fuel);

            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;

            // Grant invincibility
            _invincible      = true;
            _invincibleTimer = InvincibleDuration;
            SetLayerCollisions(false);
        }

        void SetLayerCollisions(bool enabled)
        {
            int playerLayer      = LayerMask.NameToLayer("Player");
            int enemyBulletLayer = LayerMask.NameToLayer("EnemyBullet");
            int enemyLayer       = LayerMask.NameToLayer("Enemy");
            Physics2D.IgnoreLayerCollision(playerLayer, enemyBulletLayer, !enabled);
            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer,       !enabled);
        }
    }
}
