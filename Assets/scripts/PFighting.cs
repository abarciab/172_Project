using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PFighting : HitReciever {

    [SerializeField] Rigidbody staffProjectile;
    [SerializeField] float throwForce, maxAimTime;
    public int throwDmg;
    [SerializeField] Vector3 offset, aimOffset;

    bool hasSpear, aimed,recalling, charging, stabbing;
    float chargeTime;

    [Header("Stab")]
    [SerializeField] HitBox stabHB;
    [SerializeField] int stabDmg;
    [SerializeField] float stabKB;

    public void ReturnSpear()
    {
        hasSpear = true;
        recalling = false;
    }

    public void ThrowStaff()
    {
        if (!hasSpear || !aimed) return;
        hasSpear = aimed = charging = false;

        float power = Mathf.Clamp01(chargeTime / maxAimTime);

        chargeTime = 0;

        AudioManager.instance.PlaySound(0, gameObject);
        CameraState.i.SwitchToState(CameraState.StateName.MouseFollow);

        staffProjectile.gameObject.SetActive(false);
        var dir = GetAimDir();
        staffProjectile.transform.LookAt(staffProjectile.transform.position + dir * 10);
        staffProjectile.transform.parent = null;

        staffProjectile.gameObject.SetActive(true);
        staffProjectile.AddForce(dir * (throwForce * power));

        staffProjectile.GetComponent<HitBox>().StartChecking(transform, Mathf.RoundToInt(throwDmg * power));
    }

    public void Stab()
    {
        if (!hasSpear) {RetrieveSpear(); return; }
        if (charging) return;

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
        if (!hasSpear) { RetrieveSpear(); return; }
        aimed = charging = true;

        CameraState.i.SwitchToState(CameraState.StateName.MouseOverShoulder);
        staffProjectile.gameObject.SetActive(false);

        staffProjectile.transform.parent = transform;
        staffProjectile.transform.localPosition = offset;

        staffProjectile.gameObject.SetActive(true);
        staffProjectile.isKinematic = true;
        var dir = GetAimDir();
        staffProjectile.transform.LookAt(staffProjectile.transform.position + dir * 10);
    }

    private void Update()
    {
        if (charging && chargeTime < maxAimTime) chargeTime += Time.deltaTime;
        GlobalUI.i.throwCharge.value = chargeTime / maxAimTime;
    }

    void RetrieveSpear()
    {
        if (recalling) return;
        recalling = true;
        staffProjectile.GetComponent<ThrownStaff>().Recall();
    }

    

    Vector3 GetAimDir()
    {
        return (Camera.main.transform.forward + aimOffset).normalized;
    }
    
}
