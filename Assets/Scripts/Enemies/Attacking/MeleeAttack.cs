using UnityEngine;

namespace ggj_2026_masks.Enemies.Attacking
{
    public class MeleeAttack : BaseAttack
    {
        [Header("Melee Settings")] [SerializeField]
        private float hitboxRadius = 1f;

        [SerializeField] private Vector3 hitboxOffset = Vector3.forward;
        [SerializeField] private LayerMask targetLayers;

        [Header("Timing")] [SerializeField] private float damageDelay = 0.2f;

        private bool _hasDamagedThisAttack;
        private float _damageTimer;

        protected override void Update()
        {
            base.Update();

            if (_isAttacking && !_hasDamagedThisAttack)
            {
                _damageTimer -= Time.deltaTime;
                if (_damageTimer <= 0f)
                {
                    DealDamage();
                }
            }
        }

        protected override void OnAttackStart()
        {
            _hasDamagedThisAttack = false;
            _damageTimer = damageDelay;
        }

        protected override void OnAttackFinish()
        {
        }

        private void DealDamage()
        {
            _hasDamagedThisAttack = true;

            var hitboxCenter = transform.position + transform.TransformDirection(hitboxOffset);
            var hits = Physics.OverlapSphere(hitboxCenter, hitboxRadius, targetLayers);

            foreach (var hit in hits)
            {
                // Take damage
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
