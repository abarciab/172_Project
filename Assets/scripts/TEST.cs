using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class TEST : MonoBehaviour 
{
    public bool getFact;
    public Fact testFact;
    public string factName;

    private void Update()
    {
        if (getFact) {
            getFact = false;
            testFact = Resources.Load<Fact>("facts/" + factName);
        }
    }
}
