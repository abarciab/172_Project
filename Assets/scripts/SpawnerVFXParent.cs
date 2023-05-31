using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX; 

public class SpawnerVFXParent : MonoBehaviour
{
    [SerializeField] GameObject spewVFX;
    [SerializeField] float playTime;

    void Start()
    {
        spewVFX.GetComponent<VisualEffect>().Stop();
    }

    public void TriggerSpawnVFX()
    {
        //play vfx and set timer to trigger the stop vfx Function
        spewVFX.GetComponent<VisualEffect>().Play();
        StartCoroutine(StopVFX(playTime));
    }
    IEnumerator StopVFX(float time)
    {
        yield return new WaitForSeconds(time);
        spewVFX.GetComponent<VisualEffect>().Stop();
    }
}
