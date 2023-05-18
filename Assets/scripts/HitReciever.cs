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
        public bool crit, stun;

        public HitData(int _damage = 0, GameObject _source = null, float _KB = 0, Vector3 _offset = default(Vector3), float _stunTime = 0, bool _crit = false, bool _stun = false)
        {
            damage = _damage;
            source = _source;
            KB = _KB;
            offset = _offset;
            stunTime = _stunTime;
            crit = _crit;
            stun = _stun;
        }
    }

    [HideInInspector] public UnityEvent OnHit = new UnityEvent();

    virtual public void Hit(HitData hit) { OnHit.Invoke(); }
}
