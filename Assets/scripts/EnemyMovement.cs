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
    [SerializeField] float KBDecay = 0.9f;

    float originalAngSpeed;
    float originalSpeed;

    private void Start() {
        agent = GetComponent<NavMeshAgent>();
        originalAngSpeed = agent.angularSpeed;
        originalSpeed = agent.speed;
    }

    private void Update() {
        if (gotoTarget) {
            if (!agent.enabled) { agent.enabled = true; return; }
            agent.isStopped = false;
            agent.SetDestination(target);
        }
        else if (agent.enabled) {
            agent.isStopped = true;
            agent.enabled = false;
        }
    }

    public void KnockBack(GameObject source, float _KB)
    {
        //print("source: " + source.name + ", amount: " + _KB);
        if (_KB == 0 || source == null) return;
        StartCoroutine(_KnockBack(source, _KB));
    }

    IEnumerator _KnockBack(GameObject source, float _KB)
    {
        agent.enabled = false;
        while (_KB > 0.01f) {
            var dir = (source.transform.position - transform.position).normalized;
            dir.y = 0;

            transform.position += dir * _KB;
            _KB *= KBDecay;
            yield return new WaitForEndOfFrame();
        }
        agent.enabled = true;
    }

    public void ResetSpeed()
    {
        agent.speed = originalSpeed;
    }

    public void ChangeSpeed(float newSpeed)
    {
        agent.speed = newSpeed;
    }

    public void disableRotation() {
        agent.angularSpeed = 0;
    }

    public void EnableRotation() {
        agent.angularSpeed = originalAngSpeed;
    }

    private void OnDrawGizmos()
    {
        return;
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(target, 1);
    }
}
