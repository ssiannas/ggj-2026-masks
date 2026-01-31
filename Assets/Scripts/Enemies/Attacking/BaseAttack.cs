using UnityEngine;

namespace ggj_2026_masks.Enemies.Attacking
{
    public abstract class BaseAttack : MonoBehaviour, IAttack
    {
        [Header("Attack Settings")]
        [SerializeField] protected float attackRange = 2f;
        [SerializeField] protected float attackCooldown = 1f;
        [SerializeField] protected float attackDuration = 0.5f;
        [SerializeField] protected int damage = 10;

        [Header("Animation")]
        [SerializeField] protected string attackTrigger = "Attack";

        protected Animator _animator;
        protected Transform _currentTarget;
        protected float _cooldownTimer;
        protected float _attackTimer;
        protected bool _isAttacking;

        public float AttackRange => attackRange;
        public float AttackCooldown => attackCooldown;
        public bool IsAttacking => _isAttacking;
        public bool CanAttack => _cooldownTimer <= 0f && !_isAttacking;
        
        [SerializeField] protected bool maintainsDistance = false;
        public bool MaintainDistance => maintainsDistance;

        protected virtual void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        protected virtual void Update()
        {
            if (_cooldownTimer > 0f)
                _cooldownTimer -= Time.deltaTime;

            if (_isAttacking)
            {
                _attackTimer -= Time.deltaTime;
                if (_attackTimer <= 0f)
                {
                    FinishAttack();
                }
            }
        }

        public virtual void StartAttack(Transform target)
        {
            if (!CanAttack) return;

            _currentTarget = target;
            _isAttacking = true;
            _attackTimer = attackDuration;

            if (_animator)
                _animator.SetTrigger(attackTrigger);

            OnAttackStart();
        }

        public virtual void CancelAttack()
        {
            _isAttacking = false;
            _attackTimer = 0f;
            _currentTarget = null;
        }

        protected virtual void FinishAttack()
        {
            _isAttacking = false;
            _cooldownTimer = attackCooldown;
            OnAttackFinish();
            _currentTarget = null;
        }
        
        protected abstract void OnAttackStart();
        protected abstract void OnAttackFinish();
    }
}