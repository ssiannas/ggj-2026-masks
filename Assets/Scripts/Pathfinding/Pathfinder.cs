using UnityEngine;
using System.Collections.Generic;

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
        if (_grid == null)
            return null;

        Node startNode = _grid.NodeFromWorldPoint(startPos);
        Node targetNode = _grid.NodeFromWorldPoint(targetPos);

        if (startNode == null || targetNode == null)
            return null;

        // If target is unwalkable, find nearest walkable node
        if (!targetNode.Walkable)
        {
            targetNode = FindNearestWalkableNode(targetNode);
            if (targetNode == null)
                return null;
        }

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        int iterations = 0;

        while (openSet.Count > 0 && iterations < maxIterations)
        {
            iterations++;
            
            // Find node with lowest fCost
            Node currentNode = openSet[0];
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
            foreach (Node neighbor in _grid.GetNeighbors(currentNode))
            {
                if (!neighbor.Walkable || closedSet.Contains(neighbor))
                    continue;

                int newMovementCostToNeighbor = currentNode.GCost + GetDistance(currentNode, neighbor);
                
                if (newMovementCostToNeighbor < neighbor.GCost || !openSet.Contains(neighbor))
                {
                    neighbor.GCost = newMovementCostToNeighbor;
                    neighbor.HCost = GetDistance(neighbor, targetNode);
                    neighbor.Parent = currentNode;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        // No path found - return partial path to closest node
        if (closedSet.Count > 0)
        {
            Node closestNode = null;
            int closestDistance = int.MaxValue;
            
            foreach (Node node in closedSet)
            {
                int distance = GetDistance(node, targetNode);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestNode = node;
                }
            }

            if (closestNode != null && closestNode != startNode)
            {
                return RetracePath(startNode, closestNode);
            }
        }

        return null;
    }

    private Node FindNearestWalkableNode(Node node)
    {
        int searchRadius = 1;
        int maxSearchRadius = 10;

        while (searchRadius <= maxSearchRadius)
        {
            List<Node> neighbors = GetNodesInRadius(node, searchRadius);
            Node nearest = null;
            float nearestDist = float.MaxValue;

            foreach (Node n in neighbors)
            {
                if (n.Walkable)
                {
                    float dist = Vector3.Distance(node.WorldPosition, n.WorldPosition);
                    if (dist < nearestDist)
                    {
                        nearestDist = dist;
                        nearest = n;
                    }
                }
            }

            if (nearest != null)
                return nearest;

            searchRadius++;
        }

        return null;
    }

    private List<Node> GetNodesInRadius(Node centerNode, int radius)
    {
        List<Node> nodes = new List<Node>();
        
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                if (Mathf.Abs(x) == radius || Mathf.Abs(y) == radius)
                {
                    Node neighbor = GetNodeAtOffset(centerNode, x, y);
                    if (neighbor != null)
                        nodes.Add(neighbor);
                }
            }
        }

        return nodes;
    }

    private Node GetNodeAtOffset(Node node, int offsetX, int offsetY)
    {
        // This would require grid access - simplified for now
        // In a full implementation, you'd want grid.GetNode(x, y) method
        return null;
    }

    private List<Vector3> RetracePath(Node startNode, Node endNode)
    {
        List<Vector3> path = new List<Vector3>();
        Node currentNode = endNode;

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

        List<Vector3> simplifiedPath = new List<Vector3>();
        Vector3 directionOld = Vector3.zero;

        for (int i = 1; i < path.Count; i++)
        {
            Vector3 directionNew = (path[i] - path[i - 1]).normalized;
            
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