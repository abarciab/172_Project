using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoopProjectile : MonoBehaviour
{
    public float goopAmount;

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
