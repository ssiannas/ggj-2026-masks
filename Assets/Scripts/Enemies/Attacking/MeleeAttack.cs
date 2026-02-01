using System;
using UnityEngine;

namespace ggj_2026_masks.Enemies.Attacking
{
    public class MeleeAttack : BaseAttack
    {
        [Header("Melee Settings")] [SerializeField]
        private float hitboxRadius = 1f;

        [SerializeField] private Vector3 hitboxOffset = Vector3.forward;
        [SerializeField] private LayerMask targetLayers;

        [Header("Timing")]
        [SerializeField, Tooltip("If this is more than the attack cooldown, the damage will never be applied")]
        private float damageDelay = 0.2f;

        [Header("Indicator")]
        [SerializeField] private GameObject attackIndicator;
        
        private Collider[] hitColliders = new Collider[10];
        
        private bool _hasDamagedThisAttack;
        private float _damageTimer;

        protected override void Update()
        {
            base.Update();

            if (!_isAttacking || _hasDamagedThisAttack)
            {
                return;
            }
            _damageTimer -= Time.deltaTime;
            if (_damageTimer <= 0f)
            {
                DealDamage();
            }
        }

        protected override void OnAttackStart()
        {
            _hasDamagedThisAttack = false;
            _damageTimer = damageDelay;
            if (attackIndicator != null) attackIndicator.SetActive(true);
        }

        protected override void OnAttackFinish()
        {
            if (attackIndicator != null) attackIndicator.SetActive(false);
        }

        private void DealDamage()
        {
            _hasDamagedThisAttack = true;

            var hitboxCenter = transform.position + transform.TransformDirection(hitboxOffset);
            var size = Physics.OverlapSphereNonAlloc(hitboxCenter, hitboxRadius, hitColliders, targetLayers);

            for (var i = 0; i < size; i++)
            {
                var hitCollider = hitColliders[i];
                
                // deal damage based on collider
                // Apply damage to player
                var go = hitCollider.gameObject;
                if (go.TryGetComponent<PlayerCollisionContext>(out var playerCollisionContext))
                {
                    // Do not damage players on the same layer as the attacker
                    if (gameObject.layer == LayerMask.NameToLayer("Player")) continue;
                    playerCollisionContext.PlayerController.ApplyDamage(damage);
                }
                
                // Apply damage to enemy
                if (go.TryGetComponent<EnemyController>(out var enemyController))
                {
                    enemyController.TakeDamage(damage, gameObject);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            var hitboxCenter = transform.position + transform.TransformDirection(hitboxOffset);
            Gizmos.DrawWireSphere(hitboxCenter, hitboxRadius);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
