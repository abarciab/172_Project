using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class CorruptExplorer : MonoBehaviour
{


    [Header("RangedAttack")]
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] Vector2 RangedRange;
    [SerializeField] Vector3 projectileStartOffset, projectileSize; 
    [SerializeField] float rangedResetTime, projectileAngle = 45, orbitSpeed = 1, orbitSwitchMod = 0.3f;
    [SerializeField] int rangedDmg;
    [SerializeField, Range(0, 1)] float goopAmount;
    float rangedCooldown;

    [Header("Melee Attack")]
    [SerializeField] HitBox HB;
    [SerializeField] Vector2 hitRange;
    [SerializeField] int hitDmg;
    [SerializeField] string hitAnim;

    [SerializeField] float hitKB, hitResetTime;
    float hitCooldown;
    bool melee;

    [Header("Jump")]
    [SerializeField] float jumpDist;
    [SerializeField] float jumpheight, jumpTime;
    bool jumping;

    [Header("Anims")]
    [SerializeField] Animator anim;
    [SerializeField] float walkingThreshold = 0.1f;
    [SerializeField] string hurtAnimParam;
    
    [Header("misc")]
    [SerializeField] bool debug;
    [SerializeField] float hitStunTime = 0.7f;
    [SerializeField] float agroRange = 10;
    [SerializeField] GameObject target;

    
    Vector2 oldPos;
    bool screaming, busy, alreadyHit, agro;
    EnemyMovement move;

    public void EndScream()
    {
        agro = true;
    }

    public void StartChecking()
    {
        HB.StartChecking(true, hitDmg, hitKB, gameObject);
    }

    public void EndAttack()
    {
        busy = false;
        hitCooldown= hitResetTime;
        anim.SetBool(hitAnim, false);
    }

    private void Start()
    {
        move = GetComponent<EnemyMovement>();
        target = Player.i.gameObject;
        oldPos = new Vector2(transform.position.x, transform.position.z);
        var stats = GetComponent<EnemyStats>();
        stats.OnHit.AddListener(JumpBack);
        stats.OnHit.AddListener(Stunned);
        stats.OnHit.AddListener(GetComponentInChildren<EnemySound>().TakeHit);
        
    }

    private void Update()
    {
        DoAnims();
        if (GetComponent<EnemyStats>().health <= 0) {
            anim.SetBool("dead", true);
            enabled = false;
            Player.i.enemies.Remove(move);
            Player.i.EndMelee(move);
            Destroy(gameObject, 2.5f);
            return;
        }
       
        if (target == null) target = Player.i.gameObject;
        float dist = Vector3.Distance(transform.position, target.transform.position);
        hitCooldown -= Time.deltaTime;
        rangedCooldown -= Time.deltaTime;

        if (!InAgroRange(dist)) Stop();
        if (!agro || busy) return;
        if (!Player.i.enemies.Contains(move)) Player.i.enemies.Add(move);
        melee = Player.i.TryToMelee(move);

        if (melee) 
        {
            if (dist > hitRange.y) MoveTowardTarget();
            else if (dist > hitRange.x) Hit();
            else if (dist < hitRange.x) Backup();
        }
        else {
            if (dist > RangedRange.y) MoveTowardTarget();
            else if (dist > RangedRange.x) RangedAttack();
            else if (dist < RangedRange.x) Backup();
        }
    }

    void RangedAttack()
    {
        OrbitPlayer();
        if (rangedCooldown > 0) return;
        rangedCooldown = rangedResetTime;

        var bullet = Instantiate(projectilePrefab, transform);
        bullet.GetComponent<GoopProjectile>().goopAmount = goopAmount;
        bullet.transform.localPosition = projectileStartOffset;
        bullet.transform.parent = null;
        bullet.transform.localScale = projectileSize;
        bullet.GetComponent<HitBox>().StartChecking(true, rangedDmg);

        AimAndFire(bullet);
    }

    void OrbitPlayer()
    {
        move.ChangeSpeed(orbitSpeed);
        LookAtTarget(1);
        bool left = Mathf.Sin((Time.time * orbitSwitchMod) % Mathf.PI) < 0.5f;
        var targetPos = transform.position + transform.right * (left ? -1 : 1) * 2;
        move.target = targetPos;
        move.gotoTarget = true;
        move.disableRotation();
    }

    void AimAndFire(GameObject bullet)
    {
        var rb = bullet.GetComponent<Rigidbody>();
        Vector3 targetPos = target.transform.position;

        float gravity = Physics.gravity.magnitude;
        // Selected angle in radians
        float angle = projectileAngle * Mathf.Deg2Rad;

        // Positions of this object and the target on the same plane
        Vector3 planarTarget = new Vector3(targetPos.x, 0, targetPos.z);
        Vector3 planarPostion = new Vector3(transform.position.x, 0, transform.position.z);

        // Planar distance between objects
        float distance = Vector3.Distance(planarTarget, planarPostion);
        // Distance along the y axis between objects
        float yOffset = transform.position.y - targetPos.y;

        float initialVelocity = (1 / Mathf.Cos(angle)) * Mathf.Sqrt((0.5f * gravity * Mathf.Pow(distance, 2)) / (distance * Mathf.Tan(angle) + yOffset));

        Vector3 velocity = new Vector3(0, initialVelocity * Mathf.Sin(angle), initialVelocity * Mathf.Cos(angle));

        // Rotate our velocity to match the direction between the two objects
        float angleBetweenObjects = Vector3.Angle(Vector3.forward, planarTarget - planarPostion) * (targetPos.x > transform.position.x ? 1 : -1);
        Vector3 finalVelocity = Quaternion.AngleAxis(angleBetweenObjects, Vector3.up) * velocity;

        // Fire!
        rb.velocity = finalVelocity;
    }

    void Hit()
    {
        if (hitCooldown > 0) { Backup(); return; }
        hitCooldown = hitResetTime;
        Stop();

        StopAllCoroutines();
        LookAtTarget(0.75f);       

        anim.SetBool(hurtAnimParam, false);
        anim.SetBool(hitAnim, true);

        busy = true;
    }

    void JumpBack()
    {
        if (busy) return;

        StopAllCoroutines();
        StartCoroutine(_JumpBack(Player.i.transform.position));
    }

    IEnumerator _JumpBack(Vector3 threat)
    {
        var targetPos = transform.position + (threat - transform.position).normalized * - jumpDist;
        Vector2 originalPos = new Vector2(transform.position.x, transform.position.z);
        move.gotoTarget = false;

        busy = true;

        float timeLeft = jumpTime;
        float startPos = transform.position.y;

        while (timeLeft > 0) {
            float progress = timeLeft / jumpTime;

            float yPos = startPos + Mathf.Sin(progress * Mathf.PI) * jumpheight;
            Vector2 xzPos = Vector2.Lerp(originalPos, new Vector2(targetPos.x, targetPos.z), 1 - progress);
            transform.position = new Vector3(xzPos.x, yPos, xzPos.y);

            timeLeft -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        PutOnGround();

        busy = false;
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

    void Stunned()
    {
        busy = true;
        StartCoroutine(_Stunned(hitStunTime));
    }

    void InturruptAttack(AttackStats attack, string inturrupt)
    {
        anim.SetBool(attack.animBool, false);
        anim.SetBool(inturrupt, true);
    }

    IEnumerator _Stunned(float time)
    {
        yield return new WaitForSeconds(time);

        anim.SetBool(hurtAnimParam, false);
        busy = false;
    }

    void DoAnims()
    {
        var currentPos = new Vector2(transform.position.x, transform.position.z);
        float speed = Vector2.Distance(currentPos, oldPos) / Time.deltaTime;
        oldPos = currentPos;

        anim.SetBool("movingForward", speed > walkingThreshold);
        anim.SetBool("walkBack", speed < -walkingThreshold);
    }

    void Backup()
    {
        move.NormalSpeed();
        move.disableRotation();
        LookAtTarget(0.9f);
        var dir = (target.transform.position - transform.position).normalized * -1;
        move.target = transform.position + dir * 2;
        move.gotoTarget = true;
    }

    void LookAtTarget(float smoothness)
    {
        var rot = transform.localEulerAngles;
        var original = rot;
        transform.LookAt(target.transform);
        rot.y = transform.localEulerAngles.y;
        transform.localRotation = Quaternion.Lerp(Quaternion.Euler(original), Quaternion.Euler(rot), smoothness);
    }

    void MoveTowardTarget()
    {
        move.EnableRotation();
        move.NormalSpeed();
        move.target = target.transform.position;
        move.gotoTarget = true;
    }

    void Stop()
    {
        move.gotoTarget = false;
    }

    bool InAgroRange(float dist)
    {
        if (dist > agroRange) return false;

        if (!screaming && !agro) {
            anim.SetTrigger("scream");
            screaming = true;
        }


        return true;
    }

    private void OnDrawGizmos()
    {
        if (!debug) return;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, agroRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, RangedRange.x);
        Gizmos.DrawWireSphere(transform.position, RangedRange.y);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, hitRange.x);
        Gizmos.DrawWireSphere(transform.position, hitRange.y);
    }
}
