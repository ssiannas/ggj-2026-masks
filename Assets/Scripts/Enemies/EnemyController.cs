using System;
using System.Collections.Generic;
using UnityEngine;
using ggj_2026_masks.Enemies.Attacking;
using UnityEngine.Events;

namespace ggj_2026_masks.Enemies
{
    [RequireComponent(typeof(EnemyMovementController))]
    [RequireComponent(typeof(EnemyPathfindingController))]
    public class EnemyController : MonoBehaviour
    {
        [Header("Detection")]
        [SerializeField] private float detectionRange = 5f;
        [SerializeField] private LayerMask opaqueObstacles;
        [SerializeField] private bool alwaysChase = false;
        [SerializeField] private float playerDetectionPeriodSec = 0.5f;
        [SerializeField] private string[] playerTags = {"Player 1", "Player 2"};

        [Header("Debug")]
        [SerializeField] private bool drawDetection = true;

        private EnemyMovementController _movement;
        private EnemyPathfindingController _pathfinding;
        private Animator _animator;

        private IAttack _attack;
        [SerializeField] private UnityEvent onDeath;
        
        private Transform _target;
        private float _playerDetectionTimer = 0f;
        private bool _hasTarget;
        private Vector3 _lastKnownTargetPosition;
        private readonly List<GameObject> _players = new List<GameObject>();
        private float _stunTimer;
        public float MaxHp { get; } = 100f;
        private bool _tookDamage = false;


        public float Hp { get; private set; }


        public enum EnemyState
        {
            Idle,
            Chasing,
            SearchingLastKnown,
            Attacking,
            Stunned,
        }

        public EnemyState CurrentState { get; private set; } = EnemyState.Idle;
        public Transform Target => _target;
        public bool HasTarget => _hasTarget;
        public float DetectionRange => detectionRange;

        private void Awake()
        {
            _movement = GetComponent<EnemyMovementController>();
            _pathfinding = GetComponent<EnemyPathfindingController>();
            _animator = GetComponentInChildren<Animator>();
            _attack = GetComponent<IAttack>();
            Hp = MaxHp;
        }

        private void Start()
        {
            if (!_pathfinding.Initialize())
            {
                enabled = false;
                return;
            }

            foreach (var t in playerTags)
            {
                _players.Add(GameObject.FindGameObjectWithTag(t));
            }
            TryAcquireTarget();
        }

        private void Update()
        {
            if (CurrentState != EnemyState.Stunned)
            {
                UpdatePerception();
                if (_pathfinding.ShouldUpdatePath())
                {
                    UpdatePath();
                }
            }

            ExecuteCurrentState();

            // Update animation
            if (_animator != null)
            {
                var isMoving = Vector3.Magnitude(GetComponent<Rigidbody>().linearVelocity) > 0.001f;
                _animator.SetBool("isMoving", isMoving);
            }
        }
        

        private bool HasLineOfSight(Vector3 targetPosition)
        {
            var direction = (targetPosition - transform.position).normalized;
            var distance = Vector3.Distance(transform.position, targetPosition);

            return !Physics.Raycast(transform.position, direction, out _, distance, opaqueObstacles);
        }

        private void UpdatePerception()
        {
            if (CurrentState == EnemyState.Stunned) return;
            _playerDetectionTimer += Time.deltaTime;
            if (_playerDetectionTimer < playerDetectionPeriodSec) return;
    
            _playerDetectionTimer = 0f;
    
            var previousTarget = _target;
            TryAcquireTarget();

            if (_target)
            {
                _lastKnownTargetPosition = _target.position;
                _hasTarget = true;

                if (CurrentState != EnemyState.Chasing && CurrentState != EnemyState.Attacking)
                {
                    CurrentState = EnemyState.Chasing;
                    _pathfinding.ForcePathUpdate();
                }
            }
            else
            {
                _hasTarget = false;

                if (previousTarget && CurrentState == EnemyState.Chasing)
                {
                    CurrentState = EnemyState.SearchingLastKnown;
                    _pathfinding.ForcePathUpdate();
                }
            }
        }

        public void ApplyKnockback(Vector3 velocity, float stunDuration = 0.5f)
        {
            CurrentState = EnemyState.Stunned;
            _stunTimer = stunDuration;
            _pathfinding.ClearPath();
            _movement.Stop();
    
            if (TryGetComponent<Rigidbody>(out var rb))
            {
                rb.linearVelocity = velocity;
            }
        }
        
        private void TryAcquireTarget()
        {
            _hasTarget = false;
            _target = null;

            if (_players.Count == 0)
                return;

            var closestDistance = float.MaxValue;
            GameObject closestPlayer = null;

            foreach (var player in _players)
            {
                if (!player) continue;
                
                var distance = Vector3.Distance(transform.position, player.transform.position);

                if (!alwaysChase && !_tookDamage && distance > detectionRange)
                    continue;

                if (!HasLineOfSight(player.transform.position))
                    continue;

                if (!(distance < closestDistance)) continue;
                closestDistance = distance;
                closestPlayer = player;
            }

            if (!closestPlayer) return;
            _target = closestPlayer.transform;
            _hasTarget = true;
        }

        private float GetDistanceToTarget()
        {
            if (_target == null)
                return float.MaxValue;

            return Vector3.Distance(transform.position, _target.position);
        }

        private bool IsInAttackRange()
        {
            if (_attack == null || _target == null) return false;
            return GetDistanceToTarget() <= _attack.AttackRange;
        }

        private void UpdatePath()
        {
            if (_hasTarget && _target)
            {
                _pathfinding.RequestPath(transform.position, _lastKnownTargetPosition);
                return;
            }

            if (CurrentState == EnemyState.SearchingLastKnown)
            {
                float distToLastKnown = Vector3.Distance(transform.position, _lastKnownTargetPosition);
                if (distToLastKnown < _pathfinding.WaypointReachedThreshold)
                {
                    _lastKnownTargetPosition = Vector3.zero;
                    _pathfinding.ClearPath();
                    CurrentState = EnemyState.Idle;
                    return;
                }

                _pathfinding.RequestPath(transform.position, _lastKnownTargetPosition);
            }
        }

        private void ExecuteCurrentState()
        {
            switch (CurrentState)
            {
                case EnemyState.Idle:
                    _movement.Stop();
                    break;

                case EnemyState.Chasing:
                    if (_attack != null && IsInAttackRange() && _attack.CanAttack)
                    {
                        EnterAttackState();
                    }
                    else
                    {
                        FollowPath();
                    }
                    break;

                case EnemyState.SearchingLastKnown:
                    FollowPath();
                    break;

                case EnemyState.Attacking:
                    if (_attack.MaintainDistance)
                    {
                        var desiredDistance = _attack.AttackRange * 0.9f;
                        var distanceToTarget = GetDistanceToTarget();
        
                        if (distanceToTarget < desiredDistance)
                        {
                            var awayDirection = (transform.position - _target.position).normalized;
                            awayDirection.y = 0;
                            _movement.MoveInDirection(awayDirection);
                        }
                        else
                        {
                            _movement.Stop();
                        }
                    }
                    else
                    {
                        _movement.Stop();
                    }
                    if (!_target)
                    {
                         ExitAttackState();
                         break;
                    }
                    var dirToTarget = _target.position - transform.position;
                    _movement.FaceTowards(dirToTarget);

                    if (!_attack.IsAttacking)
                    {
                        if (!_hasTarget || !IsInAttackRange())
                        {
                            ExitAttackState();
                        }
                        else if (_attack.CanAttack)
                        {
                            _attack.StartAttack(_target);
                        }
                    }
                    break;
                case EnemyState.Stunned:
                    _stunTimer -= Time.deltaTime;
                    if (_stunTimer <= 0f)
                    {
                        CurrentState = _hasTarget ? EnemyState.Chasing : EnemyState.Idle;
                        _pathfinding.ForcePathUpdate();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void FollowPath()
        {
            if (_pathfinding.PathComplete)
            {
                _pathfinding.ClearPath();
                return;
            }

            var distance = _pathfinding.GetDistanceToCurrentWaypoint(transform.position);
            var direction = _pathfinding.GetDirectionToCurrentWaypoint(transform.position);

            if (distance > _pathfinding.WaypointReachedThreshold)
            {
                _movement.MoveInDirection(direction);
                _movement.RotateTowards(direction);
            }
            else
            {
                _pathfinding.AdvanceToNextWaypoint();
            }
        }

        public void TakeDamage(float damage, GameObject source)
        {
            Hp -= damage;
            _tookDamage = true;
    
            // Aggro onto the damage source - chase regardless of distance
            if (source is not null)
            {
                _target = source.transform;
                _hasTarget = true;
                _lastKnownTargetPosition = source.transform.position;
        
                if (CurrentState != EnemyState.Stunned)
                {
                    CurrentState = EnemyState.Chasing;
                    _pathfinding.ForcePathUpdate();
                }
            }
    
            if (Hp <= 0)
            {
                Die();
            }
        }


        private void Die()
        {
            onDeath?.Invoke();
            Destroy(gameObject);
        }

        public void SetTarget(Transform newTarget)
        {
            _target = newTarget;
            _hasTarget = newTarget != null;

            if (!_hasTarget) return;
            _lastKnownTargetPosition = _target.position;
            _pathfinding.ForcePathUpdate();
        }

        public void StopMovement()
        {
            _pathfinding.ClearPath();
            _movement.Stop();
            CurrentState = EnemyState.Idle;
        }

        public void ResumeMovement()
        {
            if (_hasTarget)
            {
                _pathfinding.ForcePathUpdate();
            }
        }

        public void EnterAttackState()
        {
            CurrentState = EnemyState.Attacking;
            _movement.Stop();
            _attack?.StartAttack(_target);
        }

        public void ExitAttackState()
        {
            _attack?.CancelAttack();

            if (_hasTarget)
            {
                CurrentState = EnemyState.Chasing;
                _pathfinding.ForcePathUpdate();
            }
            else
            {
                CurrentState = EnemyState.Idle;
            }
        }


        private void OnDrawGizmosSelected()
        {
            if (!drawDetection) return;
            
            Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            Transform target = null;
            var p1Obj = GameObject.FindGameObjectWithTag("Player 1");
            var p2Obj = GameObject.FindGameObjectWithTag("Player 2");
            
            var d1 = float.MaxValue;
            var d2 = float.MaxValue;
            
            if (p1Obj != null)
            {
                d1 = Vector3.Distance(p1Obj.transform.position, transform.position);            
            }

            if (p2Obj != null)
            {
                d2 = Vector3.Distance(p2Obj.transform.position, transform.position);
            }
            var isP1Closer = d1 < d2;

            target = isP1Closer ? p1Obj?.transform : p2Obj?.transform;
            
            if (target is null) return;
            Gizmos.color = HasLineOfSight(target.position) ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
}