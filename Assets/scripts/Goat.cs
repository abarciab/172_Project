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

    [Header("Jump")]
    [SerializeField] float jumpDist;
    [SerializeField] float jumpheight, jumpTime;
    bool jumping;

    Vector3 TESTPOS;

    private void Start()
    {
        move = GetComponent<EnemyMovement>();
        oldPosition = transform.position;
        waitTime = Random.Range(waitTimeRange.x, waitTimeRange.y);
        GetComponent<EnemyStats>().OnHit.AddListener(JumpBack);
    }

    void JumpBack()
    {
        if (jumping) return;

        StopAllCoroutines();
        StartCoroutine(_JumpBack(Player.i.transform.position));
    }

    IEnumerator _JumpBack(Vector3 threat)
    {
        var targetPos = transform.position + (threat - transform.position).normalized * -jumpDist;
        Vector2 originalPos = new Vector2(transform.position.x, transform.position.z);
        move.gotoTarget = false;

        jumping = true;
        charging = false;

        float timeLeft = jumpTime;
        float startPos = transform.position.y;

        while (timeLeft > 0) {
            float progress = timeLeft / jumpTime;

            float yPos = startPos + Mathf.Sin(progress * Mathf.PI) * jumpheight;
            Vector2 xzPos = Vector2.Lerp(originalPos, new Vector2(targetPos.x, targetPos.z), 1-progress);
            transform.position = new Vector3(xzPos.x, yPos, xzPos.y);
            
            timeLeft -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        PutOnGround();

        jumping = false;
        chargeCooldown = 0;
        yield break;
    }

    void PutOnGround()
    {
        int layerMask = 1 << 7;
        Physics.Raycast(transform.position + Vector3.up * 100, Vector3.down, out var hit, 150, layerMask: layerMask);
        if (hit.collider == null) return;
        var pos = transform.position;
        pos.y = hit.point.y;
        transform.position = pos;
    }

    private void Update()
    {
        if (agro) waitTime = 0;
        waitTime -= Time.deltaTime;
        if (waitTime > 0 || jumping) return;

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
        Player.i.enemies.Add(move);
        target = Player.i.transform;
        if (charging) return;

        FaceTarget();

        float dist = Vector3.Distance(transform.position, target.position);
        chargeCooldown -= Time.deltaTime;

        if (dist <= chargeRange && chargeCooldown > 0) { move.gotoTarget = false; return; }
        
        move.target = target.position;
        move.gotoTarget = true;

        if (chargeCooldown <= 0 && dist <= chargeRange) StartCoroutine(Charge());

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

        hb.EndChecking();
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
