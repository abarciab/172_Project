using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goat : MonoBehaviour
{
    EnemyMovement move;
    [SerializeField] Animator anim;
    Vector3 oldPosition;

    private void Start()
    {
        move = GetComponent<EnemyMovement>();
        oldPosition = transform.position;
    }

    private void Update()
    {
        anim.SetBool("moving", (Vector3.Distance(transform.position, oldPosition) > 0.001f));
        oldPosition = transform.position;

        move.target = Player.i.transform.position;
        move.gotoTarget = true;
    }
}
