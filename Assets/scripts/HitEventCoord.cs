using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitEventCoord : MonoBehaviour
{
   
    public void GolemStartChecking()
    {
        var golem = GetComponentInParent<Golem>();
        if (golem != null) golem.StartChecking();
    }
    public void EndGolemAttack()
    {
        var golem = GetComponentInParent<Golem>();
        if (golem != null) golem.EndAttack();
    }

    public void RefreshGolemHB()
    {
        var golem = GetComponentInParent<Golem>();
        if (golem != null) golem.RefreshHB();
    }

    public void StartPlayerCheck()
    {
        var player = GetComponentInParent<PFighting>();
        if (player != null) player.StartChecking();
    }

    public void EndPlayerAttack(float delay = 0)
    {
        var player = GetComponentInParent<PFighting>();
        if (player != null) player.EndAttack(delay);
    }

    public void RefreshPlayerHB()
    {
        var player = GetComponentInParent<PFighting>();
        if (player != null) player.RefreshHitBox();
    }
}
