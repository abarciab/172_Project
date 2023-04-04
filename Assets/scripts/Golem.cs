using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyMovement))]
public class Golem : MonoBehaviour
{
    [SerializeField] Animator anim;
    [SerializeField] float attackRange = 1;
    [SerializeField] GameObject target;

    [Header("Attacks")]
    [SerializeField] int attack1Damage;
    [SerializeField] float attack1resetTime, attack1StunTime, attack1KB;
    float attack1Cooldown;
    [SerializeField] int quickAttackDamage;
    [SerializeField] float quickAttackresetTime, quickAttackStunTime, quickAttackKB;
    float quickAttackCooldown;

    [Header("HitBox")]
    [SerializeField] HitBox attack1HitBox;
    [SerializeField] HitBox quickAttackHitBox;

    bool stopped;
    EnemyMovement move;
    Vector3 oldPosition;
    HitBox activeHitBox;

    public void StartChecking()
    {
        if (activeHitBox != null) activeHitBox.StartChecking();
        activeHitBox = null;
    }

    private void Start() {
        oldPosition = transform.position;
        move = GetComponent<EnemyMovement>();
        attack1Cooldown = 0;
    }

    private void Update() {
        if (target == null) target = Player.i.gameObject;
        if (target == Player.i.gameObject && !Player.i.enemies.Contains(move)) Player.i.enemies.Add(move);
        if (target != Player.i.gameObject) Player.i.enemies.Remove(move);
        SetAnims();

        attack1Cooldown -= Time.deltaTime;
        quickAttackCooldown -= Time.deltaTime;
        if (target == null || stopped) { move.gotoTarget = false; return; }

        float dist = Vector3.Distance(transform.position, target.transform.position);

        if (dist > attackRange) MoveToTarget();
        else if (attack1Cooldown <= 0) DoAttack1();
        else if (quickAttackCooldown <= 0) DoQuickAttack(); 
        else BackUpFromTarget();
    }

    void SetAnims() {
        anim.SetBool("moving", (Vector3.Distance(transform.position, oldPosition) > 0.001f));
        oldPosition = transform.position;
    }    

    void MoveToTarget() {
        move.EnableRotation();
        move.target = target.transform.position;
        move.gotoTarget = true;
    }

    void BackUpFromTarget()
    {
        move.disableRotation();
        var dir = transform.forward * -5 + transform.position;
        move.target = dir;
        move.gotoTarget = true;
    }

    void DoAttack1() {
        StartCoroutine(TurnToFace(0.2f, 0.1f));

        move.disableRotation();
        anim.SetBool("attack1", true);
        attack1Cooldown = attack1resetTime;
        activeHitBox = attack1HitBox;
        stopped = true;
    }

    void DoQuickAttack() {
        StartCoroutine(TurnToFace(0.1f, 0.1f));

        move.disableRotation();
        anim.SetBool("quickAttack", true);
        quickAttackCooldown = quickAttackresetTime;
        activeHitBox = quickAttackHitBox;
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
        StartCoroutine(Resume(attack1StunTime, "attack1"));
        var hits = attack1HitBox.EndChecking();
        if (hits.Count == 0) return;

        foreach (var h in hits) {
            h.Hit(attack1Damage, gameObject, attack1KB);
        }
    }

    public void HitCheckQuickAttack() {
        StartCoroutine(Resume(quickAttackStunTime, "quickAttack"));
        var hits = quickAttackHitBox.EndChecking();
        if (hits.Count == 0) return;

        foreach (var h in hits) {
            h.Hit(quickAttackDamage, gameObject, quickAttackKB);
        }
    }


    IEnumerator Resume(float stunTime, string parameter) {
        yield return new WaitForSeconds(stunTime);
        anim.SetBool(parameter, false);
        stopped = false;
    }
}
