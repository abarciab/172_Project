using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnToFacePlayer : MonoBehaviour
{
    GameObject player;
    [SerializeField] bool yOnly = true;

    private void Start()
    {
        player = Player.i.gameObject;
    }

    void Update()
    {
        var rot = transform.localEulerAngles;
        transform.LookAt(player.transform.position);
        rot.y = transform.localEulerAngles.y;
        if (yOnly) transform.localEulerAngles = rot;
    }
}
