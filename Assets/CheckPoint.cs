using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    [SerializeField] int ID;

    private void Start()
    {
        GameManager.i.AddCheckPoint(transform, ID);
    }

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<Player>();
        if (player) PlayerPrefs.SetInt("checkpoint", ID);
    }
}
