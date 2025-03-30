using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int health = 3;
    public GameObject expSpherePrefab;
    public GameObject damageTextPrefab;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Bullet"))
        {
            Bullet bullet = other.GetComponent<Bullet>();
            if (bullet != null)
            {
                TakeDamage(bullet.damage);
                Destroy(other.gameObject);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log($"{gameObject.name} takes {damage} damage. Health remaining: {health}");

        // Отображаем урон
        if (damageTextPrefab != null)
        {
            Vector3 textPosition = transform.position + Vector3.up * 1f;
            GameObject damageTextObj = Instantiate(damageTextPrefab, textPosition, Quaternion.identity);
            DamageText damageText = damageTextObj.GetComponent<DamageText>();
            if (damageText != null)
            {
                damageText.Setup(damage, textPosition);
            }
        }

        if (health <= 0)
        {
            // Создаём сферу опыта
            Instantiate(expSpherePrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}