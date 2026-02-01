using UnityEngine;
using UnityEngine.UI;

namespace ggj_2026_masks
{
    public class CooldownIcon : MonoBehaviour
    {
        [SerializeField] private Image cooldownOverlay; // The dark/fill image (set to Filled type)
        [SerializeField] private Image iconImage; // The colored icon behind

        private float _cooldownDuration;
        private float _cooldownTimer;
        private bool _isOnCooldown;

        private void Awake()
        {
            // Setup the overlay as a filled image (bottom to top fill)
            cooldownOverlay.type = Image.Type.Filled;
            cooldownOverlay.fillMethod = Image.FillMethod.Vertical;
            cooldownOverlay.fillOrigin = (int)Image.OriginVertical.Bottom;
            cooldownOverlay.fillAmount = 0f;
        }

        private void Update()
        {
            if (!_isOnCooldown) return;

            _cooldownTimer -= Time.deltaTime;

            // Fill goes from 1 (full overlay) down to 0 (ready)
            cooldownOverlay.fillAmount = Mathf.Clamp01(_cooldownTimer / _cooldownDuration);

            if (_cooldownTimer <= 0f)
            {
                _isOnCooldown = false;
                cooldownOverlay.fillAmount = 0f;
            }
        }

        public void TriggerCooldown(float duration)
        {
            _cooldownDuration = duration;
            _cooldownTimer = duration;
            _isOnCooldown = true;
            cooldownOverlay.fillAmount = 1f;
        }

        public bool IsReady => !_isOnCooldown;
    }
}