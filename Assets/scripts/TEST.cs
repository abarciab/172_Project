using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[ExecuteAlways]
public class TEST : MonoBehaviour 
{
    public bool stayOnGround;
    Vector3 ground;

    private void Update()
    {
        Physics.Raycast(transform.position + Vector3.up * 100, Vector3.down, out var hit, 150, layerMask: ~7);
        if (hit.collider == null) return;
        ground = hit.point;

        if (stayOnGround) {
            var pos = transform.position;
            pos.y = ground.y;
            transform.position = pos;
        }
    }

    private void OnDrawGizmos()
    {
        
        Gizmos.DrawSphere(ground, 0.2f);
    }
}
