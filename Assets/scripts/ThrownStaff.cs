using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class ThrownStaff : MonoBehaviour
{
    Rigidbody rb;

    public bool recalling;
    [SerializeField] float recallSpeed = 2, recallEndThreshold = 1;
    [SerializeField] Vector3 playerOffset;
    [SerializeField] AudioSource source1, source2;
    bool landed;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        if (rb == null) return;
        rb.isKinematic = false;
        landed = false;
    }

    public bool Recall()
    {
        if (!landed) return false;

        AudioManager.instance.PlaySound(13, source1);
        rb.isKinematic = false;
        rb.useGravity = false;
        GetComponent<CapsuleCollider>().isTrigger = true;
        rb.velocity = Vector3.zero;
        recalling = true;
        GetComponentInChildren<HitBox>().StartChecking(true, Player.i.GetComponent<PFighting>().throwDmg);

        return true;
    }

    private void Update()
    {
        if (!rb.isKinematic && !recalling) transform.LookAt(transform.position + rb.velocity.normalized);

        if (!recalling) return;


        /*var dir = ((Player.i.transform.position + playerOffset) - transform.position).normalized;
        //rb.AddForce(dir * recallSpeed, ForceMode.VelocityChange);
        float speed = 
        rb.velocity = dir * recallSpeed;*/
        transform.position = Vector3.Lerp(transform.position, Player.i.transform.position, 0.1f);
        //transform.LookAt(Player.i.transform);
        transform.LookAt(transform.position + Vector3.up * 10);

        float dist = Vector3.Distance(transform.position, Player.i.transform.position);
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
    }

    

    private void OnCollisionEnter(Collision collision)
    {
        var player = collision.collider.GetComponentInParent<Player>();
        if (player != null) return;

        landed = true;
        if (!rb.isKinematic) AudioManager.instance.PlaySound(12, source2);

        GetComponentInChildren<HitBox>().EndChecking();
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;
    }
}
