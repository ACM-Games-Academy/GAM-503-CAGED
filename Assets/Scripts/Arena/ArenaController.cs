using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ArenaController : MonoBehaviour
{
    public PolygonCollider2D arenaBounds;
    public GameObject player;
    public Text messageText;

    public GameObject[] enemyPrefabs;      // Multiple enemy prefabs
    public Transform[] spawnPoints;

    [System.Serializable]
    public class Wave
    {
        public int enemyCount;
        public float spawnRate;
    }

    public Wave[] waves;
    private int currentWaveIndex = 0;

    private bool waveStarted = false;

    private void Start()
    {
        StartCoroutine(TutorialAndCountdown());
    }

    IEnumerator TutorialAndCountdown()
    {
        var playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
            playerController.EnableControls(false);

        messageText.text = "WASD or Arrow Keys to Move\nRight Click to Dash\nLeft Click to Attack";
        messageText.enabled = true;

        yield return new WaitForSeconds(5f);

        if (playerController != null)
            playerController.EnableControls(true);

        for (int i = 5; i > 0; i--)
        {
            messageText.text = $"First wave begins in {i}...";
            yield return new WaitForSeconds(1f);
        }

        messageText.text = "Fight!";
        yield return new WaitForSeconds(1f);
        messageText.enabled = false;

        StartWave();
    }

    void SpawnEnemy()
    {
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

        Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
    }

    IEnumerator SpawnWave(Wave wave)
    {
        for (int i = 0; i < wave.enemyCount; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(wave.spawnRate);
        }
    }

    void StartWave()
    {
        if (waveStarted) return;
        waveStarted = true;
        StartCoroutine(RunWaves());
    }

    IEnumerator RunWaves()
    {
        var playerHealth = player.GetComponent<PlayerHealth>();

        while (currentWaveIndex < waves.Length)
        {
            // Heal player fully at the start of each wave
            if (playerHealth != null)
                playerHealth.HealFull();

            messageText.text = $"Wave {currentWaveIndex + 1} Starting!";
            messageText.enabled = true;

            yield return new WaitForSeconds(2f);

            messageText.enabled = false;

            yield return StartCoroutine(SpawnWave(waves[currentWaveIndex]));

            // Wait for all enemies to be defeated before next wave
            yield return new WaitUntil(() => GameObject.FindGameObjectsWithTag("Enemy").Length == 0);

            currentWaveIndex++;

            if (currentWaveIndex < waves.Length)
            {
                messageText.text = $"Next wave in 10 seconds...";
                messageText.enabled = true;
                yield return new WaitForSeconds(10f);
                messageText.enabled = false;
            }
        }

        messageText.text = "All waves complete! You win!";
        messageText.enabled = true;
    }

    public bool IsPointInsideArena(Vector2 point)
    {
        return arenaBounds.OverlapPoint(point);
    }
}
