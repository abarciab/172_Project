using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pickupSpawner : MonoBehaviour
{
    [SerializeField] GameObject pickupPrefab;
    [SerializeField] float spawnSpread;
    [SerializeField] Vector2 spawnTimeRange;
    float spawnCooldown;

    private void Update()
    {
        spawnCooldown -= Time.deltaTime;
        if (spawnCooldown <= 0) SpawnPickup();
    }

    void SpawnPickup()
    {
        Vector3 pos = Random.insideUnitCircle * spawnSpread;
        spawnCooldown = Random.Range(spawnTimeRange.x, spawnTimeRange.y);
        Instantiate(pickupPrefab, transform.position + pos, Quaternion.identity, transform);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, spawnSpread);
    }
}
