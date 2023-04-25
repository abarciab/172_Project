using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitEventCoord : MonoBehaviour
{
    public void CorruptEndAttack()
    {
        var corrupt = GetComponentInParent<CorruptExplorer>();
        corrupt.EndAttack();
    }

    public void CorruptStartCheck()
    {
        var corrupt = GetComponentInParent<CorruptExplorer>();
        corrupt.StartChecking();
    }

    public void CorruptEndScream()
    {
        var corrupt = GetComponentInParent<CorruptExplorer>();
        corrupt.EndScream();
    }

    public void GoatStartCheck()
    {
        var goat = GetComponentInParent<Goat>();
        if (goat != null) goat.StartChecking();
    }
    public void GoatEndCheck()
    {
        var goat = GetComponentInParent<Goat>();
        if (goat != null) goat.EndChecking();
    }

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
