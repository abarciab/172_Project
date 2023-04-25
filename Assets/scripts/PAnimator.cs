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

    bool attacking;

    private void Start()
    {
        p = Player.i;
        move = GetComponent<PMovement>();
        fight = GetComponent<PFighting>();
        fight.OnHit.AddListener(GetHurt);
    }

    void GetHurt()
    {
        anim.SetTrigger("hurt");
    }

    private void Update()
    {
        bool moving = Mathf.Abs(p.forwardSpeed) > minWalkSpeed;

        if (move.posing) anim.SetBool("HandsOnHips", move.posing);

        anim.SetBool("Hurt", move.knockedBack);
        anim.SetBool("Sitting", move.sitting);
        anim.SetBool("Moving", moving);
        anim.SetBool("Backwards", moving && p.forwardSpeed <= -0.1f);
        anim.SetBool("Running", move.running && !move.posing);
        anim.SetBool("TurningLeft", move.turnLeft && !moving);
        anim.SetBool("TurningRight", move.turnRight && !moving);
        anim.SetBool("StaffDrawn", fight.staffDrawn && !move.posing);
        anim.SetBool("Strafe", move.strafe && Mathf.Abs(GetComponent<Rigidbody>().velocity.x + GetComponent<Rigidbody>().velocity.z) > 0.01f);

        anim.SetBool("Attacking", attacking);

        if (!attacking && fight.basicAttacking) {
            attacking = true;
            anim.SetTrigger("BasicAttack");
        }
        if (!attacking && fight.hvyAttacking) {
            attacking = true;
            anim.SetTrigger("StrongAttack");
        }
        if (!fight.hvyAttacking && !fight.basicAttacking) {
            attacking = false;
        }


        if (triggeredDash && !move.rolling) triggeredDash = false;
        if (move.rolling && !triggeredDash && !attacking) {
            triggeredDash = true;
            anim.SetTrigger("Roll");
        }
    }
}
