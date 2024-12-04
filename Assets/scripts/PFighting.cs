using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PFighting : HitReciever {

    private Player _p;

    //
    //
    //

    [SerializeField] Rigidbody staffProjectile;
    [SerializeField] float throwForce, maxAimTime, critWindow, minAimTime;
    public int throwDmg { get; private set; }
    public int critDmg { get; private set; }
    [SerializeField] Vector3 offset, aimOffset;

    [SerializeField, ReadOnly] bool hasSpear, aimed, recalling, charging, spearDrawn;
    public bool stabbing;
    [HideInInspector] public float chargeTime;
    [SerializeField] GameObject spearObj;

    [Header("Stab")]
    [SerializeField] HitBox stabHB;
    [SerializeField] int stabDmg;
    [SerializeField] float stabKB;

    [Header("Shockwave")]
    [SerializeField] Shockwave shockwave;
    [SerializeField] float shockwaveResetTime, shockwaveKB;
    [SerializeField] int shockwaveDmg;
    float sunblastCooldown;

    [Header("Sounds")]
    [SerializeField] Sound throwSpearSound;
    [SerializeField] Sound shockwaveSound, shockwaveReadySound, recallWoosh, spearCatch, critSucsess, recallError, spearBuildUp, sunBlastError;

    [Header("finalFight")]
    [SerializeField] GameObject spearTipBall;
    [SerializeField] GameObject thrownSpearBall, spearModel, heldSpearModel;

    public bool RecallReady;

    public bool chargingSpear() => charging;
    public bool Stabbing() => stabbing;
    public bool SpearOut() => spearDrawn;
    public bool Recalling() => recalling;
    public void DrawSpear() => spearDrawn = true;
    public bool HasSpear() => hasSpear;
    public float GetSunblastCooldown() => sunblastCooldown;

    private void Start()
    {
        hasSpear = true;
        aimed = false;

        throwSpearSound = Instantiate(throwSpearSound);
        shockwaveSound = Instantiate(shockwaveSound);
        shockwaveReadySound = Instantiate(shockwaveReadySound);
        recallWoosh = Instantiate(recallWoosh);
        recallError = Instantiate(recallError);
        spearCatch = Instantiate(spearCatch);
        critSucsess = Instantiate(critSucsess);
        spearBuildUp = Instantiate(spearBuildUp);
        sunBlastError = Instantiate(sunBlastError);

        _p = GetComponent<Player>();
    }

    private void Update()
    {
        spearObj.SetActive(hasSpear && (!spearDrawn || (!staffProjectile.gameObject.activeInHierarchy || !charging)));

        spearTipBall.SetActive(hasSpear && Player.i.poweredUp);

        thrownSpearBall.SetActive(!hasSpear && Player.i.poweredUp);

        if (sunblastCooldown > 0) DecrementSunblastCooldown();
        if (charging) chargeTime += Time.deltaTime;

        float chargePercent = Mathf.Min(chargeTime, maxAimTime) / maxAimTime;
        GlobalUI.i.Do(UIAction.UPDATE_CHARGE, chargePercent);
    }

    public void PutAwaySpear()
    {
        spearDrawn = false;
    }

    private void DecrementSunblastCooldown()
    {
        sunblastCooldown -= Time.deltaTime;
        GlobalUI.i.Do(UIAction.DISPLAY_SUNBLAST_COOLDOWN, sunblastCooldown);
        if (sunblastCooldown <= 0) OnSunblastReady();
    }

    private void OnSunblastReady()
    {
        shockwaveReadySound.Play(transform);
        GlobalUI.i.Do(UIAction.SUNBLAST_READY);
    }

    public void SetSpearLayer(int layer)
    {
        spearModel.layer = layer;
        heldSpearModel.layer = layer;
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
        hasSpear = true;
        recalling = charging = false;
        GlobalUI.i.Do(UIAction.CATCH_SPEAR);
        spearCatch.Play();
    }

    public int CritDmg => critDmg;

    public void ThrowSpear()
    {
        spearBuildUp.Stop();
        charging = false;
        if (!hasSpear || !aimed) return;

        aimed = false;

        if (chargeTime <= minAimTime) {
            chargeTime = 0;
            staffProjectile.gameObject.SetActive(false);
            return;
        }

        hasSpear = false;
        bool perfectThrow = chargeTime > (0.85 * maxAimTime) && chargeTime < (maxAimTime + 0.2f);
        float power = Mathf.Clamp01(chargeTime / maxAimTime);
        int damage = Mathf.RoundToInt(throwDmg * power);

        if (perfectThrow) {
            critSucsess.Play();
            power = 1;
            damage = critDmg;
        }

        chargeTime = 0;
        throwSpearSound.Play(transform);
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
        if (!hasSpear) {RetrieveSpear(); return; }
        if (charging || stabbing) return;
        stabbing = true;
    }

    public void StartChecking()
    {
        stabHB.StartChecking(true, stabDmg, stabKB, stabHB.gameObject);
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
        if (!hasSpear) { RetrieveSpear(); return; }
        aimed = charging = true;
        stabbing = false;
        spearBuildUp.Play();

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
            sunBlastError.Play();
            return;
        }

        if (!enabled) return;
        shockwaveSound.Play(transform);
        sunblastCooldown = shockwaveResetTime;

        if (hasSpear) shockwave.transform.position = transform.position;
        else shockwave.transform.position = staffProjectile.transform.position;

        shockwave.Explode(shockwaveDmg, shockwaveKB);
    }

    void RetrieveSpear()
    {
        if (recalling) return;
        if (!RecallReady) recallError.Play(restart: false);

        recalling = staffProjectile.GetComponent<ThrownStaff>().Recall();
        if (recalling) {
            recallWoosh.Play(transform);
        }
    }

    

    Vector3 GetAimDir()
    {
        return (Camera.main.transform.forward + aimOffset).normalized;
    }
    
}
