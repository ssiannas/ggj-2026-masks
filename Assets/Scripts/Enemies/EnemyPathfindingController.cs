using System.Collections.Generic;
using UnityEngine;
using ggj_2026_masks.Pathfinding;

namespace ggj_2026_masks.Enemies
{
    public class EnemyPathfindingController : MonoBehaviour
    {
        [Header("Pathfinding")]
        [SerializeField] private float waypointReachedThreshold = 0.3f;
        [SerializeField] private float pathUpdateInterval = 0.5f;

        [Header("Debug")]
        [SerializeField] private bool drawPath = true;
        [SerializeField] private Color pathColor = Color.green;

        private Pathfinder _pathfinder;
        private List<Vector3> _currentPath;
        private int _currentWaypointIndex;
        private float _pathUpdateTimer;

        public bool HasPath => _currentPath != null && _currentPath.Count > 0;
        public bool PathComplete => _currentPath == null || _currentWaypointIndex >= _currentPath.Count;
        public float WaypointReachedThreshold => waypointReachedThreshold;

        private void Awake()
        {
            _pathfinder = Pathfinder.Instance;
        }

        public bool Initialize()
        {
            if (_pathfinder != null) return true;
            _pathfinder = Pathfinder.Instance;
            return _pathfinder != null;
        }

        public bool ShouldUpdatePath()
        {
            _pathUpdateTimer += Time.deltaTime;
            if (_pathUpdateTimer >= pathUpdateInterval)
            {
                _pathUpdateTimer = 0f;
                return true;
            }
            return false;
        }

        public void ForcePathUpdate()
        {
            _pathUpdateTimer = pathUpdateInterval;
        }

        public bool RequestPath(Vector3 from, Vector3 to)
        {
            var newPath = _pathfinder.FindPath(from, to);
            if (newPath is { Count: > 0 })
            {
                _currentPath = newPath;
                _currentWaypointIndex = 0;
                return true;
            }
            return false;
        }

        public void ClearPath()
        {
            _currentPath = null;
            _currentWaypointIndex = 0;
        }

        public Vector3 GetDirectionToCurrentWaypoint(Vector3 currentPosition)
        {
            if (_currentPath == null || _currentWaypointIndex >= _currentPath.Count)
                return Vector3.zero;

            var targetWaypoint = _currentPath[_currentWaypointIndex];
            targetWaypoint.y = currentPosition.y;

            return (targetWaypoint - currentPosition).normalized;
        }

        public float GetDistanceToCurrentWaypoint(Vector3 currentPosition)
        {
            if (_currentPath == null || _currentWaypointIndex >= _currentPath.Count)
                return float.MaxValue;

            var targetWaypoint = _currentPath[_currentWaypointIndex];
            targetWaypoint.y = currentPosition.y;

            return Vector3.Distance(currentPosition, targetWaypoint);
        }

        public bool AdvanceToNextWaypoint()
        {
            _currentWaypointIndex++;
            return _currentWaypointIndex < _currentPath?.Count;
        }

        public Vector3? GetPathDestination()
        {
            if (_currentPath == null || _currentPath.Count == 0)
                return null;

            return _currentPath[^1];
        }

        private void OnDrawGizmos()
        {
            if (!drawPath || _currentPath == null || _currentPath.Count == 0)
                return;

            Gizmos.color = pathColor;

            if (_currentWaypointIndex < _currentPath.Count)
            {
                Gizmos.DrawLine(transform.position, _currentPath[_currentWaypointIndex]);
            }

            for (var i = _currentWaypointIndex; i < _currentPath.Count - 1; i++)
            {
                Gizmos.DrawLine(_currentPath[i], _currentPath[i + 1]);
                Gizmos.DrawWireSphere(_currentPath[i], 0.2f);
            }

            if (_currentPath.Count <= 0) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_currentPath[^1], 0.3f);
        }
    }
}
