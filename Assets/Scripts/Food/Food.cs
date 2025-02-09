// Assets/Scripts/Food/Food.cs
using UnityEngine;

public class Food : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        SnakeAI snake = collision.GetComponent<SnakeAI>();
        if (snake != null)
        {
            // Yılan yeme değdi
            snake.OnFoodEaten();

            // Spawner'a bildir
            Object.FindAnyObjectByType<FoodSpawner>().OnFoodEaten();

            // Kendini yok et
            Destroy(gameObject);
        }
    }
}
