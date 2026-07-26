using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpawnEntry
{
    public GameObject itemPrefab;
    [Range(0, 100)] public float chance = 100f;
    public int minCount = 1;
    public int maxCount = 1;
}

[System.Serializable]
public class SpawnPosition
{
    public Transform position;
    public bool enabled = true;
}

public class SpawnPoint : MonoBehaviour
{
    [Header("Point Chance")]
    [Range(0, 100)] public float spawnChance = 80f;

    [Header("Spawn Entries")]
    public SpawnEntry[] spawnEntries;

    [Header("Spawn Positions")]
    public List<SpawnPosition> spawnPositions = new List<SpawnPosition>();

    [Header("Spawn Settings")]
    public int maxConcurrentItems = 3;
    public float spawnRadius = 1.5f;
    public float respawnTime = 10f;
    public bool spawnOnStart = true;

    [Header("Ground Check")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 5f;
    public int maxRetries = 10;

    private List<GameObject> activeItems = new List<GameObject>();
    private bool hasSpawned;
    private bool respawning;

    private void Start()
    {
        if (spawnOnStart) SpawnItems();
    }

    private void Update()
    {
        if (!hasSpawned || respawning) return;

        activeItems.RemoveAll(item => item == null);

        if (activeItems.Count == 0)
            StartCoroutine(RespawnCoroutine());
    }

    public void SpawnItems()
    {
        Debug.Log($"[SpawnPoint] {name}: SpawnItems() called (spawnChance={spawnChance})");

        if (Random.Range(0f, 100f) > spawnChance)
        {
            Debug.Log($"[SpawnPoint] {name}: spawnChance failed ({spawnChance}%), skipping");
            return;
        }

        hasSpawned = true;

        if (spawnEntries == null || spawnEntries.Length == 0)
        {
            Debug.LogWarning($"[SpawnPoint] {name}: no spawnEntries set");
            hasSpawned = false;
            return;
        }

        bool usePositions = false;
        foreach (var p in spawnPositions)
            if (p.enabled && p.position != null) { usePositions = true; break; }

        Debug.Log($"[SpawnPoint] {name}: usePositions={usePositions}, activeItems={activeItems.Count}");

        if (usePositions)
            SpawnAtPositions();
        else
            SpawnRandom();

        Debug.Log($"[SpawnPoint] {name}: spawned {activeItems.Count} items");
        if (activeItems.Count == 0) hasSpawned = false;
    }

    private void SpawnRandom()
    {
        foreach (var entry in spawnEntries)
        {
            if (entry.itemPrefab == null)
            {
                Debug.LogWarning($"[SpawnPoint] {name}: entry has null itemPrefab");
                continue;
            }

            if (Random.Range(0f, 100f) > entry.chance) continue;

            int count = Random.Range(entry.minCount, entry.maxCount + 1);
            Debug.Log($"[SpawnPoint] {name}: rolling entry '{entry.itemPrefab.name}', count={count}");
            for (int i = 0; i < count; i++)
            {
                if (activeItems.Count >= maxConcurrentItems) break;
                TrySpawnAtRandom(entry.itemPrefab);
            }
        }
    }

    private void SpawnAtPositions()
    {
        foreach (var sp in spawnPositions)
        {
            if (!sp.enabled || sp.position == null) continue;
            if (activeItems.Count >= maxConcurrentItems) break;

            Debug.Log($"[SpawnPoint] {name}: trying position '{sp.position.name}'");

            foreach (var entry in spawnEntries)
            {
                if (activeItems.Count >= maxConcurrentItems) break;
                if (entry.itemPrefab == null) continue;
                if (Random.Range(0f, 100f) > entry.chance) continue;

                TrySpawnAtPosition(entry.itemPrefab, sp.position);
            }
        }
    }

    private void TrySpawnAtRandom(GameObject prefab)
    {
        for (int retry = 0; retry < maxRetries; retry++)
        {
            Vector2 randomPos = (Vector2)transform.position +
                Random.insideUnitCircle * spawnRadius;

            RaycastHit2D hit = Physics2D.Raycast(
                randomPos + Vector2.up * 0.5f,
                Vector2.down,
                groundCheckDistance,
                groundLayer);

            if (hit.collider != null)
            {
                var item = Instantiate(prefab, hit.point, Quaternion.identity, transform);
                activeItems.Add(item);
                Debug.Log($"[SpawnPoint] {name}: spawned '{prefab.name}' at {hit.point}");
                return;
            }
        }

        Debug.Log($"[SpawnPoint] {name}: failed to find ground after {maxRetries} retries");
    }

    private void TrySpawnAtPosition(GameObject prefab, Transform pos)
    {
        RaycastHit2D hit = Physics2D.Raycast(
            pos.position + Vector3.up * 0.5f,
            Vector2.down,
            groundCheckDistance,
            groundLayer);

        if (hit.collider != null)
        {
            var item = Instantiate(prefab, hit.point, Quaternion.identity, transform);
            activeItems.Add(item);
            Debug.Log($"[SpawnPoint] {name}: spawned '{prefab.name}' at position '{pos.name}'");
        }
        else
        {
            Debug.Log($"[SpawnPoint] {name}: position '{pos.name}' no ground found (dist={groundCheckDistance})");
        }
    }

    private IEnumerator RespawnCoroutine()
    {
        respawning = true;
        yield return new WaitForSeconds(respawnTime);
        respawning = false;
        hasSpawned = false;
        SpawnItems();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);

        foreach (var sp in spawnPositions)
        {
            if (sp.position == null) continue;
            Gizmos.color = sp.enabled ? Color.green : Color.gray;
            Gizmos.DrawWireSphere(sp.position.position, 0.3f);
            Gizmos.DrawLine(transform.position, sp.position.position);
        }
    }
}
