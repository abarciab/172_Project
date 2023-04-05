using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goat : MonoBehaviour
{
    EnemyMovement move;
    [SerializeField] Animator anim;
    Vector3 oldPosition;
    [SerializeField] bool agro;
    [SerializeField] int pointID;
    [SerializeField] Vector2 waitTimeRange;  

    Transform target;
    int lastPointID = -1;
    float waitTime;

    private void Start()
    {
        move = GetComponent<EnemyMovement>();
        oldPosition = transform.position;
        waitTime = Random.Range(waitTimeRange.x, waitTimeRange.y);
    }

    private void Update()
    {
        anim.SetBool("moving", (Vector3.Distance(transform.position, oldPosition) > 0.001f));
        oldPosition = transform.position;
        if (GetComponent<EnemyStats>().health == 0) {
            anim.SetTrigger("die");
            move.gotoTarget = false;
            return;
        }
        
        waitTime -= Time.deltaTime;
        if (waitTime > 0) return;

        if (target == null) target = GetNewTarget();
        if (target == null) return;
        var dist = Vector3.Distance(transform.position, target.position);

        if (dist > 1f) {
            move.gotoTarget = true;
            move.target = target.position;
        }
        else {
            waitTime = Random.Range(waitTimeRange.x, waitTimeRange.y);
            target = null;
        }

        if (agro) {
            move.target = Player.i.transform.position;
            move.gotoTarget = agro;
        }
    }

    Transform GetNewTarget()
    {
        if (EnemyPoint.i == null) return null;
        var point = EnemyPoint.i.FindPoint(pointID, lastPointID);
        if (point != null) lastPointID = point.GetComponent<EnemyPoint>().UniqueID;
        return point;
    }

}
