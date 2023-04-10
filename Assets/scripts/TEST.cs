using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TEST : MonoBehaviour 
{
    public EnemyMovement move;

    private void Update() {
        if (!Player.i.enemies.Contains(move))Player.i.enemies.Add(move);
    }
}
