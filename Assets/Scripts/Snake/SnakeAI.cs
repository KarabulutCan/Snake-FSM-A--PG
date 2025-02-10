using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FiniteStateMachine))]
public class SnakeAI : MonoBehaviour
{
    [Header("Snake Ayarları")]
    public float stepTime = 0.2f;   // Her adımda bekleme süresi
    public int initialSize = 3;     // Başlangıç gövde uzunluğu

    [Header("Segment Prefab (Gövde)")]
    public GameObject segmentPrefab;

    [Header("Kafa Spriteları (4 yön)")]
    public Sprite headUp;
    public Sprite headDown;
    public Sprite headLeft;
    public Sprite headRight;

    [Header("Gövde Spritesi")]
    public Sprite bodySprite;

    private List<Transform> segments = new List<Transform>();
    private FiniteStateMachine fsm;
    private float stepTimer;
    private Vector2Int foodGridPos; // Yem hedefi

    // Kafa SpriteRenderer (SnakeAI objesi üstünde olmalı)
    private SpriteRenderer headSpriteRenderer;

    private void Start()
    {
        fsm = GetComponent<FiniteStateMachine>();
        headSpriteRenderer = GetComponent<SpriteRenderer>();

        ResetState();

        // İlk State: Idle
        fsm.ChangeState(new SnakeIdleState(this));
    }

    private void Update()
    {
        stepTimer += Time.deltaTime;
        if (stepTimer >= stepTime)
        {
            stepTimer = 0f;
            MoveOneStep();
        }
    }

    /// <summary>
    /// Yılanı başa döndürür (segmentleri siler, konumu sıfırlar vb.)
    /// </summary>
    public void ResetState()
    {
        // Eski segmentleri yok et
        foreach (Transform seg in segments)
        {
            if (seg != this.transform) // kafanın kendisi değilse
            {
                Destroy(seg.gameObject);
            }
        }
        segments.Clear();

        // segments[0] -> kafa (bu objenin transform'u)
        segments.Add(this.transform);
        transform.position = Vector3.zero;

        // Başlangıç gövde uzunluğu
        for (int i = 1; i < initialSize; i++)
        {
            Grow();
        }

        // Kafayı yukarı sprite ile başlat (opsiyonel)
        if (headSpriteRenderer != null && headUp != null)
        {
            headSpriteRenderer.sprite = headUp;
        }
    }

    /// <summary>
    /// Yeni gövde segmenti ekle (yem yendiğinde çağrılır).
    /// </summary>
    public void Grow()
    {
        // Son segmentin konumuna
        Vector3 tailPos = segments[segments.Count - 1].position;
        Transform newSegment = Instantiate(segmentPrefab, tailPos, Quaternion.identity).transform;

        // Gövde sprite
        SpriteRenderer sr = newSegment.GetComponent<SpriteRenderer>();
        if (sr != null && bodySprite != null)
        {
            sr.sprite = bodySprite;
        }

        segments.Add(newSegment);
    }

    /// <summary>
    /// FoodSpawner'dan yem grid pozisyonu bilgisi alıyoruz.
    /// </summary>
    public void SetFoodPosition(Vector2Int foodPos)
    {
        foodGridPos = foodPos;
    }

    /// <summary>
    /// IdleState içinden çağrılabilir: Yem için path aranır, hareket tetiklenir.
    /// </summary>
    public void FindFoodAndMove()
    {
        Vector2Int headPos = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.y)
        );

        var path = AStarPathfinding.Instance.FindPath(headPos, foodGridPos);
        if (path == null || path.Count == 0)
        {
            Debug.Log("Yol bulunamadı!");
        }
    }

    /// <summary>
    /// Her stepTime'da bir adım at.
    /// </summary>
    private void MoveOneStep()
    {
        // 1) Yılan gövdesini engel kabul ettir
        UpdatePathfindingWithBody();

        // 2) Mevcut kafa pozisyonu
        Vector2Int headPos = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.y)
        );

        // 3) Path bul
        var path = AStarPathfinding.Instance.FindPath(headPos, foodGridPos);
        if (path != null && path.Count > 0)
        {
            Vector2Int nextPos = path[0];
            if (nextPos == headPos && path.Count > 1)
            {
                nextPos = path[1];
            }

            Vector2Int direction = nextPos - headPos;
            UpdateHeadSprite(direction);

            // Self-collision kontrolü
            if (CheckSelfCollision(nextPos))
            {
                GameManager.Instance.GameOver();
                return;
            }

            // Duvar/harita dışı kontrolü
            if (!AStarPathfinding.Instance.IsInBounds(nextPos) ||
                !AStarPathfinding.Instance.IsWalkable(nextPos))
            {
                Debug.Log("Duvara veya harita dışına ilerliyor!");
                GameManager.Instance.GameOver();
                return;
            }

            // Gövdeyi kaydır
            for (int i = segments.Count - 1; i > 0; i--)
            {
                segments[i].position = segments[i - 1].position;
            }

            // Kafayı yeni konuma taşı
            transform.position = new Vector3(nextPos.x, nextPos.y, 0f);
        }
        else
        {
            // Yol bulunamadı
            Debug.Log("Yol bulunamadı!");
            // Burada GameOver yapmak istiyorsanız:
            GameManager.Instance.GameOver();
            // veya isterseniz "return" vb. eklersiniz
        }
    }


    /// <summary>
    /// Kafayı yönüne göre sprite değiştirme
    /// </summary>
    private void UpdateHeadSprite(Vector2Int direction)
    {
        if (headSpriteRenderer == null) return;

        if (direction.x > 0)
        {
            headSpriteRenderer.sprite = headRight;
        }
        else if (direction.x < 0)
        {
            headSpriteRenderer.sprite = headLeft;
        }
        else if (direction.y > 0)
        {
            headSpriteRenderer.sprite = headUp;
        }
        else if (direction.y < 0)
        {
            headSpriteRenderer.sprite = headDown;
        }
    }

    /// <summary>
    /// nextPos eğer gövde segmentlerinden biriyle çakışırsa => çarpışma
    /// </summary>
    private bool CheckSelfCollision(Vector2Int nextPos)
    {
        // i=1 den başla -> segment[0] kafa
        for (int i = 1; i < segments.Count; i++)
        {
            Vector2Int segPos = new Vector2Int(
                Mathf.RoundToInt(segments[i].position.x),
                Mathf.RoundToInt(segments[i].position.y)
            );
            if (segPos == nextPos)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Yem yendi => yılan uzar
    /// </summary>
    public void OnFoodEaten()
    {
        Grow();
    }

    /// <summary>
    /// Segment listesini döndürür (FoodSpawner vs. kullanabilir)
    /// </summary>
    public List<Transform> GetSegments()
    {
        return segments;
    }

    /// <summary>
    /// A* pathfinding için yılan gövdesini de engel kabul etmek üzere grid'i günceller
    /// </summary>
    private void UpdatePathfindingWithBody()
    {
        ProceduralLevelGenerator generator = Object.FindAnyObjectByType<ProceduralLevelGenerator>();
        if (generator == null) return;

        // Duvar ve boundary engelleri al
        List<Vector2Int> baseObstacles = generator.GetObstaclePositions();

        // Yılan gövdesini de ekleyelim
        List<Vector2Int> combinedObstacles = new List<Vector2Int>(baseObstacles);

        // Kuyruğu hariç tutmak istersen i < segments.Count - 1
        for (int i = 1; i < segments.Count; i++)
        {
            Vector2Int segPos = new Vector2Int(
                Mathf.RoundToInt(segments[i].position.x),
                Mathf.RoundToInt(segments[i].position.y)
            );
            combinedObstacles.Add(segPos);
        }

        // Grid'i yenile
        int mapW = generator.width;
        int mapH = generator.height;
        AStarPathfinding.Instance.RefreshGrid(mapW, mapH, combinedObstacles);
    }
}
