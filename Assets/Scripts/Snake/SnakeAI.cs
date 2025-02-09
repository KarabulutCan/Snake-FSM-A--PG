// Assets/Scripts/Snake/SnakeAI.cs
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FiniteStateMachine))]
public class SnakeAI : MonoBehaviour
{
    [Header("Snake Ayarları")]
    public GameObject segmentPrefab;
    public float stepTime = 0.2f;   // Her adımda bekleme süresi
    public int initialSize = 3;     // Başlangıç gövde uzunluğu

    private List<Transform> segments = new List<Transform>();
    private FiniteStateMachine fsm;
    private float stepTimer;
    private Vector2Int foodGridPos; // Yem hedefi

    private void Start()
    {
        fsm = GetComponent<FiniteStateMachine>();
        ResetState();

        // İlk state: Idle
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

    // Yılanı sıfırla
    public void ResetState()
    {
        // Eski segmentleri sil
        foreach (Transform seg in segments)
        {
            if (seg != this.transform)
            {
                Destroy(seg.gameObject);
            }
        }
        segments.Clear();

        // Listenin ilk elemanı -> Kafa (bu obje)
        segments.Add(this.transform);
        transform.position = Vector3.zero;

        // Başlangıç uzunluğu
        for (int i = 1; i < initialSize; i++)
        {
            Grow();
        }
    }

    // Yılanı uzat (yem yendiğinde çağrılır)
    public void Grow()
    {
        Transform newSegment = Instantiate(segmentPrefab).transform;
        newSegment.position = segments[segments.Count - 1].position;
        segments.Add(newSegment);
    }

    // FoodSpawner çağıracak
    public void SetFoodPosition(Vector2Int foodPos)
    {
        foodGridPos = foodPos;
    }

    // IdleState içinde çağrılır
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

    // Her stepTime'da bir grid adımı
    private void MoveOneStep()
    {
        // 1) Yılanın gövde pozisyonlarını A* engel listesine ekle
        UpdatePathfindingWithBody();

        // 2) Şimdi path'i bul
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

            // (Opsiyonel) Self-collision kontrolü
            if (CheckSelfCollision(nextPos))
            {
                GameManager.Instance.GameOver();
                return;
            }

            // Gövdeyi kaydır
            for (int i = segments.Count - 1; i > 0; i--)
            {
                segments[i].position = segments[i - 1].position;
            }

            // Kafayı taşı
            transform.position = new Vector3(nextPos.x, nextPos.y, 0f);
        }
        else
        {
            // Yol bulunamadı logu vs.
            Debug.Log("Yol bulunamadı!");
        }
    }


    private bool CheckSelfCollision(Vector2Int nextPos)
    {
        for (int i = 1; i < segments.Count; i++) // i=0 kafanın kendisi
        {
            // Opsiyonel: TAIL'İ HARİÇ TUTMAK isterseniz 
            // if (i == segments.Count - 1) continue; 

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

    public void OnFoodEaten()
    {
        Grow();
    }

    public List<Transform> GetSegments()
    {
        return segments;
    }

    private void UpdatePathfindingWithBody()
    {
        // 1) Öncе procedural generator'dan mevcut engelleri alalım
        ProceduralLevelGenerator generator = Object.FindAnyObjectByType<ProceduralLevelGenerator>();
        if (generator == null) return;

        List<Vector2Int> baseObstacles = generator.GetObstaclePositions();
        // baseObstacles: Rastgele engeller + boundary duvarlarının grid pozisyonları

        // 2) Yeni bir liste oluşturup, yılan gövdesi pozisyonlarını da ekleyeceğiz
        List<Vector2Int> combinedObstacles = new List<Vector2Int>(baseObstacles);

        // 3) Gövde segmentlerini ekle
        //    İsterseniz kuyruk (son segment) hariç tutabilirsiniz,
        //    çünkü klasik "Snake"te kuyruk hareket ettiği anda o hücre boşalır.
        //    Ama basit tutmak için her segmenti engel sayabiliriz.
        for (int i = 1; i < segments.Count; i++) // i=0 -> kafa
        {
            Vector2Int segPos = new Vector2Int(
                Mathf.RoundToInt(segments[i].position.x),
                Mathf.RoundToInt(segments[i].position.y)
            );
            combinedObstacles.Add(segPos);
        }

        // 4) Şimdi AStarPathfinding'e bu yenilenmiş listeyi gönderiyoruz.
        //    "width, height" değerleri, buradaki SnakeAI'de de saklayabilirdiniz
        //    veya generator.width / generator.height'ı çekebilirsiniz.
        int mapW = generator.width;   // ya da public alandan
        int mapH = generator.height;  // ya da public alandan

        AStarPathfinding.Instance.RefreshGrid(mapW, mapH, combinedObstacles);
    }

}
