using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyMovement))]
public class Golem : MonoBehaviour
{
    [SerializeField] Animator anim;
    [SerializeField] float jumpBackSpeed;
    [SerializeField] GameObject target;

    [Header("Attack1")]
    [SerializeField] int attack1Damage, attack2Damage;
    [SerializeField] float attack1resetTime, attack1StunTime, attack1KB, attack1Range;
    float attack1Cooldown;

    [Header("slamAttack")]
    [SerializeField] int slamAttackDamage;
    [SerializeField] float slamRange, slamResetTime;
    float slamCooldown;

    [Header("Kick")]
    [SerializeField] int kickDamage;
    [SerializeField] float kickResetTime, kickStunTime, kickKB, kickRange;
    float kickCooldown;

    [Header("HitBox")]
    [SerializeField] HitBox attack1HitBox;
    [SerializeField] HitBox attack2HitBox, kickHitBox;

    [SerializeField, Space()] bool debug;

    public bool stopped;
    EnemyMovement move;
    Vector3 oldPosition;
    HitBox activeHitBox;
    bool attacking;

    public void StartChecking()
    {
        if (activeHitBox != null) activeHitBox.StartChecking();
        activeHitBox = null;
        attacking = true;
    }

    private void Start() {
        oldPosition = transform.position;
        move = GetComponent<EnemyMovement>();
        attack1Cooldown = 0;
        GetComponent<EnemyStats>().OnHit.AddListener(JumpBack);
    }

    private void Update() {
        if (target == null) target = Player.i.gameObject;
        if (target == Player.i.gameObject && !Player.i.enemies.Contains(move)) Player.i.enemies.Add(move);
        if (target != Player.i.gameObject) Player.i.enemies.Remove(move);
        SetAnims();

        attack1Cooldown -= Time.deltaTime;
        kickCooldown -= Time.deltaTime;
        slamCooldown -= Time.deltaTime;
        if (target == null || stopped) { move.gotoTarget = false; return; }

        float dist = Vector3.Distance(transform.position, target.transform.position);

        if (dist < kickRange) {
            if (kickCooldown <= 0) Kick();
            else JumpBack();
        }
        else if (dist < attack1Range && attack1Cooldown <= 0) Attack();
        else if (dist > attack1Range && dist < slamRange && slamCooldown <= 0) SlamAttack();
        
        if (dist > slamRange || (dist > attack1Range && slamCooldown > 0) || (dist > kickRange && attack1Cooldown > 0)) MoveToTarget();
    }

    void SetAnims() {
        anim.SetBool("moving", (Vector3.Distance(transform.position, oldPosition) > 0.001f));
        oldPosition = transform.position;
    }    

    void MoveToTarget() {
        move.NormalSpeed();
        move.EnableRotation();
        move.target = target.transform.position;
        move.gotoTarget = true;
    }

    void JumpBack()
    {
        if (attacking || stopped) return;

        move.disableRotation();
        var dir = transform.forward * -5 + transform.position;
        move.ChangeSpeed(jumpBackSpeed);
        move.target = dir;
        move.gotoTarget = true;
    }

    void SlamAttack()
    {
        //print("I WANT TO SLAM");
    }

    void Attack() {
        StartCoroutine(TurnToFace(0.2f, 0.1f));

        move.disableRotation();
        anim.SetBool("attack1", true);
        attack1Cooldown = attack1resetTime;
        slamCooldown = slamResetTime;
        activeHitBox = attack1HitBox;
        stopped = true;
    }

    void Attack2()
    {
        StartCoroutine(TurnToFace(0.2f, 0.1f));

        move.disableRotation();
        activeHitBox = attack2HitBox;
        stopped = true;
    }

    void Kick() {
        StartCoroutine(TurnToFace(0.1f, 0.1f));

        move.disableRotation();
        anim.SetBool("kick", true);
        kickCooldown = kickResetTime;
        activeHitBox = kickHitBox;
        stopped = true;
    }

    IEnumerator TurnToFace(float time, float smoothness) {
        while (time > 0) {
            if (target == null) yield break;
            time -= Time.deltaTime;
            var rot = transform.localEulerAngles;
            transform.LookAt(target.transform);
            var targetRot = new Vector3(rot.x, transform.localEulerAngles.y, rot.z);
            transform.rotation = Quaternion.Lerp(Quaternion.Euler(rot), Quaternion.Euler(targetRot), smoothness);
            yield return new WaitForEndOfFrame();
        }
    }

    public void HitCheckAttack1() {
        var hits = attack1HitBox.EndChecking();

        Attack2();

        if (hits.Count == 0) return;

        foreach (var h in hits) {
            h.Hit(attack1Damage, gameObject, attack1KB);
        }
    }
    public void HitCheckAttack2()
    {
        attacking = false;
        StartCoroutine(Resume(attack1StunTime, "attack1"));
        var hits = attack2HitBox.EndChecking();
        if (hits.Count == 0) return;

        foreach (var h in hits) {
            h.Hit(attack1Damage, gameObject, attack1KB);
        }
    }

    public void HitCheckKick() {
        attacking = false;
        StartCoroutine(Resume(kickStunTime, "kick"));
        var hits = kickHitBox.EndChecking();
        if (hits.Count == 0) return;

        foreach (var h in hits) {
            h.Hit(kickDamage, gameObject, kickKB);
        }
        attack1Cooldown = 0;
    }


    IEnumerator Resume(float stunTime, string parameter) {
        yield return new WaitForSeconds(stunTime);
        anim.SetBool(parameter, false);
        stopped = false;
    }

    private void OnDrawGizmos()
    {
        if (!debug) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, attack1Range);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, kickRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, slamRange);
    }
}
