using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour {

    [SerializeField] bool spearCanPickup, fizzleOnContact;
    [SerializeField] float fizzleTime = 0.6f; 

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<Player>();
        if (!player) player = other.GetComponentInParent<Player>();
        if (player || (spearCanPickup && other.GetComponent<ThrownStaff>())){
            Player.i.PowerUp();
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (fizzleOnContact && !(collision.gameObject.GetComponent<Player>() || collision.gameObject.GetComponentInParent<Player>())) {
            if (spearCanPickup && collision.gameObject.GetComponent<ThrownStaff>()) return;
            StartCoroutine(Fizzle());
        }
    }

    IEnumerator Fizzle()
    {
        var originalScale = transform.localScale;
        float timeLeft = fizzleTime;
        while (timeLeft > 0) {
            timeLeft -= Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, 1 - (timeLeft / fizzleTime));
            yield return new WaitForEndOfFrame();
        }
        Destroy(gameObject);
    }
}
