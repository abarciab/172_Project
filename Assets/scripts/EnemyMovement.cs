using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.HighDefinition;
using UnityEngine;
using UnityEngine.AI;
using static Unity.VisualScripting.Member;

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
            agent.isStopped = false;
            agent.SetDestination(target);
        }
        else agent.isStopped = true;
    }

    public void KnockBack(GameObject source, float _KB)
    {
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

    public void NormalSpeed()
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
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(target, 1);
    }
}
