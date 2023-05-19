using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goop : MonoBehaviour
{
    public bool expire;
    public float lifeTime = 4;
    Vector3 startScale;

    private void Start()
    {
        if (expire) Destroy(gameObject, lifeTime);
        startScale = transform.localScale;
    }

    private void Update()
    {
        lifeTime -= Time.deltaTime;
        if (!expire || lifeTime > 1.5f) return;

        transform.localScale = Vector3.Lerp(startScale, Vector3.zero, 1-(lifeTime / 1.5f));
    }

    private void OnTriggerStay(Collider other)
    {
        var player = other.GetComponent<PMovement>();
        if (player) {
            player.goopTime = .25f;
            player.GetComponent<Player>().goopTime = 0.15f;
        }

        var shockwave = other.GetComponent<Shockwave>();
        if (shockwave) Destroy(gameObject);
    }
}
