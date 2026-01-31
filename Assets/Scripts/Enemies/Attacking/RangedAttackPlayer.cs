using Unity.VisualScripting;
using UnityEngine;

namespace ggj_2026_masks.Enemies.Attacking
{
    public class RangedAttackPlayer : BaseAttack
    {
        [Header("Ranged Settings")] [SerializeField]
        private GameObject projectilePrefab;

        [SerializeField] private Transform firePoint;
        [SerializeField] private float projectileSpeed = 15f;

        [Header("Timing")] [SerializeField] private float fireDelay = 0.3f;

        private bool _hasFiredThisAttack;
        private float _fireTimer;

        protected override void Awake()
        {
            base.Awake();
            maintainsDistance = true;

            if (!firePoint)
                firePoint = transform;
        }

        protected override void Update()
        {
            base.Update();
            
            if (_isAttacking && !_hasFiredThisAttack)
            {
                _fireTimer -= Time.deltaTime;
                if (_fireTimer <= 0f)
                {
                    Debug.Log("Pew");
                    FireProjectile();
                }
            }
        }

        protected override void OnAttackStart()
        {
            _hasFiredThisAttack = false;
            _fireTimer = fireDelay;
        }

        protected override void OnAttackFinish()
        {

        }

        private void FireProjectile()
        {
            _hasFiredThisAttack = true;

            if (!projectilePrefab) return;

            var direction = (firePoint.position - transform.position);
            direction.y = 0;
            direction = direction.normalized;
            var projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(direction));

            if (projectile.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.linearVelocity = direction * projectileSpeed;
            }

            if (projectile.TryGetComponent<Projectile>(out var proj))
            {
                proj.Initialize(damage);
            }
        }


        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            if (firePoint)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(firePoint.position, 0.1f);
            }
        }
    }
}