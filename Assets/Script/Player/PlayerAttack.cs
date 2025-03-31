using UnityEngine;
using System.Collections; // NEW: Needed for Coroutines
using System.Collections.Generic;

public class PlayerAttack : MonoBehaviour
{
    // Existing weapon prefabs
    public GameObject basicBulletPrefab;
    public GameObject spreadBulletPrefab;
    public GameObject laserEffectPrefab;
    public GameObject swordHolderPrefab;

    // NEW: Garlic Aura properties
    public GameObject garlicEffectPrefab; // Optional: Visual effect for the aura
    public float garlicBaseRadius = 2.0f;
    public float garlicRadiusPerLevel = 0.5f;
    public float garlicBaseDamage = 0.5f; // Damage per tick
    public float garlicKnockbackForce = 100f;
    public float garlicTickRate = 0.5f; // How often the aura damages/checks

    // Existing attack properties
    public float attackRate = 1f;
    public int bulletDamage = 1; // Base damage for projectile/hitscan weapons
    private float damageMultiplier = 1f;
    private float attackRateMultiplier = 1f;
    private float nextAttackTime;

    // Existing level/exp properties
    public int level = 1;
    public int experience = 0;
    public int expToLevelUp = 20;

    // Existing references and state
    private PlayerUI playerUI;
    private GameObject swordHolder;
    private Animator swordAnimator;
    private bool hasSword = false;

    // Weapon management
    public Dictionary<string, int> activeWeapons = new Dictionary<string, int> { { "Basic Bullet", 1 } };
    private int maxWeapons = 5;
    private Dictionary<string, int> maxWeaponLevels = new Dictionary<string, int>
    {
        { "Basic Bullet", 5 },
        { "Spread Shot", 3 },
        { "Laser", 3 },
        { "Sword", 3 },
        { "Garlic Aura", 5 } // NEW: Max level for Garlic Aura
    };

    // NEW: Garlic Aura state variables
    private Coroutine garlicCoroutineInstance = null;
    private HashSet<Collider> enemiesInAura = new HashSet<Collider>(); // Tracks enemies currently inside
    private GameObject currentGarlicEffect = null; // Reference to the instantiated visual effect

    void Start()
    {
        playerUI = GetComponent<PlayerUI>();
        if (playerUI == null)
        {
            Debug.LogError("PlayerUI component not found on the player!");
        }
        else
        {
            playerUI.UpdateExpBar(experience, expToLevelUp);
            playerUI.UpdateWeaponsList(activeWeapons);
        }

        // NEW: Check if Garlic Aura is already active at the start (e.g., from saved state later)
        if (activeWeapons.ContainsKey("Garlic Aura") && garlicCoroutineInstance == null)
        {
            garlicCoroutineInstance = StartCoroutine(GarlicAuraCoroutine());
            ActivateGarlicVisualEffect(activeWeapons["Garlic Aura"]); // Activate visual effect
        }
    }

    void Update()
    {
        // Standard attack loop for projectile/instant weapons
        if (Time.time >= nextAttackTime)
        {
            bool didAttack = false; // MODIFIED: Track if any standard attack happened
            foreach (var weapon in activeWeapons)
            {
                // MODIFIED: Skip Garlic Aura in the standard attack loop
                if (weapon.Key == "Garlic Aura") continue;

                Transform nearestEnemy = FindNearestEnemy();
                if (nearestEnemy != null)
                {
                    Shoot(weapon.Key, weapon.Value, nearestEnemy);
                    didAttack = true; // Mark that an attack occurred
                }
            }

            // MODIFIED: Only reset timer if a standard attack was attempted/performed
            if (didAttack)
            {
                nextAttackTime = Time.time + Mathf.Max(attackRate * attackRateMultiplier, 0.1f);
            }
            // If no standard attack weapons are equipped or no enemies found,
            // prevent spamming FindNearestEnemy by still adding a small delay.
            else if (activeWeapons.Count > (activeWeapons.ContainsKey("Garlic Aura") ? 1 : 0))
            {
                // Only add delay if other weapons exist but couldn't fire (e.g., no target)
                nextAttackTime = Time.time + 0.1f; // Small delay
            }
            // If only Garlic Aura exists, no need to run this part of Update frequently
            else if (activeWeapons.ContainsKey("Garlic Aura") && activeWeapons.Count == 1)
            {
                nextAttackTime = Time.time + 1.0f; // Check less often if only aura exists
            }
        }

        // NEW: Update Garlic visual effect radius if it exists and is active
        if (currentGarlicEffect != null && activeWeapons.ContainsKey("Garlic Aura"))
        {
            float currentRadius = garlicBaseRadius + (activeWeapons["Garlic Aura"] - 1) * garlicRadiusPerLevel;
            // Assuming the visual effect is scaled uniformly
            currentGarlicEffect.transform.localScale = Vector3.one * currentRadius * 2; // Scale diameter
        }
    }

    // Handles projectile/instant attacks
    void Shoot(string weaponType, int weaponLevel, Transform nearestEnemy)
    {
        // MODIFIED: Calculate base damage here, specific weapons might modify it further
        int baseDamageForWeapon = Mathf.RoundToInt(bulletDamage * damageMultiplier);

        switch (weaponType)
        {
            case "Basic Bullet":
                GameObject bullet = Instantiate(basicBulletPrefab, transform.position, Quaternion.identity);
                // Ensure bullet has Rigidbody and Collider
                Rigidbody rb = bullet.GetComponent<Rigidbody>();
                if (rb == null) rb = bullet.AddComponent<Rigidbody>();
                rb.useGravity = false;
                // Ensure bullet has Collider
                if (bullet.GetComponent<Collider>() == null) bullet.AddComponent<SphereCollider>().isTrigger = true; // Example collider

                Vector3 direction = (nearestEnemy.position - transform.position).normalized;
                rb.velocity = direction * 10f;

                Bullet bulletScript = bullet.GetComponent<Bullet>();
                if (bulletScript == null) bulletScript = bullet.AddComponent<Bullet>();
                bulletScript.damage = baseDamageForWeapon * weaponLevel; // Scale damage by level
                Destroy(bullet, 2f);
                break;

            case "Spread Shot":
                int bulletCount = Mathf.Min(3 + weaponLevel, 5); // Example: more bullets per level
                for (int i = -(bulletCount / 2); i <= bulletCount / 2; i++)
                {
                    GameObject spreadBullet = Instantiate(spreadBulletPrefab, transform.position, Quaternion.identity);
                    // Ensure bullet has Rigidbody and Collider
                    Rigidbody spreadRb = spreadBullet.GetComponent<Rigidbody>();
                    if (spreadRb == null) spreadRb = spreadBullet.AddComponent<Rigidbody>();
                    spreadRb.useGravity = false;
                    // Ensure bullet has Collider
                    if (spreadBullet.GetComponent<Collider>() == null) spreadBullet.AddComponent<SphereCollider>().isTrigger = true; // Example collider

                    Vector3 baseDirection = (nearestEnemy.position - transform.position).normalized;
                    Vector3 spreadDirection = Quaternion.Euler(0, i * 20f, 0) * baseDirection; // Spread angle
                    spreadRb.velocity = spreadDirection * 10f;

                    Bullet spreadBulletScript = spreadBullet.GetComponent<Bullet>();
                    if (spreadBulletScript == null) spreadBulletScript = spreadBullet.AddComponent<Bullet>();
                    // Spread shot might have slightly less damage per bullet? Adjust as needed.
                    spreadBulletScript.damage = Mathf.RoundToInt(baseDamageForWeapon * 0.8f); // Example: slightly less damage
                    Destroy(spreadBullet, 2f);
                }
                break;

            case "Laser":
                // Laser logic remains the same, using baseDamageForWeapon
                Vector3 laserDirection = (nearestEnemy.position - transform.position).normalized;
                float laserRange = 10f; // Define laser range
                RaycastHit[] hits = Physics.RaycastAll(transform.position, laserDirection, laserRange);

                // Instantiate visual effect first
                GameObject laserEffect = Instantiate(laserEffectPrefab, transform.position, Quaternion.identity);
                LineRenderer lr = laserEffect.GetComponent<LineRenderer>();
                if (lr != null)
                {
                    lr.SetPosition(0, transform.position);
                    lr.SetPosition(1, transform.position + laserDirection * laserRange); // Assume max range initially
                }
                Destroy(laserEffect, 0.2f); // Short duration for the visual

                // Apply damage
                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider.CompareTag("Enemy"))
                    {
                        EnemyHealth enemyHealth = hit.collider.GetComponent<EnemyHealth>();
                        if (enemyHealth != null)
                        {
                            // Laser damage might scale differently, e.g., more base damage
                            int damage = Mathf.RoundToInt(baseDamageForWeapon * 1.5f) * weaponLevel;
                            Debug.Log($"Laser deals {damage} damage to {hit.collider.gameObject.name}");
                            enemyHealth.TakeDamage(damage);

                            // Optional: Adjust laser visual endpoint to the first enemy hit
                            // if (lr != null) {
                            //     lr.SetPosition(1, hit.point);
                            // }
                        }
                    }
                }
                break;

            case "Sword":
                // Sword logic remains similar, using baseDamageForWeapon
                if (!hasSword)
                {
                    hasSword = true;
                    swordHolder = Instantiate(swordHolderPrefab, transform); // Parent to player
                    swordAnimator = swordHolder.GetComponentInChildren<Animator>();
                    if (swordAnimator == null) Debug.LogError("Animator not found on SwordHolder prefab's children!");
                }

                // Calculate sword properties based on level
                float swordRadius = 2f + (weaponLevel - 1) * 0.5f; // Radius increases with level
                int swordDamage = baseDamageForWeapon * weaponLevel; // Damage increases with level

                // Perform attack animation
                if (swordAnimator != null)
                {
                    // Point towards nearest enemy before swinging
                    if (nearestEnemy != null)
                    {
                        Vector3 directionToEnemy = (nearestEnemy.position - transform.position).normalized;
                        // Set Y rotation only
                        float angle = Mathf.Atan2(directionToEnemy.x, directionToEnemy.z) * Mathf.Rad2Deg;
                        swordHolder.transform.rotation = Quaternion.Euler(0, angle, 0);
                    }
                    swordAnimator.Play("SwordSwing", 0, 0f); // Trigger animation
                }

                // Apply damage in an area after a short delay (sync with animation if possible)
                // For simplicity, we apply damage instantly here using OverlapSphere
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

                // NEW: Garlic Aura is handled by its coroutine, so no case needed here.
        }
    }

    // NEW: Coroutine for Garlic Aura effect
    IEnumerator GarlicAuraCoroutine()
    {
        Debug.Log("Garlic Aura Activated!");
        enemiesInAura.Clear(); // Clear set on start/restart

        while (true) // Runs as long as the coroutine is active
        {
            if (!activeWeapons.ContainsKey("Garlic Aura"))
            {
                // Safety check: Stop if weapon was somehow removed (though current logic doesn't do this)
                Debug.LogWarning("Garlic Aura coroutine running but weapon not found in active list. Stopping.");
                DeactivateGarlicVisualEffect(); // Turn off visual
                garlicCoroutineInstance = null;
                yield break; // Exit the coroutine
            }

            int weaponLevel = activeWeapons["Garlic Aura"];
            float currentRadius = garlicBaseRadius + (weaponLevel - 1) * garlicRadiusPerLevel;
            int tickDamage = Mathf.RoundToInt((garlicBaseDamage * weaponLevel) * damageMultiplier); // Damage per tick, scaled

            // Find all colliders within the aura radius
            Collider[] collidersInRange = Physics.OverlapSphere(transform.position, currentRadius);
            HashSet<Collider> enemiesDetectedThisTick = new HashSet<Collider>(); // Track enemies found in *this* specific check

            foreach (Collider col in collidersInRange)
            {
                if (col.CompareTag("Enemy")) // Make sure your enemies have the "Enemy" tag
                {
                    enemiesDetectedThisTick.Add(col); // Add to current tick's findings

                    // Apply knockback only if the enemy was NOT in the aura during the *previous* tick
                    if (!enemiesInAura.Contains(col))
                    {
                        Rigidbody enemyRb = col.GetComponent<Rigidbody>();
                        EnemyHealth enemyHealth = col.GetComponent<EnemyHealth>(); // Also get health component

                        if (enemyRb != null)
                        {
                            Vector3 knockbackDirection = (col.transform.position - transform.position).normalized;
                            // Apply force immediately
                            enemyRb.AddForce(knockbackDirection * garlicKnockbackForce, ForceMode.Impulse);
                            Debug.Log($"Garlic knocked back {col.name}");
                        }
                        else
                        {
                            Debug.LogWarning($"Enemy {col.name} has no Rigidbody for knockback.", col.gameObject);
                        }

                        // Apply damage immediately upon entry as well? Optional, or wait for tick. Let's apply on tick.
                        // if (enemyHealth != null && tickDamage > 0) {
                        //     enemyHealth.TakeDamage(tickDamage);
                        // }
                    }

                    // Apply periodic damage to all enemies currently detected inside
                    EnemyHealth currentEnemyHealth = col.GetComponent<EnemyHealth>();
                    if (currentEnemyHealth != null && tickDamage > 0)
                    {
                        currentEnemyHealth.TakeDamage(tickDamage);
                        // Debug.Log($"Garlic dealt {tickDamage} damage to {col.name}"); // Can be spammy
                    }
                }
            }

            // Update the set of enemies currently in the aura for the next tick's knockback check
            enemiesInAura = enemiesDetectedThisTick;

            // Wait for the specified tick rate before the next check
            yield return new WaitForSeconds(garlicTickRate);
        }
    }

    // NEW: Helper to activate/create the visual effect
    void ActivateGarlicVisualEffect(int level)
    {
        if (garlicEffectPrefab != null)
        {
            if (currentGarlicEffect == null)
            {
                currentGarlicEffect = Instantiate(garlicEffectPrefab, transform.position, Quaternion.identity, transform); // Parent to player
                currentGarlicEffect.transform.localPosition = Vector3.zero; // Center on player
            }
            // Update scale based on current level
            float currentRadius = garlicBaseRadius + (level - 1) * garlicRadiusPerLevel;
            currentGarlicEffect.transform.localScale = Vector3.one * currentRadius * 2; // Scale represents diameter
        }
    }

    // NEW: Helper to deactivate the visual effect
    void DeactivateGarlicVisualEffect()
    {
        if (currentGarlicEffect != null)
        {
            Destroy(currentGarlicEffect);
            currentGarlicEffect = null;
        }
        enemiesInAura.Clear(); // Clear tracked enemies when effect stops
    }


    Transform FindNearestEnemy()
    {
        // Find nearest enemy logic (remains the same)
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float closestDistance = Mathf.Infinity;
        Transform nearest = null;

        foreach (GameObject enemy in enemies)
        {
            // Optional: Check if enemy is active/valid if needed
            // if (!enemy.activeInHierarchy) continue;

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
        // Add experience logic (remains the same)
        experience += amount;
        if (playerUI != null) playerUI.UpdateExpBar(experience, expToLevelUp);
        if (experience >= expToLevelUp)
        {
            LevelUp();
        }
    }

    void LevelUp()
    {
        // Level up logic (remains the same)
        level++;
        experience -= expToLevelUp; // MODIFIED: Carry over extra experience
        expToLevelUp = Mathf.RoundToInt(expToLevelUp * 1.5f); // MODIFIED: Increase EXP requirement more significantly
        if (playerUI != null)
        {
            playerUI.UpdateExpBar(experience, expToLevelUp);
            playerUI.ShowUpgradePanel(); // Assumes PlayerUI handles showing options
        }
        Debug.Log($"Level Up! Reached level {level}. Next level at {expToLevelUp} EXP.");
    }

    // MODIFIED: Update UpgradeWeapon to handle Garlic Aura and start its coroutine
    public void UpgradeWeapon(string weapon)
    {
        bool isNewWeapon = !activeWeapons.ContainsKey(weapon);

        if (activeWeapons.ContainsKey(weapon))
        {
            // Upgrade existing weapon
            if (activeWeapons[weapon] < maxWeaponLevels[weapon])
            {
                activeWeapons[weapon]++;
                Debug.Log($"Upgraded {weapon} to level {activeWeapons[weapon]}");

                // NEW: Update Garlic visual if it's the upgraded weapon
                if (weapon == "Garlic Aura")
                {
                    ActivateGarlicVisualEffect(activeWeapons[weapon]); // Ensure visual is active and update scale
                }
            }
            else
            {
                Debug.Log($"{weapon} is already at max level ({maxWeaponLevels[weapon]})");
            }
        }
        else if (activeWeapons.Count < maxWeapons)
        {
            // Add new weapon if slot available
            if (!maxWeaponLevels.ContainsKey(weapon))
            {
                Debug.LogError($"Weapon '{weapon}' is not defined in maxWeaponLevels!");
                return;
            }

            activeWeapons.Add(weapon, 1);
            Debug.Log($"Added weapon: {weapon}. Total weapons: {activeWeapons.Count}");

            // NEW: Special handling for adding Garlic Aura
            if (weapon == "Garlic Aura")
            {
                if (garlicCoroutineInstance == null)
                {
                    garlicCoroutineInstance = StartCoroutine(GarlicAuraCoroutine());
                    ActivateGarlicVisualEffect(1); // Activate visual effect at level 1
                }
                else
                {
                    // Coroutine already running, perhaps just ensure visual is correct?
                    ActivateGarlicVisualEffect(1);
                }
            }
            // NEW: Special handling for adding Sword (ensure instance exists)
            else if (weapon == "Sword" && !hasSword)
            {
                // Instantiate the sword holder immediately when the weapon is first acquired
                // The Shoot method will handle the rest if `hasSword` is true.
                // Note: This assumes the prefab is assigned.
                if (swordHolderPrefab != null)
                {
                    swordHolder = Instantiate(swordHolderPrefab, transform);
                    swordAnimator = swordHolder.GetComponentInChildren<Animator>();
                    hasSword = true; // Mark sword as acquired
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

        if (playerUI != null) playerUI.UpdateWeaponsList(activeWeapons); // Update UI
    }


    public bool IsWeaponMaxed(string weapon)
    {
        // Check if weapon is maxed (remains the same)
        if (!activeWeapons.ContainsKey(weapon)) return false; // Can't be maxed if not active
        if (!maxWeaponLevels.ContainsKey(weapon)) return false; // Unknown weapon definition
        return activeWeapons[weapon] >= maxWeaponLevels[weapon];
    }

    public void UpgradeStat(string stat, float value)
    {
        // Upgrade player stats (remains the same)
        switch (stat)
        {
            case "Damage":
                damageMultiplier += value;
                Debug.Log($"Damage multiplier increased to {damageMultiplier}");
                break;
            case "AttackRate":
                // Ensure multiplier doesn't go below a minimum threshold (e.g., 10% of original rate)
                attackRateMultiplier = Mathf.Max(0.1f, attackRateMultiplier * (1f - value));
                Debug.Log($"Attack rate multiplier changed to {attackRateMultiplier}");
                break;
            // NEW: Add cases for other potential stats like Radius, Duration, Speed, Health etc.
            // case "AuraRadius":
            //    garlicBaseRadius += value; // Example: Increase base radius stat
            //    Debug.Log($"Garlic base radius increased to {garlicBaseRadius}");
            //    break;
            default:
                Debug.LogWarning($"Unknown stat upgrade: {stat}");
                break;
        }
        if (playerUI != null) playerUI.UpdateStatsList(); // Assumes PlayerUI has this method
    }

    // Getters (remain the same)
    public float GetDamageMultiplier() => damageMultiplier;
    public float GetAttackRateMultiplier() => attackRateMultiplier;

    // NEW: Ensure coroutine stops if the object is destroyed
    void OnDestroy()
    {
        if (garlicCoroutineInstance != null)
        {
            StopCoroutine(garlicCoroutineInstance);
            garlicCoroutineInstance = null;
        }
        DeactivateGarlicVisualEffect(); // Clean up visual effect
    }
}
//```

//**Необходимые шаги после добавления кода:**

//1.  * *Добавьте тэг "Enemy":**Убедитесь, что у ваших префабов врагов установлен тэг "Enemy" в инспекторе. Скрипт использует `CompareTag("Enemy")` для их идентификации.
//2.  **Добавьте Rigidbody врагам:**Чтобы отталкивание(`AddForce`) работало, у ваших врагов должен быть компонент `Rigidbody`. Убедитесь, что у него **отключена** гравитация (`Use Gravity = false`), если это не требуется по геймплею, и настройте массу/сопротивление (`Drag`, `Angular Drag`) по необходимости. Возможно, потребуется включить `Is Kinematic` для врагов, если вы управляете их движением через `transform.position` или `NavMeshAgent`, и тогда отталкивание через `AddForce` не сработает напрямую. В этом случае логику отталкивания нужно будет реализовать иначе (например, временно перемещая `transform` врага). *Если вы используете `NavMeshAgent`, отталкивание будет сложнее реализовать корректно.* Давайте пока предположим, что вы используете `Rigidbody` для движения или можете его добавить.
//3.  **Добавьте Collider врагам:**У врагов должен быть компонент `Collider` (например, `SphereCollider`, `CapsuleCollider` или `BoxCollider`), чтобы `Physics.OverlapSphere` мог их обнаружить.
//4.  **Создайте префаб для эффекта (Опционально):**
//    *Создайте новый пустой GameObject.
//    * Добавьте к нему дочерний объект-Сферу (`Create -> 3D Object -> Sphere`).
//    * Удалите `SphereCollider` у этой сферы.
//    * Создайте новый материал (например, "GarlicAuraMaterial"). Сделайте его полупрозрачным (Rendering Mode: Transparent, настройте цвет и Alpha). Назначьте этот материал сфере.
//    * Сохраните родительский пустой GameObject как префаб (перетащите из иерархии в папку Project).
//    * Перетащите этот префаб в поле `Garlic Effect Prefab` в инспекторе вашего игрока. Скрипт будет масштабировать этот префаб для визуализации радиуса.
//5.  **Настройте параметры в Инспекторе:**Выберите объект игрока и в компоненте `PlayerAttack` настройте новые поля: `Garlic Base Radius`, `Garlic Radius Per Level`, `Garlic Base Damage`, `Garlic Knockback Force`, `Garlic Tick Rate`. Поле `Garlic Effect Prefab` необязательно.
//6.  **Добавьте "Garlic Aura" в систему улучшений:**Убедитесь, что ваша система улучшений (которая вызывается в `LevelUp()` и показывает `UpgradePanel`) теперь может предлагать "Garlic Aura" как вариант для добавления или улучшения.

//Теперь, когда игрок выберет улучшение "Garlic Aura", вокруг него появится постоянно действующая зона, наносящая урон и отталкивающая врагов при первом вхо