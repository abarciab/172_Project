using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeRoarTrigger : MonoBehaviour
{
    [SerializeField] GameObject snakeSource;
    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<Player>();
        if (player)
        {
            snakeSource.GetComponent<SnakeRoarsSource>().PlayRoar();
        }
    }
}
