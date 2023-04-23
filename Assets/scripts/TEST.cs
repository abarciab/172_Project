using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteAlways]
public class TEST : MonoBehaviour 
{
    public bool test;

    private void Update()
    {
        if (test) {
            test = false;
            //GetComponent<HitReciever>().Hit2(new HitReciever.HitData());
        }
    }
}
