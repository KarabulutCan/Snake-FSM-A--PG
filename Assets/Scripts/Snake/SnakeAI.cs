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

    // Kafa SpriteRenderer (kendi GameObject'imizde)
    private SpriteRenderer headSpriteRenderer;

    private void Start()
    {
        fsm = GetComponent<FiniteStateMachine>();

        // Bu obje yılanın kafası, üzerinde SpriteRenderer olmalı
        headSpriteRenderer = GetComponent<SpriteRenderer>();

        ResetState();

        // İlk State: Idle
        fsm.ChangeState(new SnakeIdleState(this));
    }

    private void Update()
    {
        // Adım zamanı kontrolü
        stepTimer += Time.deltaTime;
        if (stepTimer >= stepTime)
        {
            stepTimer = 0f;
            MoveOneStep();
        }
    }

    /// <summary>
    /// Yılanı başlangıç durumuna döndürür.
    /// </summary>
    public void ResetState()
    {
        // Var olan segmentleri sil
        foreach (Transform seg in segments)
        {
            if (seg != this.transform)
            {
                Destroy(seg.gameObject);
            }
        }
        segments.Clear();

        // segments[0] -> Kafa (bu objenin Transform'u)
        segments.Add(this.transform);
        transform.position = Vector3.zero;

        // Başlangıç uzunluğu
        for (int i = 1; i < initialSize; i++)
        {
            Grow();
        }

        // İsteğe bağlı: Kafayı yukarı sprite ile başlat
        if (headSpriteRenderer != null && headUp != null)
        {
            headSpriteRenderer.sprite = headUp;
        }
    }

    /// <summary>
    /// Yeni bir gövde segmenti eklenir (yem yendiğinde).
    /// </summary>
    public void Grow()
    {
        // Son segmentin olduğu konumda başlat
        Vector3 tailPos = segments[segments.Count - 1].position;
        Transform newSegment = Instantiate(segmentPrefab, tailPos, Quaternion.identity).transform;

        // Gövdeye sprite atama
        SpriteRenderer sr = newSegment.GetComponent<SpriteRenderer>();
        if (sr != null && bodySprite != null)
        {
            sr.sprite = bodySprite;
        }

        segments.Add(newSegment);
    }

    /// <summary>
    /// FoodSpawner tarafından yemeğin grid pozisyonu bildiriliyor.
    /// </summary>
    public void SetFoodPosition(Vector2Int foodPos)
    {
        foodGridPos = foodPos;
    }

    /// <summary>
    /// IdleState içinde çağrılır: Yem için path aranır vs.
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
            // Yem ulaşılmazsa
            Debug.Log("Yol bulunamadı!");
        }
    }

    /// <summary>
    /// Her stepTime'da bir adım atar.
    /// </summary>
    private void MoveOneStep()
    {
        // 1) Yılanın gövdesini pathfinding engeli olarak işaretle
        UpdatePathfindingWithBody();

        // 2) Path'i bul
        Vector2Int headPos = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.y)
        );

        var path = AStarPathfinding.Instance.FindPath(headPos, foodGridPos);
        if (path != null && path.Count > 0)
        {
            // Normal path takip rutini...
            Vector2Int nextPos = path[0];
            if (nextPos == headPos && path.Count > 1)
            {
                nextPos = path[1];
            }

            // Kafanın gideceği yön
            Vector2Int direction = nextPos - headPos;

            // 3) Kafanın Sprite'ını yönüne göre güncelle
            UpdateHeadSprite(direction);

            // 4) Self-collision kontrolü (kendi gövdesi mi?)
            if (CheckSelfCollision(nextPos))
            {
                GameManager.Instance.GameOver();
                return; // Bu adımda hareket etme
            }

            // 5) Gövdeyi kaydır (son segmentten başlayarak)
            for (int i = segments.Count - 1; i > 0; i--)
            {
                segments[i].position = segments[i - 1].position;
            }

            // 6) Kafayı yeni konuma taşı
            transform.position = new Vector3(nextPos.x, nextPos.y, 0f);
        }
        else
        {
            // Yol bulunamadı
            Debug.Log("Yol bulunamadı!");
        }
    }

    /// <summary>
    /// Kafanın yönü (Vector2Int direction) doğrultusunda uygun kafa sprite'ı seçer.
    /// </summary>
    private void UpdateHeadSprite(Vector2Int direction)
    {
        if (headSpriteRenderer == null) return;

        // direction.x > 0 => sağ
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
    /// Bir sonraki kafa konumu "nextPos" gövdenin bir parçası mı?
    /// Evet ise true döner => GameOver
    /// </summary>
    private bool CheckSelfCollision(Vector2Int nextPos)
    {
        for (int i = 1; i < segments.Count; i++)
        {
            // opsiyonel: Kuyruğu hariç tutmak isterseniz "if (i == segments.Count - 1) continue;"
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
    /// Yem yendiğinde dışarıdan çağrılır -> Yılan uzar
    /// </summary>
    public void OnFoodEaten()
    {
        Grow();
    }

    /// <summary>
    /// FoodSpawner'ın yılan segmentlerini kontrol edebilmesi için.
    /// </summary>
    public List<Transform> GetSegments()
    {
        return segments;
    }

    /// <summary>
    /// Pathfinding'de yılanın gövdesini engel gibi işaretlemek için.
    /// </summary>
    private void UpdatePathfindingWithBody()
    {
        // Procedural generator
        ProceduralLevelGenerator generator = Object.FindAnyObjectByType<ProceduralLevelGenerator>();
        if (generator == null) return;

        // Mevcut engelleri al
        List<Vector2Int> baseObstacles = generator.GetObstaclePositions();

        // Yılan gövdesini de ekleyip "combinedObstacles" listesi oluştur
        List<Vector2Int> combinedObstacles = new List<Vector2Int>(baseObstacles);

        // Tüm segmentleri engel say (kuyruğu hariç tutmak istersen => i < segments.Count - 1)
        for (int i = 1; i < segments.Count; i++)
        {
            Vector2Int segPos = new Vector2Int(
                Mathf.RoundToInt(segments[i].position.x),
                Mathf.RoundToInt(segments[i].position.y)
            );
            combinedObstacles.Add(segPos);
        }

        // A* grid'ini yenile
        int mapW = generator.width;
        int mapH = generator.height;
        AStarPathfinding.Instance.RefreshGrid(mapW, mapH, combinedObstacles);
    }
}
