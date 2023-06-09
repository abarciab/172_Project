using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class armadilloVFXHandler : MonoBehaviour
{
    [SerializeField] GameObject armadilloController;

    public void TriggerErruption()
    {
        armadilloController.GetComponent<Armadillo>().TriggerErruptionVFX();
    }
}
