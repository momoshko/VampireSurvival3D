using UnityEngine;

public class Sword : MonoBehaviour
{
    [SerializeField] private int damage = 5; // ����, ������� ������� ���
    [SerializeField] private float damageCooldown = 0.5f; // ������� ����� ���������� �����
    private float lastDamageTime;

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && Time.time >= lastDamageTime + damageCooldown)
        {
            EnemyHealth enemyHealth = collision.gameObject.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
                lastDamageTime = Time.time;
            }
        }
    }
}