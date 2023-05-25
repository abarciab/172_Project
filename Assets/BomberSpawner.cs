using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BomberSpawner : BaseEnemy
{
    [SerializeField] GameObject bomberPrefab;
    List<GameObject> spawnedBombers = new List<GameObject>();
    [SerializeField] int maxConcurrent, numSpawnedOnDeath = 2;
    [SerializeField] float spawnResetTime, range;
    float spawnCooldown;

    protected override void Start()
    {
        base.Start();
        PutOnGround();
    }

    protected override void Die()
    {
        base.Die();
        Destroy(gameObject, numSpawnedOnDeath * 0.5f + 0.5f);
        SpawnBomber(numSpawnedOnDeath);
    }

    protected override void Update()
    {
        base.Update();

        if ((dist > range && stats.health == stats.maxHealth) || stats.health <= 0) return;

        for (int i = 0; i < spawnedBombers.Count; i++) {
            if (spawnedBombers[i] == null) spawnedBombers.RemoveAt(i);
        }
        if (spawnCooldown >= 0 || spawnedBombers.Count >= maxConcurrent) return;

        SpawnBomber(1);
    }

    void SpawnBomber(int num)
    {
        if (num == 0) return;

        var newBomber = Instantiate(bomberPrefab, transform.position, Quaternion.identity);
        spawnedBombers.Add(newBomber);
        spawnCooldown = spawnResetTime;

        StartCoroutine(waitThenSpawn(num - 1));
    }

    IEnumerator waitThenSpawn(int num)
    {
        yield return new WaitForSeconds(0.5f);
        SpawnBomber(num);
    }

    protected override void Cooldowns()
    {
        base.Cooldowns();
        spawnCooldown -= Time.deltaTime;
    }

    protected override void OnDrawGizmosSelected()
    {
        if (!debug) return;

        base.OnDrawGizmosSelected();
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
