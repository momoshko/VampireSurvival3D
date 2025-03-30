using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    private PlayerUI playerUI;
    private Rigidbody rb;
    public float knockbackForce = 5f;
    public float enemyKnockbackForce = 3f;
    private float damageCooldown = 1f;
    private float lastDamageTime;
    public GameObject damageTextPrefab; // ������ ���������� ������

    void Start()
    {
        currentHealth = maxHealth;
        playerUI = GetComponent<PlayerUI>(); // ���������� ��������� PlayerUI
        rb = GetComponent<Rigidbody>();
        lastDamageTime = -damageCooldown;
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && Time.time >= lastDamageTime + damageCooldown)
        {
            int damage = 10; // ���� �� �����
            currentHealth -= damage;

            // ���������� ����
            if (damageTextPrefab != null)
            {
                Vector3 textPosition = transform.position + Vector3.up * 1f; // ���� ���� ������
                GameObject damageTextObj = Instantiate(damageTextPrefab, textPosition, Quaternion.identity);
                DamageText damageText = damageTextObj.GetComponent<DamageText>();
                if (damageText != null)
                {
                    damageText.Setup(damage, textPosition);
                }
            }

            // ������������ ������
            Vector3 knockbackDirection = (transform.position - collision.transform.position).normalized;
            rb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);

            // ������������ �����
            Rigidbody enemyRb = collision.gameObject.GetComponent<Rigidbody>();
            if (enemyRb != null)
            {
                enemyRb.AddForce(-knockbackDirection * enemyKnockbackForce, ForceMode.Impulse);
            }

            lastDamageTime = Time.time;

            if (currentHealth <= 0)
            {
                playerUI.ShowGameOver();
                Destroy(gameObject);
            }
        }
    }

    void FixedUpdate()
    {
        // ��������� �������� ����� ������
        if (rb.velocity.magnitude > 0.1f)
        {
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, Time.fixedDeltaTime * 10f);
        }
    }
}