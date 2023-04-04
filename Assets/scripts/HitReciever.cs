using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HitReciever : MonoBehaviour
{
    [HideInInspector] public UnityEvent OnHit = new UnityEvent();
    [HideInInspector] public int _damage;
    [HideInInspector] public GameObject source;
    [HideInInspector] public float KB;

    public void Hit(int damage, GameObject _source, float _KB) {
        _damage = damage;
        source = _source;
        KB = _KB;

        OnHit.Invoke();
    }
}
