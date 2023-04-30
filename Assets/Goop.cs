using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goop : MonoBehaviour
{

    [SerializeField] float lifeTime = 4;
    Vector3 startScale;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
        startScale = transform.localScale;
    }

    private void Update()
    {
        lifeTime -= Time.deltaTime;
        if (lifeTime > 1.5f) return;

        transform.localScale = Vector3.Lerp(startScale, Vector3.zero, 1-(lifeTime / 1.5f));
    }

    private void OnTriggerStay(Collider other)
    {
        var player = other.GetComponent<PMovement>();
        if (player) {
            player.goopTime = .25f;
            player.GetComponent<Player>().goopTime = 0.15f;
        }

        var spear = other.GetComponent<ThrownStaff>();
        var shockwave = other.GetComponent<Shockwave>();
        if (spear || shockwave) Destroy(gameObject);
    }
}
