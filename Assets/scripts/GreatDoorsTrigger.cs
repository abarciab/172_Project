using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreatDoorsTrigger : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] GameObject greatDoors;
    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<Player>();
        if (player)
        {
            greatDoors.GetComponent<GreatDoorsController>().OpenDoors();
            Object.Destroy(this.gameObject);
        }
    }
}
