using System.Collections.Generic;
using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    [Header("Yem Prefab")]
    public GameObject foodPrefab;

    [Header("Harita Boyutu")]
    public int width = 20;
    public int height = 20;

    private GameObject currentFood;
    private SnakeAI snake;

    private void Start()
    {
        snake = FindObjectOfType<SnakeAI>();
        SpawnFood();
    }

    public void SpawnFood()
    {
        // Eski yemi yok et
        if (currentFood != null)
        {
            Destroy(currentFood);
        }

        // Rastgele geçerli pozisyon bul (Aşağıdaki fonksiyonda tüm kontroller var)
        Vector2Int spawnPos = GetRandomValidPosition();

        // Yem oluştur
        Vector3 pos = new Vector3(spawnPos.x, spawnPos.y, 0f);
        currentFood = Instantiate(foodPrefab, pos, Quaternion.identity);

        // Yılan AI'ya bildir
        if (snake != null)
        {
            snake.SetFoodPosition(spawnPos);
        }
    }

    private Vector2Int GetRandomValidPosition()
    {
        ProceduralLevelGenerator generator = FindObjectOfType<ProceduralLevelGenerator>();
        List<Vector2Int> obstacles = (generator != null) ? generator.GetObstaclePositions() : new List<Vector2Int>();

        // Yılan kafası grid pozisyonu
        Vector2Int snakeHeadPos = new Vector2Int(
            Mathf.RoundToInt(snake.transform.position.x),
            Mathf.RoundToInt(snake.transform.position.y)
        );

        // Belirli sayıda deneme hakkı (örneğin 200) verelim
        // (Harita çok doluysa sonsuza kadar döngüye girmemek için)
        int maxAttempts = 200;
        for (int i = 0; i < maxAttempts; i++)
        {
            // 1) Sınırlar içinde rastgele
            int rx = Random.Range(0, width);
            int ry = Random.Range(0, height);
            Vector2Int randPos = new Vector2Int(rx, ry);

            // 2) Obstacle veya boundary üzerinde mi?
            if (obstacles.Contains(randPos))
            {
                continue; // geçersiz -> sonraki deneme
            }

            // 3) Yılan segmentleri üzerinde mi?
            bool onSnake = false;
            foreach (Transform seg in snake.GetSegments())
            {
                Vector2Int segPos = new Vector2Int(
                    Mathf.RoundToInt(seg.position.x),
                    Mathf.RoundToInt(seg.position.y)
                );
                if (segPos == randPos)
                {
                    onSnake = true;
                    break;
                }
            }
            if (onSnake) continue;

            // 4) Yılan kafasından buraya A* path var mı?
            var path = AStarPathfinding.Instance.FindPath(snakeHeadPos, randPos);
            if (path == null || path.Count == 0)
            {
                // Yol bulunamadıysa bu noktayı atla
                continue;
            }

            // Yukarıdaki 4 kontrolü geçen nokta geçerlidir
            return randPos;
        }

        // Eğer bu kadar denemede bulamadıysak 
        // (harita tamamen tıkanmış olabilir)
        Debug.LogWarning("Uygun bir Food yeri bulunamadı, varsayılan (0,0) dönülüyor!");
        return Vector2Int.zero;
    }

    public void OnFoodEaten()
    {
        SpawnFood();
    }
}
