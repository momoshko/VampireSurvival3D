using UnityEngine;

public class PlayerXP : MonoBehaviour
{
    public int xp = 0;
    public PlayerAttack playerAttack; // Ссылка на скрипт атаки

    void Start()
    {
        playerAttack = GetComponent<PlayerAttack>(); // Получаем компонент
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("XPOrb"))
        {
            xp += 1;
            Destroy(other.gameObject);
            Debug.Log("XP: " + xp);

            // Улучшение: каждые 5 опыта увеличиваем скорость атаки
            if (xp % 5 == 0)
            {
                playerAttack.attackRate -= 0.05f; // Уменьшаем интервал стрельбы
                Debug.Log("Attack Rate: " + playerAttack.attackRate);
            }
        }
    }
}