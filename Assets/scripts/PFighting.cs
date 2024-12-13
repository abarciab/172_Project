using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PFighting : HitReciever
{

    [Header("Misc")]
    [SerializeField] private PlayerAbilityController _abilityController;

    private Player _p;

    //
    //
    //

    //throw:
    [SerializeField] float throwForce;
    [SerializeField] float maxAimTime;
    [SerializeField] float critWindow;
    [SerializeField] float minAimTime;
    [HideInInspector] public float chargeTime;

    [SerializeField, ReadOnly] bool aimed;
    [SerializeField, ReadOnly] bool recalling;
    [SerializeField, ReadOnly] bool charging;
    [SerializeField, ReadOnly] bool spearDrawn;
    public bool RecallReady;
    public bool chargingSpear() => charging;
    public bool Recalling() => recalling;

    [SerializeField] Rigidbody staffProjectile;
    [SerializeField] Vector3 offset;
    [SerializeField] Vector3 aimOffset;
    [SerializeField] GameObject spearObj;

    [Header("Stab")]
    [SerializeField] float stabKB;
    [SerializeField] int stabDmg;
    public bool stabbing;
    public bool Stabbing() => stabbing;
    [SerializeField] HitBox stabHB;

    [Header("Sunblast")]
    [SerializeField] float shockwaveResetTime;
    [SerializeField] float shockwaveKB;
    float sunblastCooldown;
    [SerializeField] int shockwaveDmg;
    [SerializeField] Shockwave shockwave;
    public float GetSunblastCooldown() => sunblastCooldown;

    [Header("Misc")]
    [SerializeField, ReadOnly] private bool _hasSpear;
    public int throwDmg { get; private set; }
    public int critDmg { get; private set; }
    public int CritDmg => critDmg;
    public bool SpearOut() => spearDrawn;
    public void DrawSpear() => spearDrawn = true;
    public bool HasSpear() => _hasSpear;
    public void PutAwaySpear() => spearDrawn = false;
    public void StartChecking() => stabHB.StartChecking(true, stabDmg, stabKB, stabHB.gameObject);
    Vector3 GetAimDir() => (Camera.main.transform.forward + aimOffset).normalized;

    private void Start()
    {
        _hasSpear = true;
        aimed = false;

        _p = GetComponent<Player>();
    }

    private void Update()
    {
        spearObj.SetActive(_hasSpear && (!spearDrawn || (!staffProjectile.gameObject.activeInHierarchy || !charging)));

        if (sunblastCooldown > 0) DecrementSunblastCooldown();
        if (charging) chargeTime += Time.deltaTime;

        float chargePercent = Mathf.Min(chargeTime, maxAimTime) / maxAimTime;
        GlobalUI.i.Do(UIAction.UPDATE_CHARGE, chargePercent);
    }


    private void DecrementSunblastCooldown()
    {
        sunblastCooldown -= Time.deltaTime;
        GlobalUI.i.Do(UIAction.DISPLAY_SUNBLAST_COOLDOWN, sunblastCooldown);
        if (sunblastCooldown <= 0) OnSunblastReady();
    }

    private void OnSunblastReady()
    {
        _p.Sounds.Get(PSoundKey.SUNBLASTREADY).Play(transform);  
        GlobalUI.i.Do(UIAction.SUNBLAST_READY);
    }

    public void SetSpearDmg(int dmg)
    {
        throwDmg = dmg;
        critDmg = dmg;
    }

    public void RecallReadyNotice()
    {
        RecallReady = true;
        GlobalUI.i.Do(UIAction.RECALL_READY);
    }

    public void ReturnSpear()
    {
        _hasSpear = true;
        recalling = charging = false;
        GlobalUI.i.Do(UIAction.CATCH_SPEAR);
        _p.Sounds.Get(PSoundKey.SPEAR_CATCH).Play();
    }

    public void ThrowSpear()
    {
        _p.Sounds.Get(PSoundKey.THROW_CHARGE).Stop();
        charging = false;
        if (!_hasSpear || !aimed) return;

        aimed = false;

        if (chargeTime <= minAimTime) {
            chargeTime = 0;
            staffProjectile.gameObject.SetActive(false);
            return;
        }

        _hasSpear = false;
        bool perfectThrow = chargeTime > (0.85 * maxAimTime) && chargeTime < (maxAimTime + 0.2f);
        float power = Mathf.Clamp01(chargeTime / maxAimTime);
        int damage = Mathf.RoundToInt(throwDmg * power);

        if (perfectThrow) {
            _p.Sounds.Get(PSoundKey.CRIT_HIT).Play();
            power = 1;
            damage = critDmg;
        }

        chargeTime = 0;
        _p.Sounds.Get(PSoundKey.THROW_SPEAR).Play(transform);
        staffProjectile.GetComponent<ThrownStaff>().Throw();
        
        staffProjectile.gameObject.SetActive(false);
        var dir = GetAimDir();
        staffProjectile.transform.LookAt(staffProjectile.transform.position + dir * 10);
        staffProjectile.transform.parent = null;

        staffProjectile.gameObject.SetActive(true);
        staffProjectile.AddForce(dir * (throwForce * power));

        staffProjectile.GetComponentInChildren<HitBox>().StartChecking(transform, damage, _crit: perfectThrow);

        GlobalUI.i.Do(UIAction.THROW_SPEAR);
    }

    public void Stab()
    {
        if (!enabled) return;
        if (!spearDrawn) { DrawSpear(); return; }
        if (!_hasSpear) {RetrieveSpear(); return; }
        if (charging || stabbing) return;
        stabbing = true;
    }

    public void EndAttack()
    {
        stabHB.EndChecking();
        stabbing = false;
    }

    public override void Hit(HitData hit)
    {
        if (GetComponent<PMovement>().rolling) return;

        base.Hit(hit);

        Player.i.ChangeHealth(-hit.damage);
        GetComponent<PMovement>().KnockBack(hit.source, hit.KB, hit.offset);
    }

    public void StartAimingSpear()
    {
        if (!enabled || charging) return;
        if (!spearDrawn) { DrawSpear(); return; }
        if (!_hasSpear) { RetrieveSpear(); return; }
        aimed = charging = true;
        stabbing = false;
        _p.Sounds.Get(PSoundKey.THROW_CHARGE).Play();

        //CameraState.i.SwitchToState(CameraState.StateName.MouseOverShoulder);

        staffProjectile.gameObject.SetActive(false);

        staffProjectile.transform.parent = transform;
        staffProjectile.transform.localPosition = offset;

        staffProjectile.isKinematic = true;
        var dir = GetAimDir();
        staffProjectile.transform.LookAt(staffProjectile.transform.position + dir * 10);
       
    }

    public void SwapSpear()
    {
        var dir = GetAimDir();
        staffProjectile.transform.LookAt(staffProjectile.transform.position + dir * 10);
    }

    public void ActivateShockwave()
    {
        if (sunblastCooldown > 0) {
            _p.Sounds.Get(PSoundKey.SUNBLAST_ERROR).Play();
            return;
        }

        if (!enabled) return;
        _p.Sounds.Get(PSoundKey.SUNBLAST).Play();
        sunblastCooldown = shockwaveResetTime;

        if (_hasSpear) shockwave.transform.position = transform.position;
        else shockwave.transform.position = staffProjectile.transform.position;

        shockwave.Explode(shockwaveDmg, shockwaveKB);
    }

    void RetrieveSpear()
    {
        if (recalling) return;
        if (!RecallReady) _p.Sounds.Get(PSoundKey.RECALL_ERROR).Play(restart:false);

        recalling = staffProjectile.GetComponent<ThrownStaff>().Recall();
        if (recalling) _p.Sounds.Get(PSoundKey.RECALL).Play();
    }    

}
