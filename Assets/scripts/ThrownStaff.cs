using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class ThrownStaff : MonoBehaviour
{
    Rigidbody rb;

    public bool recalling;
    [SerializeField] float recallSpeed = 2, recallEndThreshold = 1, maxSpeed, maxDist = 15;
    [SerializeField] Vector3 playerOffset;
    [SerializeField] Sound landSound, windSound, ping, blocked;
    bool landed;
    PFighting fight;

    private SpearVFXCoordinator VFXCoordinator;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        GetComponentInChildren<HitBox>().onTrigger.AddListener(CheckForBlocked);
        VFXCoordinator = GetComponent<SpearVFXCoordinator>();

        fight = Player.i.GetComponent<PFighting>();

        landSound = Instantiate(landSound);
        windSound = Instantiate(windSound);
        ping = Instantiate(ping);
        blocked = Instantiate(blocked);
    }

    void CheckForBlocked()
    {
        if (recalling) return;
        var hb = GetComponentInChildren<HitBox>();
        if (!hb.triggeredBy.CompareTag("BlockSpear")) return;

        hb.EndChecking();
        rb.velocity *= -0.5f;
        blocked.Play();
    }

    private void OnEnable()
    {
        if (rb == null) return;
        rb.isKinematic = false;
        landed = false;
        windSound.PlaySilent(transform);
        
    }

    public void OnThrow()
    {
        VFXCoordinator.EnableTrailVFX(); //activate trail PS object
    }

    public bool Recall()
    {
        if (!fight.RecallReady) return false;


        fight.RecallReady = false;
        rb.isKinematic = false;
        rb.useGravity = false;
        GetComponent<CapsuleCollider>().isTrigger = true;
        rb.velocity = Vector3.zero;
        recalling = true;
        GetComponentInChildren<HitBox>().StartChecking(true, Player.i.GetComponent<PFighting>().critDmg, _crit:true);

        return true;
    }

    private void Update()
    {
        float dist = Vector3.Distance(transform.position, Player.i.transform.position);
        if (!recalling && dist > maxDist && !fight.RecallReady) {
            ping.Play();
            fight.RecallReady = true;
        }
        windSound.PercentVolume(rb.velocity.magnitude / maxSpeed, 0.025f);
        if (!rb.isKinematic && !recalling) transform.LookAt(transform.position + rb.velocity.normalized);
        if (!recalling) return;

        transform.position = Vector3.Lerp(transform.position, Player.i.transform.position, 0.1f);
        transform.LookAt(Player.i.transform);

        if (dist <= recallEndThreshold) CompleteRecall();        
    }

    void CompleteRecall()
    {
        recalling = false;
        rb.isKinematic = true;
        rb.useGravity = true;
        GetComponent<CapsuleCollider>().isTrigger = false;
        gameObject.SetActive(false);
        Player.i.GetComponent<PFighting>().ReturnSpear();

        VFXCoordinator.DisableTrailVFX(); //deactivate trail PS object
        VFXCoordinator.DisableHSImpact();
        VFXCoordinator.PlaySpearCatch();
    }

    private void OnCollisionEnter(Collision collision)
    {
        var player = collision.collider.GetComponentInParent<Player>();
        if (player != null) return;

        landed = true;
        VFXCoordinator.EnableHSImpact();
        if (!rb.isKinematic) {
            landSound.Play();

            if (!fight.RecallReady) {
                ping.Play();
                fight.RecallReady = true;
            }
        }
        windSound.Stop();

        GetComponentInChildren<HitBox>().EndChecking();
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;

    }
}
