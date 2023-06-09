using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Properties;
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
    [SerializeField] Sound goopThrowSound;
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

    [Header("final phase")]
    [SerializeField] GameObject postProcessing;
    [SerializeField] GameObject rotatePivot, playerTPtarget, snakeTPtarget;
    [SerializeField] float rotateSpeed = 1, finalScale, finalSpitCooldown = 5;
    [SerializeField] Sound transitionSound, slitherSound, battleStartSound, hissSound;
    [SerializeField] GameObject nonBlocking, finalPhaseSpit;
    [SerializeField] string blockTag, vulnerableTag;
    bool finalPhase;

    [Header("Anims")]
    [SerializeField] Animator anim;
    [SerializeField] string sprayAnim, spitAnim, tailWhipAnim, slitherAnim, coiledAnim = "coiled";

    protected override void Start()
    {
        base.Start();
        transitionSound = Instantiate(transitionSound);
        battleStartSound = Instantiate(battleStartSound);
        slitherSound = Instantiate(slitherSound);
        hissSound = Instantiate(hissSound);
        goopThrowSound = Instantiate(goopThrowSound);

        battleStartSound.Play();
    }

    public override void EndAttack()
    {
        base.EndAttack();
        tailWhipHB.Refresh();
        currentAttack = null;
    }

    protected override void Die()
    {
        base.Die();
        postProcessing.SetActive(false);
        ShaderTransitionController.i.ResumePP();
        Destroy(gameObject);
    }

    protected override void Update()
    {
        base.Update();

        if (busy || stats.invincible) return;

        if (finalPhase) {
            FinalPhaseBehavior();
            return;
        }

        if (moveTriggerHpPercent.Count > 0 && (float) stats.health/ stats.maxHealth <= moveTriggerHpPercent[0]) {
            StartCoroutine(MoveAndSpawn());
            return;
        }
        

        if (dist < tailWhipRange.y && tailWhipCooldown <= 0) TailWhip();
        else if (dist < RangedRange.y && dist > RangedRange.x) RangedAttack();

        if (!move.gotoTarget) {
            LookAtTarget(0.05f);
        }
    }

    void FinalPhaseBehavior()
    {
        Stop();
        if (spitCooldown > 0) rotatePivot.transform.Rotate(new Vector3(0, rotateSpeed * Time.deltaTime, 0));
        if (spitCooldown <= -1) Spit();
    }

    void StartFinalPhase()
    {
        postProcessing.SetActive(true);
        ShaderTransitionController.i.PausePP();
        anim.SetBool(coiledAnim, true);
        finalPhase = true;
        GlobalUI.i.BlackOut(0.5f);
        transform.localScale = Vector3.one * finalScale;
        Player.i.transform.position = playerTPtarget.transform.position;
        transform.position = snakeTPtarget.transform.position;
        spitResetTime = finalSpitCooldown;
        projectilePrefab = finalPhaseSpit;

        //UpdateTagRecursive(transform);
        transform.tag = vulnerableTag;
    }

    void UpdateTagRecursive(Transform parent)
    {
        parent.tag = blockTag;
        for (int i = 0; i < transform.childCount; i++) {
            UpdateTagRecursive(transform.GetChild(i));
        }
    }

    IEnumerator MoveAndSpawn()
    {
        if (moveTriggerHpPercent.Count == 1) {
            moveTriggerHpPercent.Clear();
            StartFinalPhase();
            yield break;
        }

        stats.SetInvincible();

        busy = true;
        moveTriggerHpPercent.RemoveAt(0);
        wave += 1;
        StartCoroutine(SpawnEnemies());
        anim.SetBool(slitherAnim, true);

        yield return new WaitForSeconds(2);
        float _dist;

        GoToNextMoveTarget();
        _dist = 10;
        do {
            yield return new WaitForEndOfFrame();
            _dist = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(move.target.x, move.target.z));
        } while (_dist > 1f);
        FinishMove();

        StartCoroutine(SpawnEnemies());
        yield return new WaitForSeconds(3);

        GoToNextMoveTarget();
        _dist = 10;
        do {
            yield return new WaitForEndOfFrame();
            _dist = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(move.target.x, move.target.z));
        } while (_dist > 1f);
        FinishMove();

        StartCoroutine(SpawnEnemies());
        yield return new WaitForSeconds(3);

        GoToNextMoveTarget();
        _dist = 10;
        do {
            yield return new WaitForEndOfFrame();
            _dist = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(move.target.x, move.target.z));
        } while (_dist > 1f);
        FinishMove();

        anim.SetBool(slitherAnim, false);
        stats.SetVincible();
        busy = false;
    }

    

    void GoToNextMoveTarget()
    {
        move.target = moveTargets[0].transform.position;
        move.gotoTarget = true;
        slitherSound.Play();
        hissSound.Play();
    }

    void FinishMove()
    {
        moveTargets.Add(moveTargets[0]);
        moveTargets.RemoveAt(0);
        move.gotoTarget = false;
        slitherSound.Stop();
    }

    IEnumerator SpawnEnemies()
    {
        var enemies = wave == 1 ? wave1 : (wave == 2 ? wave2 : wave3);
        foreach (var e in enemies) {
            var enemy = Instantiate(e, transform.position, Quaternion.identity);
            enemy.GetComponent<EnemyStats>().inGroup = false;
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
        goopThrowSound.Play();

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
