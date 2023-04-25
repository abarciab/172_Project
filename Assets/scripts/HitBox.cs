using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HitBox : MonoBehaviour
{
    public bool checking;
    [HideInInspector] public List<HitReciever> targets = new List<HitReciever>();
    [SerializeField] List<HitBox> linkedBoxes = new List<HitBox>();

    bool hitting;
    float kb;
    int dmg;
    GameObject obj;
    Vector3 offset;
    [HideInInspector] public UnityEvent OnHit;


    public void StartChecking(bool _hitting = false, int _dmg = 0, float _kb = 0, GameObject _obj = null, Vector3 _offset = default) {
        hitting = _hitting;
        kb = _kb;
        dmg = _dmg;
        obj = _obj;
        offset = _offset;
        checking = true;
        targets.Clear();
        foreach (var l in linkedBoxes) l.StartChecking(hitting, _dmg ,_kb, _obj, _offset);
    }

    public void Refresh()
    {
        targets.Clear();
        foreach (var l in linkedBoxes) l.Refresh(); 
    }

    public List<HitReciever> EndChecking() {
        checking = false;
        foreach (var l in linkedBoxes) {
            var extras = l.EndChecking();
            foreach (var e in extras) if (!targets.Contains(e)) targets.Add(e);
        }
        return targets;
    }

    private void OnTriggerEnter(Collider other) 
    {
        Check(other);   
    }

    private void OnTriggerStay(Collider other)
    {
        Check(other);
    }

    void Check(Collider other)
    {
        if (!checking) return;

        var reciever = other.GetComponent<HitReciever>();
        if (reciever == null || targets.Contains(reciever)) return;

        if (hitting) {
            reciever.Hit2(new HitReciever.HitData(dmg, obj, kb, offset));
            OnHit.Invoke();
        }
        targets.Add(reciever);
    }
}
