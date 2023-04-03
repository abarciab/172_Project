using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBox : MonoBehaviour
{
    public bool checking;
    [HideInInspector] public List<HitReciever> targets = new List<HitReciever>();
    
    public void StartChecking() {
        checking = true;
        targets.Clear();
    }

    public List<HitReciever> EndChecking() {
        checking = false;
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
