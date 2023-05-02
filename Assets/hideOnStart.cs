using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hideOnStart : MonoBehaviour
{
    void Awake()
    {
        gameObject.SetActive(false);
        Destroy(this);
    }
}
