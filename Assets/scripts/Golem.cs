using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyMovement))]
public class Golem : MonoBehaviour
{
    [SerializeField] Animator anim;
    [SerializeField] float attackRange = 1, backupRange;
    [SerializeField] GameObject target;

    [Header("Attacks")]
    [SerializeField] int attack1Damage;
    [SerializeField] float attack1resetTime;
    float attack1Cooldown;

    [Header("HitBox")]
    [SerializeField] HitBox attack1HitBox;

    bool stopped;

    EnemyMovement move;
    Vector3 oldPosition;

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
        float dist = Vector3.Distance(transform.position, target.transform.position);

        if (stopped) return;
        if (attack1Cooldown > 0 && dist <= backupRange) {
            BackUpFromTarget();
            return;
        }
        if (dist > attackRange) {
            MoveToTarget();
            return;
        }
        else move.gotoTarget = false;

        if (attack1Cooldown <= 0) DoAttack1();
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
        anim.SetTrigger("attack1");
        attack1Cooldown = attack1resetTime;
        attack1HitBox.StartChecking();
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
        stopped = false;
        var hits = attack1HitBox.EndChecking();
        if (hits.Count == 0) return;

        foreach (var h in hits) {
            h.Hit(attack1Damage);
        }
    }
}
