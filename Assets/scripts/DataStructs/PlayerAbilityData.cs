using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AbilityType { STAB, THROW, SPECIAL }
public enum AbilityEffectType { NONE, PHYSICAL, POISON, CHAIN, PIN, SUNBLAST, CLEAN, BLOCK, MIRAGE }

[CreateAssetMenu(fileName = "new Ability", menuName = "Ability")]
public class PlayerAbilityData : ScriptableObject
{
    public string Name;
    public Sprite Icon;

    public AbilityType Type;

    [ConditionalField(nameof(Type), true, AbilityType.THROW)] public float Cooldown;
    [ConditionalField(nameof(Type), false, AbilityType.STAB)] public string StabAnimationTrigger;
    [ConditionalField(nameof(Type), false, AbilityType.STAB)] public float Duration;
    [ConditionalField(nameof(Type), true, AbilityType.THROW)] public Sound Sound;
    [ConditionalField(nameof(Type), true, AbilityType.THROW)] public float Range;
    [ConditionalField(nameof(Type), false, AbilityType.THROW)] public float MinChargeTime;
    [ConditionalField(nameof(Type), false, AbilityType.THROW)] public float MaxChargeTime;
    [ConditionalField(nameof(Type), false, AbilityType.THROW)] public float MaxSpeed;
    [ConditionalField(nameof(Type), false, AbilityType.THROW)] public GameObject ProjectilePrefab;
    [ConditionalField(nameof(Type), false, AbilityType.SPECIAL)] public GameObject VFXPrefab;

    [Space(15)]
    public AbilityEffectType Effect;

    [ConditionalField(nameof(Effect), true, AbilityEffectType.NONE, AbilityEffectType.MIRAGE, AbilityEffectType.CLEAN, AbilityEffectType.BLOCK)] public float Damage;
    [ConditionalField(nameof(Effect), false, AbilityEffectType.POISON)] public float PoisonDamage;
    [ConditionalField(nameof(Effect), false, AbilityEffectType.CHAIN)] public float ChainRange;
    [ConditionalField(nameof(Effect), false, AbilityEffectType.CHAIN)] public int ChainMax;
    [ConditionalField(nameof(Effect), false, AbilityEffectType.PIN)] public float PinTime;
    [ConditionalField(nameof(Effect), false, AbilityEffectType.BLOCK)] public float BlockAmount;
    [ConditionalField(nameof(Effect), false, AbilityEffectType.BLOCK)] public float BlockDuration;
    [ConditionalField(nameof(Effect), false, AbilityEffectType.MIRAGE)] public float LifeTime;
}

public class AbilityRuntimeData
{
    public float Cooldown;
    public float ChargeTime;
}
