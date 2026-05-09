using UnityEngine;
using WingNuts.Core;

namespace WingNuts.Player
{
    public class PlayerShooter : MonoBehaviour
    {
        [SerializeField] Transform noseCollider;

        PlayerController _controller;

        void Awake()
        {
            _controller = GetComponent<PlayerController>();
        }

        void Update()
        {
            if (_controller.CurrentState == PlayerController.State.Refueling) return;
            if (!Input.GetKeyDown(KeyCode.Space)) return;

            GameObject bullet = BulletPool.Instance.GetBullet(BulletType.Player);
            if (bullet == null) return;

            Vector3 spawnPos = noseCollider != null
                ? noseCollider.position
                : transform.position;

            bullet.transform.position = spawnPos;
            bullet.transform.rotation = transform.rotation;

            var b = bullet.GetComponent<Bullet>();
            if (b != null) b.Launch((Vector2)transform.up);
        }
    }
}
