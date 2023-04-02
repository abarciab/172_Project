using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PFighting : HitReciever
{
    [SerializeField] AudioSource swooshSource;

    [HideInInspector] public UnityEvent Attack1 = new UnityEvent();
    [HideInInspector] public UnityEvent Attack2 = new UnityEvent();
    [HideInInspector] public UnityEvent Attack3 = new UnityEvent();
    [HideInInspector] public bool attacking;
    public bool staffDrawn;

    [SerializeField] float waitTime = 1, stafDrawTime = 0.7f;
    float endAttackTime = 0;
    public int currentAttack = -1;

    public void PutAwayStaff()
    {
        EndAttack();
        currentAttack = -1;
        staffDrawn = false;
    }

    public void PressAttack()
    {
        if (currentAttack == -1) { StartCoroutine(DrawWeapon()); return; }

        if (currentAttack == 0) DoAttack1();
        else if (currentAttack == 1) DoAttack2();
        else if (currentAttack == 2) DoAttack3();
        else return;

        attacking = true;
        currentAttack += 1;
        endAttackTime = waitTime;
        GetComponent<PMovement>().StepForward();
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
    }

    private void Update()
    {
        if (currentAttack <= 0) return;
        endAttackTime -= Time.deltaTime;
        if (endAttackTime > 0) return;

        EndAttack();
    }

    IEnumerator DrawWeapon()
    {
        if (staffDrawn) yield break;
        staffDrawn = true;

        yield return new WaitForSeconds(stafDrawTime);

        currentAttack = 0;
    }

    

    void DoAttack1()
    {
        Attack1.Invoke();
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
