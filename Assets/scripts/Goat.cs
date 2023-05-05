using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Goat : BaseEnemy
{
    [Header("Charge")]
    [SerializeField] float chargeResetTime;
    [SerializeField] float chargeRange, chargeSpeed, overshootDist, chargeStartUpTime, chargeStunTime, chargeKB;
    [SerializeField] int chargeDamage;
    [SerializeField] HitBox hb;

    [Header("throw")]
    [SerializeField] float throwKB, throwRange, throwResetTime;
    float chargeCooldown, throwCooldown;

    [Header("passive behavior")]
    [SerializeField] int pointID;
    [SerializeField] Vector2 waitTimeRange; 
    int lastPointID = -1;
    float waitTime;

    [Header("Anim")]
    [SerializeField] Animator anim;
    [SerializeField] float walkThreshold;


    [Header("audio")]
    [SerializeField] AudioSource hoofBeats; 
    [SerializeField] AudioSource hurt, throwSource; 

    public override void StartChecking()
    {
        base.StartChecking();
        hb.StartChecking(transform, chargeDamage, throwKB, gameObject);
    }

    protected override void Update()
    {
        base.Update();
        DoAnims();

        if (busy || !inAgroRange) return;

        BeAgro();
    }

    void DoAnims()
    {
        anim.SetBool("moving", speed > 0.001f);
    }

    protected override void Die()
    {
        base.Die();
        StopAllCoroutines();
        hoofBeats.gameObject.SetActive(false);
        anim.SetTrigger("die");
    }

    protected override void Start()
    {
        base.Start();
        stats.OnHit.AddListener(TakeHit);
    }

    protected override void JumpBack()
    {
        base.JumpBack();
    }

    void TakeHit()
    {
        hb.EndChecking();
        Stop();
        StopAllCoroutines();
        agroRange = Mathf.Infinity;
        hurt.Play();
        busy = false;
    }

    void BeCalm()
    {
        if(target == null) target = GetNewGrazeTarget();
        if (target == null) return;
        var dist = Vector2.Distance(transform.position, target.position);

        if (dist > 1f) {
            move.gotoTarget = true;
            move.target = target.position;
        }
        else {
            waitTime = Random.Range(waitTimeRange.x, waitTimeRange.y);
            target = null;
        }
    }

    void BeAgro()
    { 
        LookAtTarget(0.5f);        
        chargeCooldown -= Time.deltaTime;

        if (dist <= chargeRange/2) {
            JumpBack();
            return;
        }

        MoveTowardTarget();

        if (chargeCooldown <= 0 && dist <= chargeRange) {
            StopAllCoroutines();
            StartCoroutine(Charge());
        }
    }

    IEnumerator Charge()
    {
        LookAtTarget(0.5f);
        move.gotoTarget = false;
        busy = true;
        var dir = target.position - transform.position;
        var targetPos = target.position + dir.normalized * overshootDist;
        anim.SetBool("charging", true);

        yield return new WaitForSeconds(chargeStartUpTime);

        hoofBeats.Play();
        hb.StartChecking(transform, chargeDamage, chargeKB, gameObject, transform.right * Random.Range(-2, 2));

        move.target = targetPos;
        move.ChangeSpeed(chargeSpeed);
        move.gotoTarget = true;

        float chargeDist = Vector3.Distance(transform.position, targetPos);
        float time = 0;
        while (chargeDist > 1f || time > 3.5f) {
            chargeDist = Vector3.Distance(transform.position, targetPos);
            yield return new WaitForEndOfFrame();
            time += Time.deltaTime;
        }

        hb.EndChecking();
        move.gotoTarget = false;
        yield return new WaitForSeconds(chargeStunTime);

        hoofBeats.Stop();
        anim.SetBool("charging", false);
        move.NormalSpeed();
        chargeCooldown = chargeResetTime;
        busy = false;
    }

    Transform GetNewGrazeTarget()
    {
        if (EnemyPoint.i == null) return null;
        var point = EnemyPoint.i.FindPoint(pointID, lastPointID);
        if (point != null) lastPointID = point.GetComponent<EnemyPoint>().UniqueID;
        return point;
    }

    protected override void OnDrawGizmosSelected()
    {
        if (!debug) return;
        base.OnDrawGizmosSelected();

        Gizmos.DrawWireSphere(transform.position, chargeRange);
        if (target == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(target.position, 1);
    }
}
