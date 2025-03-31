using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    private PlayerUI playerUI;
    private Rigidbody rb;
    public float knockbackForce = 5f;
    public float enemyKnockbackForce = 3f;
    private float damageCooldown = 2f;
    private float lastDamageTime;
    public ParticleSystem damageEffect;
    private SpriteRenderer spriteRenderer;
    private new Renderer renderer;
    private MaterialPropertyBlock materialPropertyBlock;
    [Header("Flash Effect")]
    public Color flashColor = Color.red;
    public float flashDuration = 0.05f;
    public int flashCount = 2;
    [Header("Sound Settings")]
    public AudioClip damageSound;
    public float damageSoundVolume = 0.7f;
    private AudioSource audioSource;
    private float lastSoundTime;
    private float soundCooldown = 0.2f;

    void Start()
    {
        currentHealth = maxHealth;
        playerUI = GetComponent<PlayerUI>();
        rb = GetComponent<Rigidbody>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        renderer = GetComponent<Renderer>();
        if (spriteRenderer == null && renderer == null)
        {
            Debug.LogWarning("Neither SpriteRenderer nor MeshRenderer found on player!", gameObject);
        }
        else if (renderer != null)
        {
            materialPropertyBlock = new MaterialPropertyBlock();
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        lastDamageTime = -damageCooldown;
        lastSoundTime = -soundCooldown;
    }

    void OnCollisionStay(Collision collision)
    {
        // ѕровер€ем, что столкновение произошло с врагом
        if (collision.gameObject.CompareTag("Enemy") && Time.time >= lastDamageTime + damageCooldown)
        {
            // ѕровер€ем, что столкновение произошло с телом игрока, а не с мечом
            bool isCollisionWithPlayerBody = true;
            foreach (ContactPoint contact in collision.contacts)
            {
                if (contact.thisCollider.gameObject.layer == LayerMask.NameToLayer("Sword"))
                {
                    isCollisionWithPlayerBody = false;
                    break;
                }
            }

            // ≈сли столкновение с телом игрока, наносим урон
            if (isCollisionWithPlayerBody)
            {
                int damage = 5;
                currentHealth -= damage;

                if (damageEffect != null)
                {
                    Instantiate(damageEffect, transform.position, Quaternion.identity);
                }
                StartCoroutine(FlashEffect());
                if (damageSound != null && Time.time >= lastSoundTime + soundCooldown)
                {
                    float pitch = Random.Range(0.8f, 1.2f);
                    AudioManager.Instance.PlaySound(damageSound, damageSoundVolume, pitch);
                    lastSoundTime = Time.time;
                }
                else if (damageSound == null)
                {
                    Debug.LogWarning($"Cannot play damage sound for {gameObject.name}. DamageSound is missing.");
                }

                Vector3 knockbackDirection = (transform.position - collision.transform.position).normalized;
                rb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);

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
    }

    private IEnumerator FlashEffect()
    {
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            Vector3 originalPosition = transform.localPosition;

            for (int i = 0; i < flashCount; i++)
            {
                float elapsed = 0f;
                while (elapsed < flashDuration)
                {
                    elapsed += Time.deltaTime;
                    spriteRenderer.color = Color.Lerp(originalColor, flashColor, elapsed / flashDuration);
                    float shake = Mathf.Sin(elapsed / flashDuration * Mathf.PI * 2) * 0.1f;
                    transform.localPosition = originalPosition + new Vector3(shake, 0, 0);
                    yield return null;
                }

                elapsed = 0f;
                while (elapsed < flashDuration)
                {
                    elapsed += Time.deltaTime;
                    spriteRenderer.color = Color.Lerp(flashColor, originalColor, elapsed / flashDuration);
                    float shake = Mathf.Sin(elapsed / flashDuration * Mathf.PI * 2) * 0.1f;
                    transform.localPosition = originalPosition + new Vector3(shake, 0, 0);
                    yield return null;
                }
            }

            spriteRenderer.color = originalColor;
            transform.localPosition = originalPosition;
        }
        else if (renderer != null)
        {
            renderer.GetPropertyBlock(materialPropertyBlock);
            Color originalColor = renderer.material.color;
            Vector3 originalPosition = transform.localPosition;

            for (int i = 0; i < flashCount; i++)
            {
                float elapsed = 0f;
                while (elapsed < flashDuration)
                {
                    elapsed += Time.deltaTime;
                    Color newColor = Color.Lerp(originalColor, flashColor, elapsed / flashDuration);
                    materialPropertyBlock.SetColor("_Color", newColor);
                    renderer.SetPropertyBlock(materialPropertyBlock);
                    float shake = Mathf.Sin(elapsed / flashDuration * Mathf.PI * 2) * 0.1f;
                    transform.localPosition = originalPosition + new Vector3(shake, 0, 0);
                    yield return null;
                }

                elapsed = 0f;
                while (elapsed < flashDuration)
                {
                    elapsed += Time.deltaTime;
                    Color newColor = Color.Lerp(flashColor, originalColor, elapsed / flashDuration);
                    materialPropertyBlock.SetColor("_Color", newColor);
                    renderer.SetPropertyBlock(materialPropertyBlock);
                    float shake = Mathf.Sin(elapsed / flashDuration * Mathf.PI * 2) * 0.1f;
                    transform.localPosition = originalPosition + new Vector3(shake, 0, 0);
                    yield return null;
                }
            }

            materialPropertyBlock.SetColor("_Color", originalColor);
            renderer.SetPropertyBlock(materialPropertyBlock);
            transform.localPosition = originalPosition;
        }
    }

    void FixedUpdate()
    {
        if (rb.velocity.magnitude > 0.1f)
        {
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, Time.fixedDeltaTime * 10f);
        }
    }
}