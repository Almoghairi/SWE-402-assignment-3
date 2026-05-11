using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    private const float ArenaSpawnRadius = 3.35f;
    private const float SafeCenterRadius = 1.75f;
    private const float SpawnHeight = 0.86f;

    public GameObject enemyPrefab;
    public GameObject powerupPrefab;
    public int waveNumber = 0;
    public float spawnRange = ArenaSpawnRadius;
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
        float radius = Mathf.Min(spawnRange, ArenaSpawnRadius);
        Vector2 flatPosition = Random.insideUnitCircle * radius;
        if (flatPosition.magnitude < SafeCenterRadius)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            flatPosition = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * SafeCenterRadius;
        }
        return new Vector3(flatPosition.x, SpawnHeight, flatPosition.y);
    }

    private void StopSpawning()
    {
        spawningEnabled = false;
    }
}
