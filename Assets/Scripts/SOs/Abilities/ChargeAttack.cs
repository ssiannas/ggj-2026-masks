using System.Collections;
using System.Collections.Generic;
using ggj_2026_masks.Enemies;
using Unity.VisualScripting;
using UnityEngine;

namespace ggj_2026_masks
{
    [CreateAssetMenu(fileName = "ChargeAttack", menuName = "Scriptable Objects/Masks/Bull")]
    public class ChargeAttack : MaskAbility
    {
        [SerializeField] private float chargeSpeed = 25f;
        [SerializeField] private float chargeDuration = 0.3f;
        [SerializeField] private float knockbackForce = 15f;
        [SerializeField] private float hitRadius = 1.5f;
        [SerializeField] private int damage = 10;
        [SerializeField] private LayerMask enemyLayers;
        [SerializeField] private LayerMask playerLayers;
        [SerializeField] private float coolDownDuration = 4f;

        public override void Execute(AbilityContext ctx)
        {
            ctx.StartCoroutine(ChargeRoutine(ctx));
        }

        public override float GetCooldown()
        {
            return coolDownDuration;
        }

        private IEnumerator ChargeRoutine(AbilityContext ctx)
        {
            var direction = ctx.RigidBody.linearVelocity.normalized;
            if (direction == Vector3.zero)
                direction = ctx.Transform.forward;

            var timer = 0f;
            var hitEnemies = new HashSet<Collider>();
            var hitPlayers = new HashSet<Collider>();
            
            bool foundObstacle  = false;

            while (timer < chargeDuration)
            {
                var hits = Physics.OverlapSphere(ctx.Transform.position, hitRadius, enemyLayers);
                foreach (var hit in hits)
                {
                    if (!hitEnemies.Add(hit)) continue;
                    HandleEnemyHit(hit, ctx);
                }

                var playersHit = Physics.OverlapSphere(ctx.Transform.position, hitRadius, playerLayers);
                foreach (var player in playersHit)
                {
                    if (player.transform.root == ctx.Transform.root) continue;
                    if (!hitPlayers.Add(player)) continue;
                    HandlePlayerHit(player, ctx);
                }

                ctx.RigidBody.linearVelocity = direction * chargeSpeed;

                timer += Time.deltaTime;
                yield return null;
            }
        }

        private void HandlePlayerHit(Collider hit, AbilityContext ctx)
        {
            Debug.Log(hit.gameObject.name);
            if (!hit.TryGetComponent<PlayerCollisionContext>(out var pctx)) return;
            pctx.PlayerController.Stun(0.5f);
            var knockbackDir = (hit.transform.position - ctx.Transform.position).normalized;
            knockbackDir.y = 0;
            pctx.Rigidbody.linearVelocity = knockbackDir * knockbackForce;
        }

        private void HandleEnemyHit(Collider hit, AbilityContext ctx)
        {
            if (hit.TryGetComponent<Rigidbody>(out var enemyRb))
            {
                var knockbackDir = (hit.transform.position - ctx.Transform.position).normalized;
                knockbackDir.y = 0;
                if (hit.TryGetComponent<EnemyController>(out var enemy))
                    enemy.ApplyKnockback(knockbackDir * knockbackForce, 1.0f);
            }

            if (hit.TryGetComponent<EnemyController>(out var enemyController)) enemyController.TakeDamage(damage);
        }
    }
}