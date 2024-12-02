using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour
{
    [SerializeField] Sound fireSound;

    void Start()
    {
        fireSound = Instantiate(fireSound);
        fireSound.Play(transform);
    }
}
