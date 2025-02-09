using UnityEngine;
using UnityEngine.Tilemaps;

public class CellularAutomataTilemapGenerator : MonoBehaviour
{
    [Header("Map Boyutu")]
    public int width = 50;
    public int height = 50;

    [Header("Tilemap Referansı")]
    public Tilemap tilemap;

    [Header("Tile'lar")]
    public TileBase wallTile;
    public TileBase floorTile;

    [Header("Oluşum Parametreleri")]
    [Range(0, 1f)]
    public float fillPercent = 0.45f; // İlk random duvar oranı (%45)

    public int smoothIterations = 5;  // Kaç kez Cellular Automata iterasyonu

    // Hücre datası: 1 = duvar, 0 = zemin
    private int[,] map;

    private void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        // 1) Dizi oluştur
        map = new int[width, height];

        // 2) Rastgele doldur
        RandomFillMap();

        // 3) Birkaç kez Cellular Automata uygula
        for (int i = 0; i < smoothIterations; i++)
        {
            SmoothMap();
        }

        // 4) Tilemap'e uygula
        DrawTilesOnTilemap();
    }

    private void RandomFillMap()
    {
        // Rastgele seed
        // (Eğer her seferinde aynı harita istiyorsanız seed sabit tutabilirsiniz)
        System.Random pseudoRandom = new System.Random();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Harita kenarlarını duvar yapabilirsiniz (opsiyonel)
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    map[x, y] = 1; // kenarlar duvar
                }
                else
                {
                    // Rastgele: fillPercent olasılıkla duvar
                    map[x, y] = (pseudoRandom.NextDouble() < fillPercent) ? 1 : 0;
                }
            }
        }
    }

    // Cellular Automata kuralı
    // Komşu duvar sayısı 4 veya daha fazlaysa hücre duvar, yoksa zemin
    private void SmoothMap()
    {
        int[,] newMap = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int neighbourWallCount = GetSurroundingWallCount(x, y);

                if (neighbourWallCount > 4)
                    newMap[x, y] = 1;
                else if (neighbourWallCount < 4)
                    newMap[x, y] = 0;
                else
                    newMap[x, y] = map[x, y];
            }
        }

        map = newMap;
    }

    // 8 yönlü komşulardaki duvar sayısı
    private int GetSurroundingWallCount(int gridX, int gridY)
    {
        int wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                if (neighbourX >= 0 && neighbourX < width && neighbourY >= 0 && neighbourY < height)
                {
                    if (!(neighbourX == gridX && neighbourY == gridY))
                    {
                        wallCount += map[neighbourX, neighbourY];
                    }
                }
                else
                {
                    // Harita dışını duvar sayabiliriz (daha kapalı olur)
                    wallCount++;
                }
            }
        }
        return wallCount;
    }

    // Elde ettiğimiz map dizisini Tilemap'e çiz
    private void DrawTilesOnTilemap()
    {
        // Önce tilemap'i temizleyelim
        tilemap.ClearAllTiles();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0);
                if (map[x, y] == 1)
                {
                    // Duvar
                    tilemap.SetTile(tilePos, wallTile);
                }
                else
                {
                    // Zemin
                    tilemap.SetTile(tilePos, floorTile);
                }
            }
        }
    }
}
