// Assets/Scripts/CameraController.cs
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    private Camera cam;

    private void Start()
    {
        cam = GetComponent<Camera>();

        // LevelGenerator bulalım
        ProceduralLevelGenerator generator = FindObjectOfType<ProceduralLevelGenerator>();
        if (generator != null)
        {
            int w = generator.width;
            int h = generator.height;

            // Kamerayı merkeze al
            transform.position = new Vector3(w / 2f, h / 2f, -10f);

            // Aspect ratio'ya göre orthographic size hesapla
            // Basitçe "yüksekliğin yarısı"na ayarlayalım:
            float aspect = cam.aspect; // width / height
            float desiredHalfHeight = h / 2f;

            // Ekran en-boy oranı, alanın enine mi dikine mi uzun olduğuna göre 
            // orthographicSize'ı ayarlayabiliriz.
            // formül: orthoSize = max(alanYarısı, alanYarısı / aspect?)
            // Aslında "width / 2 / aspect" = height needed
            // Tekrar basit bir kontrol yapalım:

            // Yükseklik boyutu
            float sizeBasedOnHeight = desiredHalfHeight;
            // Genişlik boyutu (en / 2)
            float sizeBasedOnWidth = (w / 2f) / aspect;

            // Hangisi büyükse onu al (böylece tüm alanı sığdırır)
            float finalSize = Mathf.Max(sizeBasedOnHeight, sizeBasedOnWidth);
            cam.orthographicSize = finalSize;
        }
        else
        {
            // Varsayılan bir konum/size
            transform.position = new Vector3(10, 10, -10);
            cam.orthographicSize = 10f;
        }
    }
}
