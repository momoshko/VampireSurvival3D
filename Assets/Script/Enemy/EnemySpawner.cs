using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public GameObject[] enemyPrefabs; // ������ �������� ������ (�������, �������, ����)
    public float spawnRate = 2f;      // ������� ������� ������ (�������)
    public float waveInterval = 30f;  // �������� ����� �������
    public float waveDuration = 10f;  // ������������ �����
    public int baseSpawnCount = 5;    // ������� ���������� ������ �� �����
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
        // ��������� �����
        if (Time.time >= nextWaveTime && !isWaveActive)
        {
            isWaveActive = true;
            Invoke("EndWave", waveDuration); // ��������� ����� ����� waveDuration
        }

        // ������� ������
        if (Time.time >= nextSpawnTime)
        {
            SpawnEnemies();
            nextSpawnTime = Time.time + spawnRate / (isWaveActive ? 0.2f : 1f); // �������� ����� � �����
        }
    }

    void SpawnEnemies()
    {
        int spawnCount = isWaveActive ? baseSpawnCount * 3 : baseSpawnCount; // ������ ������ � �����
        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 spawnPosition = GetRandomSpawnPosition();
            int enemyType = Random.Range(0, enemyPrefabs.Length); // ��������� ��� �����
            Instantiate(enemyPrefabs[enemyType], spawnPosition, Quaternion.identity);
        }
    }

    Vector3 GetRandomSpawnPosition()
    {
        Vector3 playerPos = GameObject.Find("Player").transform.position;
        float spawnDistance = 15f; // ���������� �� ������
        float angle = Random.Range(0f, 360f);
        Vector3 spawnOffset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * spawnDistance;
        return playerPos + spawnOffset;
    }

    void EndWave()
    {
        isWaveActive = false;
        nextWaveTime = Time.time + waveInterval; // ��������� ����� ����� ��������
    }
}