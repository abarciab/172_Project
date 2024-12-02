using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CheckPointData
{
    public Transform point;
    public int ID;

    public CheckPointData(Transform _point, int _ID)
    {
        point = _point;
        ID = _ID;
    }
}
