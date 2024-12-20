using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum PSoundKey { FOOTSTEP, FOOTSTEP_HARD, FOOTSTEP_GOOP, FOOTSTEP_RUN, STAB_SWISH, DRAW_SPEAR, START_CONVO, HURT, 
    DEATH, HEALTH_REGEN, END_COMBAT, HEARTBEAT, ROLL, THROW_SPEAR, SPEAR_CATCH, SUNBLAST, SUNBLASTREADY, SUNBLAST_ERROR, RECALL, CRIT_HIT, RECALL_ERROR, THROW_CHARGE}

[System.Serializable]
public class PSoundData
{
    [HideInInspector] public string Name;
    public PSoundKey Key;
    public Sound Sound;
}

public class PSound : MonoBehaviour
{
    [SerializeField] private List<PSoundData> data = new List<PSoundData>();
    public Sound Get(PSoundKey key) => data.Where(x => x.Key == key).First().Sound;

    private void OnValidate()
    {
        foreach (var d in data) d.Name = d.Key + " " + (d.Sound == null ? "(missing)" : "");
    }

    private void Start()
    {
        foreach (var d in data) d.Sound = Instantiate(d.Sound);

        Get(PSoundKey.HEARTBEAT).PlaySilent();
    }


    public void DrawSpear() => Get(PSoundKey.DRAW_SPEAR).Play();
    public void PlaySwoosh() => Get(PSoundKey.STAB_SWISH).Play();
    public void PlayFootStep() => Get(PSoundKey.FOOTSTEP).Play();
    public void PlayFootStepRun() => PlayFootStep();
}
