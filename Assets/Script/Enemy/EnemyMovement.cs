using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public float speed = 2f;
    private Transform player;
    private bool isPaused; // Флаг остановки
    private float pauseDuration = 0.5f; // Длительность остановки (в секундах)
    private float pauseTimer; // Таймер остановки

    void Start()
    {
        player = GameObject.Find("Player").transform;
    }

    void Update()
    {
        if (player != null)
        {
            if (isPaused)
            {
                pauseTimer -= Time.deltaTime;
                if (pauseTimer <= 0)
                {
                    isPaused = false; // Возобновляем движение
                }
            }
            else
            {
                Vector3 direction = (player.position - transform.position).normalized;
                transform.position += direction * speed * Time.deltaTime;
            }
        }
    }

    public void PauseMovement()
    {
        isPaused = true;
        pauseTimer = pauseDuration;
    }
}