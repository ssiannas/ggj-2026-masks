using ggj_2026_masks;
using UnityEngine;

namespace ggj_2026_masks
{
    public class PlayerUIController : MonoBehaviour
    {
        [SerializeField] private HealthBarController healthBarController;

        [SerializeField] private CooldownIcon dashCD;
        [SerializeField] private CooldownIcon abilityCD;

        public void SetHealthPercentage(float pct)
        {
            healthBarController.SetHealthPercentage(pct);
        }

        public void StartDashCD(float duration)
        {
            dashCD.TriggerCooldown(duration);
        }

        public void StartAbilityCD(float duration)
        {
            abilityCD.TriggerCooldown(duration);
        }
    }
}