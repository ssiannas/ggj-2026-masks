using System.Runtime.CompilerServices;
using UnityEngine;
using ggj_2026_masks.Enemies.Attacking;

namespace ggj_2026_masks
{
    public class PlayerAttackingController : MonoBehaviour
    {
        [Header("Detection")]
        [SerializeField] private float detectionRange = 5f;
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private LayerMask opaqueObstacles;

        private IAttack _attack;
        private Transform _target;

        public bool IsAttacking => _attack?.IsAttacking ?? false;
        public bool CanAttack => _attack?.CanAttack ?? false;
        public Transform Target => _target;

        private void Awake()
        {
            _attack = GetComponent<IAttack>();
        }

        public void Attack()
        {
            if (_attack == null)
            {
                return;
            }

            if (!_attack.CanAttack)
            {
                return;
            }

            _attack.StartAttack(null);
        }
    }
}
