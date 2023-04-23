using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HitReciever : MonoBehaviour
{
    [System.Serializable]
    public class HitData
    {
        public int damage;
        public GameObject source;
        public float KB;
        public Vector3 offset;
        public float stunTime;

        public HitData(int _damage = 0, GameObject _source = null, float _KB = 0, Vector3 _offset = default(Vector3), float _stunTime = 0)
        {
            damage = _damage;
            source = _source;
            KB = _KB;
            offset = _offset;
            stunTime = _stunTime;
        }
    }

    [HideInInspector] public UnityEvent OnHit = new UnityEvent();
    //[HideInInspector] protected int _damage;
    //[HideInInspector] protected GameObject source;
    //protected Vector3 hitSourceOffset;
    //[HideInInspector] protected float KB;


    /*public void Hit(int damage, GameObject _source, float _KB, Vector3 offset = default) {
        hitSourceOffset = offset == default ? Vector3.zero : offset;
        _damage = damage;
        source = _source;
        KB = _KB;

        OnHit.Invoke();
    }*/

    virtual public void Hit2(HitData hit) { OnHit.Invoke(); }
}
