using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPoint : MonoBehaviour
{
    public static EnemyPoint i;
    public List<EnemyPoint> OtherPoints = new List<EnemyPoint>();

    [Min(0)]
    public int ID;
    public int UniqueID;

    private void Awake()
    {
        if (i == null) i = this;
        else i.OtherPoints.Add(this);

        UniqueID = Random.Range(0, 10000);
    }

    public Transform FindPoint(int _ID, int NullID = -1)
    {
        var newList = new List<EnemyPoint>(OtherPoints);

        while (newList.Count > 0) {
            var point = newList[Random.Range(0, newList.Count)];
            if (point.ID == _ID && point.UniqueID != NullID) return point.transform;
            newList.Remove(point);
        }
        return null;
    }
}
