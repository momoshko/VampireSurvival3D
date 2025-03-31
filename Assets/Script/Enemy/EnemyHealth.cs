using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour
{
    public int health = 3;
    public GameObject expSpherePrefab;
    public ParticleSystem damageEffect;
    private Rigidbody rb;
    private SpriteRenderer spriteRenderer;
    private new Renderer renderer;
    private MaterialPropertyBlock materialPropertyBlock;
    private NavMeshAgent navMeshAgent;
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
        rb = GetComponent<Rigidbody>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        renderer = GetComponent<Renderer>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        if (spriteRenderer == null && renderer == null)
        {
            Debug.LogWarning("Neither SpriteRenderer nor MeshRenderer found on enemy!", gameObject);
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
        if (rb == null)
        {
            Debug.LogWarning("Rigidbody not found on enemy!", gameObject);
        }
        lastSoundTime = -soundCooldown;
    }

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

        if (health <= 0)
        {
            Instantiate(expSpherePrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }

    private IEnumerator FlashEffect()
    {
        bool wasNavMeshAgentEnabled = false;
        if (navMeshAgent != null && navMeshAgent.enabled)
        {
            wasNavMeshAgentEnabled = true;
            navMeshAgent.enabled = false;
        }

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

        if (wasNavMeshAgentEnabled && navMeshAgent != null)
        {
            navMeshAgent.enabled = true;
        }
    }

    void FixedUpdate()
    {
        if (rb != null && rb.velocity.magnitude > 0.1f)
        {
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, Time.fixedDeltaTime * 5f);
        }
    }
}