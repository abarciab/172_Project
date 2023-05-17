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
    [SerializeField, Range(0, 1)] float globAmount = 0.5f;

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

        if (!inAgroRange || busy) return;

        if (dist > explodeRange.y) MoveTowardTarget();
        else if (dist < explodeRange.x) Backup();
        else startExplode();
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
    }

    public override void EndAttack()
    {
        base.EndAttack();

        buildUpSound.Stop();
        if (dist > explodeRange.y) {
            anim.SetTrigger(explodeFailedTrigger);
            return;
        }
        

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
