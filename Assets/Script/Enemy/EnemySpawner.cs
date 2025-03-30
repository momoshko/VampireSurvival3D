using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public GameObject[] enemyPrefabs; // Массив префабов врагов (обычный, быстрый, танк)
    public float spawnRate = 2f;      // Базовая частота спавна (секунды)
    public float waveInterval = 30f;  // Интервал между волнами
    public float waveDuration = 10f;  // Длительность волны
    public int baseSpawnCount = 5;    // Базовое количество врагов за спавн
    private float nextSpawnTime;
    private float nextWaveTime;
    private bool isWaveActive;

    void Start()
    {
        nextSpawnTime = Time.time;
        nextWaveTime = Time.time + waveInterval;
    }

    void Update()
    {
        // Проверяем волну
        if (Time.time >= nextWaveTime && !isWaveActive)
        {
            isWaveActive = true;
            Invoke("EndWave", waveDuration); // Завершаем волну через waveDuration
        }

        // Спавним врагов
        if (Time.time >= nextSpawnTime)
        {
            SpawnEnemies();
            nextSpawnTime = Time.time + spawnRate / (isWaveActive ? 0.2f : 1f); // Ускоряем спавн в волне
        }
    }

    void SpawnEnemies()
    {
        int spawnCount = isWaveActive ? baseSpawnCount * 3 : baseSpawnCount; // Больше врагов в волне
        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 spawnPosition = GetRandomSpawnPosition();
            int enemyType = Random.Range(0, enemyPrefabs.Length); // Случайный тип врага
            Instantiate(enemyPrefabs[enemyType], spawnPosition, Quaternion.identity);
        }
    }

    Vector3 GetRandomSpawnPosition()
    {
        Vector3 playerPos = GameObject.Find("Player").transform.position;
        float spawnDistance = 15f; // Расстояние от игрока
        float angle = Random.Range(0f, 360f);
        Vector3 spawnOffset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * spawnDistance;
        return playerPos + spawnOffset;
    }

    void EndWave()
    {
        isWaveActive = false;
        nextWaveTime = Time.time + waveInterval; // Следующая волна через интервал
    }
}