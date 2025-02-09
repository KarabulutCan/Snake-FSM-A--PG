using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Over Paneli")]
    public GameObject gameOverPanel;

    private void Awake()
    {
        // Singleton örneği
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void GameOver()
    {
        // Zaten bir game over olayı yaşanmışsa tekrar tetiklemeyelim
        if (gameOverPanel.activeSelf) return;

        // Oyun durdur
        Time.timeScale = 0f;
        // Paneli göster
        gameOverPanel.SetActive(true);

        Debug.Log("Game Over!");
    }
}
