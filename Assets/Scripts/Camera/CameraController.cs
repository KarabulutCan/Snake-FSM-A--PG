using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    private Camera cam;

    private void Start()
    {
        cam = GetComponent<Camera>();

        // 1) Ekranı 16:9'a sabitlemek isterseniz:
        //Screen.SetResolution(1920, 1080, false);

        // 2) ProceduralLevelGenerator bul
        ProceduralLevelGenerator generator = FindObjectOfType<ProceduralLevelGenerator>();
        if (generator != null)
        {
            int w = generator.width;
            int h = generator.height;

            // Kamerayı merkez noktaya yerleştir
            transform.position = new Vector3(w / 2f, h / 2f, -10f);

            // Manuel 16:9 aspect
            float forcedAspect = 16f / 9f;

            // Haritanın tamamını göstermek için:
            // Yarı yükseklik = h/2
            float sizeBasedOnHeight = h / 2f;
            // Yarı genişliği 16:9'a göre hesaplarsak => (w/2) / forcedAspect
            float sizeBasedOnWidth = (w / 2f) / forcedAspect;

            // Hangisi büyükse, onu camera.orthographicSize yap
            float finalSize = Mathf.Max(sizeBasedOnHeight, sizeBasedOnWidth);

            cam.orthographicSize = finalSize;
        }
        else
        {
            // Yedek ayar
            transform.position = new Vector3(10, 10, -10);
            cam.orthographicSize = 10f;
        }
    }
}
