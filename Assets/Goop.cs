using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goop : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        var player = other.GetComponent<PMovement>();
        if (player) player.goopTime = .25f;

        var spear = other.GetComponent<ThrownStaff>();
        var shockwave = other.GetComponent<Shockwave>();
        if (spear || shockwave) Destroy(gameObject);
    }
}
