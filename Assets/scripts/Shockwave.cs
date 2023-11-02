using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Shockwave : MonoBehaviour
{
    [SerializeField] float explodeTime, explodeSpeed, spinSpeed = 2;
    float explodeRemaining;
    [SerializeField] GameObject SunblastVFX;
    [SerializeField] Vector3 startScale, heldScale;

    public void Explode(int damage, float KB)
    {
        SunblastVFX.GetComponent<VisualEffect>().Play();
        explodeRemaining = explodeTime;
        GetComponent<HitBox>().StartChecking(true, damage, KB, gameObject, Vector3.down * 2, _stun:true);
    }

    private void Update()
    {
        if (explodeRemaining <= 0) { 
            ResetExplosion(); 
            return; 
        }
        explodeRemaining -= Time.deltaTime;
        transform.localScale = Player.i.GetComponent<PFighting>().HasSpear() ? heldScale : startScale;
    }

    private void ResetExplosion()
    {
        if (transform.localScale == Vector3.zero) return;

        GetComponent<HitBox>().EndChecking();
        transform.localScale = Vector3.zero;
    }
}
