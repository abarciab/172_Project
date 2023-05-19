using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpearVFXCoordinator : MonoBehaviour
{
    [SerializeField] SkinnedMeshRenderer spear;
    [SerializeField] MeshRenderer spearMesh;
    [SerializeField] Material dullMat, shinyMat;
    [SerializeField] GameObject ps_sunblast, ps_throwTrail, VFXGraph_impactHardSurface;
    PFighting fight;

    private void Start()
    {
        fight = Player.i.GetComponent<PFighting>();
        VFXGraph_impactHardSurface.SetActive(false);
    }

    private void Update()
    {
        if (!fight.enabled || fight.GetSWcooldown() > 0) {
            ps_sunblast.SetActive(false);
            if (spear) spear.material = dullMat;
            if (spearMesh) spearMesh.material = dullMat;
        }
        else {
            ps_sunblast.SetActive(true);
            if (spear) spear.material = shinyMat;
            if (spearMesh) spearMesh.material = shinyMat;
        }

    }

    //Handle throw trail vfx
    public void EnableTrailVFX()
    {
        if (!ps_throwTrail.activeSelf)
            ps_throwTrail.SetActive(true);
    }
    public void DisableTrailVFX()
    {
        if (ps_throwTrail.activeSelf)
            ps_throwTrail.SetActive(false);
    }

    //Handle hard surface impact vfx
    public void TriggerHSImpact()
    {

    }
}
