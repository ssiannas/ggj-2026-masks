using System;
using UnityEngine;
using System.Collections.Generic;
using ggj_2026_masks.Pathfinding;

namespace ggj_2026_masks
{
    public class EnemyController : MonoBehaviour
    {
        [Header("Movement")] [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private float waypointReachedThreshold = 0.3f;

        [Header("Pathfinding")] [SerializeField]
        private float pathUpdateInterval = 0.5f;

        [SerializeField] private float targetLostTimeout = 3f;
        [SerializeField] private string playerTag = "Player";

        [Header("Detection")] [SerializeField] private float detectionRange = 5f;

        [SerializeField] private LayerMask opaqueObstacles;
        [SerializeField] private bool alwaysChase = false;

        [Header("Debug")] [SerializeField] private bool drawPath = true;
        [SerializeField] private Color pathColor = Color.green;

        private Transform _target;
        private List<Vector3> _currentPath;
        private int _currentWaypointIndex;
        private float _pathUpdateTimer;
        private float _targetLostTimer;
        private Vector3 _lastKnownTargetPosition;
        private GameObject[] _players;

        private Pathfinder _pathfinder;
        private Rigidbody _rb;
        private bool _hasTarget;

        public enum EnemyState
        {
            Idle,
            Chasing,
            SearchingLastKnown,
            Attacking,
        }

        public EnemyState CurrentState { get; private set; } = EnemyState.Idle;

        private void Start()
        {
            _pathfinder = Pathfinder.Instance;
            _rb = GetComponent<Rigidbody>();

            _players = GameObject.FindGameObjectsWithTag(playerTag);

            if (_pathfinder == null)
            {
                Debug.LogError($"EnemyController on {gameObject.name}: No Pathfinder instance found!");
                enabled = false;
                return;
            }

            FindTarget();
        }

        private void Update()
        {
            _pathUpdateTimer += Time.deltaTime;

            if (_pathUpdateTimer >= pathUpdateInterval)
            {
                _pathUpdateTimer = 0f;
                UpdatePath();
            }

            FollowPath();
        }

        private bool HasLineOfSight(Vector3 targetPosition)
        {
            var direction = (targetPosition - transform.position).normalized;
            var distance = Vector3.Distance(transform.position, targetPosition);

            return !Physics.Raycast(transform.position, direction, out var hit, distance, opaqueObstacles);
        }

        private void FindTarget()
        {
            _hasTarget = false;
            _target = null;

            if (_players.Length == 0)
                return;

            float closestDistance = float.MaxValue;
            GameObject closestPlayer = null;

            foreach (var player in _players)
            {
                float distance = Vector3.Distance(transform.position, player.transform.position);

                // Must be in range
                if (!alwaysChase && distance > detectionRange)
                    continue;

                // Must have line of sight
                if (!HasLineOfSight(player.transform.position))
                    continue;

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPlayer = player;
                }
            }

            if (closestPlayer != null)
            {
                _target = closestPlayer.transform;
                _hasTarget = true;
            }
        }

        private void HandleNoLos()
        {
            if (_lastKnownTargetPosition == Vector3.zero)
            {
                _currentPath = null;
                CurrentState = EnemyState.Idle;
                return;
            }

            // Reached last known?
            float distToLastKnown = Vector3.Distance(transform.position, _lastKnownTargetPosition);
            if (distToLastKnown < waypointReachedThreshold)
            {
                _lastKnownTargetPosition = Vector3.zero;
                _currentPath = null;
                CurrentState = EnemyState.Idle;
                return;
            }

            // Path to last known
            CurrentState = EnemyState.SearchingLastKnown;
            var pathToLastKnown = _pathfinder.FindPath(transform.position, _lastKnownTargetPosition);
            if (pathToLastKnown is { Count: > 0 })
            {
                _currentPath = pathToLastKnown;
                _currentWaypointIndex = 0;
            }
        }

        private void UpdatePath()
        {
            // Try to find/verify target with LOS
            FindTarget();

            // We can see a target - chase
            if (_hasTarget && _target != null)
            {
                _lastKnownTargetPosition = _target.position;
        
                var newPath = _pathfinder.FindPath(transform.position, _lastKnownTargetPosition);
                if (newPath is { Count: > 0 })
                {
                    _currentPath = newPath;
                    _currentWaypointIndex = 0;
                    CurrentState = EnemyState.Chasing;
                }
                return;
            }

            // Can't see target - go to last known
            HandleNoLos();
        }

        private void FollowPath()
        {
            if (_currentPath == null || _currentPath.Count == 0)
                return;

            if (_currentWaypointIndex >= _currentPath.Count)
            {
                _currentPath = null;
                return;
            }

            var targetWaypoint = _currentPath[_currentWaypointIndex];
            targetWaypoint.y = transform.position.y; // Keep on same Y plane

            var direction = (targetWaypoint - transform.position).normalized;
            var distanceToWaypoint = Vector3.Distance(transform.position, targetWaypoint);

            // Move towards waypoint
            if (distanceToWaypoint > waypointReachedThreshold)
            {
                MoveTowards(direction);
                RotateTowards(direction);
            }
            else
            {
                // Reached waypoint, move to next
                _currentWaypointIndex++;
            }
        }

        private void MoveTowards(Vector3 direction)
        {
            if (_rb)
            {
                Vector3 velocity = direction * moveSpeed;
                velocity.y = _rb.linearVelocity.y; // Preserve Y velocity for gravity
                _rb.linearVelocity = velocity;
            }
            else
            {
                transform.position += direction * (moveSpeed * Time.deltaTime);
            }
        }

        private void RotateTowards(Vector3 direction)
        {
            if (direction == Vector3.zero)
                return;

            var targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        public void SetTarget(Transform newTarget)
        {
            _target = newTarget;
            _hasTarget = newTarget != null;

            if (!_hasTarget) return;
            _lastKnownTargetPosition = _target.position;
            _pathUpdateTimer = pathUpdateInterval; // Force immediate path update
        }

        public void StopMovement()
        {
            _currentPath = null;
            CurrentState = EnemyState.Idle;

            if (_rb != null)
            {
                _rb.linearVelocity = new Vector3(0f, _rb.linearVelocity.y, 0f);
            }
        }

        public void ResumeMovement()
        {
            if (_hasTarget)
            {
                _pathUpdateTimer = pathUpdateInterval; // Force path update
            }
        }

        private void OnDrawGizmos()
        {
            if (!drawPath || _currentPath == null || _currentPath.Count == 0)
                return;

            Gizmos.color = pathColor;

            // Draw line from enemy to first waypoint
            if (_currentWaypointIndex < _currentPath.Count)
            {
                Gizmos.DrawLine(transform.position, _currentPath[_currentWaypointIndex]);
            }

            // Draw path
            for (var i = _currentWaypointIndex; i < _currentPath.Count - 1; i++)
            {
                Gizmos.DrawLine(_currentPath[i], _currentPath[i + 1]);
                Gizmos.DrawWireSphere(_currentPath[i], 0.2f);
            }

            // Draw final waypoint
            if (_currentPath.Count <= 0) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_currentPath[^1], 0.3f);
        }

        private void OnDrawGizmosSelected()
        {
            // Draw detection range
            Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            if (_target == null) return;
            Gizmos.color = HasLineOfSight(_target.position) ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, _target.position);
        }
    }
}