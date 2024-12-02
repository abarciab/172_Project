using System.Collections;
using UnityEngine;

public class Scorpion : BaseEnemy 
{
    /*
    3 attacks: launch, snip, pin

    2 phases, based on health
    in phase1:
        tries to launch ranged attacks at the player
        if they player comes close, do a little snip, then jump back and continue vollying from afar
    inf pahse2:
        stop ranged attacks unless the player runs away deliberatly
        get close to player, try to pin
        if pin is on cooldown, snip at player
     */

    [Header("RangedAttack")]
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] Vector2 RangedRange;
    [SerializeField] Vector3 projectileStartOffset, projectileSize;
    [SerializeField] float rangedResetTime, projectileAngle = 45;
    [SerializeField] int rangedDmg;
    [SerializeField, Range(0, 1)] float goopAmount;
    [SerializeField] string rangedAnim;
    float rangedCooldown;

    [Header("Snip Attack")]
    [SerializeField] HitBox snipHB;
    [SerializeField] Vector2 snipRange;
    [SerializeField] int snipDmg;
    [SerializeField] string snipAnim;
    [SerializeField] float snipKB, snipResetTime;
    float snipCooldown;

    [Header("Pin Attack")]
    [SerializeField] HitBox pinHB;
    [SerializeField] Vector2 pinRange;
    [SerializeField] int pinDmg;
    [SerializeField] string pinAnim, pinStartAnim;
    [SerializeField] float pinKB, pinResetTime;
    bool hitPin, pinning;
    float pinCooldown;

    [Header("Misc")]
    [SerializeField] Animator anim;
    [SerializeField] float phaseSwitch = 0.5f;
    int phase;

    [Header("sounds")]
    [SerializeField] Sound WalkLoop;
    [SerializeField] Sound goopThrowSound;

    [Header("Anims")]
    [SerializeField] string walkAnim;
    [SerializeField] float walkThreshold;

    protected override void Start()
    {
        base.Start();
        phase = 1;
        agroRange = Mathf.Infinity;
        pinHB.OnHit.AddListener(HitPin);

        goopThrowSound = Instantiate(goopThrowSound);
        WalkLoop = Instantiate(WalkLoop);
        WalkLoop.PlaySilent(transform);
    }

    public override void EndAttack()
    {
        base.EndAttack();
        if (!pinning) return;
        StartCoroutine(Resume(1.5f));
    }

    IEnumerator Resume(float time)
    {
        busy = true;
        yield return new WaitForSeconds(time);
        busy = false;
    }

    void HitPin() {
        hitPin = true;
    }

    protected override void Die()
    {
        base.Die();
        Destroy(gameObject, 1);
        AchievementController.i.Unlock("SCORPION_KILLED");
    }

    protected override void Update()
    {
        if (move.gotoTarget) WalkLoop.SetPercentVolume(1, 0.05f);
        else WalkLoop.SetPercentVolume(0, 0.05f);


        base.Update();
        if (busy) return;

        if (phase == 1 && (float)stats.health / stats.maxHealth <= phaseSwitch) {
            phase = 2;
            //stats.Heal(1);
        }

        if (phase == 1) {
            if (dist < snipRange.y) { Snip(); JumpBack(); }
            if (dist > RangedRange.y) MoveTowardTarget();
            else if (dist > RangedRange.x) RangedAttack();
            else if (dist < RangedRange.x) Backup();
        }
        else {
            if (dist > RangedRange.x && dist < RangedRange.y && dist > pinRange.y) RangedAttack();

            if (dist > pinRange.y - 0.5f) MoveTowardTarget();
            else if (dist < pinRange.x + 0.5f) Backup();

            if (dist > pinRange.x && dist < pinRange.y && pinCooldown <= 0) StartPin();
            else if (dist < snipRange.y) Snip();
        }
        DoAnims();
    }

    

    protected override void Cooldowns()
    {
        base.Cooldowns();
        rangedCooldown -= Time.deltaTime;
        snipCooldown -= Time.deltaTime;
        pinCooldown -= Time.deltaTime;
    }

    void Snip()
    {
        if (snipCooldown > 0) {
            Backup();
            return;
        }
        //print("snip!");
        Stop();
        StopAllCoroutines();
        LookAtTarget(0.75f);

        currentAttack = new AttackDetails(snipHB, snipAnim, snipDmg, snipResetTime, snipKB, gameObject);
        snipCooldown = snipResetTime;
        StartAttack(currentAttack, anim);
        busy = true;
    }

    void RangedAttack()
    {
        OrbitPlayer();
        if (rangedCooldown > 0) return;
        rangedCooldown = rangedResetTime;
        busy = true;
        anim.SetBool(rangedAnim, true);
    }

    public void LaunchProjectile()
    {
        busy = false;
        var projectile = InstantiateProjectile(projectilePrefab, projectileStartOffset, projectileSize);
        projectile.GetComponent<GoopProjectile>().goopAmount = goopAmount;
        projectile.GetComponent<HitBox>().StartChecking(transform, rangedDmg);
        Vector3 targetPos = target.position + Player.i.speed3D;
        AimAndFire(projectile, projectileAngle, targetPos);
        anim.SetBool(rangedAnim, false);
        goopThrowSound.Play();
    }

    void StartPin()
    {
        if (pinCooldown > 0) return;
        pinCooldown = pinResetTime;
        //print("starting pin!");

        pinning = busy = true;
        hitPin = false;
        LookAtTarget(0.75f);
        anim.SetTrigger(pinStartAnim);
    }

    void DoAnims()
    {
        anim.SetBool(walkAnim, Mathf.Abs(speed) > walkThreshold);
    }


    protected override void OnDrawGizmosSelected()
    {
        if (!debug) return;

        base.OnDrawGizmosSelected();

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, RangedRange.x);
        Gizmos.DrawWireSphere(transform.position, RangedRange.y);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, snipRange.x);
        Gizmos.DrawWireSphere(transform.position, snipRange.y);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, pinRange.x);
        Gizmos.DrawWireSphere(transform.position, pinRange.y);
    }
}
