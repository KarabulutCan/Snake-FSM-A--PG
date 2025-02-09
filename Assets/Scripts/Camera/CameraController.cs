using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    private Camera cam;

    private void Start()
    {
        cam = GetComponent<Camera>();

        // ProceduralLevelGenerator bul
        ProceduralLevelGenerator generator = Object.FindAnyObjectByType<ProceduralLevelGenerator>();
        if (generator != null)
        {
            // Oyun alanının dış sınırları
            // Örn: -1..width ve -1..height arasında duvar yerleştirdiyseniz:
            int left = -1;
            int right = generator.width;  // x = width
            int bottom = -1;
            int top = generator.height;   // y = height

            // Merkez noktayı hesapla
            float centerX = (left + right) / 2f;   // (örn. -1 + 20) / 2 = 9.5
            float centerY = (bottom + top) / 2f;   // (örn. -1 + 20) / 2 = 9.5

            // Toplam genişlik / yükseklik (örn. 21×21)
            float totalWidth = (right - left);     // 20 - (-1) = 21
            float totalHeight = (top - bottom);    // 20 - (-1) = 21

            // Yarı genişlik / yükseklik
            float halfWidth = totalWidth / 2f;     // 21 / 2 = 10.5
            float halfHeight = totalHeight / 2f;   // 21 / 2 = 10.5

            // Kameranın en-boy oranı
            float aspect = cam.aspect;  // (ekran genişliği / yüksekliği)

            // Yüksekliğe göre ortographicSize
            float sizeBasedOnHeight = halfHeight;
            // Genişliğe göre ortographicSize
            float sizeBasedOnWidth = halfWidth / aspect;

            // Hangisi büyükse onu seç (tüm haritayı sığdırmak için)
            float finalSize = Mathf.Max(sizeBasedOnHeight, sizeBasedOnWidth);

            // Kamerayı merkez noktaya taşı, Z eksenini -10 tut
            transform.position = new Vector3(centerX, centerY, -10f);

            // OrthoSize'ı ayarla
            cam.orthographicSize = finalSize;
        }
        else
        {
            // Eğer generator yoksa varsayılan
            transform.position = new Vector3(10, 10, -10);
            cam.orthographicSize = 10f;
        }
    }
}
