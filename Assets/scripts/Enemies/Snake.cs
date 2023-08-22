using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Properties;
using UnityEditor.Rendering;
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

    [Header("Goop Orb")]
    [SerializeField] GameObject orbPrefab;
    [SerializeField] float orbResetTime, orbSpeed, orbScale, orbTargetOffset;
    [SerializeField] int orbDamage;
    float orbCooldown;

    [Header("tail whip")]
    [SerializeField] Vector2 tailWhipRange;
    [SerializeField] float tailWhipResetTime;
    [SerializeField] int tailWhipDamage;
    [SerializeField] HitBox tailWhipHB;
    float tailWhipCooldown;

    [Header("Sounds")]
    [SerializeField] Sound goopThrowSound;
    [SerializeField] Sound transitionSound, slitherSound, battleStartSound, hissSound;

    [Header("Anims")]
    [SerializeField] Animator anim;
    [SerializeField] string sprayAnim, spitAnim, tailWhipAnim, slitherAnim, coiledAnim = "coiled";

    [SerializeField] int Phase = 1;

    GameObject projectileSource;

    protected override void Start()
    {
        base.Start();
        transitionSound = Instantiate(transitionSound);
        battleStartSound = Instantiate(battleStartSound);
        slitherSound = Instantiate(slitherSound);
        hissSound = Instantiate(hissSound);
        goopThrowSound = Instantiate(goopThrowSound);
        
        battleStartSound.Play();
        projectileSource = gameObject;
        busy = true;
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
        Destroy(gameObject);
    }

    protected override void Update()
    {
        base.Update();

        if (busy) return;

        if (Phase == 1) Phase1Behavior();
        if (Phase == 2) Phase2Behavior();
        if (Phase == 3) Phase3Behavior();
    }

    void Phase1Behavior()
    {
        stats.invincible = true;
        if (dist < RangedRange.y && dist > RangedRange.x) RangedAttack();
        if (stats.health <= 2000) Phase = 2;
    }

    void Phase2Behavior()
    {
        if (orbCooldown <= 0) FireOrb();
        if (stats.health <= 1000) Phase = 2;
    }

    void Phase3Behavior()
    {

    }

    void FireOrb()
    {
        orbCooldown = orbResetTime;

        busy = false;
        var orb = InstantiateProjectile(orbPrefab, projectileStartOffset, Vector3.one * orbScale);
        orb.GetComponent<GoopProjectile>().goopAmount = goopAmount;
        orb.GetComponent<HitBox>().StartChecking(transform, orbDamage);

        Vector3 targetPos = target.position + Player.i.speed3D;
        var dir =  (targetPos + Vector3.down * orbTargetOffset) - (transform.position + projectileStartOffset*2);
        orb.GetComponent<Rigidbody>().AddForce(orbSpeed * Time.deltaTime * dir.normalized);

        goopThrowSound.Play();
    }

    protected override void Cooldowns()
    {
        base.Cooldowns();
        float dt = Time.deltaTime;
        spitCooldown -= dt;
        sprayCooldown -= dt;
        tailWhipCooldown -= dt;
        orbCooldown -= dt;
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
        print("spraying!");
        spraying = true;
        sprayCooldown = sprayResetTime;
        anim.SetBool(sprayAnim, true);
    }

    
    void Spit()
    {
        print("spitting");
        spitCooldown = spitResetTime;
        anim.SetBool(spitAnim, true);
    }


    public void LaunchProjectile()
    {
        if (spraying) {
            spraying = false;
            StartCoroutine(LaunchAfterDelay(sprayDelay * 1));
            StartCoroutine(LaunchAfterDelay(sprayDelay * 2));
        }
       
        busy = false;
        var projectile = InstantiateProjectile(projectilePrefab, projectileStartOffset, projectileSize);
        projectile.GetComponent<GoopProjectile>().goopAmount = goopAmount;
        projectile.GetComponent<HitBox>().StartChecking(transform, rangedDmg);
        Vector3 targetPos = target.position + Player.i.speed3D;
        
        AimAndFire(projectile, projectileAngle, targetPos, projectileStartOffset.y, shortDist, source: projectileSource);
        goopThrowSound.Play();

        anim.SetBool(spitAnim, false);
        anim.SetBool(sprayAnim, false);
    }

    IEnumerator LaunchAfterDelay(float delay, int recurse = 0)
    {
        if (recurse < 0) yield break;
        yield return new WaitForSeconds(delay);
        LaunchProjectile();

        if (recurse > 0) StartCoroutine(LaunchAfterDelay(delay, recurse - 1));
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
