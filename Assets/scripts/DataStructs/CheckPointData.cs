using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyGroupData
{
    public Fact fact;
    public List<GameObject> enemies = new List<GameObject>();
    public int ID;
    public bool enabled;
}
