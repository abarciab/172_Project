using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class PFighting : HitReciever {

    [SerializeField] Rigidbody staffProjectile;
    [SerializeField] float throwForce, maxAimTime;
    public int throwDmg;
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

    public void DrawSpear()
    {
        spearDrawn = true;
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
    }

    public void ThrowStaff()
    {
        charging = false;
        if (!hasSpear || !aimed) return;
        aimed = false;
        CameraState.i.SwitchToState(CameraState.StateName.MouseFollow);

        if (chargeTime <= maxAimTime * 0.1f) {
            chargeTime = 0;
            staffProjectile.gameObject.SetActive(false);
            return;
        }

        hasSpear = false;
        float power = Mathf.Clamp01(chargeTime / maxAimTime);
        chargeTime = 0;
        AudioManager.instance.PlaySound(0, gameObject);
        
        staffProjectile.gameObject.SetActive(false);
        var dir = GetAimDir();
        staffProjectile.transform.LookAt(staffProjectile.transform.position + dir * 10);
        staffProjectile.transform.parent = null;

        staffProjectile.gameObject.SetActive(true);
        staffProjectile.AddForce(dir * (throwForce * power));

        staffProjectile.GetComponentInChildren<HitBox>().StartChecking(transform, Mathf.RoundToInt(throwDmg * power));
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

    public bool Stabbing()
    {
        return stabbing;
    }

    public bool spearOut()
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
    }

    public void StartAimingSpear()
    {
        if (!enabled || charging) return;
        if (!spearDrawn) { DrawSpear(); return; }
        if (!hasSpear) { RetrieveSpear(); return; }
        aimed = charging = true;
        stabbing = false;

        CameraState.i.SwitchToState(CameraState.StateName.MouseOverShoulder);
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
        AudioManager.instance.PlaySound(14, gameObject);
        swCooldown = shockwaveResetTime;

        if (hasSpear) shockwave.transform.position = transform.position;
        else shockwave.transform.position = staffProjectile.transform.position;

        shockwave.Explode(shockwaveDmg, shockwaveKB);
    }

    private void Update()
    {
        spearObj.SetActive(hasSpear);
        var playSound = false;
        if (swCooldown > 0) playSound = true;
        swCooldown -= Time.deltaTime;
        if (swCooldown <= 0 && playSound) AudioManager.instance.PlaySound(15, gameObject); 
        if (charging && chargeTime < maxAimTime) chargeTime += Time.deltaTime;
        GlobalUI.i.throwCharge.value = chargeTime / maxAimTime;
    }

    void RetrieveSpear()
    {
        if (recalling) return;
        recalling = staffProjectile.GetComponent<ThrownStaff>().Recall();
    }

    

    Vector3 GetAimDir()
    {
        return (Camera.main.transform.forward + aimOffset).normalized;
    }
    
}
