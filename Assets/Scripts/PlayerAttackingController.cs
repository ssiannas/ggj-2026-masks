using UnityEngine;
using ggj_2026_masks.Enemies.Attacking;

namespace ggj_2026_masks
{
    public class PlayerAttackingController : MonoBehaviour
    {
        [Header("Detection")]
        [SerializeField] private float detectionRange = 5f;
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private LayerMask opaqueObstacles;

        private IAttack _attack;
        private Transform _target;

        public bool IsAttacking => _attack?.IsAttacking ?? false;
        public bool CanAttack => _attack?.CanAttack ?? false;
        public Transform Target => _target;

        private void Awake()
        {
            _attack = GetComponent<IAttack>();
        }

        public void Attack()
        {
            if (_attack == null)
            {
                return;
            }

            if (!_attack.CanAttack)
            {
                return;
            }

            TryAcquireTarget();
            _attack.StartAttack(_target);
        }

        private void TryAcquireTarget()
        {
            _target = null;

            var colliders = Physics.OverlapSphere(transform.position, detectionRange, enemyLayer);

            if (colliders.Length == 0) return;

            var closestDistance = float.MaxValue;
            Transform closestEnemy = null;

            foreach (var col in colliders)
            {
                var distance = Vector3.Distance(transform.position, col.transform.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = col.transform;
                }
            }

            _target = closestEnemy;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0f, 1f, 1f, 0.2f);
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            if (_target != null)
            {
                Gizmos.DrawLine(transform.position, _target.position);
            }
        }
    }
}
