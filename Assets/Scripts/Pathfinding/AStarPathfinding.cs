// Assets/Scripts/Pathfinding/AStarPathfinding.cs
using UnityEngine;
using System.Collections.Generic;

public class Node
{
    public Vector2Int gridPos;
    public bool isWalkable;
    public float gCost;
    public float hCost;
    public Node parent;

    public float fCost
    {
        get { return gCost + hCost; }
    }

    public Node(Vector2Int pos, bool walkable = true)
    {
        gridPos = pos;
        isWalkable = walkable;
    }
}

public class AStarPathfinding : MonoBehaviour
{
    public static AStarPathfinding Instance;

    [Header("Grid Ayarları")]
    public int width = 20;
    public int height = 20;
    public float nodeSize = 1f;

    private Node[,] grid;

    private void Awake()
    {
        // Singleton
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        CreateGrid();
    }

    private void CreateGrid()
    {
        grid = new Node[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = new Node(new Vector2Int(x, y), true);
            }
        }
    }

    public void RefreshGrid(int newWidth, int newHeight, List<Vector2Int> obstaclePositions)
    {
        width = newWidth;
        height = newHeight;
        CreateGrid();
        SetObstacles(obstaclePositions);
    }

    public void SetObstacles(List<Vector2Int> obstaclePositions)
    {
        // Tüm node'ları yeniden walkable yap
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y].isWalkable = true;
            }
        }

        // Engelli noktaları kapat
        foreach (var pos in obstaclePositions)
        {
            if (IsInBounds(pos))
            {
                grid[pos.x, pos.y].isWalkable = false;
            }
        }
    }

    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int end)
    {
        Node startNode = GetNode(start);
        Node endNode = GetNode(end);

        if (startNode == null || endNode == null) return null;

        List<Node> openSet = new List<Node>() { startNode };
        HashSet<Node> closedSet = new HashSet<Node>();

        startNode.gCost = 0;
        startNode.hCost = Vector2Int.Distance(start, end);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost ||
                    (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == endNode)
            {
                // Yol bulundu
                return RetracePath(startNode, endNode);
            }

            foreach (Node neighbor in GetNeighbors(currentNode))
            {
                if (!neighbor.isWalkable || closedSet.Contains(neighbor))
                    continue;

                float tentativeGCost = currentNode.gCost + Vector2Int.Distance(currentNode.gridPos, neighbor.gridPos);

                if (tentativeGCost < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = tentativeGCost;
                    neighbor.hCost = Vector2Int.Distance(neighbor.gridPos, endNode.gridPos);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        // Yol bulunamadı
        return null;
    }

    private List<Vector2Int> RetracePath(Node startNode, Node endNode)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode.gridPos);
            currentNode = currentNode.parent;
        }
        path.Reverse();
        return path;
    }

    private Node GetNode(Vector2Int pos)
    {
        if (!IsInBounds(pos)) return null;
        return grid[pos.x, pos.y];
    }

    private bool IsInBounds(Vector2Int pos)
    {
        return (pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height);
    }

    private List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();

        Vector2Int[] directions =
        {
            new Vector2Int( 1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int( 0, 1),
            new Vector2Int( 0,-1)
        };

        foreach (var dir in directions)
        {
            Vector2Int neighborPos = node.gridPos + dir;
            if (IsInBounds(neighborPos))
            {
                neighbors.Add(GetNode(neighborPos));
            }
        }
        return neighbors;
    }
}
