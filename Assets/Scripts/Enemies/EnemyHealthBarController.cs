using UnityEngine;
using UnityEngine.UI;

namespace ggj_2026_masks.Enemies
{
    public class EnemyHealthBarController : MonoBehaviour
    {

            [SerializeField] private HealthBarController healthBar;
            [SerializeField] private bool faceCamera = true;
            [SerializeField] private Vector3 localOffset = new Vector3(0, 2f, 0);

            
            private Camera _camera;
            private EnemyController _ec;
            private Transform _followTargets;

            private void Awake()
            {
                _ec = GetComponentInParent<EnemyController>();
                _camera = Camera.main;
                _followTargets = transform.parent;
                transform.SetParent(null);
                if (_camera == null) return;
                transform.rotation = _camera.transform.rotation; 
            }

            private void LateUpdate()
            {
                if (_followTargets is null || _ec is null || _ec.Hp <= 0) 
                {
                    Destroy(healthBar.gameObject);
                    Destroy(gameObject);
                    return;
                } 
                if (_ec is not null)
                {
                    healthBar.SetHealthPercentage(_ec.Hp / _ec.MaxHp);
                }
                transform.position = _followTargets.position + localOffset;
            }
    }
}
