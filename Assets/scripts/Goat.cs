using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Goat : MonoBehaviour
{
    EnemyMovement move;
    [SerializeField] Animator anim;
    Vector3 oldPosition;
    
    [SerializeField] int pointID;
    [SerializeField] Vector2 waitTimeRange;  

    Transform target;
    int lastPointID = -1;
    float waitTime;
    [SerializeField] bool debug;

    [Header("agro")]
    [SerializeField] bool agro;
    [SerializeField] float chargeResetTime, chargeRange, chargeSpeed, overshootDist, chargeStartUpTime, chargeStunTime, chargeKB;
    [SerializeField] int chargeDamage;
    [SerializeField] HitBox hb;
    bool charging;
    float chargeCooldown;

    Vector3 TESTPOS;

    private void Start()
    {
        move = GetComponent<EnemyMovement>();
        oldPosition = transform.position;
        waitTime = Random.Range(waitTimeRange.x, waitTimeRange.y);
    }

    private void Update()
    {
        if (agro) waitTime = 0;
        waitTime -= Time.deltaTime;
        if (waitTime > 0) return;

        

        if (GetComponent<EnemyStats>().health == 0) {
            anim.SetTrigger("die");
            move.gotoTarget = false;
            return;
        }

        anim.SetBool("moving", (Vector2.Distance(transform.position, oldPosition) > 0.001f));
        oldPosition = transform.position;

        if (agro) BeAgro();
        else BeCalm();
    }

    void BeCalm()
    {
        if(target == null) target = GetNewTarget();
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
        target = Player.i.transform;
        if (charging) return;

        FaceTarget();

        float dist = Vector3.Distance(transform.position, target.position);
        chargeCooldown -= Time.deltaTime;

        //if (dist <= chargeRange && chargeCooldown <= 0) { move.gotoTarget = false; return; }
        
        move.target = target.position;
        move.gotoTarget = true;

        if (chargeCooldown <= 0) StartCoroutine(Charge());

        move.target = Player.i.transform.position;
    }

    void FaceTarget(float t = 0.05f)
    {
        var originalRot = transform.localEulerAngles;
        transform.LookAt(target.position);
        var targetRot = transform.localEulerAngles;
        targetRot.x = targetRot.z = 0;
        transform.localRotation = Quaternion.Lerp(Quaternion.Euler(originalRot), Quaternion.Euler(targetRot), t);
    }

    IEnumerator Charge()
    {
        FaceTarget(0.5f);
        move.gotoTarget = false;
        charging = true;
        var dir = target.position - transform.position;
        var targetPos = target.position + dir.normalized * overshootDist;
        TESTPOS = targetPos;
        anim.SetBool("charging", true);

        yield return new WaitForSeconds(chargeStartUpTime);

        hb.StartChecking(transform, chargeDamage, chargeKB, gameObject);

        move.target = targetPos;
        move.ChangeSpeed(chargeSpeed);
        move.gotoTarget = true;

        float dist = Vector3.Distance(transform.position, targetPos);
        while (dist > 0.1f) {
            dist = Vector3.Distance(transform.position, targetPos);
            yield return new WaitForEndOfFrame();
        }
        
        move.gotoTarget = false;
        yield return new WaitForSeconds(chargeStunTime);

        anim.SetBool("charging", false);
        move.NormalSpeed();
        chargeCooldown = chargeResetTime;
        charging = false;
    }

    Transform GetNewTarget()
    {
        if (EnemyPoint.i == null) return null;
        var point = EnemyPoint.i.FindPoint(pointID, lastPointID);
        if (point != null) lastPointID = point.GetComponent<EnemyPoint>().UniqueID;
        return point;
    }

    private void OnDrawGizmos()
    {
        if (!debug) return;

        Gizmos.DrawWireSphere(transform.position, chargeRange);
        if (target == null) return;
        Gizmos.DrawSphere(TESTPOS, 0.5f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(target.position, 1);
    }

}
