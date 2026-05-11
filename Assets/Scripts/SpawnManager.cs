using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public GameObject enemyPrefab;
    public GameObject powerupPrefab;
    public int waveNumber = 0;
    public float spawnRange = 8f;
    private bool spawningEnabled = true;

    private void OnEnable()
    {
        GameManager.OnGameOver += StopSpawning;
    }

    private void OnDisable()
    {
        GameManager.OnGameOver -= StopSpawning;
    }

    private void Start()
    {
        SpawnNextWave();
    }

    private void Update()
    {
        if (!spawningEnabled)
        {
            return;
        }

        int enemyCount = FindObjectsByType<Enemy>(FindObjectsSortMode.None).Length;
        if (enemyCount == 0)
        {
            SpawnNextWave();
        }
    }

    private void SpawnNextWave()
    {
        waveNumber++;
        SpawnEnemyWave(waveNumber);
        Instantiate(powerupPrefab, GenerateSpawnPosition(), powerupPrefab.transform.rotation);
        GameManager.RaiseWaveStarted(waveNumber);
    }

    public void SpawnEnemyWave(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Instantiate(enemyPrefab, GenerateSpawnPosition(), enemyPrefab.transform.rotation);
        }
    }

    public Vector3 GenerateSpawnPosition()
    {
        float spawnPosX = Random.Range(-spawnRange, spawnRange);
        float spawnPosZ = Random.Range(-spawnRange, spawnRange);
        Vector3 position = new Vector3(spawnPosX, 0.75f, spawnPosZ);
        if (position.magnitude < 2.5f)
        {
            position = position.normalized * 3f;
        }
        return position;
    }

    private void StopSpawning()
    {
        spawningEnabled = false;
    }
}
