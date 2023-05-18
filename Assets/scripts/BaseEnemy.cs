using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyMovement), typeof(EnemyStats))]
public class BaseEnemy : MonoBehaviour 
{
    [System.Serializable]
    protected class AttackDetails
    {
        public HitBox HB;
        public string animBool;
        float resetTime, KB;
        int damage;
        GameObject obj;

        public AttackDetails() {}

        public AttackDetails(HitBox HB, string animBool, int damage = 0, float resetTime = 0, float KB = 0, GameObject obj = null)
        {
            this.HB = HB;
            this.animBool = animBool;
            this.resetTime = resetTime;
            this.KB = KB;
            this.damage = damage;
            this.obj = obj;
        }

        public void StartChecking()
        {
            HB.StartChecking(true, damage, KB, obj);
        }

        public void ResetCooldown(ref float cooldown)
        {
            cooldown = resetTime;
        }
    }

    protected EnemyMovement move;
    protected EnemyStats stats;
    protected Transform target;
    protected float dist, speed;
    protected bool busy, inAgroRange, stunned;
    [SerializeField] protected float agroRange;
    [SerializeField] protected bool debug;
    [SerializeField] int meleePriority;
    Vector3 oldPos;

    [Header("Jump")]
    [SerializeField] float jumpDist;
    [SerializeField] float jumpheight, jumpTime;

    [Header("Orbit")]
    [SerializeField] protected float orbitSpeed;
    [SerializeField] protected float orbitSwitchMod;
    float orbitOffset;
    protected AttackDetails currentAttack;

    

    public float Dist()
    {
        return dist;
    }

    public int MeleePriority()
    {
        return meleePriority;
    }

    protected void AimAndFire(GameObject projectile, float verticalAngle)
    {
        AimAndFire(projectile, verticalAngle, target.position);
    }

    protected void AimAndFire(GameObject projectile, float verticalAngle, Vector3 targetPos)
    {
        var rb = projectile.GetComponent<Rigidbody>();

        float gravity = Physics.gravity.magnitude;
        // Selected angle in radians
        float angle = verticalAngle * Mathf.Deg2Rad;

        // Positions of this object and the target on the same plane
        Vector3 planarTarget = new Vector3(targetPos.x, 0, targetPos.z);
        Vector3 planarPostion = new Vector3(transform.position.x, 0, transform.position.z);

        // Distance along the y axis between objects
        float yOffset = transform.position.y - targetPos.y;

        float initialVelocity = (1 / Mathf.Cos(angle)) * Mathf.Sqrt((0.5f * gravity * Mathf.Pow(dist, 2)) / (dist * Mathf.Tan(angle) + yOffset));
        Vector3 velocity = new Vector3(0, initialVelocity * Mathf.Sin(angle), initialVelocity * Mathf.Cos(angle));

        // Rotate our velocity to match the direction between the two objects
        float angleBetweenObjects = Vector3.Angle(Vector3.forward, planarTarget - planarPostion) * (targetPos.x > transform.position.x ? 1 : -1);
        Vector3 finalVelocity = Quaternion.AngleAxis(angleBetweenObjects, Vector3.up) * velocity;

        // Fire!
        rb.velocity = finalVelocity;
    }

    protected GameObject InstantiateProjectile(GameObject prefab, Vector3 offset, Vector3 scale)
    {
        var bullet = Instantiate(prefab, transform);
        bullet.transform.localPosition = offset;
        bullet.transform.parent = null;
        bullet.transform.localScale = scale;
        return bullet;
    }

    virtual public void StartChecking()
    {
        if (currentAttack != null) currentAttack.StartChecking();
    }

    virtual protected void StartAttack(AttackDetails attack, Animator anim)
    {
        anim.SetBool(attack.animBool, true);
        currentAttack = attack;
    }

    virtual public void EndAttack()
    {
        busy = false;
    }

    virtual protected void Awake()
    {
        move = GetComponent<EnemyMovement>();
        stats = GetComponent<EnemyStats>();
    }

    virtual protected void Start()
    {
        target = Player.i.gameObject.transform;
        orbitOffset = Random.Range(0, 1000);
        oldPos = new Vector2(transform.position.x, transform.position.z);

        stats.OnHit.AddListener(JumpBack);
    }
    
    virtual protected void Update()
    {
        stunned = stats.stunTimeLeft > 0;
        GetDist();
        SetSpeed();
        Cooldowns();

        if (stats.dead()) { Die(); return; }
        CheckAgro();
    }

    void CheckAgro()
    {
        inAgroRange =  dist < agroRange;
        Player.i.Notify(this, inAgroRange);
    }

    void GetDist()
    {
        var pos = new Vector2(transform.position.x, transform.position.z);
        var targetPos = new Vector2(target.position.x, target.position.z);
        dist = Vector2.Distance(pos, targetPos);
    }

    void SetSpeed()
    {
        var currentPos = new Vector2(transform.position.x, transform.position.z);
        speed = Vector2.Distance(currentPos, oldPos) / Time.deltaTime;
        oldPos = currentPos;
    }

    virtual protected void Die()
    {
        Stop();
        enabled = false;
        Player.i.Notify(this, false);
    }

    virtual protected void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, agroRange);
    }

    virtual protected void JumpBack()
    {
        agroRange = Mathf.Infinity;
        if (busy) return;

        StopAllCoroutines();
        StartCoroutine(_JumpBack(Player.i.transform.position));
    }

    virtual protected void Cooldowns() {}

    IEnumerator _JumpBack(Vector3 threat)
    {
        var targetPos = transform.position + (threat - transform.position).normalized * -jumpDist;
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

    virtual protected void Stun(float time)
    {
        busy = true;
        StartCoroutine(DoStun(time));
    }
    virtual protected IEnumerator DoStun(float time)
    {
        yield return new WaitForSeconds(time);
        busy = false;
    }

    virtual protected void Backup()
    {
        move.NormalSpeed();
        move.disableRotation();
        LookAtTarget(0.9f);
        var dir = (target.transform.position - transform.position).normalized * -1;
        move.target = transform.position + dir * 2;
        move.gotoTarget = true;
    }

    virtual protected void OrbitPlayer()
    {
        move.ChangeSpeed(orbitSpeed);
        LookAtTarget(1);
        bool left = Mathf.Sin((Time.time * orbitSwitchMod + orbitOffset) % Mathf.PI) < 0.5f;
        var targetPos = transform.position + transform.right * (left ? -1 : 1) * 2;
        move.target = targetPos;
        move.gotoTarget = true;
        move.disableRotation();
    }
    
    virtual protected void LookAtTarget(float smoothness)
    {
        var rot = transform.localEulerAngles;
        var original = rot;
        transform.LookAt(target.transform);
        rot.y = transform.localEulerAngles.y;
        transform.localRotation = Quaternion.Lerp(Quaternion.Euler(original), Quaternion.Euler(rot), smoothness);
    }

    virtual protected void MoveTowardTarget()
    {
        move.EnableRotation();
        move.NormalSpeed();
        move.target = target.transform.position;
        move.gotoTarget = true;
    }

    virtual protected void Stop()
    {
        move.gotoTarget = false;
    }

    virtual protected void PutOnGround()
    {
        int layerMask = 1 << 7;
        Physics.Raycast(transform.position + Vector3.up * 100, Vector3.down, out var hit, 150, layerMask: layerMask);
        if (hit.collider == null) return;
        var pos = transform.position;
        pos.y = hit.point.y;
        transform.position = pos;
    }
}
