using System.Collections.Generic;
using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    [Header("Yem Prefab (Tek)")]
    public GameObject foodPrefab; // Üzerinde Food.cs olan tek prefab

    [Header("Farklı Yem Spriteleri")]
    public Sprite[] foodSprites;  // Rastgele seçmek istediğiniz spriteler

    [Header("Harita Boyutu")]
    public int width = 20;
    public int height = 20;

    private GameObject currentFood;
    private SnakeAI snake;

    private void Start()
    {
        snake = Object.FindAnyObjectByType<SnakeAI>();
        SpawnFood();
    }

    public void SpawnFood()
    {
        // Eski yemi yok et
        if (currentFood != null)
        {
            Destroy(currentFood);
        }

        // Rastgele geçerli pozisyon bul (aşağıdaki fonksiyon tüm kontrolleri yapar)
        Vector2Int spawnPos = GetRandomValidPosition();

        // Yem oluştur
        Vector3 pos = new Vector3(spawnPos.x, spawnPos.y, 0f);
        currentFood = Instantiate(foodPrefab, pos, Quaternion.identity);

        // Rastgele sprite atama
        // (Prefab'in üzerinde bir SpriteRenderer olduğunu varsayıyoruz)
        SpriteRenderer sr = currentFood.GetComponent<SpriteRenderer>();
        if (sr != null && foodSprites != null && foodSprites.Length > 0)
        {
            int index = Random.Range(0, foodSprites.Length);
            sr.sprite = foodSprites[index];
        }

        // Yılan AI'ya bildir
        if (snake != null)
        {
            snake.SetFoodPosition(spawnPos);
        }
    }

    private Vector2Int GetRandomValidPosition()
    {
        // Procedural haritadan engelleri okuyalım
        ProceduralLevelGenerator generator = Object.FindAnyObjectByType<ProceduralLevelGenerator>();
        List<Vector2Int> obstacles = (generator != null)
            ? generator.GetObstaclePositions()
            : new List<Vector2Int>();

        // Yılan kafası grid pozisyonu
        Vector2Int snakeHeadPos = new Vector2Int(
            Mathf.RoundToInt(snake.transform.position.x),
            Mathf.RoundToInt(snake.transform.position.y)
        );

        // Harita çok doluysa aşırı döngüye girmemek için max deneme sayısı
        int maxAttempts = 200;
        for (int i = 0; i < maxAttempts; i++)
        {
            // (1) Sınırlar içinde rastgele
            int rx = Random.Range(0, width);
            int ry = Random.Range(0, height);
            Vector2Int randPos = new Vector2Int(rx, ry);

            // (2) Obstacle veya boundary üzerinde mi?
            if (obstacles.Contains(randPos))
            {
                continue;
            }

            // (3) Yılan segmentleri üzerinde mi?
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

            // (4) Yılan kafasından buraya A* path var mı?
            var path = AStarPathfinding.Instance.FindPath(snakeHeadPos, randPos);
            if (path == null || path.Count == 0)
            {
                // Yol bulunamadıysa bu noktayı atla
                continue;
            }

            // 4 koşulu da geçen nokta uygundur
            return randPos;
        }

        // 200 deneme içinde yer bulamadıysa 
        Debug.LogWarning("Uygun bir Food yeri bulunamadı, varsayılan (0,0) dönülüyor!");
        return Vector2Int.zero;
    }

    public void OnFoodEaten()
    {
        SpawnFood();
    }
}
