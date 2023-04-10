using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AttackStats
{
    public enum AttackType { none, basic, heavy, special};

    public string name;
    public AttackType type;
    public int damage;
    public float knockBack;

    public bool extras;
    [ConditionalHide(nameof(extras))]
    public float range, resetTime, selfStunTime;
    [ConditionalHide(nameof(extras))]
    HitBox attackHB;

    [HideInInspector] public float attackCooldown;

    public void OnValidate()
    {
        name = string.IsNullOrEmpty(name) ? type.ToString() : name;
    }
}
