using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AttackStats
{
    public enum AttackType { none, basic, heavy, special};
    public enum StatusType { ready, too_close, too_far, on_cooldown}

    public string name;
    public AttackType type;
    public int damage;
    public float knockBack;

    public bool extras;
    [ConditionalHide(nameof(extras))]
    public float resetTime, selfStunTime;
    [ConditionalHide(nameof(extras))]
    public Vector2 range;
    [ConditionalHide(nameof(extras))]
    public HitBox hitBox;
    [ConditionalHide(nameof(extras))]
    public Color gizmosColor;
    [ConditionalHide(nameof(extras))]
    public bool drawGizmo;
    [ConditionalHide(nameof(extras))]
    public string animBool;


    [HideInInspector] public float Cooldown;

    public void OnValidate()
    {
        name = string.IsNullOrEmpty(name) ? type.ToString() : name;
    }

    public StatusType status(float dist = 0)
    {
        if (Cooldown >= 0) return StatusType.on_cooldown;
        if (dist > range.y) return StatusType.too_far;
        if (dist < range.x) return StatusType.too_close;
        return StatusType.ready;
    }

    public void Reset()
    {
        Cooldown = resetTime;
    }

    public void DrawGizmos(Vector3 position)
    {
        if (!drawGizmo) return;

        Gizmos.color = gizmosColor;
        Gizmos.DrawWireSphere(position, range.x);
        Gizmos.DrawWireSphere(position, range.y);
    }
}
