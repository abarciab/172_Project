using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitEventCoord : MonoBehaviour
{
    public void GolemAttack1() {
        var golem = GetComponentInParent<Golem>();
        if (golem != null) golem.HitCheckAttack1();
    }

    public void GolemAttack2()
    {
        var golem = GetComponentInParent<Golem>();
        if (golem != null) golem.HitCheckAttack2();
    }

    public void GolemStartChecking()
    {
        var golem = GetComponentInParent<Golem>();
        if (golem != null) golem.StartChecking();
    }

    public void GolemQuickAttack() {
        var golem = GetComponentInParent<Golem>();
        if (golem != null) golem.HitCheckKick();
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
