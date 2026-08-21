using UnityEngine;
using System.Collections;

public class PropSpawner : MonoBehaviour
{
    [Header("Prop Settings")]
    // Changed to an array so you can drop multiple different powerup prefabs here
    public GameObject[] propPrefabs;

    [Header("Spawn Boundaries")]
    public float minX = -5f;
    public float maxX = 5f;
    public float fixedY = 0.2f;
    public float minZ = -3.5f;
    public float maxZ = 3.5f;

    [Header("Timer Settings")]
    public float minTime = 15f;
    public float maxTime = 30f;

    void Start()
    {
        if (propPrefabs == null || propPrefabs.Length == 0)
        {
            Debug.LogError("Please assign at least one Prop Prefab to the GameSceneManager!");
            return;
        }

        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minTime, maxTime);
            yield return new WaitForSeconds(waitTime);

            float randomX = Random.Range(minX, maxX);
            float randomZ = Random.Range(minZ, maxZ);
            Vector3 spawnPosition = new Vector3(randomX, fixedY, randomZ);

            // Pick a random prop from your list
            int randomIndex = Random.Range(0, propPrefabs.Length);
            GameObject selectedPrefab = propPrefabs[randomIndex];

            Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);

            Debug.Log($"Spawned {selectedPrefab.name} at {spawnPosition}");
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = new Vector3((minX + maxX) / 2, fixedY, (minZ + maxZ) / 2);
        Vector3 size = new Vector3(maxX - minX, 0.1f, maxZ - minZ);
        Gizmos.DrawWireCube(center, size);
    }
}