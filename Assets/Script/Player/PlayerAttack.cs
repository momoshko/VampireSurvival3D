using UnityEngine;
using System.Collections.Generic;

public class PlayerAttack : MonoBehaviour
{
    public GameObject basicBulletPrefab;
    public GameObject spreadBulletPrefab;
    public GameObject laserEffectPrefab;
    public GameObject swordHolderPrefab;
    public float attackRate = 1f;
    public int bulletDamage = 1;
    private float damageMultiplier = 1f;
    private float attackRateMultiplier = 1f;
    private float nextAttackTime;
    public int level = 1;
    public int experience = 0;
    public int expToLevelUp = 20;
    private PlayerUI playerUI;
    private GameObject swordHolder;
    private Animator swordAnimator;
    private bool hasSword = false;

    public Dictionary<string, int> activeWeapons = new Dictionary<string, int> { { "Basic Bullet", 1 } };
    private int maxWeapons = 5;
    private Dictionary<string, int> maxWeaponLevels = new Dictionary<string, int>
    {
        { "Basic Bullet", 5 },
        { "Spread Shot", 3 },
        { "Laser", 3 },
        { "Sword", 3 }
    };

    void Start()
    {
        playerUI = GetComponent<PlayerUI>();
        playerUI.UpdateExpBar(experience, expToLevelUp);
        playerUI.UpdateWeaponsList(activeWeapons);
    }

    void Update()
    {
        if (Time.time >= nextAttackTime)
        {
            foreach (var weapon in activeWeapons)
            {
                Transform nearestEnemy = FindNearestEnemy();
                if (nearestEnemy != null)
                {
                    Shoot(weapon.Key, weapon.Value, nearestEnemy);
                }
            }
            nextAttackTime = Time.time + Mathf.Max(attackRate * attackRateMultiplier, 0.1f);
        }
    }

    void Shoot(string weaponType, int weaponLevel, Transform nearestEnemy)
    {
        int modifiedDamage = Mathf.RoundToInt(bulletDamage * damageMultiplier);

        switch (weaponType)
        {
            case "Basic Bullet":
                GameObject bullet = Instantiate(basicBulletPrefab, transform.position, Quaternion.identity);
                Rigidbody rb = bullet.AddComponent<Rigidbody>();
                rb.useGravity = false;
                Vector3 direction = (nearestEnemy.position - transform.position).normalized;
                rb.velocity = direction * 10f;
                Bullet bulletScript = bullet.AddComponent<Bullet>();
                bulletScript.damage = modifiedDamage * weaponLevel;
                Destroy(bullet, 2f);
                break;
            case "Spread Shot":
                int bulletCount = Mathf.Min(3 + weaponLevel, 5);
                for (int i = -(bulletCount / 2); i <= bulletCount / 2; i++)
                {
                    GameObject spreadBullet = Instantiate(spreadBulletPrefab, transform.position, Quaternion.identity);
                    Rigidbody spreadRb = spreadBullet.AddComponent<Rigidbody>();
                    spreadRb.useGravity = false;
                    Vector3 spreadDirection = Quaternion.Euler(0, i * 20f, 0) * (nearestEnemy.position - transform.position).normalized;
                    spreadRb.velocity = spreadDirection * 10f;
                    Bullet spreadBulletScript = spreadBullet.AddComponent<Bullet>();
                    spreadBulletScript.damage = modifiedDamage;
                    Destroy(spreadBullet, 2f);
                }
                break;
            case "Laser":
                Vector3 laserDirection = (nearestEnemy.position - transform.position).normalized;
                float laserRange = 10f;
                RaycastHit[] hits = Physics.RaycastAll(transform.position, laserDirection, laserRange);
                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider.CompareTag("Enemy"))
                    {
                        EnemyHealth enemyHealth = hit.collider.GetComponent<EnemyHealth>();
                        if (enemyHealth != null)
                        {
                            int damage = modifiedDamage * (weaponLevel + 1);
                            Debug.Log($"Laser deals {damage} damage to {hit.collider.gameObject.name}");
                            enemyHealth.TakeDamage(damage);
                        }
                    }
                }
                GameObject laserEffect = Instantiate(laserEffectPrefab, transform.position, Quaternion.identity);
                LineRenderer lr = laserEffect.GetComponent<LineRenderer>();
                lr.SetPosition(0, transform.position);
                lr.SetPosition(1, transform.position + laserDirection * laserRange);
                Destroy(laserEffect, 0.2f);
                break;
            case "Sword":
                if (!hasSword)
                {
                    hasSword = true;
                    swordHolder = Instantiate(swordHolderPrefab, transform);
                    swordAnimator = swordHolder.GetComponentInChildren<Animator>();
                    if (swordAnimator == null)
                    {
                        Debug.LogError("Animator not found on SwordHolder!");
                    }
                }
                float swordRadius = 2f + weaponLevel * 0.5f;
                Collider[] hitEnemies = Physics.OverlapSphere(transform.position, swordRadius);
                foreach (Collider enemy in hitEnemies)
                {
                    if (enemy.CompareTag("Enemy"))
                    {
                        EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
                        if (enemyHealth != null)
                        {
                            int damage = modifiedDamage * weaponLevel;
                            Debug.Log($"Sword deals {damage} damage to {enemy.gameObject.name}");
                            enemyHealth.TakeDamage(damage);
                        }
                    }
                }
                if (nearestEnemy != null)
                {
                    Vector3 directionToEnemy = (nearestEnemy.position - transform.position).normalized;
                    float angle = Mathf.Atan2(directionToEnemy.x, directionToEnemy.z) * Mathf.Rad2Deg;
                    swordHolder.transform.rotation = Quaternion.Euler(0, angle, 0);
                }
                if (swordAnimator != null)
                {
                    swordAnimator.Play("SwordSwing", 0, 0f);
                }
                break;
        }
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
        playerUI.UpdateExpBar(experience, expToLevelUp);
        if (experience >= expToLevelUp)
        {
            LevelUp();
        }
    }

    void LevelUp()
    {
        level++;
        experience = 0;
        expToLevelUp += 20;
        playerUI.UpdateExpBar(experience, expToLevelUp);
        playerUI.ShowUpgradePanel();
    }

    public void UpgradeWeapon(string weapon)
    {
        if (activeWeapons.ContainsKey(weapon))
        {
            if (activeWeapons[weapon] < maxWeaponLevels[weapon])
            {
                activeWeapons[weapon]++;
                Debug.Log($"Upgraded {weapon} to level {activeWeapons[weapon]}");
            }
            else
            {
                Debug.Log($"{weapon} is already at max level ({maxWeaponLevels[weapon]})");
            }
        }
        else if (activeWeapons.Count < maxWeapons)
        {
            activeWeapons.Add(weapon, 1);
            Debug.Log($"Added weapon: {weapon}. Total weapons: {activeWeapons.Count}");
        }
        else
        {
            Debug.LogWarning($"Cannot add {weapon}. Max weapons ({maxWeapons}) reached.");
        }
        playerUI.UpdateWeaponsList(activeWeapons);
    }

    public bool IsWeaponMaxed(string weapon)
    {
        return activeWeapons.ContainsKey(weapon) && activeWeapons[weapon] >= maxWeaponLevels[weapon];
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
                attackRateMultiplier *= (1f - value);
                Debug.Log($"Attack rate multiplier decreased to {attackRateMultiplier}");
                break;
            default:
                Debug.LogWarning($"Unknown stat: {stat}");
                break;
        }
        playerUI.UpdateStatsList(); // Обновляем список улучшений после изменения
    }

    // Геттеры для множителей
    public float GetDamageMultiplier()
    {
        return damageMultiplier;
    }

    public float GetAttackRateMultiplier()
    {
        return attackRateMultiplier;
    }
}