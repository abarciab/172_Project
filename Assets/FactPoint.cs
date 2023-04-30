using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactPoint : MonoBehaviour
{
    [SerializeField] Fact factToAdd;
    [SerializeField] int ifThisManyFacts = 0;

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<Player>();
        if (!player) return;

        if (FactManager.i.numFacts() == ifThisManyFacts) { FactManager.i.AddFact(factToAdd); Destroy(gameObject); }
    }
}
