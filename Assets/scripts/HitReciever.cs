using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HitReciever : MonoBehaviour
{
    [HideInInspector] public UnityEvent OnHit = new UnityEvent();
    [HideInInspector] public int _damage;

    public void Hit(int damage) {
        _damage = damage;
        OnHit.Invoke();
    }
}
