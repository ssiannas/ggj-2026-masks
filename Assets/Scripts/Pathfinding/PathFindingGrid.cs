using UnityEngine;
using System.Collections.Generic;

namespace ggj_2026_masks.Pathfinding
{
    public class PathfindingGrid : MonoBehaviour
    {
        public static PathfindingGrid Instance { get; private set; }

        [Header("Grid Settings")] [SerializeField]
        private Vector2 gridWorldSize = new Vector2(50f, 50f);

        [SerializeField] private float nodeRadius = 0.5f;
        [SerializeField] private LayerMask obstacleLayer;

        [Header("Debug")] [SerializeField] private bool drawGizmos = true;
        [SerializeField] private bool drawOnlyUnwalkable = false;
        [SerializeField] private Color walkableColor = new Color(0f, 1f, 0f, 0.3f);
        [SerializeField] private Color unwalkableColor = new Color(1f, 0f, 0f, 0.7f);
        [SerializeField] private Color gridBoundsColor = Color.cyan;

        private Node[,] _grid;
        private float _nodeDiameter;
        private int _gridSizeX, _gridSizeY;

        public int MaxSize => _gridSizeX * _gridSizeY;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            _nodeDiameter = nodeRadius * 2f;
            _gridSizeX = Mathf.RoundToInt(gridWorldSize.x / _nodeDiameter);
            _gridSizeY = Mathf.RoundToInt(gridWorldSize.y / _nodeDiameter);

            CreateGrid();
        }

        public void CreateGrid()
        {
            _grid = new Node[_gridSizeX, _gridSizeY];
            Vector3 worldBottomLeft = transform.position
                                      - Vector3.right * gridWorldSize.x / 2f
                                      - Vector3.forward * gridWorldSize.y / 2f;

            for (int x = 0; x < _gridSizeX; x++)
            {
                for (int y = 0; y < _gridSizeY; y++)
                {
                    Vector3 worldPoint = worldBottomLeft
                                         + Vector3.right * (x * _nodeDiameter + nodeRadius)
                                         + Vector3.forward * (y * _nodeDiameter + nodeRadius);

                    bool walkable = !Physics.CheckSphere(worldPoint, nodeRadius, obstacleLayer);
                    _grid[x, y] = new Node(walkable, worldPoint, x, y);
                }
            }
        }

        public void RefreshGrid()
        {
            if (_grid == null)
            {
                CreateGrid();
                return;
            }

            Vector3 worldBottomLeft = transform.position
                                      - Vector3.right * gridWorldSize.x / 2f
                                      - Vector3.forward * gridWorldSize.y / 2f;

            for (int x = 0; x < _gridSizeX; x++)
            {
                for (int y = 0; y < _gridSizeY; y++)
                {
                    Vector3 worldPoint = worldBottomLeft
                                         + Vector3.right * (x * _nodeDiameter + nodeRadius)
                                         + Vector3.forward * (y * _nodeDiameter + nodeRadius);

                    _grid[x, y].Walkable = !Physics.CheckSphere(worldPoint, nodeRadius, obstacleLayer);
                }
            }
        }

        public Node NodeFromWorldPoint(Vector3 worldPosition)
        {
            float percentX = (worldPosition.x - transform.position.x + gridWorldSize.x / 2f) / gridWorldSize.x;
            float percentY = (worldPosition.z - transform.position.z + gridWorldSize.y / 2f) / gridWorldSize.y;

            percentX = Mathf.Clamp01(percentX);
            percentY = Mathf.Clamp01(percentY);

            int x = Mathf.RoundToInt((_gridSizeX - 1) * percentX);
            int y = Mathf.RoundToInt((_gridSizeY - 1) * percentY);

            return _grid[x, y];
        }

        public List<Node> GetNeighbors(Node node)
        {
            List<Node> neighbors = new List<Node>(8);

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                        continue;

                    int checkX = node.GridX + x;
                    int checkY = node.GridY + y;

                    if (checkX >= 0 && checkX < _gridSizeX && checkY >= 0 && checkY < _gridSizeY)
                    {
                        // Optional: prevent diagonal movement through corners
                        if (x != 0 && y != 0)
                        {
                            if (!_grid[node.GridX + x, node.GridY].Walkable ||
                                !_grid[node.GridX, node.GridY + y].Walkable)
                                continue;
                        }

                        neighbors.Add(_grid[checkX, checkY]);
                    }
                }
            }

            return neighbors;
        }

        public bool IsWalkable(Vector3 worldPosition)
        {
            Node node = NodeFromWorldPoint(worldPosition);
            return node != null && node.Walkable;
        }

        private void OnDrawGizmos()
        {
            // Always draw grid bounds
            Gizmos.color = gridBoundsColor;
            Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 0.5f, gridWorldSize.y));

            if (!drawGizmos || _grid == null)
                return;

            foreach (Node node in _grid)
            {
                if (drawOnlyUnwalkable && node.Walkable)
                    continue;

                if (node.Walkable)
                {
                    Gizmos.color = walkableColor;
                    Gizmos.DrawWireCube(node.WorldPosition, Vector3.one * (_nodeDiameter - 0.1f));
                }
                else
                {
                    Gizmos.color = unwalkableColor;
                    Gizmos.DrawCube(node.WorldPosition, Vector3.one * (_nodeDiameter - 0.05f));
            
                    // Draw X on unwalkable
                    Gizmos.color = Color.white;
                    var halfSize = (_nodeDiameter - 0.1f) * 0.4f;
                    var pos = node.WorldPosition + Vector3.up * 0.01f;
                    Gizmos.DrawLine(pos + new Vector3(-halfSize, 0, -halfSize), pos + new Vector3(halfSize, 0, halfSize));
                    Gizmos.DrawLine(pos + new Vector3(-halfSize, 0, halfSize), pos + new Vector3(halfSize, 0, -halfSize));
                }
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(gridBoundsColor.r, gridBoundsColor.g, gridBoundsColor.b, 0.1f);
            Gizmos.DrawCube(transform.position, new Vector3(gridWorldSize.x, 0.1f, gridWorldSize.y));
    
            if (Instance == null)
                return;
        
            Gizmos.color = new Color(1f, 1f, 1f, 0.1f);
            var bottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2f - Vector3.forward * gridWorldSize.y / 2f;
    
            for (int x = 0; x <= _gridSizeX; x++)
            {
                var start = bottomLeft + Vector3.right * x * _nodeDiameter;
                Gizmos.DrawLine(start, start + Vector3.forward * gridWorldSize.y);
            }
    
            for (int y = 0; y <= _gridSizeX; y++)
            {
                var start = bottomLeft + Vector3.forward * y * _nodeDiameter;
                Gizmos.DrawLine(start, start + Vector3.right * gridWorldSize.x);
            }
        }
        

    }

    public class Node
    {
        public bool Walkable;
        public Vector3 WorldPosition;
        public readonly int GridX;
        public readonly int GridY;

        public int GCost; // Distance from start
        public int HCost; // Distance to target (heuristic)
        public int FCost => GCost + HCost;

        public Node Parent;

        public Node(bool walkable, Vector3 worldPosition, int gridX, int gridY)
        {
            Walkable = walkable;
            WorldPosition = worldPosition;
            GridX = gridX;
            GridY = gridY;
        }
    }
}