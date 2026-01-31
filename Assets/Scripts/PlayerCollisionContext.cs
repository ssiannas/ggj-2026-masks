using UnityEngine;

namespace ggj_2026_masks
{
    public class PlayerCollisionContext : MonoBehaviour
    {
        public PlayerController PlayerController;

        void Awake()
        {
            PlayerController = GetComponentInParent<PlayerController>();
        }
    }
}
