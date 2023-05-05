using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CorruptDefender : BaseEnemy
{
    [Header("slam attack")]
    [SerializeField] HitBox slamHB;
    [SerializeField] Vector2 slamRange;
    [SerializeField] int slamDmg;
    [SerializeField] string slamAnim;
    [SerializeField] float slamKB, slamResetTime;
    float slamCooldown;
    

    [Header("charge attack")]
    [SerializeField] HitBox chargeHB;
    [SerializeField] Vector2 chargeRange;
    [SerializeField] int chargeDmg;
    [SerializeField] string startChargeAnim, chargeAnim;
    [SerializeField] float chargeKB, chargeResetTime, chargeSpeed;
    float chargeCooldown;
    Vector3 chargeTarget;
    public bool charging;

    [Header("")]

    [Header("Misc")]
    [SerializeField] Animator anim;
    [SerializeField] int meleePriotity;
    bool melee;

    

    protected override void Update()
    {
        base.Update();

        if (charging) {
            float chargeDist = Vector3.Distance(transform.position, chargeTarget);
            if (chargeDist < .5f) EndAttack();
        }

        if (busy || !inAgroRange) return;
        melee = Player.i.CheckMelee(this, meleePriotity);
        
        if (melee) {
            if (dist > chargeRange.x && dist < chargeRange.y && chargeCooldown <= 0) WindUpCharge();
            else if (dist > slamRange.y) MoveTowardTarget();
            else if (dist > slamRange.x) MeleeAttack();
            else if (dist < slamRange.x) Backup();
        }
        else {
            OrbitPlayer();
        }
    }

    public void Charge()
    {
        anim.SetBool(chargeAnim, true);
        PickChargeTarget();

        move.ChangeSpeed(chargeSpeed);
        move.target = chargeTarget;
        move.gotoTarget = true;

        currentAttack = new AttackDetails(chargeHB, chargeAnim);
        chargeHB.StartChecking(true, chargeDmg, chargeKB, gameObject, Vector3.right * 2);

        charging = true;
    }

    void PickChargeTarget()
    {
        chargeTarget = target.position;
    }

    void WindUpCharge()
    {
        Stop();
        anim.SetTrigger(startChargeAnim);
        busy = true;
    }

    protected override void Die()
    {
        base.Die();
        anim.SetBool("dead", true);
        Destroy(gameObject, 2.5f);
    }

    void MeleeAttack()
    {
        if (slamCooldown > 0) {
            Backup();
            return;
        }
        Stop();
        StopAllCoroutines();
        LookAtTarget(0.75f);

        currentAttack = new AttackDetails(chargeHB, slamAnim, slamDmg, slamResetTime, slamKB, gameObject);
        StartAttack(currentAttack, anim);
    }

    public override void EndAttack()
    {
        base.EndAttack();
        currentAttack.HB.EndChecking();
        anim.SetBool(currentAttack.animBool, false);
        if (charging) {
            print("end charge");
            charging = false;
            chargeCooldown = chargeResetTime;
        }
    }

    protected override void Cooldowns()
    {
        slamCooldown -= Time.deltaTime;
        chargeCooldown -= Time.deltaTime;
    }

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void OnDrawGizmosSelected()
    {
        if (!debug) return;

        base.OnDrawGizmosSelected();

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, chargeRange.x);
        Gizmos.DrawWireSphere(transform.position, chargeRange.y);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, slamRange.x);
        Gizmos.DrawWireSphere(transform.position, slamRange.y);
    }
}
