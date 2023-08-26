    using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Destructible : HitReciever
{
    [SerializeField] Fact prerequisiteFact;
    [SerializeField] List<Fact> addWhenDestroy = new List<Fact>();
    [SerializeField] Sound playWhenDestroyed;

    private void Start()
    {
        if (playWhenDestroyed) playWhenDestroyed = Instantiate(playWhenDestroyed);
    }

    public override void Hit(HitData hit)
    {
        if (prerequisiteFact != null && !FactManager.i.IsPresent(prerequisiteFact)) return;

        base.Hit(hit);

        if (hit.damage == 0) { print("0 dmg"); return; }

        if (playWhenDestroyed) playWhenDestroyed.Play();
        Destroy(gameObject);

        foreach (var f in addWhenDestroy) FactManager.i.AddFact(f);
    }

}
