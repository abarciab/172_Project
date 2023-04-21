using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderTransitionTrigger : MonoBehaviour
{
    [SerializeField] bool start;
    [SerializeField] Transform destTrigger;

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<Player>();
        if (player) {

            if (!start) {
                ShaderTransitionController.i.EndTransition();
                Destroy(this);
                return;
            }

            var dist = Vector3.Distance(destTrigger.position, player.transform.position);
            ShaderTransitionController.i.StartTransition(dist, destTrigger.position);
            Destroy(this);
        }
    }
}
