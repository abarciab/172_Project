using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyMovement))]
public class Golem : MonoBehaviour
{
    [SerializeField] Animator anim;
    [SerializeField] float jumpBackSpeed;
    [SerializeField] GameObject target;

    [SerializeField] List<AttackStats> attacks;

    /*[Header("Attack1")]
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
    float kickCooldown;*/

    [Header("HitBox")]
    [SerializeField] HitBox attack1HitBox;
    [SerializeField] HitBox attack2HitBox, kickHitBox;

    [SerializeField, Space()] bool debug;

    public bool stopped;
    EnemyMovement move;
    Vector3 oldPosition;
    public AttackStats currentAttack;
    bool attacking;

    private void OnValidate()
    {
        foreach (var a in attacks) a.OnValidate();
    }

    public void StartChecking()
    {
        if (currentAttack != null) currentAttack.hitBox.StartChecking(true, currentAttack.damage, currentAttack.knockBack, gameObject);
        attacking = true;
    }

    public void RefreshHB()
    {
        currentAttack.hitBox.Refresh();
    }

    public void EndAttack()
    {
        attacking = false;
        currentAttack.hitBox.EndChecking();
        currentAttack.Cooldown = currentAttack.resetTime;
        
        StartCoroutine(Resume(currentAttack.selfStunTime, currentAttack.animBool));
        print("ended attack: " + currentAttack.animBool);
        currentAttack = null;
    }

    private void Start() {
        oldPosition = transform.position;
        move = GetComponent<EnemyMovement>();
        GetComponent<EnemyStats>().OnHit.AddListener(JumpBack);
        foreach (var a in attacks) a.Reset();
    }

    private void Update() {
        SetTarget();
        SetAnims();
        DoCooldowns();

        if (target == null || stopped) { move.gotoTarget = false; return; }
        float dist = Vector3.Distance(transform.position, target.transform.position);

        if (TrySlamAttack(dist)) return;
        if (tryDoubleHit(dist)) return;
        if (TryKick(dist)) return;
        move.gotoTarget = false;
    }

    AttackStats GetAttack(AttackStats.AttackType type)
    {
        foreach (var a in attacks) if (a.type == type) return a;
        return null;
    }

    bool TrySlamAttack(float dist)
    {
        var attack = GetAttack(AttackStats.AttackType.special);
        var status = attack.status(dist);

        switch (status) {
            case AttackStats.StatusType.ready:
                StartAttack(attack);
                break;
            case AttackStats.StatusType.too_close:
                return false;
            case AttackStats.StatusType.too_far:
                MoveToTarget();
                break;
            case AttackStats.StatusType.on_cooldown:
                return false;
        }
        return true;
    }

    bool TryKick(float dist)
    {
        var attack = GetAttack(AttackStats.AttackType.basic);
        var status = attack.status(dist);

        switch (status) {
            case AttackStats.StatusType.ready:
                StartAttack(attack);
                break;
            case AttackStats.StatusType.too_close:
                JumpBack();
                break;
            case AttackStats.StatusType.on_cooldown:
                return false;
        }
        return true;
    }

    bool tryDoubleHit(float dist)
    {
        var attack = GetAttack(AttackStats.AttackType.heavy);
        var status = attack.status(dist);

        switch (status) {
            case AttackStats.StatusType.ready:
                StartAttack(attack);
                break;
            case AttackStats.StatusType.too_close:
                return false;
            case AttackStats.StatusType.too_far:
                MoveToTarget();
                break;
            case AttackStats.StatusType.on_cooldown:
                return false;
        }
        return true;
    }

    void SetTarget()
    {
        if (target == null) target = Player.i.gameObject;
        if (target == Player.i.gameObject && !Player.i.enemies.Contains(move)) Player.i.enemies.Add(move);
        if (target != Player.i.gameObject) Player.i.enemies.Remove(move);
    }

    void DoCooldowns()
    {
        foreach (var a in attacks) a.Cooldown -= Time.deltaTime;
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

    void StartAttack(AttackStats attack)
    {
        StartCoroutine(TurnToFace(0.2f, 0.1f));

        anim.SetBool(attack.animBool, true);
        move.disableRotation();
        attack.Cooldown = attack.resetTime;
        stopped = true;
        currentAttack = attack;
        print("Starting attack: " + currentAttack.animBool);
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

   

    IEnumerator Resume(float stunTime, string parameter) {
        yield return new WaitForSeconds(stunTime);
        anim.SetBool(parameter, false);
        stopped = false;
    }

    private void OnDrawGizmos()
    {
        if (!debug) return;

        foreach (var a in attacks) a.DrawGizmos(transform.position);
    }
}
