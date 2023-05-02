using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitEventCoord : MonoBehaviour
{
    public void ScorpEndAttack()
    {
        var scorp = GetComponentInParent<Scorpion>();
        if (scorp) scorp.EndAttack();
    }

    public void ScorpLaunch()
    {
        var scorp = GetComponentInParent<Scorpion>();
        if (scorp) scorp.RangedAttack();
    }
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

    public void EndPlayerAttack()
    {
        var player = GetComponentInParent<PFighting>();
        if (player != null) player.EndAttack();
    }
}
