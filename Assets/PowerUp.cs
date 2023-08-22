using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour {
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Player>() || other.GetComponentInParent<Player>()) {
            Player.i.PowerUp();
            Destroy(gameObject);
        }
    }
}
