using UnityEngine;
using UnityEngine.UI;

namespace ggj_2026_masks
{
    public class HealthBarController : MonoBehaviour
    {
        [SerializeField] private Image healthBarImage;
        [SerializeField] private float animationSpeed = 10.0f;
        private float _animatedHealthPct;

        private float _currentHealthPct;

        private float _originalWidth;

        private void Awake()
        {
            _originalWidth = healthBarImage.rectTransform.sizeDelta.x;
            _currentHealthPct = 1f;
            _animatedHealthPct = 1f;
        }

        private void Update()
        {
            if (Mathf.Abs(_animatedHealthPct - _currentHealthPct) > 0.001f)
            {
                _animatedHealthPct = Mathf.Lerp(_animatedHealthPct, _currentHealthPct, animationSpeed * Time.deltaTime);
                ApplyHealthBar(_animatedHealthPct);
            }
        }

        public void SetHealthPercentage(float pct)
        {
            _currentHealthPct = Mathf.Clamp01(pct);
        }

        private void ApplyHealthBar(float pct)
        {
            var sizeDelta = healthBarImage.rectTransform.sizeDelta;
            sizeDelta.x = _originalWidth * pct;
            healthBarImage.rectTransform.sizeDelta = sizeDelta;
        }
    }
}