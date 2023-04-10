using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBox : MonoBehaviour
{
    public bool checking;
    [HideInInspector] public List<HitReciever> targets = new List<HitReciever>();
    [SerializeField] List<HitBox> linkedBoxes = new List<HitBox>();

    bool hitting;
    float kb;
    int dmg;
    GameObject obj;
    
    public void StartChecking(bool _hitting = false, int _dmg = 0, float _kb = 0, GameObject _obj = null) {
        hitting = _hitting;
        kb = _kb;
        dmg = _dmg;
        obj = _obj;
        checking = true;
        targets.Clear();
        foreach (var l in linkedBoxes) l.StartChecking();
    }

    public void Refresh()
    {
        targets.Clear();
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

        if (hitting) reciever.Hit(dmg, obj, kb);
        targets.Add(reciever);
    }
}
