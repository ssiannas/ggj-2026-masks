using UnityEngine;

namespace ggj_2026_masks.Enemies.Attacking
{
    public interface IAttack
    {
        float AttackRange { get; }
        float AttackCooldown { get; }
        bool IsAttacking { get; }
        bool CanAttack { get; }
        
        void StartAttack(Transform target);
        void CancelAttack();
    }
}

