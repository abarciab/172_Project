using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerFact : MonoBehaviour
{
    [SerializeField] Fact fact;
    [SerializeField] GameObject enableOnTrigger;

    private void Update()
    {
        if (FactManager.i.IsPresent(fact) && enableOnTrigger != null && !enableOnTrigger.activeInHierarchy) {
            if (enableOnTrigger) enableOnTrigger.SetActive(true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<Player>();
        if (!player) player = other.GetComponentInParent<Player>();
        if (!player) return;

        if (fact) FactManager.i.AddFact(fact);
        if (enableOnTrigger) enableOnTrigger.SetActive(true);

        Destroy(gameObject);
    }
}
