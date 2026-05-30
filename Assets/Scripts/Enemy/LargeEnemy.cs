using UnityEngine;

namespace WingNuts.Enemy
{
    // 3 HP, fires every 4 s. 40% chance to break formation and hunt the player.
    // Set maxHp=3, fireInterval=4, scoreValue=300 in the prefab Inspector.
    public class LargeEnemy : EnemyBase
    {
        [SerializeField] float huntSpeed    = 3f;
        [SerializeField] float huntTurnRate = 60f;

        protected override void EnterActive()
        {
            base.EnterActive();
            if (Random.value < 0.4f)
                State = EnemyState.Hunt;
        }

        protected override void UpdateHunt()
        {
            if (PlayerTransform == null) return;

            Vector2 toPlayer = ((Vector2)PlayerTransform.position - (Vector2)transform.position).normalized;

            // Atan2(-x, y) gives the z-rotation needed so that transform.up faces toPlayer.
            float desired  = Mathf.Atan2(-toPlayer.x, toPlayer.y) * Mathf.Rad2Deg;
            float current  = transform.eulerAngles.z;
            float newAngle = Mathf.MoveTowardsAngle(current, desired, huntTurnRate * Time.deltaTime);

            transform.rotation  = Quaternion.Euler(0f, 0f, newAngle);
            transform.position += transform.up * huntSpeed * Time.deltaTime;

            TickFire();
        }
    }
}
