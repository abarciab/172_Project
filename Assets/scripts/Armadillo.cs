using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Armadillo : BaseEnemy
{
    [Header("swipe Attack")]
    [SerializeField] HitBox swipeHB;
    [SerializeField] Vector2 swipeRange;
    [SerializeField] int swipeDmg;
    [SerializeField] string swipeAnim;
    [SerializeField] float swipeKB, swipeResetTime;
    float swipeCooldown;

    [Header("Anims")]
    [SerializeField] Animator anim;
    [SerializeField] string rollAnim;

    protected override void Die()
    {
        base.Die();
        Destroy(gameObject);
    }

    protected override void Update()
    {
        base.Update();

        if (busy) return;

        if (dist > swipeRange.y) MoveTowardTarget();
        else if (dist < swipeRange.x) Backup();
        else Swipe();
    }

    protected override void Cooldowns()
    {
        base.Cooldowns();
        swipeCooldown -= Time.deltaTime;
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
    }

    protected override void OnDrawGizmosSelected()
    {
        if (!debug) return;

        base.OnDrawGizmosSelected();

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, swipeRange.x);
        Gizmos.DrawWireSphere(transform.position, swipeRange.y);
    }
}
