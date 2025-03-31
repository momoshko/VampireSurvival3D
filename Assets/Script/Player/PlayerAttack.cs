using UnityEngine;
using System.Collections; // Для корутин
using System.Collections.Generic;

public class PlayerAttack : MonoBehaviour
{
    // Префабы оружий
    public GameObject basicBulletPrefab;
    public GameObject spreadBulletPrefab;
    public GameObject laserEffectPrefab;
    public GameObject swordHolderPrefab;

    // Свойства Garlic Aura
    public GameObject garlicEffectPrefab; // Опциональный визуальный эффект для ауры
    public float garlicBaseRadius = 2.0f;
    public float garlicRadiusPerLevel = 0.5f;
    public float garlicBaseDamage = 0.5f; // Урон за тик
    public float garlicKnockbackForce = 20f; // Уменьшено с 100f до 20f для более мягкого отталкивания
    public float garlicTickRate = 0.5f; // Частота тиков урона

    // Свойства атаки
    public float attackRate = 1f;
    public int bulletDamage = 1; // Базовый урон для снарядов/хитсканов
    private float damageMultiplier = 1f;
    private float attackRateMultiplier = 1f;
    private float nextAttackTime;

    // Свойства уровня и опыта
    public int level = 1;
    public int experience = 0;
    public int expToLevelUp = 20;

    // Ссылки и состояние
    private PlayerUI playerUI;
    private GameObject swordHolder;
    private Animator swordAnimator;
    private bool hasSword = false;

    // Управление оружием
    public Dictionary<string, int> activeWeapons = new Dictionary<string, int> { { "Basic Bullet", 1 } };
    private int maxWeapons = 5;
    private Dictionary<string, int> maxWeaponLevels = new Dictionary<string, int>
    {
        { "Basic Bullet", 5 },
        { "Spread Shot", 3 },
        { "Laser", 3 },
        { "Sword", 3 },
        { "Garlic Aura", 5 }
    };

    // Состояние Garlic Aura
    private Coroutine garlicCoroutineInstance = null;
    private HashSet<Collider> enemiesInAura = new HashSet<Collider>(); // Отслеживает врагов в зоне
    private GameObject currentGarlicEffect = null; // Ссылка на визуальный эффект

    void Awake()
    {
        // Инициализация playerUI в Awake, чтобы гарантировать, что она доступна
        playerUI = GetComponent<PlayerUI>();
        if (playerUI == null)
        {
            Debug.LogError("PlayerUI component not found on the player!");
        }
    }

    void Start()
    {
        // Проверка, есть ли Garlic Aura на старте
        if (activeWeapons.ContainsKey("Garlic Aura") && garlicCoroutineInstance == null)
        {
            garlicCoroutineInstance = StartCoroutine(GarlicAuraCoroutine());
            ActivateGarlicVisualEffect(activeWeapons["Garlic Aura"]);
        }
    }

    void Update()
    {
        // Стандартный цикл атаки для снарядов/мгновенных оружий
        if (Time.time >= nextAttackTime)
        {
            bool didAttack = false;
            foreach (var weapon in activeWeapons)
            {
                // Пропускаем Garlic Aura, так как она обрабатывается корутиной
                if (weapon.Key == "Garlic Aura") continue;

                Transform nearestEnemy = FindNearestEnemy();
                if (nearestEnemy != null)
                {
                    Shoot(weapon.Key, weapon.Value, nearestEnemy);
                    didAttack = true;
                }
            }

            // Обновляем таймер только если была выполнена атака
            if (didAttack)
            {
                nextAttackTime = Time.time + Mathf.Max(attackRate * attackRateMultiplier, 0.1f);
            }
            else if (activeWeapons.Count > (activeWeapons.ContainsKey("Garlic Aura") ? 1 : 0))
            {
                nextAttackTime = Time.time + 0.1f;
            }
            else if (activeWeapons.ContainsKey("Garlic Aura") && activeWeapons.Count == 1)
            {
                nextAttackTime = Time.time + 1.0f;
            }
        }

        // Обновляем масштаб визуального эффекта Garlic Aura
        if (currentGarlicEffect != null && activeWeapons.ContainsKey("Garlic Aura"))
        {
            float currentRadius = garlicBaseRadius + (activeWeapons["Garlic Aura"] - 1) * garlicRadiusPerLevel;
            currentGarlicEffect.transform.localScale = Vector3.one * currentRadius * 2;
        }
    }

    void LateUpdate()
    {
        // Инициализация UI в LateUpdate, чтобы гарантировать, что PlayerUI.Start уже выполнился
        if (playerUI != null)
        {
            playerUI.UpdateExpBar(experience, expToLevelUp);
            playerUI.UpdateWeaponsList(activeWeapons);
        }
    }

    // Обработка атак снарядами/мгновенными ударами
    void Shoot(string weaponType, int weaponLevel, Transform nearestEnemy)
    {
        int baseDamageForWeapon = Mathf.RoundToInt(bulletDamage * damageMultiplier);

        switch (weaponType)
        {
            case "Basic Bullet":
                GameObject bullet = Instantiate(basicBulletPrefab, transform.position, Quaternion.identity);
                Rigidbody rb = bullet.GetComponent<Rigidbody>();
                if (rb == null) rb = bullet.AddComponent<Rigidbody>();
                rb.useGravity = false;
                if (bullet.GetComponent<Collider>() == null) bullet.AddComponent<SphereCollider>().isTrigger = true;

                Vector3 direction = (nearestEnemy.position - transform.position).normalized;
                rb.velocity = direction * 10f;

                Bullet bulletScript = bullet.GetComponent<Bullet>();
                if (bulletScript == null) bulletScript = bullet.AddComponent<Bullet>();
                bulletScript.damage = baseDamageForWeapon * weaponLevel;
                Destroy(bullet, 2f);
                break;

            case "Spread Shot":
                int bulletCount = Mathf.Min(3 + weaponLevel, 5);
                for (int i = -(bulletCount / 2); i <= bulletCount / 2; i++)
                {
                    GameObject spreadBullet = Instantiate(spreadBulletPrefab, transform.position, Quaternion.identity);
                    Rigidbody spreadRb = spreadBullet.GetComponent<Rigidbody>();
                    if (spreadRb == null) spreadRb = spreadBullet.AddComponent<Rigidbody>();
                    spreadRb.useGravity = false;
                    if (spreadBullet.GetComponent<Collider>() == null) spreadBullet.AddComponent<SphereCollider>().isTrigger = true;

                    Vector3 baseDirection = (nearestEnemy.position - transform.position).normalized;
                    Vector3 spreadDirection = Quaternion.Euler(0, i * 20f, 0) * baseDirection;
                    spreadRb.velocity = spreadDirection * 10f;

                    Bullet spreadBulletScript = spreadBullet.GetComponent<Bullet>();
                    if (spreadBulletScript == null) spreadBulletScript = spreadBullet.AddComponent<Bullet>();
                    spreadBulletScript.damage = Mathf.RoundToInt(baseDamageForWeapon * 0.8f);
                    Destroy(spreadBullet, 2f);
                }
                break;

            case "Laser":
                Vector3 laserDirection = (nearestEnemy.position - transform.position).normalized;
                float laserRange = 10f;
                RaycastHit[] hits = Physics.RaycastAll(transform.position, laserDirection, laserRange);

                GameObject laserEffect = Instantiate(laserEffectPrefab, transform.position, Quaternion.identity);
                LineRenderer lr = laserEffect.GetComponent<LineRenderer>();
                if (lr != null)
                {
                    lr.SetPosition(0, transform.position);
                    lr.SetPosition(1, transform.position + laserDirection * laserRange);
                }
                Destroy(laserEffect, 0.2f);

                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider.CompareTag("Enemy"))
                    {
                        EnemyHealth enemyHealth = hit.collider.GetComponent<EnemyHealth>();
                        if (enemyHealth != null)
                        {
                            int damage = Mathf.RoundToInt(baseDamageForWeapon * 1.5f) * weaponLevel;
                            Debug.Log($"Laser deals {damage} damage to {hit.collider.gameObject.name}");
                            enemyHealth.TakeDamage(damage);
                        }
                    }
                }
                break;

            case "Sword":
                if (!hasSword)
                {
                    hasSword = true;
                    swordHolder = Instantiate(swordHolderPrefab, transform);
                    swordAnimator = swordHolder.GetComponentInChildren<Animator>();
                    if (swordAnimator == null) Debug.LogError("Animator not found on SwordHolder prefab's children!");
                }

                float swordRadius = 2f + (weaponLevel - 1) * 0.5f;
                int swordDamage = baseDamageForWeapon * weaponLevel;

                if (swordAnimator != null)
                {
                    if (nearestEnemy != null)
                    {
                        Vector3 directionToEnemy = (nearestEnemy.position - transform.position).normalized;
                        float angle = Mathf.Atan2(directionToEnemy.x, directionToEnemy.z) * Mathf.Rad2Deg;
                        swordHolder.transform.rotation = Quaternion.Euler(0, angle, 0);
                    }
                    swordAnimator.Play("SwordSwing", 0, 0f);
                }

                Collider[] hitEnemies = Physics.OverlapSphere(transform.position, swordRadius);
                foreach (Collider enemy in hitEnemies)
                {
                    if (enemy.CompareTag("Enemy"))
                    {
                        EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
                        if (enemyHealth != null)
                        {
                            Debug.Log($"Sword deals {swordDamage} damage to {enemy.gameObject.name}");
                            enemyHealth.TakeDamage(swordDamage);
                        }
                    }
                }
                break;
        }
    }

    // Корутина для Garlic Aura
    IEnumerator GarlicAuraCoroutine()
    {
        Debug.Log("Garlic Aura Activated!");
        enemiesInAura.Clear();

        while (true)
        {
            if (!activeWeapons.ContainsKey("Garlic Aura"))
            {
                Debug.LogWarning("Garlic Aura coroutine running but weapon not found in active list. Stopping.");
                DeactivateGarlicVisualEffect();
                garlicCoroutineInstance = null;
                yield break;
            }

            int weaponLevel = activeWeapons["Garlic Aura"];
            float currentRadius = garlicBaseRadius + (weaponLevel - 1) * garlicRadiusPerLevel;
            int tickDamage = Mathf.RoundToInt((garlicBaseDamage * weaponLevel) * damageMultiplier);

            Collider[] collidersInRange = Physics.OverlapSphere(transform.position, currentRadius);
            HashSet<Collider> enemiesDetectedThisTick = new HashSet<Collider>();

            foreach (Collider col in collidersInRange)
            {
                if (col.CompareTag("Enemy"))
                {
                    enemiesDetectedThisTick.Add(col);

                    if (!enemiesInAura.Contains(col))
                    {
                        Rigidbody enemyRb = col.GetComponent<Rigidbody>();
                        EnemyHealth enemyHealth = col.GetComponent<EnemyHealth>();

                        if (enemyRb != null)
                        {
                            Vector3 knockbackDirection = (col.transform.position - transform.position).normalized;
                            enemyRb.AddForce(knockbackDirection * garlicKnockbackForce, ForceMode.Impulse);
                            Debug.Log($"Garlic knocked back {col.name}");

                            // Ограничиваем максимальную скорость после отталкивания
                            float maxSpeed = 10f;
                            if (enemyRb.velocity.magnitude > maxSpeed)
                            {
                                enemyRb.velocity = enemyRb.velocity.normalized * maxSpeed;
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"Enemy {col.name} has no Rigidbody for knockback.", col.gameObject);
                        }
                    }

                    EnemyHealth currentEnemyHealth = col.GetComponent<EnemyHealth>();
                    if (currentEnemyHealth != null && tickDamage > 0)
                    {
                        currentEnemyHealth.TakeDamage(tickDamage);
                    }
                }
            }

            enemiesInAura = enemiesDetectedThisTick;
            yield return new WaitForSeconds(garlicTickRate);
        }
    }

    // Активация визуального эффекта Garlic Aura
    void ActivateGarlicVisualEffect(int level)
    {
        if (garlicEffectPrefab != null)
        {
            if (currentGarlicEffect == null)
            {
                currentGarlicEffect = Instantiate(garlicEffectPrefab, transform.position, Quaternion.identity, transform);
                currentGarlicEffect.transform.localPosition = Vector3.zero;
            }
            float currentRadius = garlicBaseRadius + (level - 1) * garlicRadiusPerLevel;
            currentGarlicEffect.transform.localScale = Vector3.one * currentRadius * 2;
        }
    }

    // Деактивация визуального эффекта Garlic Aura
    void DeactivateGarlicVisualEffect()
    {
        if (currentGarlicEffect != null)
        {
            Destroy(currentGarlicEffect);
            currentGarlicEffect = null;
        }
        enemiesInAura.Clear();
    }

    Transform FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float closestDistance = Mathf.Infinity;
        Transform nearest = null;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                nearest = enemy.transform;
            }
        }
        return nearest;
    }

    public void AddExperience(int amount)
    {
        experience += amount;
        if (playerUI != null) playerUI.UpdateExpBar(experience, expToLevelUp);
        if (experience >= expToLevelUp)
        {
            LevelUp();
        }
    }

    void LevelUp()
    {
        level++;
        experience -= expToLevelUp;
        expToLevelUp = Mathf.RoundToInt(expToLevelUp * 1.5f);
        if (playerUI != null)
        {
            playerUI.UpdateExpBar(experience, expToLevelUp);
            playerUI.ShowUpgradePanel();
        }
        Debug.Log($"Level Up! Reached level {level}. Next level at {expToLevelUp} EXP.");
    }

    public void UpgradeWeapon(string weapon)
    {
        bool isNewWeapon = !activeWeapons.ContainsKey(weapon);

        if (activeWeapons.ContainsKey(weapon))
        {
            if (activeWeapons[weapon] < maxWeaponLevels[weapon])
            {
                activeWeapons[weapon]++;
                Debug.Log($"Upgraded {weapon} to level {activeWeapons[weapon]}");

                if (weapon == "Garlic Aura")
                {
                    ActivateGarlicVisualEffect(activeWeapons[weapon]);
                }
            }
            else
            {
                Debug.Log($"{weapon} is already at max level ({maxWeaponLevels[weapon]})");
            }
        }
        else if (activeWeapons.Count < maxWeapons)
        {
            if (!maxWeaponLevels.ContainsKey(weapon))
            {
                Debug.LogError($"Weapon '{weapon}' is not defined in maxWeaponLevels!");
                return;
            }

            activeWeapons.Add(weapon, 1);
            Debug.Log($"Added weapon: {weapon}. Total weapons: {activeWeapons.Count}");

            if (weapon == "Garlic Aura")
            {
                if (garlicCoroutineInstance == null)
                {
                    garlicCoroutineInstance = StartCoroutine(GarlicAuraCoroutine());
                    ActivateGarlicVisualEffect(1);
                }
                else
                {
                    ActivateGarlicVisualEffect(1);
                }
            }
            else if (weapon == "Sword" && !hasSword)
            {
                if (swordHolderPrefab != null)
                {
                    swordHolder = Instantiate(swordHolderPrefab, transform);
                    swordAnimator = swordHolder.GetComponentInChildren<Animator>();
                    hasSword = true;
                    if (swordAnimator == null) Debug.LogError("Animator not found on SwordHolder prefab's children!");
                }
                else
                {
                    Debug.LogError("SwordHolderPrefab is not assigned in the inspector!");
                }
            }
        }
        else
        {
            Debug.LogWarning($"Cannot add {weapon}. Max weapons ({maxWeapons}) reached.");
        }

        if (playerUI != null) playerUI.UpdateWeaponsList(activeWeapons);
    }

    public bool IsWeaponMaxed(string weapon)
    {
        if (!activeWeapons.ContainsKey(weapon)) return false;
        if (!maxWeaponLevels.ContainsKey(weapon)) return false;
        return activeWeapons[weapon] >= maxWeaponLevels[weapon];
    }

    public void UpgradeStat(string stat, float value)
    {
        switch (stat)
        {
            case "Damage":
                damageMultiplier += value;
                Debug.Log($"Damage multiplier increased to {damageMultiplier}");
                break;
            case "AttackRate":
                attackRateMultiplier = Mathf.Max(0.1f, attackRateMultiplier * (1f - value));
                Debug.Log($"Attack rate multiplier changed to {attackRateMultiplier}");
                break;
            default:
                Debug.LogWarning($"Unknown stat upgrade: {stat}");
                break;
        }
        if (playerUI != null) playerUI.UpdateStatsList();
    }

    public float GetDamageMultiplier() => damageMultiplier;
    public float GetAttackRateMultiplier() => attackRateMultiplier;

    void OnDestroy()
    {
        if (garlicCoroutineInstance != null)
        {
            StopCoroutine(garlicCoroutineInstance);
            garlicCoroutineInstance = null;
        }
        DeactivateGarlicVisualEffect();
    }
}