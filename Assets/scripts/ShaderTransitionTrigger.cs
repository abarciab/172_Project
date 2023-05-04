using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderTransitionTrigger : MonoBehaviour
{
    [SerializeField] bool start;
    [SerializeField] Transform destTrigger;
    [SerializeField] int stage;

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<Player>();
        if (player) {

            if (!start) {
                ShaderTransitionController.i.EndTransition(stage);
                Destroy(this);
                return;
            }

            var dist = Vector3.Distance(destTrigger.position, player.transform.position);
            ShaderTransitionController.i.StartTransition(dist, destTrigger.position, stage);
            Destroy(this);
        }
    }
}
