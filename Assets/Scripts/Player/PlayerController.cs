using UnityEngine;

namespace WingNuts.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerStats))]
    public class PlayerController : MonoBehaviour
    {
        public enum State { Normal, Refueling }
        public State CurrentState { get; private set; } = State.Normal;

        [SerializeField] float baseSpeed   = 5f;
        [SerializeField] float turnSpeed   = 90f; // degrees/s

        Rigidbody2D  _rb;
        PlayerStats  _stats;

        // Set by TankerPlane during docking
        Transform _tankerDockPoint;

        void Awake()
        {
            _rb    = GetComponent<Rigidbody2D>();
            _stats = GetComponent<PlayerStats>();
        }

        void Update()
        {
            if (CurrentState == State.Normal)
                HandleInput();
        }

        void FixedUpdate()
        {
            if (CurrentState == State.Normal)
                ApplyMovement();
            else if (CurrentState == State.Refueling)
                ApplyDocking();
        }

        void HandleInput()
        {
            // Rotation
            float turn = 0f;
            if (Input.GetKey(KeyCode.LeftArrow))  turn =  1f;
            if (Input.GetKey(KeyCode.RightArrow)) turn = -1f;
            transform.Rotate(0f, 0f, turn * turnSpeed * Time.deltaTime);

            // Speed + drain multiplier
            if (Input.GetKey(KeyCode.UpArrow))
            {
                _stats.DrainMultiplier = 2f;
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                _stats.DrainMultiplier = 0.9f;
            }
            else
            {
                _stats.DrainMultiplier = 1f;
            }
        }

        void ApplyMovement()
        {
            float speedMult = 1f;
            if (Input.GetKey(KeyCode.UpArrow))        speedMult = 1.5f;
            else if (Input.GetKey(KeyCode.DownArrow)) speedMult = 0.75f;

            _rb.linearVelocity = transform.up * baseSpeed * speedMult;
        }

        void ApplyDocking()
        {
            if (_tankerDockPoint == null) { ExitRefueling(); return; }
            Vector2 target = _tankerDockPoint.position;
            Vector2 newPos = Vector2.MoveTowards(_rb.position, target, baseSpeed * Time.fixedDeltaTime);
            _rb.MovePosition(newPos);

            // Exit refueling if any arrow key is pressed
            if (Input.GetKey(KeyCode.LeftArrow)  || Input.GetKey(KeyCode.RightArrow) ||
                Input.GetKey(KeyCode.UpArrow)    || Input.GetKey(KeyCode.DownArrow))
                ExitRefueling();
        }

        public void EnterRefueling(Transform dockPoint)
        {
            CurrentState    = State.Refueling;
            _tankerDockPoint = dockPoint;
            _stats.DrainMultiplier = 1f;
        }

        public void ExitRefueling()
        {
            CurrentState     = State.Normal;
            _tankerDockPoint = null;
        }
    }
}
