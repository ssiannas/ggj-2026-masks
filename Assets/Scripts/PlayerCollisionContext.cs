using UnityEngine;

namespace ggj_2026_masks
{
    public class PlayerCollisionContext : MonoBehaviour
    {
        public PlayerController PlayerController { get; private set; }
        public Rigidbody Rigidbody { get; private set; }

        void Awake()
        {
            PlayerController = GetComponentInParent<PlayerController>();
            Rigidbody = GetComponentInParent<Rigidbody>();
        }
    }
}
