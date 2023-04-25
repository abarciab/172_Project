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
    [SerializeField] float agroRange = 10;
    [SerializeField] bool agro;
    [SerializeField] float chargeResetTime, chargeRange, chargeSpeed, overshootDist, chargeStartUpTime, chargeStunTime, chargeKB, throwKB, throwRange, throwResetTime;
    [SerializeField] int chargeDamage;
    [SerializeField] HitBox hb;
    bool attacking;
    float chargeCooldown, throwCooldown;

    [Header("Jump")]
    [SerializeField] float jumpDist;
    [SerializeField] float jumpheight, jumpTime;
    bool jumping;

    [Header("audio")]
    [SerializeField] AudioSource hoofBeats; 
    [SerializeField] AudioSource hurt, throwSource; 

    Vector3 TESTPOS;

    public void StartChecking()
    {
        hb.StartChecking(transform, chargeDamage, throwKB, gameObject);
    }

    public void EndChecking()
    {
        hb.EndChecking();
        attacking = false;
        throwCooldown = throwResetTime;
    }

    private void Start()
    {
        move = GetComponent<EnemyMovement>();
        oldPosition = transform.position;
        waitTime = Random.Range(waitTimeRange.x, waitTimeRange.y);
        GetComponent<EnemyStats>().OnHit.AddListener(TakeHit);
    }

    void TakeHit()
    {
        hurt.Play();
        JumpBack();
    }

    void JumpBack()
    {
        if (jumping) return;

        StopAllCoroutines();
        anim.SetBool("charging", false);
        attacking = false;
        throwCooldown = 0;
        StartCoroutine(_JumpBack(Player.i.transform.position));
    }

    IEnumerator _JumpBack(Vector3 threat)
    {
        var targetPos = transform.position + (threat - transform.position).normalized * -jumpDist;
        Vector2 originalPos = new Vector2(transform.position.x, transform.position.z);
        move.gotoTarget = false;

        jumping = true;
        attacking = false;

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
        waitTime -= Time.deltaTime;
        if (waitTime > 0 || jumping) return;

        if (GetComponent<EnemyStats>().health == 0) {
            anim.SetTrigger("die");
            move.gotoTarget = false;
            hoofBeats.gameObject.SetActive(false);
            hb.EndChecking();
            Player.i.enemies.Remove(move);
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
        float dist = Vector3.Distance(transform.position, Player.i.transform.position);
        if (dist > agroRange) {
            return;
        }

        if (!Player.i.enemies.Contains(move)) Player.i.enemies.Add(move);
        target = Player.i.transform;
        if (attacking) return;

        FaceTarget();

        
        chargeCooldown -= Time.deltaTime;
        var stats = GetComponent<EnemyStats>();

        if (dist <= chargeRange/2 || (chargeCooldown > 0 && throwCooldown <= 0 && stats.health > stats.maxHealth/2)) {
            if (throwCooldown <= 0 && stats.health > stats.maxHealth / 2) StartCoroutine(MoveToThrow());
            else JumpBack();
            return;
        }
        
        move.target = target.position;
        move.gotoTarget = true;

        if (chargeCooldown <= 0 && dist <= chargeRange) {
            StopAllCoroutines();
            StartCoroutine(Charge());
        }
        move.target = Player.i.transform.position;
    }

    IEnumerator MoveToThrow()
    {
        print("START THROW");

        attacking = true;
        throwCooldown = Mathf.Infinity;

        move.target = Player.i.transform.position;
        move.gotoTarget = true;

        float dist = Vector3.Distance(transform.position, Player.i.transform.position);
        while (dist > throwRange) {
            dist = Vector3.Distance(transform.position, Player.i.transform.position);
            yield return new WaitForEndOfFrame();
        }

        var rot = transform.localEulerAngles;
        transform.LookAt(Player.i.transform);
        rot.y = transform.localEulerAngles.y;
        transform.localEulerAngles = rot;

        move.gotoTarget = false;
        anim.SetTrigger("throw");
        throwSource.Play();

        print("END THROW");
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
        print("START CHARGE");

        FaceTarget(0.5f);
        move.gotoTarget = false;
        attacking = true;
        var dir = target.position - transform.position;
        var targetPos = target.position + dir.normalized * overshootDist;
        TESTPOS = targetPos;
        anim.SetBool("charging", true);

        yield return new WaitForSeconds(chargeStartUpTime);

        hoofBeats.Play();
        hb.StartChecking(transform, chargeDamage, chargeKB, gameObject, transform.right * Random.Range(-2, 2));

        move.target = targetPos;
        move.ChangeSpeed(chargeSpeed);
        move.gotoTarget = true;

        float dist = Vector3.Distance(transform.position, targetPos);
        float time = 0;
        while (dist > 1f || time > 3.5f) {
            dist = Vector3.Distance(transform.position, targetPos);
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
        attacking = false;
        print("DONE CHARGING");
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
