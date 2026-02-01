using System.Collections;
using ggj_2026_masks.Enemies;
using UnityEngine;

namespace ggj_2026_masks
{
    [CreateAssetMenu(fileName = "TestAbility", menuName = "Scriptable Objects/Masks/Rope")]
    public class RopeAbility : MaskAbility
    {
        [SerializeField] private float coolDownS = 1.5f;
        [SerializeField] private float throwDurationSec = 0.060f;
        [SerializeField] private float throwRange = 10.0f;
        [SerializeField] private float pullDurationSec = 0.020f;
        [SerializeField] private LayerMask obstacleLayers;
        [SerializeField] private LayerMask targetLayers;
        [SerializeField] private LineRenderer ropeLinePrefab;

        public override void Execute(AbilityContext abilityContext)
        {
            abilityContext.PerformCoroutine(ExecuteRope(abilityContext));
        }

        private IEnumerator ExecuteRope(AbilityContext context)
        {
            var origin = context.RopeOrigin!.position;
            var direction = context.Transform.forward;
            direction.y = 0;
            direction = direction.normalized;

            LineRenderer lineRenderer = null;
            if (ropeLinePrefab != null) lineRenderer = Instantiate(ropeLinePrefab);

            // Throw stage
            yield return ThrowRope(context, origin, direction, lineRenderer);

            // Check what we hit
            var (hitType, hitPoint, hitTransform) = CastRope(origin, direction);

            if (hitType == RopeHitType.None)
            {
                DestroyRope(lineRenderer);
                yield break;
            }

            // Pull stage
            yield return Pull(context, hitType, hitPoint, hitTransform, lineRenderer);

            DestroyRope(lineRenderer);
        }

        private void DestroyRope(LineRenderer lineRenderer)
        {
            if (lineRenderer != null) Destroy(lineRenderer.gameObject);
        }

        private IEnumerator ThrowRope(AbilityContext context, Vector3 origin, Vector3 direction,
            LineRenderer lineRenderer)
        {
            if (lineRenderer != null) lineRenderer.positionCount = 2;

            var elapsed = 0f;
            while (elapsed < throwDurationSec)
            {
                elapsed += Time.deltaTime;
                var t = elapsed / throwDurationSec;
                var currentEnd = origin + direction * (throwRange * t);
                Debug.Log($"Start Rope: {origin}, direction: {direction}, end: {currentEnd}");

                if (lineRenderer != null)
                {
                    lineRenderer.SetPosition(0, origin);
                    lineRenderer.SetPosition(1, currentEnd);
                }

                yield return null;
            }
        }

        private (RopeHitType, Vector3, Transform) CastRope(Vector3 origin, Vector3 direction)
        {
            var combinedLayers = LayerMask.GetMask("Enemies", "Obastacles", "Player");

            if (!Physics.SphereCast(origin, 0.3f, direction, out var hit, throwRange, combinedLayers))
            {
                Debug.Log("Rope hit tsoufio bruv.");
                return (RopeHitType.None, Vector3.zero, null);
            }

            // Check what we hit
            var hitObject = hit.collider.gameObject;

            // Check for player
            if (LayerMask.LayerToName(hitObject.layer) == "Player")
            {
                Debug.Log($"Found player: {hitObject.name}");
                return (RopeHitType.Player, hit.point, hit.transform);
            }

            // Check for enemy
            var enemyController = hitObject.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                Debug.Log($"Found enemy: {hitObject.name}");
                return (RopeHitType.Enemy, hit.point, hit.transform);
            }

            // Check for obstacle (by layer)
            if (((1 << hitObject.layer) & obstacleLayers) != 0)
            {
                Debug.Log($"Found Obstacle: {hitObject.name}");
                return (RopeHitType.Obstacle, hit.point, hit.transform);
            }

            Debug.Log("LEGEND XWRIS TO D, LEGEN PRO MAX :)");
            return (RopeHitType.None, Vector3.zero, null);
        }

        private IEnumerator Pull(AbilityContext context, RopeHitType hitType, Vector3 hitPoint, Transform hitTransform,
            LineRenderer lineRenderer)
        {
            var elapsed = 0f;
            var startPosition = context.Transform.position;
            var targetStartPosition = hitTransform != null ? hitTransform.position : hitPoint;

            switch (hitType)
            {
                case RopeHitType.Obstacle:
                    hitPoint.y = startPosition.y;
                    // Pull player towards obstacle
                    while (elapsed < pullDurationSec)
                    {
                        elapsed += Time.deltaTime;
                        var t = elapsed / pullDurationSec;
                        var newPos = Vector3.Lerp(startPosition, hitPoint, t);
                        context.RigidBody.MovePosition(newPos);

                        if (lineRenderer != null)
                        {
                            lineRenderer.SetPosition(0, newPos);
                            lineRenderer.SetPosition(1, hitPoint);
                        }

                        yield return null;
                    }

                    break;

                case RopeHitType.Enemy:
                case RopeHitType.Player:
                    startPosition.y = targetStartPosition.y;
                    // Pull target towards player
                    var targetRb = hitTransform.GetComponent<Rigidbody>();
                    if (targetRb != null)
                        while (elapsed < pullDurationSec)
                        {
                            elapsed += Time.deltaTime;
                            var t = elapsed / pullDurationSec;
                            var newTargetPos = Vector3.Lerp(targetStartPosition, startPosition, t);
                            targetRb.MovePosition(newTargetPos);

                            if (lineRenderer != null)
                            {
                                yield return newTargetPos.y = context.RopeOrigin.position.y;
                                lineRenderer.SetPosition(0, context.RopeOrigin.position);
                                lineRenderer.SetPosition(1, newTargetPos);
                            }

                            yield return null;
                        }

                    break;
            }
        }

        public override float GetCooldown()
        {
            return coolDownS;
        }

        private enum RopeHitType
        {
            None,
            Enemy,
            Player,
            Obstacle
        }
    }
}