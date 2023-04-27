using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteAlways]
public class TEST : MonoBehaviour 
{
    public float num1;
    public Vector2 limits;

    private void Update()
    {
        print(Mathf.Clamp(num1, limits.x, limits.y));
    }
}
