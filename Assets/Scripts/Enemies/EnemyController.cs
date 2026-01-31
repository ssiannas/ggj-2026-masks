using System;
using System.Collections.Generic;
using UnityEngine;

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
        [SerializeField] private string[] playerTags = {"Player 1", "Player 2"};

        [Header("Debug")]
        [SerializeField] private bool drawDetection = true;

        private EnemyMovementController _movement;
        private EnemyPathfindingController _pathfinding;

        private Transform _target;
        private bool _hasTarget;
        private Vector3 _lastKnownTargetPosition;
        private readonly List<GameObject> _players = new List<GameObject>();

        public enum EnemyState
        {
            Idle,
            Chasing,
            SearchingLastKnown,
            Attacking,
        }

        public EnemyState CurrentState { get; private set; } = EnemyState.Idle;
        public Transform Target => _target;
        public bool HasTarget => _hasTarget;
        public float DetectionRange => detectionRange;

        private void Awake()
        {
            _movement = GetComponent<EnemyMovementController>();
            _pathfinding = GetComponent<EnemyPathfindingController>();
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
            UpdatePerception();
            if (_pathfinding.ShouldUpdatePath())
            {
                UpdatePath();
            }

            ExecuteCurrentState();
        }


        private bool HasLineOfSight(Vector3 targetPosition)
        {
            var direction = (targetPosition - transform.position).normalized;
            var distance = Vector3.Distance(transform.position, targetPosition);

            return !Physics.Raycast(transform.position, direction, out _, distance, opaqueObstacles);
        }

        private void UpdatePerception()
        {
            if (_target == null)
            {
                TryAcquireTarget();
                return;
            }

            bool canSeeTarget = HasLineOfSight(_target.position);
            bool inRange = alwaysChase || Vector3.Distance(transform.position, _target.position) <= detectionRange;

            if (canSeeTarget && inRange)
            {
                _lastKnownTargetPosition = _target.position;
                _hasTarget = true;

                if (CurrentState == EnemyState.SearchingLastKnown || CurrentState == EnemyState.Idle)
                {
                    CurrentState = EnemyState.Chasing;
                    _pathfinding.ForcePathUpdate();
                }
            }
            else
            {
                if (CurrentState == EnemyState.Chasing)
                {
                    CurrentState = EnemyState.SearchingLastKnown;
                    _pathfinding.ForcePathUpdate();
                }
                _hasTarget = false;
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

                if (!alwaysChase && distance > detectionRange)
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

        public float GetDistanceToTarget()
        {
            if (_target == null)
                return float.MaxValue;

            return Vector3.Distance(transform.position, _target.position);
        }


        private void UpdatePath()
        {
            if (_hasTarget && _target != null)
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
                case EnemyState.SearchingLastKnown:
                    FollowPath();
                    break;

                case EnemyState.Attacking:
                    _movement.Stop();
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
        }

        public void ExitAttackState()
        {
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