using UnityEngine;

namespace ggj_2026_masks.Enemies
{
    public class EnemyMovementController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotationSpeed = 10f;

        private Rigidbody _rb;
        private bool _isMoving;

        public float MoveSpeed => moveSpeed;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        public void MoveInDirection(Vector3 direction)
        {
            if (direction == Vector3.zero)
                return;

            direction.Normalize();
            _isMoving = true;

            if (_rb)
            {
                Vector3 velocity = direction * moveSpeed;
                velocity.y = _rb.linearVelocity.y;
                _rb.linearVelocity = velocity;
            }
            else
            {
                transform.position += direction * (moveSpeed * Time.deltaTime);
            }
        }

        public void RotateTowards(Vector3 direction)
        {
            if (direction == Vector3.zero)
                return;

            direction.y = 0f;
            var targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        public void FaceTowards(Vector3 direction)
        {
            var directionNorm = direction.normalized;
            directionNorm.y = 0f;
            RotateTowards(directionNorm);
        }

        public void Stop()
        {
            _isMoving = false;

            if (_rb)
            {
                _rb.linearVelocity = new Vector3(0f, _rb.linearVelocity.y, 0f);
            }
        }

        public bool IsMoving => _isMoving;
    }
}
