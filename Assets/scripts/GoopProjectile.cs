using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoopProjectile : MonoBehaviour
{
    public float goopAmount, homingAmount;

    private void Update()
    {
        var dir = transform.position - Player.i.transform.position;
        GetComponent<Rigidbody>().velocity += dir * Time.deltaTime * homingAmount;
    }

    private void OnTriggerEnter(Collider other)
    {
        var spear = other.GetComponent<ThrownStaff>();
        if (!spear) spear = other.GetComponentInParent<ThrownStaff>();
        if (spear) Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Explode();
    }

    void Explode()
    {
        GoopManager.i.SpawnGoop(transform.position, goopAmount);
        Destroy(gameObject);
    }
}
