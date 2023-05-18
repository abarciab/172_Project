using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildrenFact : MonoBehaviour
{
    [SerializeField] Fact fact;
    [SerializeField] int childNum;

    private void Update()
    {
        if (transform.childCount == childNum) {
            FactManager.i.AddFact(fact);
            enabled = false;
        }
    }
}
