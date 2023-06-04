using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PAnimator : MonoBehaviour
{
    [Header("parameters")]
    [SerializeField] float minWalkSpeed = 0.1f, minChargeTime = 0.06f;

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
        bool frozen = GlobalUI.i.DisplayingImage();
        bool moving = Mathf.Abs(p.forwardSpeed) > minWalkSpeed && !frozen;

        if (move.posing) anim.SetBool("HandsOnHips", move.posing);

        anim.SetBool("Hurt", move.knockedBack);
        anim.SetBool("Sitting", move.sitting);
        anim.SetBool("Moving", moving);
        anim.SetBool("Backwards", moving && p.forwardSpeed <= -0.1f);
        anim.SetBool("Running", move.running && !move.posing && !move.rolling);
        anim.SetBool("TurningLeft", move.turnLeft && !moving);
        anim.SetBool("TurningRight", move.turnRight && !moving);
        anim.SetBool("StaffDrawn", fight.SpearOut());
        anim.SetBool("chargingThrow", fight.chargingSpear() && fight.chargeTime > minChargeTime);
        anim.SetBool("hasSpear", fight.HasSpear());

        anim.SetBool("Talking", GlobalUI.i.talking);
        
        anim.SetBool("Strafe", !frozen && move.strafe && Mathf.Abs(GetComponent<Rigidbody>().velocity.x + GetComponent<Rigidbody>().velocity.z) > 0.01f);

        

        if (!attacking && fight.Stabbing()) {
            attacking = true;
            anim.SetTrigger("BasicAttack");
        }

        if (!fight.Stabbing() || frozen) {
            attacking = false;
        }

        anim.SetBool("Attacking", attacking);

        if ((move.running || move.rolling || move.knockedBack) && attacking) {
            fight.EndAttack();
        }

        if (triggeredDash && !move.rolling) triggeredDash = false;
        if (move.rolling && !triggeredDash && !attacking) {
            triggeredDash = true;
            anim.SetTrigger("Roll");
        }
    }
}
