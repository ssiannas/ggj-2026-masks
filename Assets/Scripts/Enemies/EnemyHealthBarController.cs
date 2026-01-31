using UnityEngine;
using UnityEngine.UI;

namespace ggj_2026_masks.Enemies
{
    public class EnemyHealthBarController : MonoBehaviour
    {

            [SerializeField] private HealthBarController healthBar;
            [SerializeField] private bool faceCamera = true;

            private Camera _camera;
            private EnemyController _ec;

            private void Awake()
            {
                _ec = GetComponentInParent<EnemyController>();
                _camera = Camera.main;
            }

            private void LateUpdate()
            {
                if (_ec is not null)
                {
                    healthBar.SetHealthPercentage(_ec.Hp / _ec.MaxHp);
                }

                if (faceCamera && _camera is not null)
                {
                    transform.forward = _camera.transform.forward;
                }
            }
    }
}
