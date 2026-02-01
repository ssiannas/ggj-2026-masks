using UnityEngine;

namespace ggj_2026_masks.Enemies.Attacking
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float lifetime = 5f;
        [SerializeField] private LayerMask targetLayers;

        private int _damage;

        public void Initialize(int damage)
        {
            _damage = damage;
            Destroy(gameObject, lifetime);
        }

        private void OnTriggerEnter(Collider other)
        {
            if ((targetLayers & (1 << other.gameObject.layer)) == 0)
            {
                // Not target, do not apply damage
                return;
            }
            // Target layer hit 
            if (other.gameObject.TryGetComponent<PlayerCollisionContext>(out var pctx))
            { 
               pctx.PlayerController.ApplyDamage(_damage); 
            }
            else if (other.gameObject.TryGetComponent<EnemyController>(out var ec))
            {
                ec.TakeDamage(_damage, gameObject);
            }
            Destroy(gameObject);
        }
    }
}