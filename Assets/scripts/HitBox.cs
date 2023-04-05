using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBox : MonoBehaviour
{
    public bool checking;
    [HideInInspector] public List<HitReciever> targets = new List<HitReciever>();
    [SerializeField] List<HitBox> linkedBoxes = new List<HitBox>();
    
    public void StartChecking() {
        checking = true;
        targets.Clear();
        foreach (var l in linkedBoxes) l.StartChecking();
    }

    public List<HitReciever> EndChecking() {
        checking = false;
        foreach (var l in linkedBoxes) {
            var extras = l.EndChecking();
            foreach (var e in extras) if (!targets.Contains(e)) targets.Add(e);
        }
        return targets;
    }

    private void OnTriggerEnter(Collider other) {

        if (!checking) return;

        var reciever = other.GetComponent<HitReciever>();
        if (reciever == null || targets.Contains(reciever)) return;

        targets.Add(reciever);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!checking) return;

        var reciever = other.GetComponent<HitReciever>();
        if (reciever == null || targets.Contains(reciever)) return;

        targets.Add(reciever);
    }
}
