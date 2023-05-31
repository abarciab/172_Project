using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Snake : BaseEnemy
{
    [Header("Spit")]
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] Vector2 RangedRange;
    [SerializeField] Vector3 projectileStartOffset, projectileSize;
    [SerializeField] float spitResetTime, projectileAngle = 45, shortDist;
    [SerializeField] int rangedDmg;
    [SerializeField, Range(0, 1)] float goopAmount;
    float spitCooldown;

    [Header("Spray")]
    [SerializeField] float sprayResetTime;
    [SerializeField] float sprayDelay;
    float sprayCooldown;
    bool spraying;

    [Header("tail whip")]
    [SerializeField] Vector2 tailWhipRange;
    [SerializeField] float tailWhipResetTime;
    [SerializeField] int tailWhipDamage;
    [SerializeField] HitBox tailWhipHB;
    float tailWhipCooldown;

    [Header("enemy spawning")]
    [SerializeField] List<GameObject> moveTargets = new List<GameObject>();
    [SerializeField] List<GameObject> wave1 = new List<GameObject>(), wave2 = new List<GameObject>(), wave3 = new List<GameObject>();
    [SerializeField] List<float> moveTriggerHpPercent = new List<float>();
    int wave = 0;

    [Header("Anims")]
    [SerializeField] Animator anim;
    [SerializeField] string sprayAnim, spitAnim, tailWhipAnim, slitherAnim;

    public override void EndAttack()
    {
        base.EndAttack();
        tailWhipHB.Refresh();
        currentAttack = null;
    }

    protected override void Update()
    {
        base.Update();

        //print("busy: " + busy + ", wipcooldown: " + tailWhipCooldown);

        if (busy) return;

        if (moveTriggerHpPercent.Count > 0 && (float) stats.health/ stats.maxHealth <= moveTriggerHpPercent[0]) {
            StartCoroutine(MoveAndSpawn());
            return;
        }
        if (dist < tailWhipRange.y && tailWhipCooldown <= 0) TailWhip();
        else if (dist < RangedRange.y && dist > RangedRange.x) RangedAttack();

        if (!move.gotoTarget) {
            LookAtTarget(0.05f);
        }

        //StartChecking
    }

    IEnumerator MoveAndSpawn()
    {
        print("MOVING!");

        busy = true;
        moveTriggerHpPercent.RemoveAt(0);
        wave += 1;
        StartCoroutine(SpawnEnemies());
        yield return new WaitForSeconds(3);
        float _dist;

        GoToNextMoveTarget();
        _dist = 10;
        do {
            print("moving to first point");
            yield return new WaitForEndOfFrame();
            _dist = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(move.target.x, move.target.z));
        } while (_dist > 1f);
        FinishMove();

        StartCoroutine(SpawnEnemies());
        yield return new WaitForSeconds(3);

        GoToNextMoveTarget();
        _dist = 10;
        do {
            print("moving to second point");
            yield return new WaitForEndOfFrame();
            _dist = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(move.target.x, move.target.z));
        } while (_dist > 1f);
        FinishMove();

        StartCoroutine(SpawnEnemies());
        yield return new WaitForSeconds(3);

        GoToNextMoveTarget();
        _dist = 10;
        do {
            print("moving to third point");
            yield return new WaitForEndOfFrame();
            _dist = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(move.target.x, move.target.z));
        } while (_dist > 1f);
        FinishMove();

        print("DONE MOVING!");
        busy = false;
    }

    void GoToNextMoveTarget()
    {
        move.target = moveTargets[0].transform.position;
        move.gotoTarget = true;
        anim.SetBool(slitherAnim, true);
    }

    void FinishMove()
    {
        moveTargets.Add(moveTargets[0]);
        moveTargets.RemoveAt(0);
        move.gotoTarget = false;
        anim.SetBool(slitherAnim, false);
    }

    IEnumerator SpawnEnemies()
    {
        var enemies = wave == 1 ? wave1 : (wave == 2 ? wave2 : wave3);
        foreach (var e in enemies) {
            var enemy = Instantiate(e, transform.position, Quaternion.identity);
            enemy.GetComponent<BaseEnemy>().agroRange = Mathf.Infinity;
            yield return new WaitForSeconds(1f);
        }
    }

    void TailWhip()
    {
        busy = true;
        tailWhipCooldown = tailWhipResetTime;
        currentAttack = new AttackDetails(tailWhipHB, tailWhipAnim, tailWhipDamage, 0, 50, gameObject);
        anim.SetTrigger(tailWhipAnim);
    }

    void RangedAttack()
    {
        busy = true;
        if (sprayCooldown <= 0) Spray();
        else if (spitCooldown <= 0) Spit();
        else busy = false;
    }

    void Spray()
    {
        spraying = true;
        sprayCooldown = sprayResetTime;
        anim.SetBool(sprayAnim, true);
    }

    protected override void Cooldowns()
    {
        base.Cooldowns();
        spitCooldown -= Time.deltaTime;
        sprayCooldown -= Time.deltaTime;
        tailWhipCooldown -= Time.deltaTime;
    }

    void Spit()
    {
        spitCooldown = spitResetTime;
        anim.SetBool(spitAnim, true);
    }


    public void LaunchProjectile()
    {
        if (spraying) {
            spraying = false;
            StartCoroutine(LaunchAfterDelay(sprayDelay));
            StartCoroutine(LaunchAfterDelay(sprayDelay * 2));
        }
        busy = false;
        var projectile = InstantiateProjectile(projectilePrefab, projectileStartOffset, projectileSize);
        projectile.GetComponent<GoopProjectile>().goopAmount = goopAmount;
        projectile.GetComponent<HitBox>().StartChecking(transform, rangedDmg);
        Vector3 targetPos = target.position + Player.i.speed3D;
        AimAndFire(projectile, projectileAngle, targetPos, projectileStartOffset.y, shortDist);

        anim.SetBool(spitAnim, false);
        anim.SetBool(sprayAnim, false);
    }

    IEnumerator LaunchAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        LaunchProjectile();
    }

    protected override void OnDrawGizmosSelected()
    {
        if (!debug) return;

        base.OnDrawGizmosSelected();
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, RangedRange.x);
        Gizmos.DrawWireSphere(transform.position, RangedRange.y);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, tailWhipRange.x);
        Gizmos.DrawWireSphere(transform.position, tailWhipRange.y);
    }
}
