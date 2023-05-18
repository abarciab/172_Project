using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyVisibility : MonoBehaviour
{
    [SerializeField] GameObject controller, copy;

    void Update()
    {
        copy.SetActive(controller.activeInHierarchy);
    }
}
