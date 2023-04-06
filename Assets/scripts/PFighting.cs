using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

public class PFighting : HitReciever
{
    [SerializeField] AudioSource swooshSource;

    [HideInInspector] public UnityEvent Attack1 = new UnityEvent();
    [HideInInspector] public UnityEvent Attack2 = new UnityEvent();
    [HideInInspector] public UnityEvent Attack3 = new UnityEvent();
    [HideInInspector] public bool attacking;
    public bool staffDrawn;

    [SerializeField] float waitTime = 1, stafDrawTime = 0.7f;
    

    [Header("Damage")]
    [SerializeField] int attack1Damage = 10;
    [SerializeField] int attack2Damage = 15, attack3Damage = 30;
    [SerializeField] float _KB = 20;

    [Header("Dependencies")]
    [SerializeField] HitBox hitBox;

    float endAttackTime = 0;
    int currentAttack = -1;
    float stunTime;
    HitBox activeHitBox;

    public void Inturrupt(float _stunTime)
    {
        stunTime = _stunTime;
        activeHitBox = null;
        EndAttack();
    }

    public void PutAwayStaff()
    {
        EndAttack();
        currentAttack = -1;
        staffDrawn = false;
    }

    public void DrawWeapon()
    {
        StartCoroutine(_DrawWeapon());
    }

    public void PressAttack()
    {
        if (stunTime > 0 || GetComponent<PMovement>().knockedBack) return;

        if (currentAttack == -1) DrawWeapon();

        if (currentAttack == 0) DoAttack1();
        else if (currentAttack == 1) DoAttack2();
        else if (currentAttack == 2) DoAttack3();
        else return;

        activeHitBox = hitBox;
        attacking = true;
        currentAttack += 1;
        endAttackTime = waitTime;
        GetComponent<PMovement>().StepForward();
    }

    public void HitCheckStaff()
    {
        if (activeHitBox == null) return;
        var hits = activeHitBox.EndChecking();
        activeHitBox = null;

        if (hits.Count == 0) return;

        foreach (var h in hits) {
            int attackDmg = currentAttack == 0 ? attack1Damage : (currentAttack == 1 ? attack2Damage : attack3Damage);
            h.Hit(attackDmg, gameObject, _KB);
        }
    }
    
    void EndAttack() {
        attacking = false;
        currentAttack = 0;
        endAttackTime = waitTime;
    }

    private void Start()
    {
        OnHit.AddListener(_Hit);
        if (currentAttack == 0) staffDrawn = true;
        endAttackTime = waitTime;
    }

    void _Hit() {
        int damage = _damage;
        Player.i.ChangeHealth(-damage);
        GetComponent<PMovement>().KnockBack(source, _KB);
    }

    private void Update()
    {
        stunTime -= Time.deltaTime;
        if (currentAttack <= 0) return;
        endAttackTime -= Time.deltaTime;
        if (endAttackTime > 0) return;

        EndAttack();
    }

    IEnumerator _DrawWeapon()
    {
        if (staffDrawn) yield break;
        staffDrawn = true;

        yield return new WaitForSeconds(stafDrawTime);

        currentAttack = 0;
    }

    void DoAttack1()
    {
        Attack1.Invoke();
        hitBox.StartChecking();
    }

    void DoAttack2()
    {
        Attack2.Invoke();
    }

    void DoAttack3()
    {
        Attack3.Invoke();
    }
}
