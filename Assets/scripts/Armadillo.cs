using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;

public class Armadillo : BaseEnemy
{
    [Header("Swipe")]
    [SerializeField] HitBox swipeHB;
    [SerializeField] Vector2 swipeRange;
    [SerializeField] int swipeDmg;
    [SerializeField] float swipeKB, swipeResetTime;
    float swipeCooldown;

    [Header("Roll")]
    [SerializeField] Vector2 rollRange;
    [SerializeField] float rollSpeed, phase2RollSpeed, rollResetTime, rollKB, rollGoopRate, goopLifeTime;
    [SerializeField, Range(0, 1)] float goopAmount;
    [SerializeField] int rollDamage;
    [SerializeField] HitBox rollHB;
    float rollCooldown, timeRolling, rollGoopTimeLeft;
    public bool rolling;
    Vector3 rollTarget;

    [Header("Dig")]
    [SerializeField] float digMinDist;
    [SerializeField] Vector2 digTimeRange;
    [SerializeField] GameObject surfaceDecal, enemyPrefab;
    [SerializeField] int digDamage;
    [SerializeField] float digResetTime, digKB;
    [SerializeField] HitBox digHB;
    [SerializeField] GameObject erruptionVFX;
    [SerializeField] float ERRUPTION_TIME;
    public float digCooldown, digTimeLeft, erruptionCooldown;
    public bool digging, errupting = false;
    public int numEnemiesToSpawn = 1;
    List<GameObject> minions = new List<GameObject>();

    [Header("Anims")]
    [SerializeField] Animator anim;
    [SerializeField] string rollAnim, swipeAnim, digAnim;

    protected override void Die()
    {
        base.Die();
        foreach (var m in minions) Destroy(m);
        Destroy(gameObject);
    }

    public override void EndAttack()
    {
        //print("end attack");
        base.EndAttack();

        if (digging) EndDig();
        if (rolling) EndRoll();

        move.ResetSpeed();
        move.EnableRotation();

        rollCooldown = rollResetTime;
        swipeHB.EndChecking();
    }

    void EndRoll()
    {
        //print("end roll");
        rollHB.EndChecking();
        busy = true;
        
        rolling = false;
        anim.SetBool(rollAnim, false);
        timeRolling = 0;
        stats.SetVincible();
    }

    void EndDig()
    {
        digCooldown = digResetTime;
        surfaceDecal.SetActive(false);
        digging = false;
        stats.SetVincible();
        digHB.EndChecking();
        GoopManager.i.SpawnGoop(transform.position, 1, goopLifeTime);
    }

    public void FinishUnroll()
    {
        //print("done unrolling!");
        move.EnableRotation();
        move.target = target.position;
        move.gotoTarget = true;
        busy = false;
        rollCooldown = rollResetTime;
    }

    public void StartDigCountdown()
    {
        //print("starting countdown");
        digging = true;
        digTimeLeft = Random.Range(digTimeRange.x, digTimeRange.y);
    }

    protected override void Start()
    {
        base.Start();
        rollCooldown = rollResetTime;
        digCooldown = digResetTime;
        erruptionVFX.GetComponent<VisualEffect>().Stop();
    }

    protected override void Update()
    {
        base.Update();

        for (int i = 0; i < minions.Count; i++) {
            if (minions[i] == null) minions.RemoveAt(i);
        }

        if ((float)stats.health / stats.maxHealth < 0.5) {
            numEnemiesToSpawn = 2;
            rollSpeed = phase2RollSpeed;
        }

        if (rolling) {
            if (Vector3.Distance(transform.position, rollTarget) <= 0.1f || timeRolling > 5) EndAttack();
            timeRolling += Time.deltaTime;
            rollGoopTimeLeft -= Time.deltaTime;
            if (rollGoopTimeLeft <= 0) {
                GoopManager.i.SpawnGoop(transform.position, goopAmount, goopLifeTime);
                rollGoopTimeLeft = rollGoopRate;
            }
        }

        if (digging) {
            digTimeLeft -= Time.deltaTime;
            if (digTimeLeft > 0) return;

            if (surfaceDecal.activeInHierarchy) {
                transform.position = surfaceDecal.transform.position;
                PutOnGround();
                anim.SetBool(digAnim, false);


                return;
            }
            surfaceDecal.transform.position = target.transform.position + Vector3.up;
            surfaceDecal.SetActive(true);
            digHB.StartChecking(transform, digDamage, digKB);
            digTimeLeft = 1;
        }

        //handle erruption vfx
        if (erruptionCooldown <= 0 && errupting){
            erruptionVFX.GetComponent<VisualEffect>().Stop();
            errupting = false;
            //print("erruptionVFX cooldown: " + erruptionCooldown);
        }

        if (busy) return;

        if (digCooldown <= 0 && dist > digMinDist) StartDig();
        else if (rollCooldown <= 0 && dist < rollRange.y && dist > rollRange.x) StartRoll();
        else if (dist > swipeRange.y) MoveTowardTarget();
        else if (dist < swipeRange.x) Backup();
        else Swipe();
    }

    void StartDig()
    {
        //print("start dig!");
        if (enemyPrefab) {
            for (int i = 0; i < numEnemiesToSpawn; i++) {
                if (minions.Count > 2) break;
                Vector3 offset = new Vector3(Random.Range(-2, 2), 0, Random.Range(-2, 2));
                var newMinion = Instantiate(enemyPrefab, transform.position + offset, Quaternion.identity);
                newMinion.GetComponent<EnemyStats>().inGroup = false;
                minions.Add(newMinion);
            }
        }
        LookAtTarget(1);
        move.gotoTarget = false;
        busy = true;
        anim.SetBool(rollAnim, false);
        anim.SetBool(digAnim, true);
        stats.SetInvincible();
        GoopManager.i.SpawnGoop(transform.position, 1, goopLifeTime);
    }

    void StartRoll()
    {
        //print("start roll");
        stats.SetInvincible();
        move.target = target.transform.position;
        rollTarget = move.target;
        move.gotoTarget = false;
        move.disableRotation();
        anim.SetBool(rollAnim, true);
        busy = true;
        LookAtTarget(1);
    }

    public void RollMove()
    {
        move.ChangeSpeed(rollSpeed);
        rollHB.StartChecking(transform, rollDamage, rollKB);
        move.gotoTarget = true;
        rolling = true;
        timeRolling = 0;
    }

    protected override void Cooldowns()
    {
        base.Cooldowns();
        swipeCooldown -= Time.deltaTime;
        rollCooldown -= Time.deltaTime;
        digCooldown -= Time.deltaTime;
        erruptionCooldown -= Time.deltaTime;
    }

    void Swipe()
    {
        if (swipeCooldown > 0) {
            Backup();
            return;
        }

        Stop();
        StopAllCoroutines();
        LookAtTarget(0.75f);

        currentAttack = new AttackDetails(swipeHB, swipeAnim, swipeDmg, swipeResetTime, swipeKB, gameObject);
        swipeCooldown = swipeResetTime;
        StartAttack(currentAttack, anim);
        busy = true;
        //print("swipe");
    }

    protected override void OnDrawGizmosSelected()
    {
        if (!debug) return;

        base.OnDrawGizmosSelected();

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, swipeRange.x);
        Gizmos.DrawWireSphere(transform.position, swipeRange.y);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, rollRange.x);
        Gizmos.DrawWireSphere(transform.position, rollRange.y);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, digMinDist);
    }
    
    public void TriggerErruptionVFX()
    {
        if (!errupting)
        {
            erruptionVFX.GetComponent<VisualEffect>().Play();
            errupting = true;
            erruptionCooldown = ERRUPTION_TIME;
            //print("erruptionVFX cooldown: " + erruptionCooldown);
        }
    }
}
