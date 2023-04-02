using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMovement : MonoBehaviour
{
    NavMeshAgent agent;
    public bool gotoTarget;
    public Vector3 target;
    public Vector3 centerOffset;

    float originalAngSpeed;

    private void Start() {
        agent = GetComponent<NavMeshAgent>();
        originalAngSpeed = agent.angularSpeed;
    }

    private void Update() {
        if (gotoTarget) {
            agent.isStopped = false;
            agent.SetDestination(target);
        }
        else agent.isStopped = true; 
    }

    public void disableRotation() {
        agent.angularSpeed = 0;
    }

    public void EnableRotation() {
        agent.angularSpeed = originalAngSpeed;
    }
}
