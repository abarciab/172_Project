using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BomberSpawner : BaseEnemy
{
    [SerializeField] GameObject bomberPrefab, rarePrefab;
    List<GameObject> spawnedBombers = new List<GameObject>();
    [SerializeField] int maxConcurrent, numSpawnedOnDeath = 2;
    [SerializeField] float spawnResetTime, range;
    [SerializeField, Range(0, 1)] float rareChance;
    [SerializeField] bool useManualAgro = false;
    [SerializeField] float manualAgroRange = 45;

    [SerializeField] SpawnerVFXParent spawnVFX;

    float spawnCooldown;

    protected override void Start()
    {
        base.Start();
        PutOnGround();
        spawnCooldown = Random.Range(0, spawnResetTime);
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

        var prefab = Random.Range(0.0f, 1) < rareChance ? rarePrefab : bomberPrefab;
        var newBomber = Instantiate(prefab, transform.position, Quaternion.identity);
        newBomber.GetComponent<EnemyStats>().inGroup = false;
        if (useManualAgro) newBomber.GetComponent<BaseEnemy>().agroRange = manualAgroRange;
        spawnedBombers.Add(newBomber);
        spawnCooldown = spawnResetTime;

        //play spawning vfx
        spawnVFX.TriggerSpawnVFX();

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
