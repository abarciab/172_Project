using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class SpearVFXCoordinator : MonoBehaviour
{
    [SerializeField] SkinnedMeshRenderer spear;
    [SerializeField] MeshRenderer spearMesh;
    [SerializeField] Material dullMat, shinyMat;
    [SerializeField] GameObject ps_sunblast, ps_throwTrail, VFX_impactHardSurface, VFX_spearCatch, VFX_spearCatch2;
    PFighting fight;

    private void Start()
    {
        fight = Player.i.GetComponent<PFighting>();
        VFX_impactHardSurface.SetActive(false);
        //VFX_spearCatch.SetActive(false);
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
    public void EnableHSImpact()
    {
        VFX_impactHardSurface.SetActive(true);

    }
    public void DisableHSImpact()
    {
        VFX_impactHardSurface.SetActive(false);

    }

    //Handle spear catch vfx
    public void PlaySpearCatch()
    {
        //VFX_spearCatch.SetActive(true);
        VFX_spearCatch.GetComponent<VisualEffect>().Play();
        VFX_spearCatch2.GetComponent<VisualEffect>().Play();

    }
}
