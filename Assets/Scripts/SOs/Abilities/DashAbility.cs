using System.Collections;
using UnityEngine;

namespace ggj_2026_masks
{
    // Add to asset editor
    [CreateAssetMenu(fileName = "TestAbility", menuName = "Scriptable Objects/Masks/Dash")]
    public class DashAbility : MaskAbility
    {
        [SerializeField] private float coolDownS = 1.5f;
        [SerializeField] private float dashDuration = 0.25f;
        [SerializeField] private float dashSpeed = 15f;
        public override void Execute(AbilityContext abilityContext)
        {
            abilityContext.PerformCoroutine(DashRoutine(abilityContext));
        }
        
        public override float GetCooldown() => coolDownS;
        
        private IEnumerator DashRoutine(AbilityContext ctx)
        {
            var direction = ctx.RigidBody.linearVelocity.normalized;
            var timer = 0f;

            while (timer < dashDuration)
            {
                ctx.RigidBody.linearVelocity = direction * dashSpeed;
                timer += Time.deltaTime;
                yield return null;
            }
        }
    }
}