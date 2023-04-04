using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TEST : MonoBehaviour
{
    public GameObject recieverObj;
    public bool testHit;

    private void Update() {
        if (recieverObj == null) enabled = false;
        var reciever = recieverObj.GetComponent<HitReciever>();
        if (reciever == null) enabled = false;

        if (testHit) {
            testHit = false;
            //reciever.Hit(10);
        }
    }
}
