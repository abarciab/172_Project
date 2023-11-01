using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorruptBomber : BaseEnemy
{

    [Header("Anims")]
    [SerializeField] Animator anim;
    [SerializeField] string walkAnim, explodeTrigger, deathTrigger, explodeFailedTrigger;
    [SerializeField] float walkThreshold;

    [Header("Explode")]
    [SerializeField] Vector2 explodeRange;
    [SerializeField] int numGlobs = 4;
    [SerializeField, Range(0, 1)] float globAmount = 0.5f, explodeSpeed;
    [SerializeField] GameObject explodeObj;
    [SerializeField] Vector3 explodeOffset = new Vector3(0, 1, 0);
    [SerializeField] int explodeDamage;
    [SerializeField] float explodeDelay = 1f;
    bool startingExplode;

    [Header("Sounds")]
    [SerializeField] Sound explodeSound;
    [SerializeField] Sound buildUpSound;

    protected override void Start()
    {
        base.Start();
        explodeSound = Instantiate(explodeSound);
        buildUpSound = Instantiate(buildUpSound);
    }

    protected override void Update()
    {
        base.Update();
        DoAnims();

        if (!inAgroRange || busy || stunned) return;

        if (startingExplode) {
            explodeDelay -= Time.deltaTime;
            if (explodeDelay <= 0) startExplode();
            return;
        }
        if (dist > explodeRange.y) MoveTowardTarget();
        else if (dist < explodeRange.x) Backup();
        else {
            startingExplode = true;
        }
    }

    protected override void Die()
    {
        base.Die();
        anim.SetTrigger(deathTrigger);
        Destroy(gameObject, 1.5f);
    }

    void startExplode()
    {
        buildUpSound.Play(transform);
        anim.SetTrigger(explodeTrigger);
        busy = true;
        var explosionVFX = Instantiate(explodeObj, transform.position + explodeOffset, Quaternion.identity);
        explosionVFX.GetComponent<HitBox>().StartChecking(true, explodeDamage);
        stats.HideBody();
        Stop();
    }

    public override void EndAttack()
    {
        base.EndAttack();

        buildUpSound.Stop();
        /*if (dist > explodeRange.y) {
            anim.SetTrigger(explodeFailedTrigger);
            return;
        }*/
        

        for (int i = 0; i < numGlobs; i++) {
            GoopManager.i.SpawnGoop(transform.position, globAmount);
            enabled = false;
            Destroy(gameObject, 1f);
            
            explodeSound.Play(transform);
        }
    }

    void DoAnims()
    {
        anim.SetBool(walkAnim, Mathf.Abs(speed) > walkThreshold);
    }

    protected override void OnDrawGizmosSelected()
    {
        if (!debug) return;

        base.OnDrawGizmosSelected();

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, explodeRange.x);
        Gizmos.DrawWireSphere(transform.position, explodeRange.y);
    }
}
