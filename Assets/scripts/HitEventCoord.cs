using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitEventCoord : MonoBehaviour
{
    public void GolemAttack1() {
        var golem = GetComponentInParent<Golem>();
        if (golem != null) golem.HitCheckAttack1();
    }

    public void PlayerStaffAttack()
    {
        var player = GetComponentInParent<PFighting>();
        if (player != null) player.HitCheckStaff();
    }
}
