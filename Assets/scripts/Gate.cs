using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : MonoBehaviour 
{ 
    public string prompt;
    public Vector3 outputPos;

    private void OnTriggerEnter(Collider collision)
    {
        var player = collision.GetComponent<Player>();
        if (player) player.AddInteractable(this);
    }

    public void Interact()
    {
        Player.i.transform.position = transform.position + outputPos;
        Player.i.RemoveInteractable(this);
    }

    private void OnDrawGizmos()
    {
        int layermask = 1 << 7;
        Physics.Raycast(transform.position + outputPos, Vector3.down, out var hit, 100, layermask);
        if (hit.collider != null) outputPos.y = hit.point.y - transform.position.y;

        Gizmos.DrawWireSphere(transform.position + outputPos, 0.5f);
    }
}
