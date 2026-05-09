// Script Execution Order: CameraFollow must run BEFORE BackgroundScroller.
// Set this in Project Settings → Script Execution Order (e.g. -100).
using UnityEngine;

namespace WingNuts.Core
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] float smoothTime = 0.1f;

        Vector3 _velocity;

        void LateUpdate()
        {
            if (target == null) return;
            Vector3 desired = new Vector3(target.position.x, target.position.y, -10f);
            transform.position = Vector3.SmoothDamp(
                transform.position, desired, ref _velocity, smoothTime);
        }
    }
}
