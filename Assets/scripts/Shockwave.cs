using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Shockwave : MonoBehaviour
{
    [SerializeField] float explodeTime, explodeSpeed, spinSpeed = 2;
    float explodeRemaining;
    [SerializeField] GameObject SunblastVFX;
    [SerializeField] Vector3 startScale;

    public void Explode(int damage, float KB)
    {
        //transform.localScale = Vector3.zero;
        SunblastVFX.GetComponent<VisualEffect>().Play();
        explodeRemaining = explodeTime;
        GetComponent<HitBox>().StartChecking(true, damage, KB, gameObject, Vector3.down * 2, _stun:true);
    }

    private void Update()
    {
        if (explodeRemaining <= 0) { ResetExplosion(); return; }
        explodeRemaining -= Time.deltaTime;
        //transform.localEulerAngles += Vector3.up * spinSpeed;
        transform.localScale = startScale;
    }

    private void ResetExplosion()
    {
        if (transform.localScale == Vector3.zero) return;

        GetComponent<HitBox>().EndChecking();
        transform.localScale = Vector3.zero;
    }
}
