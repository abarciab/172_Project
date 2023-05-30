using System.Collections;
using System.Collections.Generic;
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

    [Header("Anims")]
    [SerializeField] Animator anim;
    [SerializeField] string sprayAnim, spitAnim, tailWhipAnim;

    public override void EndAttack()
    {
        base.EndAttack();
    }

    protected override void Update()
    {
        base.Update();

        if (busy) return;

        if (dist < tailWhipRange.y && tailWhipCooldown <= 0) TailWhip();
        else if (dist < RangedRange.y && dist > RangedRange.x) RangedAttack();
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
