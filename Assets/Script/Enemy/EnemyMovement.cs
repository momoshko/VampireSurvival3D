using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public float speed = 2f;
    private Transform player;
    private bool isPaused; // ���� ���������
    private float pauseDuration = 0.5f; // ������������ ��������� (� ��������)
    private float pauseTimer; // ������ ���������

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
                    isPaused = false; // ������������ ��������
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