using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArenaController : MonoBehaviour
{
    public PolygonCollider2D arenaBounds;
    public GameObject player;
    public Text messageText;

    public GameObject[] enemyPrefabs;
    public Transform[] spawnPoints;

    [Header("Arena Layout Objects")]
    public GameObject spikeFloor;
    public GameObject platformPrefab;
    public Transform[] platformSpawnPoints;

    private List<GameObject> activePlatforms = new List<GameObject>();

    [System.Serializable]
    public enum ArenaLayout
    {
        Default,
        SpikesArena,
        ThinArena,
        WideArena
    }

    [System.Serializable]
    public class Wave
    {
        public int enemyCount;
        public float spawnRate = 0.5f;
        public ArenaLayout layout;

        [Tooltip("Optional: If set, only these enemy prefabs will spawn for this wave.")]
        public GameObject[] customEnemyPrefabs;

        [Tooltip("Optional: Delay before spawning enemies (after layout applies).")]
        public float preWaveDelay = 0.5f;
    }

    public Wave[] waves;
    private int currentWaveIndex = 0;
    private bool waveStarted = false;
    private bool tutorialSkipped = false;

    private void Start()
    {
        StartCoroutine(TutorialAndCountdown());
    }

    IEnumerator TutorialAndCountdown()
    {
        var playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
            playerController.EnableControls(false);

        messageText.text = "WASD or Arrow Keys to Move\nRight Click to Dash\nLeft Click to Attack\n\nPress [SPACE] to start!";
        messageText.enabled = true;

        while (!Input.GetKeyDown(KeyCode.Space))
        {
            yield return null;
        }

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

    void SpawnEnemy(GameObject[] enemyPool)
    {
        if (enemyPool == null || enemyPool.Length == 0)
            enemyPool = enemyPrefabs;

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject enemyPrefab = enemyPool[Random.Range(0, enemyPool.Length)];
        Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
    }

    IEnumerator SpawnWave(Wave wave)
    {
        GameObject[] enemyPool = (wave.customEnemyPrefabs != null && wave.customEnemyPrefabs.Length > 0)
            ? wave.customEnemyPrefabs
            : enemyPrefabs;

        for (int i = 0; i < wave.enemyCount; i++)
        {
            SpawnEnemy(enemyPool);
            if (wave.spawnRate > 0f)
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
            if (playerHealth != null)
                playerHealth.HealFull();

            Wave wave = waves[currentWaveIndex];

            ApplyArenaLayout(wave.layout);

            float countdown = 5f;
            if (currentWaveIndex > 0)
            {
                ArenaLayout nextLayout = wave.layout;

                while (countdown > 0)
                {
                    messageText.text = nextLayout == ArenaLayout.SpikesArena
                        ? $"Next wave in {Mathf.CeilToInt(countdown)}...\nGet to the Platforms!"
                        : $"Next wave in {Mathf.CeilToInt(countdown)}...";
                    messageText.enabled = true;
                    countdown -= Time.deltaTime;
                    yield return null;
                }
            }

            messageText.enabled = false;

            ActivateArenaHazards(wave.layout);

            if (wave.preWaveDelay > 0f)
                yield return new WaitForSeconds(wave.preWaveDelay);

            yield return StartCoroutine(SpawnWave(wave));

            yield return new WaitUntil(() =>
                GameObject.FindGameObjectsWithTag("Enemy").Length == 0 ||
                (playerHealth != null && playerHealth.IsDead())
            );

            if (playerHealth != null && playerHealth.IsDead())
            {
                messageText.text = "You were defeated!";
                messageText.enabled = true;
                ResetArena();
                yield break;
            }

            currentWaveIndex++;
            ResetArena();

            if (currentWaveIndex < waves.Length && currentWaveIndex > 0)
            {
                float restCountdown = 5f;
                while (restCountdown > 0)
                {
                    messageText.text = $"Rest countdown {Mathf.CeilToInt(restCountdown)}...";
                    messageText.enabled = true;
                    restCountdown -= Time.deltaTime;
                    yield return null;
                }
                messageText.enabled = false;
            }
        }

        messageText.text = "All waves complete! You win!";
        messageText.enabled = true;
        ResetArena();
    }

    void ApplyArenaLayout(ArenaLayout layout)
    {
        spikeFloor.SetActive(false);

        foreach (var p in activePlatforms)
            Destroy(p);
        activePlatforms.Clear();

        switch (layout)
        {
            case ArenaLayout.SpikesArena:
                foreach (Transform t in platformSpawnPoints)
                {
                    GameObject platform = Instantiate(platformPrefab, t.position, Quaternion.identity);
                    activePlatforms.Add(platform);
                }
                break;
        }
    }

    void ActivateArenaHazards(ArenaLayout layout)
    {
        if (layout == ArenaLayout.SpikesArena)
            spikeFloor.SetActive(true);
    }

    void ResetArena()
    {
        spikeFloor.SetActive(false);
        foreach (var p in activePlatforms)
            Destroy(p);
        activePlatforms.Clear();
    }

    public bool IsPointInsideArena(Vector2 point)
    {
        return arenaBounds.OverlapPoint(point);
    }
}
