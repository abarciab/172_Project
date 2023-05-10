using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

public class Scorpion : BaseEnemy 
{
    /*
    3 attacks: launch, snip, pin

    2 phases, based on health
    in phase1:
        tries to launch ranged attacks at the player
        if they player comes close, do a little snip, then jump back and continue vollying from afar
    inf pahse2:
        stop ranged attacks unless the player runs away deliberatly
        get close to player, try to pin
        if pin is on cooldown, snip at player
     */

    [Header("RangedAttack")]
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] Vector2 RangedRange;
    [SerializeField] Vector3 projectileStartOffset, projectileSize;
    [SerializeField] float rangedResetTime, projectileAngle = 45;
    [SerializeField] int rangedDmg;
    [SerializeField, Range(0, 1)] float goopAmount;
    [SerializeField] string rangedAnim;
    float rangedCooldown;

    [Header("Snip Attack")]
    [SerializeField] HitBox snipHB;
    [SerializeField] Vector2 snipRange;
    [SerializeField] int snipDmg;
    [SerializeField] string snipAnim;
    [SerializeField] float snipKB, snipResetTime;
    float snipCooldown;

    [Header("Pin Attack")]
    [SerializeField] HitBox pinHB;
    [SerializeField] Vector2 pinRange;
    [SerializeField] int pinDmg;
    [SerializeField] string pinAnim, pinStartAnim;
    [SerializeField] float pinKB, pinResetTime;
    bool hitPin, pinning;
    float pinCooldown;

    [Header("Misc")]
    [SerializeField] Animator anim;
    [SerializeField] float phaseSwitch = 0.5f;
    int phase;

    [Header("Anims")]
    [SerializeField] string walkAnim;
    [SerializeField] float walkThreshold;

    protected override void Start()
    {
        base.Start();
        phase = 1;
        agroRange = Mathf.Infinity;
        pinHB.OnHit.AddListener(HitPin);
    }

    public override void EndAttack()
    {
        base.EndAttack();
        if (!pinning) return;
        StartCoroutine(Resume(1.5f));
        //print("Pin complete! hit: " + hitPin);
    }

    IEnumerator Resume(float time)
    {
        busy = true;
        yield return new WaitForSeconds(time);
        busy = false;
    }

    void HitPin() {
        hitPin = true;
    }

    protected override void Die()
    {
        base.Die();
        Destroy(gameObject, 1);
    }

    protected override void Update()
    {
        base.Update();
        if (busy) return;

        if (phase == 1 && (float)stats.health / stats.maxHealth <= phaseSwitch) {
            phase = 2;
            stats.Heal(1);
        }

        if (phase == 1) {
            if (dist < snipRange.y) { Snip(); JumpBack(); }
            if (dist > RangedRange.y) MoveTowardTarget();
            else if (dist > RangedRange.x) RangedAttack();
            else if (dist < RangedRange.x) Backup();
        }
        else {
            if (dist > RangedRange.x && dist < RangedRange.y && dist > pinRange.y) RangedAttack();

            if (dist > pinRange.y - 0.5f) MoveTowardTarget();
            else if (dist < pinRange.x + 0.5f) Backup();

            if (dist > pinRange.x && dist < pinRange.y && pinCooldown <= 0) StartPin();
            else if (dist < snipRange.y) Snip();
        }
        DoAnims();
    }

    

    protected override void Cooldowns()
    {
        base.Cooldowns();
        rangedCooldown -= Time.deltaTime;
        snipCooldown -= Time.deltaTime;
        pinCooldown -= Time.deltaTime;
    }

    void Snip()
    {
        if (snipCooldown > 0) {
            Backup();
            return;
        }
        //print("snip!");
        Stop();
        StopAllCoroutines();
        LookAtTarget(0.75f);

        currentAttack = new AttackDetails(snipHB, snipAnim, snipDmg, snipResetTime, snipKB, gameObject);
        snipCooldown = snipResetTime;
        StartAttack(currentAttack, anim);
        busy = true;
    }

    void RangedAttack()
    {
        OrbitPlayer();
        if (rangedCooldown > 0) return;
        rangedCooldown = rangedResetTime;
        busy = true;
        anim.SetBool(rangedAnim, true);
    }

    public void LaunchProjectile()
    {
        busy = false;
        var projectile = InstantiateProjectile(projectilePrefab, projectileStartOffset, projectileSize);
        projectile.GetComponent<GoopProjectile>().goopAmount = goopAmount;
        projectile.GetComponent<HitBox>().StartChecking(transform, rangedDmg);
        Vector3 targetPos = target.position + Player.i.speed3D;
        AimAndFire(projectile, projectileAngle, targetPos);
        anim.SetBool(rangedAnim, false);
    }

    void StartPin()
    {
        if (pinCooldown > 0) return;
        pinCooldown = pinResetTime;
        //print("starting pin!");

        pinning = busy = true;
        hitPin = false;
        LookAtTarget(0.75f);
        anim.SetTrigger(pinStartAnim);
    }

    void DoAnims()
    {
        anim.SetBool(walkAnim, Mathf.Abs(speed) > walkThreshold);
    }


    protected override void OnDrawGizmosSelected()
    {
        if (!debug) return;

        base.OnDrawGizmosSelected();

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, RangedRange.x);
        Gizmos.DrawWireSphere(transform.position, RangedRange.y);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, snipRange.x);
        Gizmos.DrawWireSphere(transform.position, snipRange.y);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, pinRange.x);
        Gizmos.DrawWireSphere(transform.position, pinRange.y);
    }


    /*
       

    [Header("RangedAttack")]
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] Vector2 RangedRange;
    [SerializeField] Vector3 projectileStartOffset, projectileSize;
    [SerializeField] float rangedResetTime, projectileAngle = 45, orbitSpeed = 1, orbitSwitchMod = 0.3f;
    [SerializeField] int rangedDmg;
    [SerializeField, Range(0, 1)] float goopAmount;
    [SerializeField] string rangedAnim;
    float rangedCooldown;

    [Header("Snip Attack")]
    [SerializeField] HitBox HB;
    [SerializeField] Vector2 hitRange;
    [SerializeField] int snipDmg;
    [SerializeField] string snipAnim;
    [SerializeField] float hitKB, hitResetTime;
    float hitCooldown;

    [Header("Pin Attack")]
    [SerializeField] int PinDmg;
    [SerializeField] string pinStartAnim, pinHitAnim;
    [SerializeField] float pinResetTime;
    float pinCooldown;

    [Header("Anims")]
    [SerializeField] Animator anim;
    [SerializeField] float walkingThreshold = 0.1f;

    [Header("Jump")]
    [SerializeField] float jumpDist;
    [SerializeField] float jumpheight, jumpTime;

    [Header("Phases")]
    [SerializeField] float healthPercentThreshold = 0.5f;
    [SerializeField] bool debug;
    bool phase2;

    Vector3 oldPos;
    bool busy, jumpBackOnEnd;
    GameObject target;
    EnemyMovement move;

    public void StartChecking()
    {
        HB.StartChecking(true, snipDmg, hitKB, gameObject);
    }

    public void EndAttack()
    {
        if (jumpBackOnEnd) JumpBack();
        busy = false;
        hitCooldown = hitResetTime;
        anim.SetBool(pinStartAnim, false);
        HB.EndChecking();
        anim.SetBool(pinHitAnim, false);
    }

    private void Update()
    {
        DoAnims();
        if (GetComponent<EnemyStats>().health <= 0) {
            anim.SetBool("dead", true);
            enabled = false;
            Destroy(gameObject, 2.5f);
            return;
        }

        if (target == null) target = Player.i.gameObject;
        float dist = Vector3.Distance(transform.position, target.transform.position);
        hitCooldown -= Time.deltaTime;
        rangedCooldown -= Time.deltaTime;


        if (busy) { Stop(); return; };
        //if (!Player.i.enemies.Contains(move)) Player.i.enemies.Add(move);
        var stats = GetComponent<EnemyStats>();
        phase2 = (stats.health / (float) stats.maxHealth) < healthPercentThreshold;

        if (!phase2) {
            if (dist > RangedRange.y) MoveTowardTarget();
            else if (dist < hitRange.y) Snip(true);
            else if (dist < RangedRange.x) Backup();
            else StartRangedAttack();
        }
        else {
            if (dist > RangedRange.y) MoveTowardTarget();
            else if (dist > RangedRange.x) { StartRangedAttack(); MoveTowardTarget(); }
            else if (dist < hitRange.y) Pin(true);
            else if (dist < RangedRange.x) Backup();
        }
    }

    private void Start()
    {
        move = GetComponent<EnemyMovement>();
        target = Player.i.gameObject;
        oldPos = new Vector2(transform.position.x, transform.position.z);
        var stats = GetComponent<EnemyStats>();
        stats.OnHit.AddListener(JumpBack);
        stats.OnHit.AddListener(GetComponentInChildren<EnemySound>().TakeHit);
        HB.OnHit.AddListener(PinHit);
    }

    void PinHit()
    {
        anim.SetBool(pinHitAnim, true);
    }

    void StartRangedAttack()
    {
        OrbitPlayer();
        if (rangedCooldown > 0) return;
        rangedCooldown = rangedResetTime;
        anim.SetTrigger(rangedAnim);
        busy = true;
    }

    public void RangedAttack()
    {
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

    void DoAnims()
    {
        var currentPos = new Vector2(transform.position.x, transform.position.z);
        float speed = Vector2.Distance(currentPos, oldPos) / Time.deltaTime;
        oldPos = currentPos;

        anim.SetBool("walking", Mathf.Abs(speed) > walkingThreshold);
    }

    void AimAndFire(GameObject bullet)
    {
        var rb = bullet.GetComponent<Rigidbody>();
        Vector3 targetPos = target.transform.position;

        float gravity = Physics.gravity.magnitude;
        float angle = projectileAngle * Mathf.Deg2Rad;

        Vector3 planarTarget = new Vector3(targetPos.x, 0, targetPos.z);
        Vector3 planarPostion = new Vector3(transform.position.x, 0, transform.position.z);

        float distance = Vector3.Distance(planarTarget, planarPostion);
        float yOffset = transform.position.y - targetPos.y;

        float initialVelocity = (1 / Mathf.Cos(angle)) * Mathf.Sqrt((0.5f * gravity * Mathf.Pow(distance, 2)) / (distance * Mathf.Tan(angle) + yOffset));
        Vector3 velocity = new Vector3(0, initialVelocity * Mathf.Sin(angle), initialVelocity * Mathf.Cos(angle));

        float angleBetweenObjects = Vector3.Angle(Vector3.forward, planarTarget - planarPostion) * (targetPos.x > transform.position.x ? 1 : -1);
        Vector3 finalVelocity = Quaternion.AngleAxis(angleBetweenObjects, Vector3.up) * velocity;

        rb.velocity = finalVelocity;
    }

    void Snip(bool jumpBackWhenDone = false)
    {
        if (hitCooldown > 0) { Backup(); return; }
        hitCooldown = hitResetTime;
        Stop();

        StopAllCoroutines();
        LookAtTarget(0.75f);

        anim.SetBool(snipAnim, true);
        busy = true;
        jumpBackOnEnd = jumpBackWhenDone;
    }

    void Pin(bool snipIfCooldown = false)
    {
        if (pinCooldown > 0) { 
            if (snipIfCooldown) Snip(); 
            return; 
        }
        pinCooldown = pinResetTime;
        Stop();

        StopAllCoroutines();
        LookAtTarget(0.75f);

        anim.SetBool(pinStartAnim, true);
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

    void PutOnGround()
    {
        int layerMask = 1 << 7;
        Physics.Raycast(transform.position + Vector3.up * 100, Vector3.down, out var hit, 150, layerMask: layerMask);
        if (hit.collider == null) return;
        var pos = transform.position;
        pos.y = hit.point.y;
        transform.position = pos;
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

    private void OnDrawGizmos()
    {
        if (!debug) return;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, RangedRange.x);
        Gizmos.DrawWireSphere(transform.position, RangedRange.y);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, hitRange.x);
        Gizmos.DrawWireSphere(transform.position, hitRange.y);
    }
    */

}
