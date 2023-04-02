using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Destructible : HitReciever
{
    private void Awake() {
        OnHit.AddListener(_Hit);
    }

    void _Hit() {
        int damage = _damage;
        Destroy(gameObject);
    }

}
