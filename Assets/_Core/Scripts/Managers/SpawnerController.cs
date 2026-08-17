using UnityEngine;
using System.Collections;

public class SpawnerController : MonoBehaviour {
    [Header("Spawn Configuration")]
    [SerializeField] private GameObject enemyPrefab;      // Enemy prefab to spawn
    [SerializeField] private float spawnInterval = 1.5f;  // Time between spawns (seconds)
    [SerializeField] private int maxEnemies = 20;         // Maximum enemies alive at once
    [SerializeField] private float spawnOffset = 0.5f;    // Distance outside the camera viewport

    [Header("XP Drop")]
    [SerializeField] private GameObject xpDropPrefab;

    [Header("Player Reference")]
    [SerializeField] private Transform player;            // Player transform (auto-found if null)

    private Camera mainCamera;
    private int currentEnemyCount = 0;

    private void Start() {
        // Find player if not assigned
        if (player == null) {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
            else Debug.LogError("SpawnerController: Player not found!");
        }

        mainCamera = Camera.main;
        if (mainCamera == null) Debug.LogError("SpawnerController: Main Camera not found!");

        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop() {
        while (true) {
            yield return new WaitForSeconds(spawnInterval);
            if (currentEnemyCount >= maxEnemies) continue;

            Vector3 spawnPos = GetSpawnPosition();
            GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

            // 🔥 Injeta a referência do jogador no inimigo
            EnemyController enemyCtrl = enemy.GetComponent<EnemyController>();
            if (enemyCtrl != null) {
                enemyCtrl.SetPlayerReference(player); // player já é uma variável no SpawnerController
                enemyCtrl.OnDeath.AddListener(() => OnEnemyDeath(enemy));
            }

            currentEnemyCount++;
        }
    }

    // Handler for the death event
    private void OnEnemyDeath(GameObject enemy) {
        // Decrement enemy counter
        currentEnemyCount--;

        // Spawn XP drop at enemy position
        if (xpDropPrefab != null) {
            Instantiate(xpDropPrefab, enemy.transform.position, Quaternion.identity);
        }

        // Add kill to GameManager
        if (GameplayManager.Instance != null) {
            GameplayManager.Instance.AddKill();
        }
    }

    // Calculates a spawn position just outside the camera viewport
    private Vector3 GetSpawnPosition() {
        // Get world-space bounds of the camera viewport
        Vector3 min = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 max = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, 0));

        // Choose a random side (0=left, 1=right, 2=top, 3=bottom)
        int side = Random.Range(0, 4);
        Vector3 spawnPos = Vector3.zero;

        switch (side) {
            case 0: // Left
                spawnPos.x = min.x - spawnOffset;
                spawnPos.y = Random.Range(min.y + spawnOffset, max.y - spawnOffset);
                break;
            case 1: // Right
                spawnPos.x = max.x + spawnOffset;
                spawnPos.y = Random.Range(min.y + spawnOffset, max.y - spawnOffset);
                break;
            case 2: // Top
                spawnPos.y = max.y + spawnOffset;
                spawnPos.x = Random.Range(min.x + spawnOffset, max.x - spawnOffset);
                break;
            case 3: // Bottom
                spawnPos.y = min.y - spawnOffset;
                spawnPos.x = Random.Range(min.x + spawnOffset, max.x - spawnOffset);
                break;
        }

        return spawnPos;
    }

    // Optional: clean up listeners when object is destroyed
    private void OnDestroy() {
        // If using pooling, you might want to unsubscribe all listeners
        // But since we destroy enemies, it's not strictly necessary.
    }
}