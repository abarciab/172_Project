using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactPoint : MonoBehaviour
{
    [SerializeField] Fact factToAdd, notIfThisIsPresent, factToRemove;
    [SerializeField] int ifThisManyFacts = -1;

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<Player>();
        if (!player || (notIfThisIsPresent != null && FactManager.i.IsPresent(notIfThisIsPresent))) return;

        if (ifThisManyFacts == -1 || FactManager.i.numFacts() == ifThisManyFacts) { 
            FactManager.i.AddFact(factToAdd); 
            FactManager.i.RemoveFact(factToRemove); 
            Destroy(gameObject); 
        }
    }
}
