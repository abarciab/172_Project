using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrownStaff : MonoBehaviour
{
    Rigidbody rb;

    public bool recalling;
    [SerializeField] float recallSpeed = 2, recallEndThreshold = 1;
    [SerializeField] Vector3 playerOffset;
    [SerializeField] AudioSource source1, source2;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        if (rb == null) return;
        rb.isKinematic = false;
    }

    public void Recall()
    {
        AudioManager.instance.PlaySound(13, source1);
        rb.isKinematic = false;
        rb.useGravity = false;
        GetComponent<CapsuleCollider>().isTrigger = true;
        rb.velocity = Vector3.zero;
        recalling = true;
        GetComponent<HitBox>().StartChecking(true, Player.i.GetComponent<PFighting>().throwDmg);
    }

    private void Update()
    {
        if (!rb.isKinematic) transform.LookAt(transform.position + rb.velocity.normalized);

        if (!recalling) return;

        
        var dir = ((Player.i.transform.position + playerOffset) - transform.position).normalized;
        rb.AddForce(dir * recallSpeed, ForceMode.VelocityChange);

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

        if (!rb.isKinematic) AudioManager.instance.PlaySound(12, source2);

        GetComponent<HitBox>().EndChecking();
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;
    }
}
