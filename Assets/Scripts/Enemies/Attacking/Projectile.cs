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
            targetLayers = LayerMask.GetMask("Player");
            Destroy(gameObject, lifetime);
        }

        private void OnTriggerEnter(Collider other)
        {
            // Damage player
            if ((targetLayers & (1 << other.gameObject.layer)) == 0)
            {
                return;
            }
            Destroy(gameObject);
        }
    }
}