using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace ggj_2026_masks.Pathfinding
{
    public class PathfindingGrid : MonoBehaviour
    {
        public static PathfindingGrid Instance { get; private set; }

        [Header("Grid Settings")]
        [SerializeField] private Vector2 gridWorldSize = new Vector2(50f, 50f);
        [SerializeField] private float nodeRadius = 0.5f;
        [SerializeField] private LayerMask[] obstacleLayers;

        [Header("Debug")] 
        [SerializeField] private bool drawGizmos = true;
        [SerializeField] private bool drawOnlyUnwalkable = false;
        [SerializeField] private bool drawGridPreview = true;  
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

        private void CreateGrid()
        {
            _grid = new Node[_gridSizeX, _gridSizeY];
            var worldBottomLeft = transform.position
                                  - Vector3.right * gridWorldSize.x / 2f
                                  - Vector3.forward * gridWorldSize.y / 2f;

            for (var x = 0; x < _gridSizeX; x++)
            {
                for (var y = 0; y < _gridSizeY; y++)
                {
                    var worldPoint = worldBottomLeft
                                     + Vector3.right * (x * _nodeDiameter + nodeRadius)
                                     + Vector3.forward * (y * _nodeDiameter + nodeRadius);

                    var layerChecks = obstacleLayers.ToList();
                    var walkable = layerChecks.All(lm => !Physics.CheckSphere(worldPoint, nodeRadius, lm));
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

            var worldBottomLeft = transform.position
                                  - Vector3.right * gridWorldSize.x / 2f
                                  - Vector3.forward * gridWorldSize.y / 2f;

            for (var x = 0; x < _gridSizeX; x++)
            {
                for (var y = 0; y < _gridSizeY; y++)
                {
                    var worldPoint = worldBottomLeft
                                     + Vector3.right * (x * _nodeDiameter + nodeRadius)
                                     + Vector3.forward * (y * _nodeDiameter + nodeRadius);
                    var layerChecks = obstacleLayers.ToList();
                    var walkable = layerChecks.All(lm => !Physics.CheckSphere(worldPoint, nodeRadius, lm));
                    _grid[x, y].Walkable = walkable;
                }
            }
        }

        public Node NodeFromWorldPoint(Vector3 worldPosition)
        {
            var percentX = (worldPosition.x - transform.position.x + gridWorldSize.x / 2f) / gridWorldSize.x;
            var percentY = (worldPosition.z - transform.position.z + gridWorldSize.y / 2f) / gridWorldSize.y;

            percentX = Mathf.Clamp01(percentX);
            percentY = Mathf.Clamp01(percentY);

            var x = Mathf.RoundToInt((_gridSizeX - 1) * percentX);
            var y = Mathf.RoundToInt((_gridSizeY - 1) * percentY);

            return _grid[x, y];
        }

        public Node GetNode(int x, int y)
        {
            if (x >= 0 && x < _gridSizeX && y >= 0 && y < _gridSizeY)
            {
                return _grid[x, y];
            }
            return null;
        }
        
        public List<Node> GetNeighbors(Node node)
        {
            var neighbors = new List<Node>(8);

            for (var x = -1; x <= 1; x++)
            {
                for (var y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                        continue;

                    var checkX = node.GridX + x;
                    var checkY = node.GridY + y;

                    if (checkX < 0 || checkX >= _gridSizeX || checkY < 0 || checkY >= _gridSizeY) continue;
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

            return neighbors;
        }

        public bool IsWalkable(Vector3 worldPosition)
        {
            var node = NodeFromWorldPoint(worldPosition);
            return node is { Walkable: true };
        }
        
        private void OnDrawGizmos()
        {
            // Always draw grid bounds
            Gizmos.color = gridBoundsColor;
            Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 0.5f, gridWorldSize.y));

            if (!drawGizmos) return;

            var diameter = nodeRadius * 2f;
            var sizeX = Mathf.RoundToInt(gridWorldSize.x / diameter);
            var sizeY = Mathf.RoundToInt(gridWorldSize.y / diameter);

            var bottomLeft = transform.position 
                             - Vector3.right * gridWorldSize.x / 2f 
                             - Vector3.forward * gridWorldSize.y / 2f;

            var layersToCheck = obstacleLayers.ToList();
            var canCheckWalkable = layersToCheck.All(lm => !Physics.CheckSphere(bottomLeft, diameter, lm));

            for (var x = 0; x < sizeX; x++)
            {
                for (var y = 0; y < sizeY; y++)
                {
                    var pos = bottomLeft 
                              + Vector3.right * (x * diameter + nodeRadius)
                              + Vector3.forward * (y * diameter + nodeRadius);

                    if (canCheckWalkable)
                    {
                        var walkable = layersToCheck.All(lm => !Physics.CheckSphere(pos, nodeRadius, lm));

                        if (walkable)
                        {
                            if (drawOnlyUnwalkable) continue;
                            Gizmos.color = walkableColor;
                            Gizmos.DrawWireCube(pos, Vector3.one * (diameter - 0.1f));
                        }
                        else
                        {
                            Gizmos.color = unwalkableColor;
                            Gizmos.DrawCube(pos, Vector3.one * (diameter - 0.05f));
                    
                            // Draw X on unwalkable
                            Gizmos.color = Color.white;
                            var halfSize = (diameter - 0.1f) * 0.4f;
                            var xPos = pos + Vector3.up * 0.01f;
                            Gizmos.DrawLine(xPos + new Vector3(-halfSize, 0, -halfSize), xPos + new Vector3(halfSize, 0, halfSize));
                            Gizmos.DrawLine(xPos + new Vector3(-halfSize, 0, halfSize), xPos + new Vector3(halfSize, 0, -halfSize));
                        }
                    }
                    else
                    {
                        Gizmos.color = new Color(1f, 1f, 1f, 0.15f);
                        Gizmos.DrawWireCube(pos, Vector3.one * (diameter - 0.1f));
                    }
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
    
            for (var x = 0; x <= _gridSizeX; x++)
            {
                var start = bottomLeft + Vector3.right * x * _nodeDiameter;
                Gizmos.DrawLine(start, start + Vector3.forward * gridWorldSize.y);
            }
    
            for (var y = 0; y <= _gridSizeX; y++)
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