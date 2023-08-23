using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoopProjectile : MonoBehaviour
{
    public float goopAmount, homingAmount;
    [SerializeField] bool destroyedBySpear = true;
    Vector3 velocity;
    [SerializeField] Collider actualCollider;
    bool bounced;
    [SerializeField] HitReciever.HitData bouncedHit;

    [SerializeField] List<Renderer> meshes = new List<Renderer>();
    [SerializeField] Material glowingMat, normalMat;


    private void Update()
    {
        var dir = transform.position - Player.i.transform.position;
        GetComponent<Rigidbody>().velocity += dir * Time.deltaTime * homingAmount;

        if (!destroyedBySpear) foreach (var r in meshes) r.material = Player.i.poweredUp ? glowingMat : normalMat;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<EnemyStats>() && bounced) {
            other.GetComponent<EnemyStats>().Hit(bouncedHit, true);
            Destroy(gameObject);
            return;
        }

        var spear = other.GetComponent<ThrownStaff>();
        if (!spear) spear = other.GetComponentInParent<ThrownStaff>();
        if (spear) {
            if (destroyedBySpear) Destroy(gameObject);
            else if (Player.i.poweredUp) {
                GetComponent<Rigidbody>().velocity *= -2;
                Player.i.poweredUp = false;
                actualCollider.enabled = false;
                bounced = true;
                //StartCoroutine(ReEnableCollider());
            }
            else {
                actualCollider.enabled = false;
                StartCoroutine(ReEnableCollider());
            }
        }
    }

    IEnumerator ReEnableCollider()
    {
        yield return new WaitForSeconds(0.5f);
        actualCollider.enabled = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        var spear = collision.gameObject.GetComponent<ThrownStaff>();
        if (!spear) spear = collision.gameObject.GetComponentInParent<ThrownStaff>();
        if (!spear) {
            Explode();
            return;
        }       
    }

    void Explode()
    {
        GoopManager.i.SpawnGoop(transform.position, goopAmount);
        Destroy(gameObject);
    }
}
