using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpearVFXCoordinator : MonoBehaviour
{
    [SerializeField] SkinnedMeshRenderer spear;
    [SerializeField] MeshRenderer spearMesh;
    [SerializeField] Material dullMat, shinyMat;
    [SerializeField] GameObject extras;
    PFighting fight;

    private void Start()
    {
        fight = Player.i.GetComponent<PFighting>();
    }

    private void Update()
    {
        if (!fight.enabled || fight.GetSWcooldown() > 0) {
            extras.SetActive(false);
            if (spear) spear.material = dullMat;
            if (spearMesh) spearMesh.material = dullMat;
        }
        else {
            extras.SetActive(true);
            if (spear) spear.material = shinyMat;
            if (spearMesh) spearMesh.material = shinyMat;
        }
    }
}
