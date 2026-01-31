using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace ggj_2026_masks.Pathfinding 
{
public class Pathfinder : MonoBehaviour
{
    public static Pathfinder Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private int maxIterations = 1000;
    
    private PathfindingGrid _grid;

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
    }

    private void Start()
    {
        _grid = PathfindingGrid.Instance;
        if (_grid == null)
        {
            Debug.LogError("Pathfinder: No PathfindingGrid found in scene!");
        }
    }

    public List<Vector3> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        if (!_grid)
            return null;

        var startNode = _grid.NodeFromWorldPoint(startPos);
        var targetNode = _grid.NodeFromWorldPoint(targetPos);

        if (startNode == null || targetNode == null)
            return null;

        // If start is unwalkable, find nearest walkable
        if (!startNode.Walkable)
        {
            startNode = FindNearestWalkableNode(startNode);
            if (startNode == null)
                return null;
        }

        // If target is unwalkable, find nearest walkable
        if (!targetNode.Walkable)
        {
            targetNode = FindNearestWalkableNode(targetNode);
            if (targetNode == null)
                return null;
        }

        var openSet = new List<Node>();
        var closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        var iterations = 0;

        while (openSet.Count > 0 && iterations < maxIterations)
        {
            iterations++;
            
            // Find node with lowest fCost
            var currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].FCost < currentNode.FCost || 
                    (openSet[i].FCost == currentNode.FCost && openSet[i].HCost < currentNode.HCost))
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            // Path found
            if (currentNode == targetNode)
            {
                return RetracePath(startNode, targetNode);
            }

            // Check neighbors
            foreach (var neighbor in _grid.GetNeighbors(currentNode))
            {
                if (!neighbor.Walkable || closedSet.Contains(neighbor))
                    continue;

                var newMovementCostToNeighbor = currentNode.GCost + GetDistance(currentNode, neighbor);

                if (newMovementCostToNeighbor >= neighbor.GCost && openSet.Contains(neighbor)) continue;
                neighbor.GCost = newMovementCostToNeighbor;
                neighbor.HCost = GetDistance(neighbor, targetNode);
                neighbor.Parent = currentNode;

                if (!openSet.Contains(neighbor))
                    openSet.Add(neighbor);
            }
        }

        // No path found - return partial path to closest node
        if (closedSet.Count <= 0) return null;
        Node closestNode = null;
        var closestDistance = int.MaxValue;
            
        foreach (var node in closedSet)
        {
            var distance = GetDistance(node, targetNode);
            if (distance >= closestDistance) continue;
            closestDistance = distance;
            closestNode = node;
        }

        if (closestNode != null && closestNode != startNode)
        {
            return RetracePath(startNode, closestNode);
        }

        return null;
    }

    private Node FindNearestWalkableNode(Node node)
    {
        if (node.Walkable) return node;
    
        // BFS outward to find nearest walkable
        Queue<Node> queue = new Queue<Node>();
        HashSet<Node> visited = new HashSet<Node>();
    
        queue.Enqueue(node);
        visited.Add(node);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            var list = _grid.GetNeighbors(current);
            foreach (var neighbor in list.Where(neighbor => !visited.Contains(neighbor)))
            {
                visited.Add(neighbor);

                if (neighbor.Walkable)
                {
                    return neighbor;
                }

                // Limit search radius
                if (visited.Count > 100) return null;

                queue.Enqueue(neighbor);
            }
        }
        return null;
    }

    private List<Node> GetNodesInRadius(Node centerNode, int radius)
    {
        var nodes = new List<Node>();
        
        for (var x = -radius; x <= radius; x++)
        {
            for (var y = -radius; y <= radius; y++)
            {
                if (Mathf.Abs(x) != radius && Mathf.Abs(y) != radius) continue;
                var neighbor = GetNodeAtOffset(centerNode, x, y);
                if (neighbor != null)
                    nodes.Add(neighbor);
            }
        }

        return nodes;
    }

    private Node GetNodeAtOffset(Node node, int offsetX, int offsetY)
    {
        return _grid.GetNode(node.GridX + offsetX, node.GridY + offsetY);
    }


    private List<Vector3> RetracePath(Node startNode, Node endNode)
    {
        var path = new List<Vector3>();
        var currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode.WorldPosition);
            currentNode = currentNode.Parent;
        }

        path.Reverse();
        return SimplifyPath(path);
    }

    private List<Vector3> SimplifyPath(List<Vector3> path)
    {
        if (path.Count < 2)
            return path;

        var simplifiedPath = new List<Vector3>();
        var directionOld = Vector3.zero;

        for (var i = 1; i < path.Count; i++)
        {
            var directionNew = (path[i] - path[i - 1]).normalized;
            
            if (directionNew != directionOld)
            {
                simplifiedPath.Add(path[i - 1]);
            }
            
            directionOld = directionNew;
        }

        simplifiedPath.Add(path[^1]);
        return simplifiedPath;
    }

    private int GetDistance(Node nodeA, Node nodeB)
    {
        var dstX = Mathf.Abs(nodeA.GridX - nodeB.GridX);
        var dstY = Mathf.Abs(nodeA.GridY - nodeB.GridY);

        // Diagonal movement costs 14, straight costs 10 (approximation of sqrt(2))
        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        
        return 14 * dstX + 10 * (dstY - dstX);
    }
}
}