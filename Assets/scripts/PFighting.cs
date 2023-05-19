using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PFighting : HitReciever {

    [SerializeField] Rigidbody staffProjectile;
    [SerializeField] float throwForce, maxAimTime, critWindow, minAimTime;
    public int throwDmg, critDmg;
    [SerializeField] Vector3 offset, aimOffset;

    bool hasSpear, aimed, recalling, charging, spearDrawn;
    public bool stabbing;
    float chargeTime;
    [SerializeField] GameObject spearObj;

    [Header("Stab")]
    [SerializeField] HitBox stabHB;
    [SerializeField] int stabDmg;
    [SerializeField] float stabKB;

    [Header("Shockwave")]
    [SerializeField] Shockwave shockwave;
    [SerializeField] float shockwaveResetTime, shockwaveKB;
    [SerializeField] int shockwaveDmg;
    float swCooldown;

    [Header("Sounds")]
    [SerializeField] Sound throwSpearSound;
    [SerializeField] Sound shockwaveSound, shockwaveReadySound, recallWoosh, spearCatch, critSucsess;

    [Header("Tutorial")]
    [SerializeField] Fact anyThrow;
    [SerializeField] Fact recall, fullThrow, criticalThrow, stabbedOnce, shockWave, throwWeak;

    public bool RecallReady;

    public void DrawSpear()
    {
        spearDrawn = true;
    }

    public bool HasSpear()
    {
        return hasSpear;
    }

    public void PutAwaySpear()
    {
        //if (stabbing || charging) return;
        //spearDrawn = false;
    }

    public float GetSWcooldown()
    {
        return swCooldown;
    }

    public void ReturnSpear()
    {
        hasSpear = true;
        recalling = charging = false;
        spearCatch.Play();
    }

    public void ThrowStaff()
    {
        charging = false;
        if (!hasSpear || !aimed) return;
        aimed = false;
        CameraState.i.SwitchToState(CameraState.StateName.MouseFollow);

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
        
        staffProjectile.gameObject.SetActive(false);
        var dir = GetAimDir();
        staffProjectile.transform.LookAt(staffProjectile.transform.position + dir * 10);
        staffProjectile.transform.parent = null;

        staffProjectile.gameObject.SetActive(true);
        staffProjectile.AddForce(dir * (throwForce * power));

        staffProjectile.GetComponentInChildren<HitBox>().StartChecking(transform, FactManager.i.IsPresent(throwWeak) ? 0 : damage, _crit: perfectThrow);

        var fMan = FactManager.i;
        if (!fMan.IsPresent(anyThrow)) fMan.AddFact(anyThrow);
        else if (!fMan.IsPresent(fullThrow) && power == 1) fMan.AddFact(fullThrow);
        else if (!fMan.IsPresent(criticalThrow) && perfectThrow) fMan.AddFact(criticalThrow);
    }

    public void Stab()
    {
        if (!enabled) return;
        if (!spearDrawn) { DrawSpear(); return; }
        if (!hasSpear) {RetrieveSpear(); return; }
        if (charging || stabbing) return;
        stabbing = true;
        FactManager.i.AddFact(stabbedOnce);
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

    public bool chargingSpear()
    {
        return charging;
    }

    public bool Stabbing()
    {
        return stabbing;
    }

    public bool SpearOut()
    {
        return spearDrawn;
    }

    public override void Hit(HitData hit)
    {
        if (GetComponent<PMovement>().rolling) return;

        base.Hit(hit);

        Player.i.ChangeHealth(-hit.damage);
        GetComponent<PMovement>().KnockBack(hit.source, hit.KB, hit.offset);
    }

    private void Start()
    {
        hasSpear = true;
        aimed = false;
        throwSpearSound = Instantiate(throwSpearSound);
        shockwaveSound = Instantiate(shockwaveSound);
        shockwaveReadySound = Instantiate(shockwaveReadySound);
        recallWoosh = Instantiate(recallWoosh);
        spearCatch = Instantiate(spearCatch);
        critSucsess = Instantiate(critSucsess);
    }

    public void StartAimingSpear()
    {
        if (!enabled || charging) return;
        if (!spearDrawn) { DrawSpear(); return; }
        if (!hasSpear) { RetrieveSpear(); return; }
        aimed = charging = true;
        stabbing = false;

        CameraState.i.SwitchToState(CameraState.StateName.MouseOverShoulder);
        SwapSpear();
    }

    public void SwapSpear()
    {
        //print("swap");

        staffProjectile.gameObject.SetActive(false);

        staffProjectile.transform.parent = transform;
        staffProjectile.transform.localPosition = offset;

        staffProjectile.gameObject.SetActive(true);
        staffProjectile.isKinematic = true;
        var dir = GetAimDir();
        staffProjectile.transform.LookAt(staffProjectile.transform.position + dir * 10);
    }

    public void ActivateShockwave()
    {
        if (swCooldown > 0 || !enabled) return;
        shockwaveSound.Play(transform);
        swCooldown = shockwaveResetTime;
        FactManager.i.AddFact(shockWave);

        if (hasSpear) shockwave.transform.position = transform.position;
        else shockwave.transform.position = staffProjectile.transform.position;

        shockwave.Explode(shockwaveDmg, shockwaveKB);
    }

    private void Update()
    {
        spearObj.SetActive(hasSpear && !staffProjectile.gameObject.activeInHierarchy);

        var playSound = false;
        if (swCooldown > 0) playSound = true;
        swCooldown -= Time.deltaTime;
        if (swCooldown <= 0 && playSound) shockwaveReadySound.Play(transform);
        if (charging) chargeTime += Time.deltaTime;
        GlobalUI.i.throwCharge.value = Mathf.Min(chargeTime, maxAimTime) / maxAimTime;
    }

    void RetrieveSpear()
    {
        if (recalling) return;
        recalling = staffProjectile.GetComponent<ThrownStaff>().Recall();
        if (recalling) {
            recallWoosh.Play(transform);
            FactManager.i.AddFact(recall);
        }
    }

    

    Vector3 GetAimDir()
    {
        return (Camera.main.transform.forward + aimOffset).normalized;
    }
    
}
