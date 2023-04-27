using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

public class PFightingOLD : HitReciever
{

    [SerializeField] AudioSource swooshSource;
    public bool staffDrawn;

    [SerializeField] List<AttackStats> attacks = new List<AttackStats>();

    [Header("Dependencies")]
    [SerializeField] HitBox hitBox;

    [HideInInspector] public bool basicAttacking, hvyAttacking;
    AttackStats currentAttack;

    private void OnValidate()
    {
        foreach (var a in attacks) {
            a.OnValidate();
        }
    }

    public void Inturrupt(float _stunTime)
    {
        EndAttack();
    }

    public void PutAwayStaff()
    {
        EndAttack();
        staffDrawn = false;
    }

    public void DrawWeapon()
    {
        staffDrawn = true;
    }

    public void RefreshHitBox()
    {
        hitBox.Refresh();
    }

    public void StartAttack(AttackStats.AttackType attack)
    {
        if (basicAttacking || hvyAttacking) return;
        var a = GetAttackFromType(attack);

        if (!staffDrawn) DrawWeapon();

        basicAttacking = a.type == AttackStats.AttackType.basic;
        hvyAttacking = !basicAttacking;
        currentAttack = a;
    }

    public void StartChecking()
    {
        hitBox.StartChecking(true, currentAttack.damage, currentAttack.knockBack, gameObject);
    }

    public void EndAttack(float delay)
    {
        StartCoroutine(waitThenEndAttack(delay));
    }

    AttackStats GetAttackFromType(AttackStats.AttackType type)
    {
        foreach (var a in attacks) if (a.type == type) return a;
        return attacks[0];
    }


    void EndAttack() {
        basicAttacking = hvyAttacking = false;
        hitBox.EndChecking();
    }

    IEnumerator waitThenEndAttack(float delay)
    {
        yield return new WaitForSeconds(delay);
        EndAttack();
    }

    public override void Hit(HitData hit)
    {
        base.Hit(hit);

        if (GetComponent<PMovement>().rolling) return;

        Player.i.ChangeHealth(-hit.damage);
        GetComponent<PMovement>().KnockBack(hit.source, hit.KB, hit.offset);
    }
}
