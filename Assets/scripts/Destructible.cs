using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Destructible : HitReciever
{
    public override void Hit(HitData hit)
    {
        base.Hit(hit);

        Destroy(gameObject);
    }

}
