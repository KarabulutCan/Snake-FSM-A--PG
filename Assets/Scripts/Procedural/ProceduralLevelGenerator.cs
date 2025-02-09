// Assets/Scripts/Procedural/ProceduralLevelGenerator.cs
using System.Collections.Generic;
using UnityEngine;

public class ProceduralLevelGenerator : MonoBehaviour
{
    [Header("Harita Boyutu")]
    public int width = 20;
    public int height = 20;

    [Header("Engel Prefab")]
    public GameObject obstaclePrefab;

    [Header("Duvar (Boundary) Prefab")]
    public GameObject boundaryPrefab;

    [Header("Engel Oluşma İhtimali [0-1]")]
    [Range(0f, 1f)]
    public float obstacleChance = 0.1f;

    // Engellerin grid pozisyonlarını tutar
    private List<Vector2Int> obstaclePositions = new List<Vector2Int>();

    private void Start()
    {
        GenerateLevel();
    }

    public void GenerateLevel()
    {
        // Eski engelleri/duvarları temizle
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        obstaclePositions.Clear();

        // 1) Rastgele engeller
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (Random.value < obstacleChance)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    obstaclePositions.Add(pos);

                    Vector3 worldPos = new Vector3(x, y, 0f);
                    Instantiate(obstaclePrefab, worldPos, Quaternion.identity, transform);
                }
            }
        }

        // 2) Sınır duvarlarını oluştur
        CreateBoundaryWalls();

        // 3) AStarPathfinding'i güncelle
        if (AStarPathfinding.Instance != null)
        {
            AStarPathfinding.Instance.RefreshGrid(width, height, obstaclePositions);
        }
    }

    private void CreateBoundaryWalls()
    {
        // Haritanın etrafını kapatacak şekilde:
        // Alt sıra (y = -1)
        for (int x = -1; x <= width; x++)
        {
            Vector2Int pos = new Vector2Int(x, -1);
            obstaclePositions.Add(pos);

            Vector3 worldPos = new Vector3(x, -1, 0f);
            Instantiate(boundaryPrefab, worldPos, Quaternion.identity, transform);
        }

        // Üst sıra (y = height)
        for (int x = -1; x <= width; x++)
        {
            Vector2Int pos = new Vector2Int(x, height);
            obstaclePositions.Add(pos);

            Vector3 worldPos = new Vector3(x, height, 0f);
            Instantiate(boundaryPrefab, worldPos, Quaternion.identity, transform);
        }

        // Sol sütun (x = -1)
        for (int y = 0; y < height; y++)
        {
            Vector2Int pos = new Vector2Int(-1, y);
            obstaclePositions.Add(pos);

            Vector3 worldPos = new Vector3(-1, y, 0f);
            Instantiate(boundaryPrefab, worldPos, Quaternion.identity, transform);
        }

        // Sağ sütun (x = width)
        for (int y = 0; y < height; y++)
        {
            Vector2Int pos = new Vector2Int(width, y);
            obstaclePositions.Add(pos);

            Vector3 worldPos = new Vector3(width, y, 0f);
            Instantiate(boundaryPrefab, worldPos, Quaternion.identity, transform);
        }
    }

    // Obstacles listesini isteyenler için
    public List<Vector2Int> GetObstaclePositions()
    {
        return obstaclePositions;
    }


}
