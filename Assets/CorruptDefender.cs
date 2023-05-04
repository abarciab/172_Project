using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorruptDefender : BaseEnemy
{
    [Header("RangedAttack")]
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] Vector2 RangedRange;
    [SerializeField] Vector3 projectileStartOffset, projectileSize;
    [SerializeField] float rangedResetTime, projectileAngle = 45;
    [SerializeField] int rangedDmg;
    [SerializeField, Range(0, 1)] float goopAmount;
    [SerializeField] string rangedAnim;
    float rangedCooldown;

    [Header("Melee Attack")]
    [SerializeField] HitBox HB;
    [SerializeField] Vector2 hitRange;
    [SerializeField] int hitDmg, meleePriotity;
    [SerializeField] string hitAnim;
    [SerializeField] float hitKB, hitResetTime;
    float hitCooldown;
    bool melee;

    [Header("Misc")]
    [SerializeField] Animator anim;

    protected override void Update()
    {
        base.Update();
        if (busy) return;

        melee = Player.i.CheckMelee(this, meleePriotity);

        if (melee) {
            if (dist > hitRange.y) MoveTowardTarget();
            else if (dist > hitRange.x) MeleeAttack();
            else if (dist < hitRange.x) Backup();
        }
        else {
            if (dist > RangedRange.y) MoveTowardTarget();
            else if (dist > RangedRange.x) RangedAttack();
            else if (dist < RangedRange.x) Backup();
        }
    }

    protected override void Die()
    {
        base.Die();
        anim.SetBool("dead", true);
        Destroy(gameObject, 2.5f);
    }

    void MeleeAttack()
    {
        if (hitCooldown > 0) {
            Backup();
            return;
        }
        Stop();
        StopAllCoroutines();
        LookAtTarget(0.75f);

        currentAttack = new AttackDetails(HB, hitAnim, hitDmg, hitResetTime, hitKB, gameObject);
        StartAttack(currentAttack, anim);
    }

    void RangedAttack()
    {
        OrbitPlayer();
        if (rangedCooldown > 0) return;
        rangedCooldown = rangedResetTime;

        var projectile = InstantiateProjectile(projectilePrefab, projectileStartOffset, projectileSize);
        projectile.GetComponent<GoopProjectile>().goopAmount = goopAmount;
        projectile.GetComponent<HitBox>().StartChecking(transform, rangedDmg);
        AimAndFire(projectile, projectileAngle);
    }

    public override void EndAttack()
    {
        base.EndAttack();
        currentAttack.HB.EndChecking();
        anim.SetBool(currentAttack.animBool, false);
    }

    protected override void Cooldowns()
    {
        hitCooldown -= Time.deltaTime;
        rangedCooldown -= Time.deltaTime;
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
        Gizmos.DrawWireSphere(transform.position, RangedRange.x);
        Gizmos.DrawWireSphere(transform.position, RangedRange.y);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, hitRange.x);
        Gizmos.DrawWireSphere(transform.position, hitRange.y);
    }
}
