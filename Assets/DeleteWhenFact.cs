using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteWhenFact : MonoBehaviour
{
    [SerializeField] Fact fact;

    void Update()
    {
        if (FactManager.i.IsPresent(fact)) Destroy(gameObject);
    }
}
