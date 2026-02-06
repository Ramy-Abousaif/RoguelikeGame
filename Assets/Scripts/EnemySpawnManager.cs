using UnityEngine;
using UnityEngine.AI;

public class EnemySpawnManager : MonoBehaviour
{
    [Header("Spawn Data")]
    [SerializeField] private EnemySpawnTable spawnTable;

    [Header("Spawn Rules")]
    [SerializeField] private float spawnInterval = 4f;
    [SerializeField] private int maxAlive = 20;
    [SerializeField] private float minDistanceFromPlayer = 15f;
    [SerializeField] private float maxDistanceFromPlayer = 35f;
    [SerializeField] private int maxAttempts = 25; // how many tries per spawn
    [SerializeField] private float navSampleDistance = 2f; // radius for NavMesh sampling

    private float timer;
    private float elapsedTime;
    private Transform player;
    private int aliveCount;

    private void Awake()
    {
        player = GameObject.FindWithTag("Player").transform;
        if (!player)
            Debug.LogError("EnemySpawnManager: No player found in scene!");
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        if (aliveCount >= maxAlive)
            return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            timer = spawnInterval;
            TrySpawn();
        }
    }

    private void TrySpawn()
    {
        if (!TryFindSpawnPosition(out Vector3 spawnPos))
            return;

        GameObject prefab = spawnTable.GetRandom(elapsedTime, GetPlayerLevel());
        if (prefab == null)
            return;

        GameObject enemy = PoolManager.Instance.Spawn(prefab, spawnPos, Quaternion.identity);
        aliveCount++;

        // if (enemy.TryGetComponent<Character>(out var c))
        // {
        //     c.OnDeath += () => aliveCount--;
        // }
    }

    private bool TryFindSpawnPosition(out Vector3 pos)
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            // Pick a random direction and distance from player
            Vector3 dir = Random.insideUnitSphere;
            dir.y = 0f;
            dir.Normalize();
            float dist = Random.Range(minDistanceFromPlayer, maxDistanceFromPlayer);
            Vector3 candidate = player.position + dir * dist;

            // Snap to nearest NavMesh position
            if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, navSampleDistance, NavMesh.AllAreas))
                continue;

            // Visibility check
            if (IsVisibleToCamera(hit.position))
                continue;

            pos = hit.position;
            return true;
        }

        pos = default;
        return false;
    }

    private bool IsVisibleToCamera(Vector3 pos)
    {
        Camera cam = Camera.main;
        if (!cam) return false;

        Vector3 vp = cam.WorldToViewportPoint(pos);
        return vp.z > 0 && vp.x >= 0 && vp.x <= 1 && vp.y >= 0 && vp.y <= 1;
    }

    private int GetPlayerLevel()
    {
        // Hook into your player progression system
        return 0;
    }
}