using System.Collections.Generic;
using UnityEngine;

public class CaveResourceSpawner : MonoBehaviour
{
    [Header("Spawn Area")]
    [SerializeField] private BoxCollider2D spawnArea;

    [Header("Resource Nodes")]
    [SerializeField] private GameObject[] nodePrefabs;

    [Header("Spawn Settings")]
    [SerializeField] private int maxNodes = 20;
    [SerializeField] private int maxSpawnAttempts = 100;
    [SerializeField] private float minimumSpacing = 1.25f;

    [Header("Blocked Spawn Zones")]
    [SerializeField] private Collider2D[] blockedSpawnZones;

    private readonly List<Vector3> spawnedPositions = new();

    private void Start()
    {
        SpawnNodes();
    }

    private void SpawnNodes()
    {
        if (spawnArea == null ||
            nodePrefabs == null ||
            nodePrefabs.Length == 0)
        {
            return;
        }

        int spawnedCount = 0;
        int attempts = 0;

        while (spawnedCount < maxNodes &&
               attempts < maxSpawnAttempts)
        {
            attempts++;

            Vector3 spawnPosition =
                GetRandomPositionInsideArea();

            if (IsBlocked(spawnPosition))
                continue;

            if (IsTooCloseToAnotherNode(spawnPosition))
                continue;

            GameObject prefab =
                GetRandomNodePrefab();

            if (prefab == null)
                continue;

            Instantiate(
                prefab,
                spawnPosition,
                Quaternion.identity
            );

            spawnedPositions.Add(spawnPosition);

            spawnedCount++;
        }

        Debug.Log(
            $"Spawned {spawnedCount} resource nodes."
        );
    }

    private Vector3 GetRandomPositionInsideArea()
    {
        Bounds bounds = spawnArea.bounds;

        float x = Random.Range(
            bounds.min.x,
            bounds.max.x
        );

        float y = Random.Range(
            bounds.min.y,
            bounds.max.y
        );

        return new Vector3(x, y, 0f);
    }

    private GameObject GetRandomNodePrefab()
    {
        int index = Random.Range(
            0,
            nodePrefabs.Length
        );

        return nodePrefabs[index];
    }

    private bool IsBlocked(Vector3 position)
    {
        if (blockedSpawnZones == null)
            return false;

        foreach (Collider2D zone in blockedSpawnZones)
        {
            if (zone != null &&
                zone.OverlapPoint(position))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsTooCloseToAnotherNode(Vector3 position)
    {
        foreach (Vector3 existingPosition in spawnedPositions)
        {
            float distance = Vector2.Distance(
                position,
                existingPosition
            );

            if (distance < minimumSpacing)
                return true;
        }

        return false;
    }
}