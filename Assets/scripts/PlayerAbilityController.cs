using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum InputType { NONE, DOWN, STAY, UP}
public class PlayerAbilityController : MonoBehaviour
{
    [SerializeField] private float _perfectThrowThreshold = 0.1f;
    [SerializeField] private float _critDmgMod = 2f;
    private Player _p;

    private List<PlayerAbilityData> _abilities = new List<PlayerAbilityData>();
    private List<AbilityRuntimeData> _data = new List<AbilityRuntimeData>();

    private const int abilityCount = 3;

    public void OnLeftClick(InputType type) => EvaluateInput(type, 0);        
    public void OnRightClick(InputType type) => EvaluateInput(type, 1);
    public void OnSpecial(InputType type) => EvaluateInput(type, 2);

    private void Start() {
        _p = Player.i;
    }

    private void Update() {
        for (int i = 0; i < _abilities.Count; i++) {
            if (_abilities[i]) _data[i].Cooldown -= Time.deltaTime;
        }
    }

    private void Initialize() {
        _abilities.Clear();
        _data.Clear();
        for (int i = 0; i < abilityCount; i++) {
            _abilities.Add(null);
            _data.Add(null);
        }
    }
    
    private void EvaluateInput(InputType type, int abilityIndex) {
        if (_abilities.Count != abilityCount) Initialize();
        var ability = _abilities[abilityIndex];
        if (ability == null) return;

        if (ability.Type == AbilityType.STAB) OnStabInput(type, ability, abilityIndex);
        if (ability.Type == AbilityType.THROW) OnThrowInput(type, ability, abilityIndex);
        if (ability.Type == AbilityType.SPECIAL) OnSpecialInput(type, ability, abilityIndex);
    }

    private void OnStabInput(InputType type, PlayerAbilityData ability, int index) {
        if (type == InputType.DOWN) {
            if (_data[index].Cooldown > 0f) return;
        }
    }

    private void OnThrowInput(InputType type, PlayerAbilityData ability, int index) {
        if (type == InputType.DOWN) {
            _p.Sounds.Get(PSoundKey.THROW_CHARGE).Play();
        }

        if (type == InputType.STAY) {
            var charge = _data[index].ChargeTime = Time.deltaTime;
            _data[index].ChargeTime = charge;
            GlobalUI.i.Do(UIAction.UPDATE_CHARGE, Mathf.Clamp01(charge / ability.MaxChargeTime));
        }

        if (type == InputType.UP) {
            if (_data[index].ChargeTime < ability.MinChargeTime) StopChargingThrow();
            else ThrowSpear(ability, _data[index].ChargeTime);

            _data[index].ChargeTime = 0;
            _p.Sounds.Get(PSoundKey.THROW_CHARGE).Stop();
        }
    }

    private void StopChargingThrow() { }

    private void ThrowSpear(PlayerAbilityData ability, float chargeTime) {
        bool perfectThrow = Mathf.Abs(ability.MaxChargeTime - chargeTime) < _perfectThrowThreshold;
        float percentCharge = Mathf.Clamp01(chargeTime / ability.MaxChargeTime);
        float damage = percentCharge * ability.Damage;

        if (perfectThrow) {
            damage *= _critDmgMod;
            _p.Sounds.Get(PSoundKey.CRIT_HIT).Play();
        }
        _p.Sounds.Get(PSoundKey.THROW_SPEAR).Play();

        //spawn prefab, pass in ability, add force

        GlobalUI.i.Do(UIAction.THROW_SPEAR);
    }

    private void OnSpecialInput(InputType type, PlayerAbilityData ability, int index) {
        if (type != InputType.DOWN || _data[index].Cooldown > 0) return; 

        ActivateSpecial(ability);
        _data[index].Cooldown = ability.Cooldown;
    }

    private void ActivateSpecial(PlayerAbilityData ability) {
        if (ability.Effect == AbilityEffectType.SUNBLAST) Sunblast(ability);
        if (ability.Effect == AbilityEffectType.CLEAN) Clean(ability);
        if (ability.Effect == AbilityEffectType.BLOCK) Block(ability);
        if (ability.Effect == AbilityEffectType.MIRAGE) Mirage(ability);
    }

    private void Sunblast(PlayerAbilityData data) {

    }

    private void Clean(PlayerAbilityData data) {

    }

    private void Block(PlayerAbilityData data) {

    }

    private void Mirage(PlayerAbilityData data) {

    }
}
