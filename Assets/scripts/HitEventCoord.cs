using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitEventCoord : MonoBehaviour
{
    public void StartDigCount()
    {
        var arm = GetComponentInParent<Armadillo>();
        if (arm) arm.StartDigCountdown();
    }

    public void FinishUnroll()
    {
        var arm = GetComponentInParent<Armadillo>();
        if (arm) arm.FinishUnroll();
    }

    public void StartArmRoll()
    {
        var arm = GetComponentInParent<Armadillo>();
        if (arm) arm.RollMove();
    }

    public void ScorpEndAttack()
    {
        var scorp = GetComponentInParent<Scorpion>();
        if (scorp) scorp.EndAttack();
    }

    public void StartChecking()
    {
        var enemy = GetComponentInParent<BaseEnemy>();
        if (enemy) enemy.StartChecking();
    }

    public void EndAttack()
    {
        var enemy = GetComponentInParent<BaseEnemy>();
        if (enemy) enemy.EndAttack();
    }

    public void DefenderCharge()
    {
        var defeneder = GetComponentInParent<CorruptDefender>();
        if (defeneder) defeneder.Charge();
    }

    public void ScorpLaunch()
    {
        var scorp = GetComponentInParent<Scorpion>();
        if (scorp) scorp.LaunchProjectile();
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

    public void CorruptLaunch()
    {
        var corrupt = GetComponentInParent<CorruptExplorer>();
        corrupt.LaunchProjectile();
    }

    public void CorruptEndScream()
    {
        var corrupt = GetComponentInParent<CorruptExplorer>();
        corrupt.EndScream();
    }


    public void StartPlayerCheck()
    {
        var player = GetComponentInParent<PFighting>();
        if (player != null) player.StartChecking();
    }

    public void SwitchSpear()
    {
        var player = GetComponentInParent<PFighting>();
        if (player != null) player.SwapSpear();
    }

    public void EndPlayerAttack()
    {
        var player = GetComponentInParent<PFighting>();
        if (player != null) player.EndAttack();
    }
}
