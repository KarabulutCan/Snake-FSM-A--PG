using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ProceduralLevelGenerator : MonoBehaviour
{
    [Header("Harita Boyutu")]
    public int width = 20;
    public int height = 20;

    [Header("Tilemap ve Tile'lar")]
    // DİKKAT: Artık iki ayrı Tilemap kullanıyoruz
    public Tilemap floorTilemap;   // Zemin Tilemap
    public Tilemap wallTilemap;    // Duvar Tilemap

    public TileBase wallTile;
    public TileBase floorTile;

    [Header("Cellular Automata Parametreleri")]
    [Range(0f, 1f)]
    public float fillPercent = 0.45f;
    public int smoothIterations = 5;

    [Header("Spawn Bölgesi (0,0 etrafında boşluk)")]
    public int spawnAreaWidth = 10;
    public int spawnAreaHeight = 10;

    // Harita datası (1=duvar, 0=zemin)
    private int[,] map;

    // A* engeller için duvar listesini tutar
    private List<Vector2Int> obstaclePositions = new List<Vector2Int>();

    private void Start()
    {
        GenerateLevel();
    }

    public void GenerateLevel()
    {
        // 1) Tilemap'leri temizle (zemin + duvar)
        if (floorTilemap != null) floorTilemap.ClearAllTiles();
        if (wallTilemap != null) wallTilemap.ClearAllTiles();

        obstaclePositions.Clear();

        // 2) Haritayı rastgele oluştur (Cellular Automata)
        map = new int[width, height];
        RandomFillMap();

        for (int i = 0; i < smoothIterations; i++)
        {
            SmoothMap();
        }

        // (0,0) etrafında spawnAreaWidth × spawnAreaHeight bölgesini her zaman zemin yap
        ForceFloorArea(spawnAreaWidth, spawnAreaHeight);

        // 3) İki ayrı Tilemap'e çiz
        DrawTilesOnTilemaps();

        // 4) A* pathfinding'i güncelle
        if (AStarPathfinding.Instance != null)
        {
            AStarPathfinding.Instance.RefreshGrid(width, height, obstaclePositions);
        }
    }

    private void RandomFillMap()
    {
        System.Random prng = new System.Random();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (prng.NextDouble() < fillPercent)
                    map[x, y] = 1; // duvar
                else
                    map[x, y] = 0; // zemin
            }
        }
    }

    private void SmoothMap()
    {
        int[,] newMap = new int[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int neighbourCount = GetSurroundingWallCount(x, y);
                if (neighbourCount > 4)
                    newMap[x, y] = 1;
                else if (neighbourCount < 4)
                    newMap[x, y] = 0;
                else
                    newMap[x, y] = map[x, y];
            }
        }
        map = newMap;
    }

    private int GetSurroundingWallCount(int cx, int cy)
    {
        int count = 0;
        for (int nx = cx - 1; nx <= cx + 1; nx++)
        {
            for (int ny = cy - 1; ny <= cy + 1; ny++)
            {
                if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                {
                    if (!(nx == cx && ny == cy))
                    {
                        count += map[nx, ny];
                    }
                }
                else
                {
                    // Harita dışını duvar say
                    count++;
                }
            }
        }
        return count;
    }

    /// <summary>
    /// (0,0) etrafında belirli bir genişlik/yükseklikteki alanı zorunlu zemin yapar.
    /// </summary>
    private void ForceFloorArea(int areaWidth, int areaHeight)
    {
        for (int x = 0; x < areaWidth; x++)
        {
            for (int y = 0; y < areaHeight; y++)
            {
                if (x < width && y < height)
                {
                    map[x, y] = 0; // daima zemin
                }
            }
        }
    }

    /// <summary>
    /// Haritayı iki farklı Tilemap'e (floor + wall) çizer.
    /// </summary>
    private void DrawTilesOnTilemaps()
    {
        // Varsa yoksa kontrol
        if (floorTilemap == null || wallTilemap == null)
        {
            Debug.LogWarning("FloorTilemap veya WallTilemap atanmamış!");
            return;
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Tilemap koordinatına dönüştür
                Vector3Int tilePos = new Vector3Int(x, y, 0);

                // Duvar mı zemin mi?
                if (map[x, y] == 1)
                {
                    // Duvar tile'ı
                    wallTilemap.SetTile(tilePos, wallTile);
                    obstaclePositions.Add(new Vector2Int(x, y));
                }
                else
                {
                    // Zemin tile'ı
                    floorTilemap.SetTile(tilePos, floorTile);
                }
            }
        }
    }

    public List<Vector2Int> GetObstaclePositions()
    {
        return obstaclePositions;
    }
}
