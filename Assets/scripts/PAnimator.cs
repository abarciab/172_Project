using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PAnimator : MonoBehaviour
{
    [Header("parameters")]
    [SerializeField] float minWalkSpeed = 0.1f;

    [Header("Dependencies")]
    [SerializeField] Animator anim;
    Player p;
    PMovement move;
    PFighting fight;
    bool triggeredDash;

    private void Start()
    {
        p = Player.i;
        move = GetComponent<PMovement>();
        fight = GetComponent<PFighting>();

        fight.Attack1.AddListener(Attack1);
        fight.Attack2.AddListener(Attack2);
        fight.Attack3.AddListener(Attack3);     
    }

    private void Update()
    {
        bool moving = Mathf.Abs(p.forwardSpeed) > minWalkSpeed;

        anim.SetBool("Sitting", move.sitting);
        anim.SetBool("Moving", moving);
        anim.SetBool("Running", move.running);
        anim.SetBool("TurningLeft", move.turnLeft && !moving);
        anim.SetBool("TurningRight", move.turnRight && !moving);
        anim.SetBool("StaffDrawn", fight.staffDrawn);
        anim.SetBool("Attacking", fight.attacking);

        if (triggeredDash && !move.dashing) triggeredDash = false;
        if (move.dashing && !triggeredDash) {
            triggeredDash = true;
            anim.SetTrigger("Roll");
        }
    }

    void Attack1() { anim.SetTrigger("Attack1");  }
    void Attack2() { anim.SetTrigger("Attack2");  }
    void Attack3() { anim.SetTrigger("Attack3");  }
}
