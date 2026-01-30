using UnityEngine;
using System.Collections.Generic;
using ggj_2026_masks.Pathfinding;

namespace ggj_2026_masks
{
public class EnemyController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float waypointReachedThreshold = 0.3f;
    
    [Header("Pathfinding")]
    [SerializeField] private float pathUpdateInterval = 0.5f;
    [SerializeField] private float targetLostTimeout = 3f;
    [SerializeField] private string playerTag = "Player";
    
    [Header("Detection")]
    [SerializeField] private float detectionRange = 20f;
    [SerializeField] private bool alwaysChase = true;
    
    [Header("Debug")]
    [SerializeField] private bool drawPath = true;
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
        Stuck
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

        if (!_hasTarget || _target is null)
        {
            FindTarget();
            
            if (!_hasTarget)
            {
                CurrentState = EnemyState.Idle;
                return;
            }
        }

        // Check if target is in range
        if (!alwaysChase && _target && Vector3.Distance(transform.position, _target.position) > detectionRange)
        {
            CurrentState = EnemyState.Idle;
            _currentPath = null;
            return;
        }

        // Update path periodically
        if (_pathUpdateTimer >= pathUpdateInterval)
        {
            _pathUpdateTimer = 0f;
            UpdatePath();
        }

        // Follow path
        FollowPath();
    }

    private void FindTarget()
    {
        if (_players.Length == 0)
        {
            _hasTarget = false;
            _target = null;
            return;
        }

        // Find the closest player
        var closestDistance = float.MaxValue;
        GameObject closestPlayer = null;

        foreach (GameObject player in _players)
        {
            var distance = Vector3.Distance(transform.position, player.transform.position);

            if (!(distance < closestDistance)) continue;
            closestDistance = distance;
            closestPlayer = player;
        }

        if (closestPlayer is null) return;
        _target = closestPlayer.transform;
        _hasTarget = true;
        _lastKnownTargetPosition = _target.position;
    }

    private void UpdatePath()
    {
        if (!_target) return;

        var targetPosition = _target.position;
        _lastKnownTargetPosition = targetPosition;

        var newPath = _pathfinder.FindPath(transform.position, targetPosition);

        if (newPath is { Count: > 0 })
        {
            _currentPath = newPath;
            _currentWaypointIndex = 0;
            CurrentState = EnemyState.Chasing;
            _targetLostTimer = 0f;
        }
        else
        {
            _targetLostTimer += pathUpdateInterval;
            
            if (_targetLostTimer >= targetLostTimeout)
            {
                CurrentState = EnemyState.Stuck;
            }
        }
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

        Vector3 targetWaypoint = _currentPath[_currentWaypointIndex];
        targetWaypoint.y = transform.position.y; // Keep on same Y plane
        
        Vector3 direction = (targetWaypoint - transform.position).normalized;
        float distanceToWaypoint = Vector3.Distance(transform.position, targetWaypoint);

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
        if (_rb != null)
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

        Quaternion targetRotation = Quaternion.LookRotation(direction);
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
        
        if (_hasTarget)
        {
            _lastKnownTargetPosition = _target.position;
            _pathUpdateTimer = pathUpdateInterval; // Force immediate path update
        }
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
        for (int i = _currentWaypointIndex; i < _currentPath.Count - 1; i++)
        {
            Gizmos.DrawLine(_currentPath[i], _currentPath[i + 1]);
            Gizmos.DrawWireSphere(_currentPath[i], 0.2f);
        }

        // Draw final waypoint
        if (_currentPath.Count > 0)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_currentPath[^1], 0.3f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
}