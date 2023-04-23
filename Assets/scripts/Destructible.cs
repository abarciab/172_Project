using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Destructible : HitReciever
{
    public override void Hit2(HitData hit)
    {
        base.Hit2(hit);

        Destroy(gameObject);
    }

}
